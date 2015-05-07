using JsonExSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StanVre
{
#region Interface----------------------------------------------------\\

    public interface IResponseListener
    {
        void getSpeech(GoogleResponse deserialized);
        void audioLevel(int al);
    }

#endregion _____________________________________//

    /// <summary>
    ///  class Voice
    ///  assemble record voice from micro and recognize with GoogleSpeechApi
    /// </summary>
    public class Voice : IMicroListener, IRecognitionListener, IResponseListener
    {

#region FIELDS------------------------------------------------------\\

        private Micro micro;
        private VoiceRecognition voice;
        private IResponseListener responseListener;

#endregion _____________________________________//

#region ResponseListener-------------------------------------------\\

        public virtual void getSpeech(GoogleResponse deserialized)
        {
        }
        public virtual void audioLevel(int al)
        {
        }

#endregion _____________________________________//

#region MicroListener-----------------------------------------------\\

        public virtual void getAudioLevel(int al)
        {
            responseListener.audioLevel(al);
        }
        public virtual void toRecognize(byte[] data)
        {
            voice.recognize(data);
        }

#endregion _____________________________________//

#region RecognitionListener ________________________
//                                                                                        \\

        public virtual void getResponse(StreamReader SR_Response)
        {
            string json = SR_Response.ReadToEnd();
            json = json.Split('\n')[1];
            if (json.Length == 0)
            {
                return;
            }
            Serializer serializer = new Serializer(typeof(GoogleResponse));
            GoogleResponse deserialized;
            deserialized = (GoogleResponse)serializer.Deserialize(json);
            responseListener.getSpeech(deserialized);
        }

#endregion _____________________________________//

        public Voice()
        {
        }
        public void initDefault(IResponseListener resl)
        {
            settingMicro(this, false, 6, 500);
            settingVoiceRecognition(this, micro.getSettings().SAMPLE_RATE, micro.getSettings().CHANELS, micro.getSettings().PRECISION, false);
            setResponseListener(resl);
        }
        public void settingMicro(IMicroListener ml, bool b, int tr, int tv)
        {
            micro = new Micro(ml,b,tr,tv);
        }
        public void settingVoiceRecognition(IRecognitionListener rl, int rate, int cha, int prec, bool b)
        {
            voice = new VoiceRecognition(rate, cha, prec, b);
            voice.setRecognitionListener(rl);
        }
        public void setResponseListener(IResponseListener rl)
        {
            responseListener = rl;
        }
        public void startRecognize()
        {
            micro.StarTrek();
        }
        public void stopRecognize()
        {
            micro.StopRecording();
        }
    }
}