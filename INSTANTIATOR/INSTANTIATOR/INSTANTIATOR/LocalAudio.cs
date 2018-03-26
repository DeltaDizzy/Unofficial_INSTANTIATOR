using UnityEngine;
using System.Collections;
using System.IO;

namespace INSTANTIATOR
{
    public class LocalAudio : MonoBehaviour
    {
        Vessel soundVessel = FlightGlobals.ActiveVessel;
        private string _audioPath;
        public AudioPath { get; set; }
    
        public float minDistance = 1;
        public float maxDistance = FlightGlobals.ActiveVessel.altitude + 10;
        public bool loop = true;
        public float volume = 1;
        
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
       
        
        public void Start()
        {
            LocalAudio = gameObject.AddComponent<AudioSource>() as AudioSource;
            LocalAudio.clip = soundFile;
            LocalAudio.minDistance = minDistance * scale;
            LocalAudio.maxDistance = maxDistance * scale;
            LocalAudio.loop = loop;
            LocalAudio.volume = volume;
            LocalAudio.playOnAwake = true;
            LocalAudio.spatialBlend = 1f;
            LocalAudio.rolloffMode = AudioRolloffMode.Linear;
            if(soundVessel.altitde <= audioRadius)
            {
                LocalAudio.Play();
            }
        }
    }
}
