using Dank_Player_V3._0.Components;
using Dank_Player_V3._0.Components.Bindable;
using NAudio.CoreAudioApi;
using NAudio.Wave; 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.Bass.Misc;

using Path = System.IO.Path;
using WinForms = System.Windows.Forms;

namespace Dank_Player_V3._0
{
    public partial class MainWindow
    {
        private List<TrackListItem> _playlistItems;
        private Dictionary<string, string> _backgroundItems;

        private TimeSpan _totalTime;
        private DispatcherTimer _timerVideoTime;
        private DispatcherTimer _timerBassIntensityTick;

        private bool _isPlaying;
        private bool _isDragging;
        private bool _isTitleFixed;

        private string _currSource;

        private StreamPlayHandler streamPlayHandler;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMediaEvents();
            InitializeInteractionEvents();
            SetInitialDefaultValues();

            controlGrid.Visibility = Visibility.Visible;
        }

        #region CONTROL
        private void InitializeMediaEvents()
        {
            _isTitleFixed = false;
            this.MouseMove += (s, e) =>
            {
                if (this.Cursor == Cursors.None)
                    this.Cursor = Cursors.Arrow;

                if (!_isTitleFixed)
                {
                    double differenceX = (Mouse.GetPosition(this).X - this.Width / 2);
                    double differenceY = (Mouse.GetPosition(this).Y - this.Height / 2);

                    //debugText.Content = $"X:{differenceX} Y:{differenceY}";

                    txtMainTitle.Margin = new Thickness((differenceX / 10) / 2, (differenceY / 10) / 2, 0, 0);
                }
            };

            this.KeyUp += (s, e) =>
            {
                if (e.Key == Key.H && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    this.Cursor = Cursors.None;
                }
            };

            mediaBackground.MediaEnded += (s, e) =>
            {
                mediaBackground.Position = TimeSpan.FromMilliseconds(0);
            };

            streamPlayHandler = new StreamPlayHandler();
            streamPlayHandler.Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            streamPlayHandler.MediaLoaded += (s, e) => { MediaLoaded(e.currFile); };
        }

