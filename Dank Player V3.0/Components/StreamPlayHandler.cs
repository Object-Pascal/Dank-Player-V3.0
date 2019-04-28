using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace Dank_Player_V3._0
{
    public class StreamPlayHandler
    {
        public int handle { get; private set; }

        public event EventHandler<MixerLoadEventArgs> MediaLoaded;

        public void Init(int device, int freq, BASSInit flags, IntPtr win) 
            => Bass.BASS_Init(device, freq, flags, win);

        public void Init(int device, int freq, BASSInit flags, IntPtr win, Guid clsid) 
            => Bass.BASS_Init(device, freq, flags, win, clsid);

        public void Play()
        {
            Bass.BASS_ChannelPlay(handle, false);
        }

        public void Load(string file)
        {
            handle = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            MediaLoaded?.Invoke(this, new MixerLoadEventArgs()
            {
                currFile = file
            });
        }

        public void Pause()
        {
            Bass.BASS_ChannelPause(handle);
        }

        public void Stop()
        {
            Bass.BASS_ChannelStop(handle);
        }

        public void UpdatePosition(TimeSpan time)
        {
            Bass.BASS_ChannelSetPosition(handle, Bass.BASS_ChannelSeconds2Bytes(handle, time.TotalSeconds));
        }

        public TimeSpan GetPosition()
        {
            return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(handle, Bass.BASS_ChannelGetPosition(handle)));
        }

        public TimeSpan GetChannelLength()
        {
            return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(handle, Bass.BASS_ChannelGetLength(handle)));
        }
    }

    public class MixerEndEventArgs : EventArgs
    {
        public int currHandle { get; set; }
        public int currChannel { get; set; }
        public int currData { get; set; }
        public IntPtr currUser { get; set; }
    }

    public class MixerLoadEventArgs : EventArgs
    {
        public string currFile { get; set; }
    }
}
