using System;
using MvvmHelpers;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xam.Plugins.OnDeviceCustomVision;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace IgniteTourCDMX.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        private string _results;
        private ImageSource _image;

        public MainViewModel()
        {
            RunMlLocallyCommand = new Command(async () => await RunMlExecuted(true));
            RunMlCommand = new Command(async () => await RunMlExecuted());
        }

        public ICommand RunMlCommand { get; }

        public ICommand RunMlLocallyCommand { get; }

        public string Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        private async Task RunMlExecuted(bool runLocally = false)
        {
            if (CrossMedia.Current.IsTakePhotoSupported)
            {
                var mediaItem = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });
                if (mediaItem != null)
                {
                    Image = ImageSource.FromStream(() => mediaItem.GetStream());
                    await ClassifyImageAsync(mediaItem, runLocally);
                }
            }
        }

        private async Task ClassifyImageAsync(MediaFile mediaItem, bool runLocally = false)
        {
            if (runLocally)
            {
                var tags = await CrossImageClassifier.Current.ClassifyImage(mediaItem.GetStream());
                if (tags?.Any() ?? false)
                {
                    Results = string.Join("\n", tags.Select(item => $"{item.Tag} - {item.Probability}"));
                }
                else
                {
                    Results = "No Result :(";
                }
            }
            else
            {
                var client = new CustomVisionPredictionClient();
                client.ApiKey = "APIKEY";
                client.Endpoint = "ENDPOINT";
                var result = await client.ClassifyImageAsync(Guid.Parse("ID"), "ITERATION", mediaItem.GetStream());
                if (result?.Predictions?.Any() ?? false)
                {
                    Results = string.Join("\n", result.Predictions.Select(item => $"{item.TagName} - {item.Probability}"));
                }
                else
                {
                    Results = "No Result :(";
                }
            }
        }
    }
}
