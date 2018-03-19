using UnityEngine;
using System.Collections;

namespace INSTANTIATOR
{
    public class LocalAudio
    {
        public static AudioSource soundSource = new GameObject().AddComponent<AudioSource>() as AudioSource;

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

        public static AudioDataLoadState clipReady = new AudioDataLoadState();
        public static CelestialBody targetbody = FlightGlobals.GetBodyByName(body);
        internal void InitSound()
       {
          
          Debug.Log("[INSTANTIATOR]: Initializing LocalSound: " + name + " around body " + body + ".");
            
            WWW getClip = new WWW(audioPath);
        
          soundSource.transform.SetParent(targetbody.scaledBody.transform);
          soundSource.transform.localPosition = Vector3.zero;
          soundSource.name = name;
          soundSource.clip = getClip.audioClip;
          
       }

        
    }
}
