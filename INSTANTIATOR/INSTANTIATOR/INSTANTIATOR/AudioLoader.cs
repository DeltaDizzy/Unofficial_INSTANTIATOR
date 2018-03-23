using UnityEngine;

namespace INSTANTIATOR
{
  public class AudioLoader : MonoBehaviour
  {
    private string _audioPath;
    public AudioPath { get; set; }
    
    public float minDistance = 1;
    public float maxDistance = FlightGlobals.ActiveVessel.altitude + 10;
    public bool loop = true;
    public float volume = 1;
  }
}