        private void SetInitialDefaultValues()
        {
            _playlistItems = new List<TrackListItem>();
            _backgroundItems = new Dictionary<string, string>();
            _totalTime = new TimeSpan();
            _timerVideoTime = new DispatcherTimer();
            _timerBassIntensityTick = new DispatcherTimer();

            txtMainTitle.Content = "Start by creating a new playlist >>";
            txtCompactTitle.Text = string.Empty;
            btnMenuRight.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));

            mediaBackground.Volume = 0;

            playerSlider.IsEnabled = false;
            volumeSlider.IsEnabled = false;
            btnShuffleEnabled.IsEnabled = false;
            btnPause.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnPrevious.IsEnabled = false;
            btnResetPlaylist.IsEnabled = false;
        }

        private void ButtonStyleChangeMove(object sender, MouseEventArgs args)
        {
            (sender as Grid).Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        }

        private void ButtonStyleChangeLeave(object sender, MouseEventArgs args)
        {
            (sender as Grid).Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
        }

        private void ButtonStyleChangeDown(object sender, MouseEventArgs args)
        {
            (sender as Grid).Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
        }

        private void InitializeInteractionEvents()
        {
            txtSearch.TextChanged += (s, e) =>
            {
                lstTrackList.ItemsSource = _playlistItems.Where(x => x.title.ToLower().Contains(txtSearch.Text.ToLower().Trim())).ToList();
            };

            btnPause.Click += (s, e) =>
            {
                if (_isPlaying)
                {
                    streamPlayHandler.Pause();
                    btnPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PlayCircle;
                    _isPlaying = false;
                    btnPause.ToolTip = "Play";
                }
                else
                {
                    streamPlayHandler.Play();
                    btnPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PauseCircle;
                    _isPlaying = true;
                    btnPause.ToolTip = "Pause";
                }
            };

            btnPrevious.Click += (s, e) =>
            {
                PreviousTrack();
            };

            btnNext.Click += (s, e) =>
            {
                NextTrack();
            };

            btnShuffleEnabled.Checked += (s, e) =>
            {
                btnShuffleEnabled.ToolTip = btnShuffleEnabled.IsChecked.Value ? "Shuffle on" : "Shuffle off";
                btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
            };

            btnShuffleEnabled.Unchecked += (s, e) =>
            {
                btnShuffleEnabled.ToolTip = btnShuffleEnabled.IsChecked.Value ? "Shuffle on" : "Shuffle off";
                btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
            };

            txtCompactTitle.MouseMove += (s, e) =>
            {
                if (svCompactTitle.ScrollableWidth > 0 && !Animation.isAnimatingHardScrollViewHorizontalOffset)
                {
                    Animation.HardAnimateScrollViewHorizontalOffset(svCompactTitle, svCompactTitle.ActualWidth + svCompactTitle.ScrollableWidth, 10);
                }
            };

            txtCompactTitle.MouseLeave += (s, e) =>
            {
                Animation.canAnimateHardScrollViewHorizontalOffset = false;
                svCompactTitle.ScrollToHorizontalOffset(0);
            };

            btnMenuLeft.MouseUp += (s, e) =>
            {
                flyoutMenuLeft.IsOpen = true;
                btnMenuLeft.Visibility = Visibility.Hidden;
                Animation.AnimateGridObjectMargin(btnMenuLeft, new Thickness(-20, 0, 0, 0), TimeSpan.FromMilliseconds(150));
                Animation.AnimateGridObjectOpacity(txtMainTitle, 0, TimeSpan.FromMilliseconds(300));
                (s as Grid).Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                txtMainTitle.IsEnabled = false;

                string destination = Directory.GetCurrentDirectory() + "/backgrounds";
                string[] motionBackgrounds = FileHelper.GetFiles(destination, "*.mov|*.mp4", SearchOption.AllDirectories);
                StackPanel mainStack = new StackPanel();
                mainStack.Orientation = Orientation.Vertical;

                _backgroundItems.Clear();
                for (int i = 0; i < motionBackgrounds.Length; i++)
                {
                    _backgroundItems.Add(Path.GetFileNameWithoutExtension(motionBackgrounds[i]), motionBackgrounds[i]);
                }

                for (int i = 0; i < motionBackgrounds.Length; i++)
                {
                    string thumbnailLocation = motionBackgrounds[i].Replace(Path.GetFileName(motionBackgrounds[i]), Path.GetFileNameWithoutExtension(motionBackgrounds[i]) + ".jpeg");

                    Image thumbnail = new Image()
                    {
                        Height = 160,
                        Margin = new Thickness(10, 10, 10, 10),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    if (File.Exists(thumbnailLocation))
                    {
                        thumbnail.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(thumbnailLocation));
                    }

                    Grid container = new Grid()
                    {
                        Height = 160,
                        Margin = new Thickness(10, 10, 10, 10),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    Label name = new Label()
                    {
                        Content = Path.GetFileNameWithoutExtension(motionBackgrounds[i]),
                    };

                    name.MouseUp += (se, ev) =>
                    {
                        mediaBackground.Source = new Uri(_backgroundItems[(se as Label).Content.ToString()]);
                    };

                    container.Children.Add(thumbnail);
                    container.Children.Add(name);
                    mainStack.Children.Add(container);
                }
                backgroundSelectorContainer.Content = mainStack;
            };

            flyoutMenuLeft.ClosingFinished += (s, e) =>
            {
                btnMenuLeft.Visibility = Visibility.Visible;
                Animation.AnimateGridObjectMargin(btnMenuLeft, new Thickness(0, 0, 0, 0), TimeSpan.FromMilliseconds(150));

                if (!flyoutMenuRight.IsOpen)
                {
                    Animation.AnimateGridObjectOpacity(txtMainTitle, 1, TimeSpan.FromMilliseconds(300));
                    txtMainTitle.IsEnabled = true;
                }
            };

            btnMenuRight.MouseUp += (s, e) =>
            {
                flyoutMenuRight.IsOpen = true;
                btnMenuRight.Visibility = Visibility.Hidden;
                Animation.AnimateGridObjectMargin(btnMenuRight, new Thickness(0, 0, -20, 0), TimeSpan.FromMilliseconds(150));
                Animation.AnimateGridObjectOpacity(txtMainTitle, 0, TimeSpan.FromMilliseconds(300));
                (s as Grid).Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                txtMainTitle.IsEnabled = false;
            };

            flyoutMenuRight.ClosingFinished += (s, e) =>
            {
                btnMenuRight.Visibility = Visibility.Visible;
                Animation.AnimateGridObjectMargin(btnMenuRight, new Thickness(0, 0, 0, 0), TimeSpan.FromMilliseconds(150));
                
                if (!flyoutMenuLeft.IsOpen)
                {
                    Animation.AnimateGridObjectOpacity(txtMainTitle, 1, TimeSpan.FromMilliseconds(300));
                    txtMainTitle.IsEnabled = true;
                }
            };

            btnNewPlaylist.Click += async(s, e) =>
            {
                WinForms.FolderBrowserDialog importDialog = new WinForms.FolderBrowserDialog();
                if (importDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(importDialog.SelectedPath, "*.mp3", SearchOption.AllDirectories);

                    if (files.Length > 1)
                    {
                        _playlistItems.Clear();
                        lstTrackList.Items.Clear();

                        btnNewPlaylist.Visibility = Visibility.Hidden;
                        tracksLoadingIndicator.Visibility = Visibility.Visible;

                        txtMainTitle.Content = "Loading playlist...";

                        List<string> durations = await Task.Run(() =>
                        {
                            List<string> temp = new List<string>();
                            for (int i = 0; i < files.Length; i++)
                            {
                                try
                                {
                                    Mp3FileReader reader = new Mp3FileReader(files[i]);
                                    temp.Add(reader.TotalTime.ToString(@"mm\:ss"));
                                }
                                catch (InvalidOperationException) { }
                            }
                            return temp;
                        });

                        for (int i = 0; i < files.Length; i++)
                        {
                            try
                            {
                                _playlistItems.Add(new TrackListItem(Path.GetFileNameWithoutExtension(files[i]), durations[i], files[i]));
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                _playlistItems.Add(new TrackListItem(Path.GetFileNameWithoutExtension(files[i]), "00:00", files[i]));
                            }
                        }

                        lstTrackList.ItemsSource = _playlistItems;

                        int totalMinutes = 0;
                        int totalSeconds = 0;

                        for (int i = 0; i < _playlistItems.Count; i++)
                        {
                            totalMinutes += int.Parse((_playlistItems[i].duration).Split(':')[0]);
                            totalSeconds += int.Parse((_playlistItems[i].duration).Split(':')[1]);
                        }

                        txtTotalTime.Content = new TimeSpan(0, totalMinutes, totalSeconds).ToString();

                        txtMainTitle.Content = "Playlist loaded >>";
                        lstTrackList.Visibility = Visibility.Visible;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;

                        btnNewPlaylist.IsEnabled = false;
                        btnNewPlaylist.Visibility = Visibility.Hidden;

                        txtSearch.IsEnabled = true;
                        btnResetPlaylist.IsEnabled = true;
                    }
                    else
                    {
                        if (_playlistItems.Count <= 0)
                            btnNewPlaylist.Visibility = Visibility.Visible;

                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
                        txtMainTitle.Content = "Start by creating a new playlist >>";
                        MessageBox.Show("No .mp3 files found in the specified directory", "Invalid directory", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            btnResetPlaylist.Click += async(s, e) =>
            {
                WinForms.FolderBrowserDialog importDialog = new WinForms.FolderBrowserDialog();
                if (importDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(importDialog.SelectedPath, "*.mp3", SearchOption.AllDirectories);

                    if (files.Length > 1)
                    {
                        streamPlayHandler.Stop();

                        _playlistItems.Clear();
                        lstTrackList.ItemsSource = null;

                        txtMaxTime.Text = "00:00";
                        txtCurrTime.Text = "00:00";
                        txtTotalTime.Content = "00:00:00";

                        lstTrackList.Visibility = Visibility.Hidden;
                        tracksLoadingIndicator.Visibility = Visibility.Visible;

                        playerSlider.IsEnabled = false;
                        volumeSlider.IsEnabled = false;
                        btnShuffleEnabled.IsEnabled = false;
                        btnPause.IsEnabled = false;
                        btnNext.IsEnabled = false;
                        btnPrevious.IsEnabled = false;
                        btnResetPlaylist.IsEnabled = false;
                        txtSearch.IsEnabled = false;

                        playerSlider.Value = 0;

                        txtMainTitle.Content = "Loading playlist...";
                        txtCompactTitle.Text = string.Empty;

                        List<string> durations = await Task.Run(() =>
                        {
                            List<string> temp = new List<string>();
                            for (int i = 0; i < files.Length; i++)
                            {
                                Mp3FileReader reader = new Mp3FileReader(files[i]);
                                temp.Add(reader.TotalTime.ToString(@"mm\:ss"));
                            }
                            return temp;
                        });

                        for (int i = 0; i < files.Length; i++)
                        {
                            _playlistItems.Add(new TrackListItem(Path.GetFileNameWithoutExtension(files[i]), durations[i], files[i]));
                        }

                        lstTrackList.ItemsSource = _playlistItems;

                        int totalMinutes = 0;
                        int totalSeconds = 0;

                        for (int i = 0; i < _playlistItems.Count; i++)
                        {
                            totalMinutes += int.Parse((_playlistItems[i].duration).Split(':')[0]);
                            totalSeconds += int.Parse((_playlistItems[i].duration).Split(':')[1]);
                        }

                        txtTotalTime.Content = new TimeSpan(0, totalMinutes, totalSeconds).ToString();

                        txtMainTitle.Content = "Playlist loaded >>";
                        btnResetPlaylist.IsEnabled = false;
                        lstTrackList.Visibility = Visibility.Hidden;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
                        lstTrackList.Visibility = Visibility.Visible;

                        txtSearch.IsEnabled = true;
                        btnResetPlaylist.IsEnabled = true;
                    }
                    else
                    {
                        btnNewPlaylist.Visibility = Visibility.Visible;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
                        txtMainTitle.Content = "Start by creating a new playlist >>";
                        MessageBox.Show("No .mp3 files found in the specified directory", "Invalid directory", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            lstTrackList.SelectionChanged += (s, e) =>
            {
                if ((s as ListView).SelectedItem != null)
                {
                    TrackListItem item = lstTrackList.SelectedItem as TrackListItem;
                    Animation.AnimateGridObjectOpacity(txtMainTitle, 0, TimeSpan.FromMilliseconds(300), () =>
                    {
                        txtCompactTitle.Text = item.title;
                        string initialTitle = string.Empty;
                        if (item.title.Length > 50)
                        {
                            initialTitle = item.title.Remove(49);
                            initialTitle += "...";
                        }
                        else
                        {
                            initialTitle = item.title;
                        }

                        txtMainTitle.Content = initialTitle;

                        FormattedText formattedText = new FormattedText(
                            txtCompactTitle.Text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(txtCompactTitle.FontFamily, txtCompactTitle.FontStyle, txtCompactTitle.FontWeight, txtCompactTitle.FontStretch),
                            txtCompactTitle.FontSize,
                            Brushes.Black,
                            new NumberSubstitution(), 
                            TextFormattingMode.Display
                        );
                        debugText.Content = $"width: {formattedText.Width}";

                        if (!flyoutMenuLeft.IsOpen && !flyoutMenuRight.IsOpen)
                        {
                            Animation.AnimateGridObjectOpacity(txtMainTitle, 1, TimeSpan.FromMilliseconds(300));
                        }
                    });

                    streamPlayHandler.Stop();
                    streamPlayHandler.Load(item.path);

                    _currSource = item.path;
                    _isPlaying = true;

                    playerSlider.IsEnabled = true;
                    volumeSlider.IsEnabled = true;
                    btnPause.IsEnabled = true;
                    btnNext.IsEnabled = true;
                    btnShuffleEnabled.IsEnabled = true;
                    btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
                }
            };

            volumeSlider.ValueChanged += (s, e) =>
            {
                streamPlayHandler.UpdateVolume((float)Math.Round(volumeSlider.Value / 100, 2));
            };

            playerSlider.ValueChanged += (s, e) =>
            {
                if (!_isDragging && playerSlider.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (_totalTime.TotalSeconds > 0)
                    {
                        streamPlayHandler.UpdatePosition(TimeSpan.FromSeconds(playerSlider.Value));
                        txtCurrTime.Text = streamPlayHandler.GetPosition().ToString(@"mm\:ss");
                    }
                }
            };
        }

        private void NextTrack()
        {
            TrackListItem nextTrack = null;
            if (!btnShuffleEnabled.IsChecked.Value)
            {
                TrackListItem currTrack = lstTrackList.SelectedItem as TrackListItem;
                int currIndex = _playlistItems.IndexOf(currTrack);

                if (currIndex == _playlistItems.Count - 1)
                    nextTrack = _playlistItems[0];
                else
                    nextTrack = _playlistItems[currIndex + 1];
            }
            else
            {
                nextTrack = _playlistItems[new Random().Next(0, _playlistItems.Count)];
            }

            streamPlayHandler.Stop();
            streamPlayHandler.Load(nextTrack.path);
            lstTrackList.SelectedItem = nextTrack;

            playerSlider.IsEnabled = true;
            btnPause.IsEnabled = true;
            btnNext.IsEnabled = true;
            btnShuffleEnabled.IsEnabled = true;
            btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
        }

        private void PreviousTrack()
        {
            TrackListItem currTrack = lstTrackList.SelectedItem as TrackListItem;
            TrackListItem nextTrack = null;
            int currIndex = _playlistItems.IndexOf(currTrack);

            if (currIndex == 0)
                nextTrack = _playlistItems[_playlistItems.Count - 1];
            else
                nextTrack = _playlistItems[currIndex - 1];

            streamPlayHandler.Stop();
            streamPlayHandler.Load(nextTrack.path);

            lstTrackList.SelectedItem = nextTrack;
        }

        private void playerThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_totalTime.TotalSeconds > 0)
            {
                streamPlayHandler.UpdatePosition(TimeSpan.FromSeconds(playerSlider.Value));
            }
            _isDragging = false;
        }

        private void playerThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void playerThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            txtCurrTime.Text = TimeSpan.FromSeconds(playerSlider.Value).ToString(@"mm\:ss");
        }

        private void playerThumb_DragDelta_1(object sender, DragDeltaEventArgs e)
        {
            streamPlayHandler.UpdateVolume((float)Math.Round(volumeSlider.Value / 100, 2));
        }

        private void SetFixTitle(object sender, MouseButtonEventArgs e)
        {
            _isTitleFixed = !_isTitleFixed;
            if (_isTitleFixed)
                txtMainTitle.Margin = new Thickness(0,0,0,0);
        }

        private void MediaLoaded(string file)
        {
            _totalTime = streamPlayHandler.GetChannelLength();
            playerSlider.Maximum = _totalTime.TotalSeconds;
            playerSlider.Value = 0;
            txtMaxTime.Text = _totalTime.ToString(@"mm\:ss");

            _timerVideoTime = new DispatcherTimer();
            _timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            _timerVideoTime.Tick += (s, ev) =>
            {
                if (_isPlaying && !_isDragging)
                {
                    playerSlider.Value = streamPlayHandler.GetPosition().TotalSeconds;
                    txtCurrTime.Text = streamPlayHandler.GetPosition().ToString(@"mm\:ss");
                }

                if (streamPlayHandler.GetPosition().TotalSeconds == streamPlayHandler.GetChannelLength().TotalSeconds)
                    this.NextTrack();
            };
            _timerVideoTime.Start();

            _timerBassIntensityTick = new DispatcherTimer();
            _timerBassIntensityTick.Interval = TimeSpan.FromMilliseconds(10);

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    
            streamPlayHandler.Play();

            btnPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PauseCircle;
            _isPlaying = true;
            btnPause.ToolTip = "Pause";

            _timerBassIntensityTick.Tick += async(s, ev) =>
            {
                await Task.Run(() =>
                {
                    float[] buffer = new float[2048];
                    Bass.BASS_ChannelGetData(streamPlayHandler.handle, buffer, (int)BASSData.BASS_DATA_FFT4096);

                    Visuals vis = new Visuals();
                    int freq = Utils.FFTIndex2Frequency(4, 4096, 44100);
                    float freqIncr = vis.DetectFrequency(streamPlayHandler.handle, freq, freq + 17, true) * 6;

                    Dispatcher.Invoke(() =>
                    {
                        //debugText.Content = freqIncr.ToString();
                        bassIntensityBar.Value = freqIncr * 3;
                        Animation.AnimateLabelObjectFontSize(txtMainTitle, 60 + freqIncr, TimeSpan.FromMilliseconds(100));
                    });
                });
            };
            _timerBassIntensityTick.Start();
        }
        #endregion
    }
}