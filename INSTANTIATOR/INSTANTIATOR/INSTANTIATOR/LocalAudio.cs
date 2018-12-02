using UnityEngine;
using System.Collections;
using System.IO;

namespace INSTANTIATOR
{
    public class LocalAudio : MonoBehaviour
    {
        public static AudioSource soundSource = new GameObject().AddComponent<AudioSource>() as AudioSource;

        Vessel soundVessel = FlightGlobals.ActiveVessel;
        private static string _audioPath;
        public static string AudioPath { get; set; }
    
        public static double minDistance = 1;
        public static double maxDistance = FlightGlobals.ActiveVessel.altitude + 10;
        public static bool loop = true;
        public static float volume = 1;
        
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
       
        
        public void InitandPlay()
        {
            AudioClip audioClip = GameDatabase.Instance.GetAudioClip(name);

            soundSource.clip = audioClip;
            soundSource.playOnAwake = true;
            soundSource.spatialBlend = 1f;
            soundSource.rolloffMode = AudioRolloffMode.Linear;
            if(soundVessel.altitude <= audioRadius)
            {
                soundSource.Play();
            }
        }
    }
}
