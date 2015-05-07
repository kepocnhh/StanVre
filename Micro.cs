using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace StanVre
{

#region Interface----------------------------------------------------\\

    public interface IMicroListener
    {
        void getAudioLevel(int al);
        void toRecognize(byte[] data);
    }

#endregion _____________________________________//

    public class Micro : IMicroListener
    {
        public struct MicroSettings
        {
            public int ONE_BUFFER_SIZE;
            public int CHANELS;
            public int PRECISION;
            public int SAMPLE_RATE;
            public MicroSettings(int one, int chanels, int prec, int rate)
            {
                ONE_BUFFER_SIZE = one;
                CHANELS = chanels;
                PRECISION = prec;
                SAMPLE_RATE = rate;
            }
        }

#region FIELDS------------------------------------------------------\\

        //private int audiolevel = 0;
        private WaveInEvent recorder;
        private List<byte[]> buffer;
        private int TALK_VOLUME = 500;
        private int TALK_RANG = 6;
        private MicroSettings microWav = new MicroSettings(3200, 1, 16, 16000);
        private MicroSettings microFlac = new MicroSettings(35280, 2, 32, 44100);
        private MicroSettings settings;
        private bool toFlac;

        //private int ONE_BUFFER_SIZE = 3200;
        //public int CHANELS = 1;
        //public int PRECISION = 16;
        //public int SAMPLE_RATE = 16000;

        //private int ONE_BUFFER_SIZE = 35280;
        //public int CHANELS = 2;
        //public int PRECISION = 32;
        //public int SAMPLE_RATE = 44100;

        private IMicroListener microListener;
        
#endregion _____________________________________//


#region MicroListener-----------------------------------------------\\

        public virtual void getAudioLevel(int al)
        {
        }
        public virtual void toRecognize(byte[] data)
        {
        }

#endregion _____________________________________//


        public Micro()
        {
            setMicroListener(this);
            setMicroSettings(false);
        }
        public Micro(IMicroListener ml, bool b, int tr, int tv)
        {
            setMicroListener(ml);
            setMicroSettings(b);
            setTalkRang(tr);
            setTalkVolume(tv);
        }
        public void setMicroListener(IMicroListener ml)
        {
            microListener = ml;
        }
        public void setTalkRang(int tr)
        {
            TALK_RANG = tr;
        }
        public void setTalkVolume(int tv)
        {
            TALK_VOLUME = tv;
        }
        public void setMicroSettings(bool b)
        {
            toFlac = b;
            if(b)
            {
                settings = microFlac;
            }
            else
            {
                settings = microWav;
            }
        }
        public MicroSettings getSettings()
        {
            return settings;
        }
        public bool recordToFlac()
        {
            return toFlac;
        }
        public void setMicroSettings(int one, int chanels, int prec, int rate)
        {
            settings = new MicroSettings(one, chanels, prec, rate);
        }

        private int GetAudioLevel(byte[] buf)
        {
            int lvl = 0;
            for (int index = 0; index < buf.Length; index += 2)
            {
                short sample = (short)((buf[index + 1] << 8) | buf[index]);
                lvl += (int)Math.Abs(sample / 2768f);
            }
            return lvl;
        }
        private int GetRecordLevel()
        {
            return buffer.Count;
        }
        public void StarTrek()
        {
            recorder = new WaveInEvent();
            recorder.WaveFormat = new WaveFormat(settings.SAMPLE_RATE, settings.PRECISION, settings.CHANELS);
            recorder.DataAvailable += RecorderOnDataAvailable;
            recorder.StartRecording();
            buffer = new List<byte[]>();
        }
        private void RecorderOnDataAvailable(object sender, WaveInEventArgs w)
        {
            int audiolevel = GetAudioLevel(w.Buffer);
                microListener.getAudioLevel(audiolevel);
            if (audiolevel > TALK_VOLUME)
            {
                var buf = new byte[settings.ONE_BUFFER_SIZE];
                w.Buffer.CopyTo(buf, 0);
                buffer.Add(buf);
                byte[] audiobuffer = prepareVoice();
                if (checkReady())
                {
                    microListener.toRecognize(audiobuffer);
                }
            }
            else
            {
                if (GetRecordLevel() > 0)
                {
                    byte[] audiobuffer = prepareVoice();
                    microListener.toRecognize(audiobuffer);
                    buffer.Clear();
                }
            }
        }
        private bool checkReady()
        {
            return (GetRecordLevel() / TALK_RANG) * TALK_RANG == GetRecordLevel();
        }
        private byte[] prepareVoice()
        {
            byte[] audiobuffer = new byte[settings.ONE_BUFFER_SIZE * GetRecordLevel()];
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[i].CopyTo(audiobuffer, i * settings.ONE_BUFFER_SIZE);
            }
            return audiobuffer;
        }
        public void StopRecording()
        {
            recorder.StopRecording();
            buffer = null;
        }

    }
}