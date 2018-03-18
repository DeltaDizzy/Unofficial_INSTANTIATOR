using UnityEngine

namespace INSTANTIATOR
{
    public struct LocalSound
    {
       public string name;
       public string body;
       public string filePath;
       public int outerSoundRadius;
       public int innerSoundRadius;
       public bool loop;
       public LocalSound(string n, string b, string fp, string osr, string isr, string l)
       {
         name = n;
         body = b;
         filePath = fp;
         outerSoundRadius = int.Parse(osr);
         innerSoundRadius = int.Parse(isr);
         loop = bool.Parse(l);
       }
     
       internal void InitSound()
       {
          CelestialBody targetbody = FlightGlobals.GetBodyByName(body);
          Debug.Log(INSTANTIATOR.Log("Initializing LocalSound: " + name + " around body " + body + "."));
          GameObject soundsource = GameObject.AddComponent(AudioSource);
        
          obj.transform.SetParent(targetbody.scaledbody.transform);
          obj.transform.localPosition = Vector3.zero;
          
       }
    }
}
