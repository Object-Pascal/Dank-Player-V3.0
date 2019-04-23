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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.BassWasapi;
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
        private DispatcherTimer _timerVolumeIntensityTick;

        private bool _isPlaying;
        private bool _isDragging;

        private string _currSource;

        private int handle;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMediaEvents();
            InitializeInteractionEvents();
            SetInitialDefaultValues();

            controlGrid.Visibility = Visibility.Visible;
            animationGrid.Visibility = Visibility.Hidden;

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        #region CONTROL
        private void InitializeMediaEvents()
        {
            this.MouseMove += (s, e) =>
            {
                double differenceX = (Mouse.GetPosition(this).X - this.Width / 2);
                double differenceY = (Mouse.GetPosition(this).Y - this.Height / 2);

                //debugText.Content = $"X:{differenceX} Y:{differenceY}";

                txtMainTitle.Margin = new Thickness((differenceX / 10) / 2, (differenceY / 10) / 2, 0, 0);
            };

            mediaBackground.MediaEnded += (s, e) =>
            {
                mediaBackground.Position = TimeSpan.FromMilliseconds(0);
            };

            mainPlayer.MediaEnded += (s, e) =>
            {
                NextTrack();
            };
        }

        private void SetInitialDefaultValues()
        {
            mainPlayer.LoadedBehavior = MediaState.Manual;
            mainPlayer.UnloadedBehavior = MediaState.Manual;

            _playlistItems = new List<TrackListItem>();
            _backgroundItems = new Dictionary<string, string>();
            _totalTime = new TimeSpan();
            _timerVideoTime = new DispatcherTimer();
            _timerVolumeIntensityTick = new DispatcherTimer();

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
                    mainPlayer.Pause();
                    btnPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PlayCircle;
                    _isPlaying = false;
                    btnPause.ToolTip = "Play";
                    Bass.BASS_ChannelPause(handle);
                }
                else
                {
                    mainPlayer.Play();
                    btnPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.PauseCircle;
                    _isPlaying = true;
                    btnPause.ToolTip = "Pause";
                    Bass.BASS_ChannelPlay(handle, false);
                }
            };

            btnPrevious.Click += (s, e) =>
            {
                TrackListItem currTrack = lstTrackList.SelectedItem as TrackListItem;
                TrackListItem nextTrack = null;
                int currIndex = _playlistItems.IndexOf(currTrack);

                if (currIndex == 0)
                    nextTrack = _playlistItems[_playlistItems.Count - 1];
                else
                    nextTrack = _playlistItems[currIndex - 1];

                mainPlayer.Source = new Uri(nextTrack.path);
                lstTrackList.SelectedItem = nextTrack;
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

            btnMenuLeft.MouseUp += async(s, e) =>
            {
                flyoutMenuLeft.IsOpen = true;
                btnMenuLeft.Visibility = Visibility.Hidden;
                Animation.AnimateGridObjectMargin(btnMenuLeft, new Thickness(-20, 0, 0, 0), TimeSpan.FromMilliseconds(150));
                Animation.AnimateGridObjectOpacity(txtMainTitle, 0, TimeSpan.FromMilliseconds(300));
                (s as Grid).Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                txtMainTitle.IsEnabled = false;

                string destination = (System.Reflection.Assembly.GetEntryAssembly().Location).Replace("Dank Player V3.0.exe", "backgrounds");
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

                        lstTrackList.Visibility = Visibility.Visible;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
                        btnNewPlaylist.IsEnabled = false;
                        btnNewPlaylist.Visibility = Visibility.Hidden;

                        volumeSlider.IsEnabled = true;
                        txtSearch.IsEnabled = true;
                        btnShuffleEnabled.IsEnabled = true;
                        btnResetPlaylist.IsEnabled = true;
                    }
                    else
                    {
                        if (_playlistItems.Count <= 0)
                            btnNewPlaylist.Visibility = Visibility.Visible;

                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
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
                        mainPlayer.Stop();
                        mainPlayer.Source = null;
                        _playlistItems.Clear();
                        lstTrackList.ItemsSource = null;

                        txtMaxTime.Text = "0:00";

                        lstTrackList.Visibility = Visibility.Hidden;
                        tracksLoadingIndicator.Visibility = Visibility.Visible;

                        playerSlider.IsEnabled = false;
                        volumeSlider.IsEnabled = false;
                        btnShuffleEnabled.IsEnabled = false;
                        btnPause.IsEnabled = false;
                        btnNext.IsEnabled = false;
                        btnPrevious.IsEnabled = false;
                        btnResetPlaylist.IsEnabled = false;

                        playerSlider.Value = 0;

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

                        btnResetPlaylist.IsEnabled = false;
                        lstTrackList.Visibility = Visibility.Hidden;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
                        lstTrackList.Visibility = Visibility.Visible;

                        volumeSlider.IsEnabled = true;
                        txtSearch.IsEnabled = true;
                        btnShuffleEnabled.IsEnabled = true;
                        btnResetPlaylist.IsEnabled = true;
                    }
                    else
                    {
                        btnNewPlaylist.Visibility = Visibility.Visible;
                        tracksLoadingIndicator.Visibility = Visibility.Hidden;
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
                        txtCompactTitle.Text = initialTitle;

                        if (!flyoutMenuLeft.IsOpen && !flyoutMenuRight.IsOpen)
                        {
                            Animation.AnimateGridObjectOpacity(txtMainTitle, 1, TimeSpan.FromMilliseconds(300));
                        }
                    });

                    mainPlayer.Source = new Uri(item.path);
                    _currSource = item.path;
                    mainPlayer.Play();
                    _isPlaying = true;

                    playerSlider.IsEnabled = true;
                    btnPause.IsEnabled = true;
                    btnNext.IsEnabled = true;
                    btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
                }
            };

            volumeSlider.ValueChanged += (s, e) =>
            {
                mainPlayer.Volume = volumeSlider.Value / 100;
            };

            playerSlider.ValueChanged += (s, e) =>
            {
                if (!_isDragging)
                {
                    if (_totalTime.TotalSeconds > 0)
                    {
                        mainPlayer.Position = TimeSpan.FromSeconds(playerSlider.Value);
                        txtCurrTime.Text = TimeSpan.FromSeconds(mainPlayer.Position.TotalSeconds).ToString(@"mm\:ss");
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

            mainPlayer.Source = new Uri(nextTrack.path);
            lstTrackList.SelectedItem = nextTrack;

            Bass.BASS_ChannelStop(handle);
            Bass.BASS_ChannelPlay(handle, false);

            playerSlider.IsEnabled = true;
            btnPause.IsEnabled = true;
            btnNext.IsEnabled = true;
            btnPrevious.IsEnabled = !btnShuffleEnabled.IsChecked.Value;
        }

        private void playerThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (_totalTime.TotalSeconds > 0)
            {
                mainPlayer.Position = TimeSpan.FromSeconds(playerSlider.Value);
            }
            _isDragging = false;
        }

        private void playerThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void playerThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            txtCurrTime.Text = TimeSpan.FromSeconds(playerSlider.Value).ToString(@"mm\:ss");
        }

        private void playerThumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            mainPlayer.Volume = volumeSlider.Value / 100;
        }

        private void mainPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            _totalTime = mainPlayer.NaturalDuration.TimeSpan;
            playerSlider.Maximum = mainPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            playerSlider.Value = 0;
            txtMaxTime.Text = _totalTime.ToString(@"m\:ss");

            _timerVideoTime = new DispatcherTimer();
            _timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            _timerVideoTime.Tick += (s, ev) =>
            {
                if (_isPlaying && !_isDragging)
                {
                    playerSlider.Value = mainPlayer.Position.TotalSeconds;
                    txtCurrTime.Text = TimeSpan.FromSeconds(mainPlayer.Position.TotalSeconds).ToString(@"mm\:ss");
                }
            }; 

            _timerVideoTime.Start();

            _timerVolumeIntensityTick = new DispatcherTimer();
            _timerVolumeIntensityTick.Interval = TimeSpan.FromMilliseconds(10);

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            mainPlayer.Stop();
    
            handle = Bass.BASS_StreamCreateFile(_currSource, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            debugText.Visibility = Visibility.Visible;

            _timerVolumeIntensityTick.Tick += async(s, ev) =>
            {
                await Task.Run(() =>
                {
                    volumeIntensityBar.Dispatcher.Invoke(() =>
                    {
                        //double value = Math.Round((device.AudioMeterInformation.MasterPeakValue * 100), 1);
                        //volumeIntensityBar.Value = value;

                        float[] buffer = new float[2048];
                        Bass.BASS_ChannelGetData(handle, buffer, (int)BASSData.BASS_DATA_FFT4096); 

                        int hz = Utils.FFTIndex2Frequency(4, 4096, 44100);
                        Visuals vis = new Visuals();
                        float t = vis.DetectFrequency(handle, hz, hz + 18, true);

                        debugText.Content = t.ToString();
                        volumeIntensityBar.Value = t * 15;
                        Animation.AnimateLabelObjectFontSize(txtMainTitle, 60 + (t * 10), TimeSpan.FromMilliseconds(100));
                    });
                });
            };
            _timerVolumeIntensityTick.Start();
        }
        #endregion

        #region ANIMATION
        private void ShowAnimationPane(object sender, MouseButtonEventArgs args)
        {
            //mediaBackground.Source = null;

            Animation.AnimateGridObjectOpacity(controlGrid, 1, 0, TimeSpan.FromMilliseconds(500));
            controlGrid.Visibility = Visibility.Hidden;

            animationGrid.Visibility = Visibility.Visible;
            Animation.AnimateGridObjectOpacity(animationGrid, 0, 1, TimeSpan.FromMilliseconds(500));

            List<Ellipse> particles = new List<Ellipse>();
            int maxBoundsX = 100;
            int maxBoundsY = 100;

            Random rnd = new Random();

            DispatcherTimer timerUpdate = new DispatcherTimer();
            timerUpdate.Interval = TimeSpan.FromMilliseconds(300);
            timerUpdate.Start();
            timerUpdate.Tick += (s, e) =>
            {
                Ellipse particle = new Ellipse();
                particle.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255)));

                int size = rnd.Next(10, 100);
                particle.Width = size;
                particle.Height = size;

                int left = rnd.Next(0, (int)this.Width);
                int top = rnd.Next(0, (int)this.Height);                     

                particle.Margin = new Thickness(left, top, 0, 0);
                particle.Opacity = 0;
                animationPane.Children.Add(particle);

                Animation.AnimateGridObjectOpacity(particle, 0, 1, TimeSpan.FromMilliseconds(1500), true);
                Animation.AnimateGridObjectMargin(particle, new Thickness(particle.Margin.Left + rnd.Next(-75, 75), particle.Margin.Top + rnd.Next(-75, 75), 0, 0), TimeSpan.FromMilliseconds(2500));
            };

            this.KeyUp += (s, e) =>
            {
                timerUpdate.Stop();
                HideAnimationPane(s, e);
            };
        }

        private void HideAnimationPane(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape && controlGrid.Visibility != Visibility.Visible)
            {
                animationPane.Children.Clear();

                Animation.AnimateGridObjectOpacity(animationGrid, 1, 0, TimeSpan.FromMilliseconds(500));
                animationGrid.Visibility = Visibility.Hidden;

                controlGrid.Visibility = Visibility.Visible;
                Animation.AnimateGridObjectOpacity(controlGrid, 0, 1, TimeSpan.FromMilliseconds(500));
            }
        }
        #endregion
    }
}