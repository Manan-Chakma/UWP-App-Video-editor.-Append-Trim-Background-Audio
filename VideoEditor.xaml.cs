using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Editing;
using Windows.Media.Core;
using Windows.Media.Playback;
using System.Threading.Tasks;

using Windows.Media.Transcoding;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Media.FaceAnalysis;
using System.Threading;
using Windows.System.Threading;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Media.Capture.Frames;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace EmotionDetector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoEditor : Page
    {
        private MediaComposition composition = new MediaComposition();
        private MediaStreamSource mediaStreamSource;







        private StorageItemAccessList storageItemAccessList;
       // private MainPage rootPage ;
        //private MediaComposition composition;
        private StorageFile pickedFile;

        private StorageFile secondVideoFile;
        private IRandomAccessStream writeStream;

        public VideoEditor()
        {
            this.InitializeComponent();
            trimClip.IsEnabled = false;
            RenderButton.IsEnabled = false;
            addAudio.IsEnabled = false;
            Append.IsEnabled = false;
            secondCli.IsEnabled = false;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            mediaElement.Source = null;
            mediaStreamSource = null;
            base.OnNavigatedFrom(e);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {


            // Make sure we don't run out of entries in StoreItemAccessList.
            // As we don't need to persist this across app sessions/pages
            // every time should be sufficient for us.
            storageItemAccessList = StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Clear();
        }


        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            secondCli.IsEnabled = true;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");

            pickedFile = await picker.PickSingleFileAsync();
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Add(pickedFile);

            var clip = await MediaClip.CreateFromFileAsync(pickedFile);

            if (pickedFile == null)
            {
                return;
            }


            else {
                composition = new MediaComposition();
                composition.Clips.Add(clip);
                mediaElement.Position = TimeSpan.Zero;
                mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
                mediaElement.SetMediaStreamSource(mediaStreamSource);

                trimClip.IsEnabled = true;
                addAudio.IsEnabled = true;
            }
        }

        private async void secondClip_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            secondVideoFile = await picker.PickSingleFileAsync();
            if (secondVideoFile == null)
            {
               // rootPage.NotifyUser("File picking cancelled", NotifyType.ErrorMessage);
                return;
            }

            mediaElement.SetSource(await secondVideoFile.OpenReadAsync(), secondVideoFile.ContentType);
            Append.IsEnabled = true;
            trimClip.IsEnabled = false;
            RenderButton.IsEnabled = false;
            addAudio.IsEnabled = false;
            
        }

        public void close()
        {
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource(
           (int)mediaElement.ActualWidth,
           (int)mediaElement.ActualHeight);

            mediaElement.SetMediaStreamSource(mediaStreamSource);
            
        }

        private async void addAudio_Click(object sender, RoutedEventArgs e)
        {
            try {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                picker.FileTypeFilter.Add(".mp3");
                picker.FileTypeFilter.Add(".wav");
                picker.FileTypeFilter.Add(".flac");
                Windows.Storage.StorageFile audioFile = await picker.PickSingleFileAsync();
                if (audioFile == null)
                {
                    //
                    txtBlock2.Text = "only .mp3, .wav, . flac file is acceptable";
                    return;
                }

                // These files could be picked from a location that we won't have access to later
                var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
                storageItemAccessList.Add(audioFile);

                var backgroundTrack = await BackgroundAudioTrack.CreateFromFileAsync(audioFile);

                composition.BackgroundAudioTracks.Add(backgroundTrack);
                RenderButton.IsEnabled = true;
            }
            catch {
                return;
            }

        }


        private async void trimming_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                long x = Int64.Parse(startTime.Text);
                long y = Int64.Parse(endTime.Text);
                var clip = await MediaClip.CreateFromFileAsync(pickedFile);

                if (Append.IsEnabled == true) {

                    
                    var secondClip = await MediaClip.CreateFromFileAsync(secondVideoFile);
                    
                    clip.TrimTimeFromStart = new TimeSpan(x * 10000000);
                    secondClip.TrimTimeFromEnd = new TimeSpan(y * 10000000);
                    composition = new MediaComposition();
                    composition.Clips.Add(clip);
                    composition.Clips.Add(secondClip);
                    mediaElement.Position = TimeSpan.Zero;
                    mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
                    mediaElement.SetMediaStreamSource(mediaStreamSource);
                    trimClip.IsEnabled = false;


                }
                else
                {
                    
                    clip.TrimTimeFromStart = new TimeSpan(x * 10000000);
                    clip.TrimTimeFromEnd = new TimeSpan(y * 10000000);
                    composition = new MediaComposition();
                    composition.Clips.Add(clip);
                    mediaElement.Position = TimeSpan.Zero;
                    mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
                    mediaElement.SetMediaStreamSource(mediaStreamSource);
                    trimClip.IsEnabled = false;

                }

                

                
                
                 
                 trimmed.Text = "Clip trimmed";
                 RenderButton.IsEnabled = true;
                
                

                
            }
            catch {
                Error.Text = "Start or End time is not set yet";
                
                return;

            }

        }

        private async void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MP4 files", new List<string>() { ".mp4" });
            picker.SuggestedFileName = "RenderedComposition.mp4";

            Windows.Storage.StorageFile pickedFile = await picker.PickSaveFileAsync();
            if (pickedFile != null)
            {
                // Call RenderToFileAsync
                var saveOperation = composition.RenderToFileAsync(pickedFile, MediaTrimmingPreference.Precise);

                saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>(async (info, progress) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                    {
                        //ShowErrorMessage(string.Format("Saving file... Progress: {0:F0}%", progress));
                        txtBlock.Text = "Progress" + progress + "%";
                    }));
                });
                saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>(async (info, status) =>
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                    {
                        try
                        {
                            var results = info.GetResults();
                            if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                            {
                                //ShowErrorMessage("Saving was unsuccessful");
                                txtBlock.Text = "Saving was unsuccessful";
                            }
                            else
                            {
                                // ShowErrorMessage("Trimmed clip saved to file");
                                txtBlock.Text = "Clip save to file";
                                mediaElement.Source = null;
                                RenderButton.IsEnabled = false;
                            }
                        }
                        finally
                        {
                            
                        }

                    }));
                });
            }
            else
            {
                
                txtBlock.Text = "User cancelled file selection";
            }
        }




        private async void Append_Click(object sender, RoutedEventArgs e)
        {

            var clip = await MediaClip.CreateFromFileAsync(pickedFile);
            var secondClip = await MediaClip.CreateFromFileAsync(secondVideoFile);

            composition = new MediaComposition();
            composition.Clips.Add(clip);
            composition.Clips.Add(secondClip);

            // Render to MediaElement.
            mediaElement.Position = TimeSpan.Zero;
            mediaStreamSource = composition.GeneratePreviewMediaStreamSource((int)mediaElement.ActualWidth, (int)mediaElement.ActualHeight);
            mediaElement.SetMediaStreamSource(mediaStreamSource);
            trimmed.Text = "Appended";
            trimClip.IsEnabled = true;
            RenderButton.IsEnabled = true;
            addAudio.IsEnabled = true;
            
            secondCli.IsEnabled = false;

            // rootPage.NotifyUser("Clips appended", NotifyType.StatusMessage);

        }

        private async void New_Click(object sender, RoutedEventArgs e)
        {

           // var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();
        }








        private async Task<string> Capture(StorageFile file, TimeSpan timeOfFrame, Size imageDimension)
        {
            if (file == null)
            {
                return null;
            }

            //Get FrameWidth & FrameHeight
            List<string> encodingPropertiesToRetrieve = new List<string>();
            encodingPropertiesToRetrieve.Add("System.Video.FrameHeight");
            encodingPropertiesToRetrieve.Add("System.Video.FrameWidth");
            IDictionary<string, object> encodingProperties = await file.Properties.RetrievePropertiesAsync(encodingPropertiesToRetrieve);
            uint frameHeight = (uint)encodingProperties["System.Video.FrameHeight"];
            uint frameWidth = (uint)encodingProperties["System.Video.FrameWidth"];

            //Get image stream
            var clip = await MediaClip.CreateFromFileAsync(file);
            var composition = new MediaComposition();
            composition.Clips.Add(clip);
            var imageStream = await composition.GetThumbnailAsync(timeOfFrame, (int)frameWidth, (int)frameHeight, VideoFramePrecision.NearestFrame);

            //Create BMP
            var writableBitmap = new WriteableBitmap((int)frameWidth, (int)frameHeight);
            writableBitmap.SetSource(imageStream);

            //Get stream from BMP
            string mediaCaptureFileName = "IMG" + Guid.NewGuid().ToString().Substring(0, 4) + ".jpg";
            var saveAsTarget = await CreateMediaFile(mediaCaptureFileName);
            Stream stream = writableBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);

            using (var writeStream = await saveAsTarget.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, writeStream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)writableBitmap.PixelWidth,
                    (uint)writableBitmap.PixelHeight,
                    96,
                    96,
                    pixels);
                await encoder.FlushAsync();
                using (var outputStream = writeStream.GetOutputStreamAt(0))
                {
                    await outputStream.FlushAsync();
                }
            }
            return saveAsTarget.Name;
        }



        public async Task<StorageFile> CreateMediaFile(string filename)
        {
            StorageFolder _mediaFolder = KnownFolders.PicturesLibrary;
            return await _mediaFolder.CreateFileAsync(filename);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Page1), null);
        }
    }



 }



