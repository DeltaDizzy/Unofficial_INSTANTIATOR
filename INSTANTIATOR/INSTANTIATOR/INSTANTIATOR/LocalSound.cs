using UnityEngine

namespace INSTANTIATOR
{
    public struct LocalSound
    {
       public string name;
       public string body;
       public string filePath;
       public int outerRadius;
       public int innerRadius;
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
          GameObject soundSource = GameObject.AddComponent<AudioSource>();
        
          soundSource.transform.SetParent(targetbody.scaledBody.transform);
          soundSource.transform.localPosition = Vector3.zero;
          soundSource.name = name; 
           
          if(FlightGlobals.activeVessel.altitude >= innerRadius && FlightGlobals.activeVessel.altitude <= outerRadius)
          {
                
          }
          
       }
    }
}
