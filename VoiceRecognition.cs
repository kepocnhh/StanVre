using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;
using System.IO;
using System.Threading;
using System.Net;

namespace StanVre
{

#region Interface----------------------------------------------------\\

    public interface IRecognitionListener
    {
        void getResponse(StreamReader SR_Response);
    }
    
#endregion _____________________________________//


    public class VoiceRecognition : IRecognitionListener
    {

#region FIELDS------------------------------------------------------\\

        object lockObj = new object();
        private IRecognitionListener recognitionListener;
        int RATE;
        int CHANELS;
        int SAMPLE_ENCODING;
        string CONTENT_TYPE;
        const string URL =
            //"http://www.google.com/speech-api/v1/recognize?xjerr=1&client=chromium&lang=de-DE&maxresults=1&pfilter=0";
            //"https://www.google.com/speech-api/v1/recognize?xjerr=1&client=chromium&lang=en-US";
            "https://www.google.com/speech-api/v2/recognize?output=json&lang=ru-RU&key=AIzaSyBOti4mM-6x9WDnZIjIeyEU21OpBXqWBgw";
        private const string CONTENT_TYPE_FLAC = "audio/x-flac";
        private const string CONTENT_TYPE_WAV = "audio/l16";

#endregion _____________________________________//


#region RecognitionListener ________________________
//                                                                                        \\

        public virtual void getResponse(StreamReader SR_Response)
        {
        }

//____________________________________________//
#endregion

        //public VoiceRecognition(int r, int c, int s, string ct)
        //{
        //    RATE = r;
        //    CHANELS = c;
        //    SAMPLE_ENCODING = s;
        //    CONTENT_TYPE = ct;
        //    setRecognitionListener(this);
        //}
        public VoiceRecognition(int r, int c, int s, bool ct)
        {
            RATE = r;
            CHANELS = c;
            SAMPLE_ENCODING = s;
            if(ct)
            {
                CONTENT_TYPE = CONTENT_TYPE_FLAC;
            }
            else
            {
                CONTENT_TYPE = CONTENT_TYPE_WAV;
            }
            setRecognitionListener(this);
        }
        public void setRecognitionListener(IRecognitionListener rl)
        {
            recognitionListener = rl;
        }
        private byte[] Wav2FlacBuffConverter(byte[] Buffer)
        {
            Stream OutWavStream = new MemoryStream();
            Stream OutFlacStream = new MemoryStream();
            AudioPCMConfig pcmconf = new AudioPCMConfig(SAMPLE_ENCODING, CHANELS, RATE);
            WAVWriter wr = new WAVWriter(null, OutWavStream, pcmconf);
            wr.Write(new AudioBuffer(pcmconf, Buffer, Buffer.Length / 2));
            OutWavStream.Seek(0, SeekOrigin.Begin);
            WAVReader audioSource = new WAVReader(null, OutWavStream);
            if (audioSource.PCM.SampleRate != RATE)
            {
                return null;
            }
            AudioBuffer buff = new AudioBuffer(audioSource, 0x10000);
            FlakeWriter flakeWriter = new FlakeWriter(null, OutFlacStream, audioSource.PCM);
            flakeWriter.CompressionLevel = 8;
            while (audioSource.Read(buff, -1) != 0)
            {
                flakeWriter.Write(buff);
            }
            OutFlacStream.Seek(0, SeekOrigin.Begin);
            byte[] barr = new byte[OutFlacStream.Length];
            OutFlacStream.Read(barr, 0, (int)OutFlacStream.Length);
            return barr;
        }
        public void recognize(byte[] data)
        {
            if (CONTENT_TYPE.Equals(CONTENT_TYPE_FLAC))
            {
                data = Wav2FlacBuffConverter(data);
            }
            new Thread(postrecognize).Start(data);
        }
        private void postrecognize(object obj)
        {
            byte[] datain = (byte[])obj;
            lock (lockObj)
            {
                HttpWebRequest _HWR_SpeechToText = null;
                _HWR_SpeechToText = (HttpWebRequest)WebRequest.Create(URL);
                _HWR_SpeechToText.Method = "POST";
                _HWR_SpeechToText.ContentType = CONTENT_TYPE + "; rate=" + RATE;
                _HWR_SpeechToText.ContentLength = datain.Length;
                using (Stream stream = _HWR_SpeechToText.GetRequestStream())
                {
                    stream.Write(datain, 0, datain.Length);
                    stream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)_HWR_SpeechToText.GetResponse();
                Stream s = response.GetResponseStream();
                recognitionListener.getResponse(new StreamReader(s));
            }
        }

    }
}