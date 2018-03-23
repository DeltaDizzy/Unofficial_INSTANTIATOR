using UnityEngine;
using System.Collections;
using System.IO;

namespace INSTANTIATOR
{
    public class LocalAudio
    {
        //Create AudioSource
       public static AudioSource soundSource = new GameObject().AddComponent<AudioSource>() as AudioSource;
        
       //parser 
       public static string name;
       public static string body;
       public static string audioPath;
       public static int audioRadius;
       public LocalAudio(string n, string b, string mp, string ar)
       {
            name = n;
            body = b;
            audioPath = mp;
            audioRadius = int.Parse(ar);
       }
       
        //Set path to music directory
        private string _audiopath;
        public string AudioPath
        {
            get
            {
                
                if (String.IsNullOrEmpty(_audioPath))
                {
                    _audioPath = Directory.GetParent(KSPUtil.ApplicationRootPath).FullName.ToString().Replace("\\", "/") + "/Audio";
                    INSTANTIATOR.Log("Music path is: " + _musicPath);
                }
                return _audioPath;
            }
        }
        
        Directory.CreateDirectory(AudioPath);
        
        IEnumerator LoadFile(string path)
        {
            WWW www = new WWW("file://" + path);
            Debug.Log("Loading audio at: " + path);

            AudioClip clip = www.GetAudioClip(false);
            while (!clip.isReadyToPlay)
                yield return www;

            Debug.Log("Audio Loaded");
            clip.name = Path.GetFileName(path);
            AudioClip databaseClip = GameDatabase.Instance.GetAudioClip(name);

        }

        public static AudioDataLoadState clipReady = new AudioDataLoadState();
        public static CelestialBody targetbody = FlightGlobals.GetBodyByName(body);
        internal void InitSound()
       {
          
          Debug.Log("[INSTANTIATOR]: Initializing LocalSound: " + name + " around body " + body + ".");
            
        
          soundSource.transform.SetParent(targetbody.scaledBody.transform);
          soundSource.transform.localPosition = Vector3.zero;
          soundSource.name = name;
          soundSource.clip = 
          
       }

        
    }
}
