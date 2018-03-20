using UnityEngine;

namespace INSTANTIATOR
{
  public class MusicUtils : MonoBehaviour
  {
    private static string _musicPath;
        public static string MusicPath
        {
            get
            {
                if (String.IsNullOrEmpty(_musicPath))
                {
                    _musicPath = Directory.GetParent(KSPUtil.ApplicationRootPath).FullName.ToString().Replace("\\", "/") + "/Music";
                    INSTANTIATOR.Log("Music path is: " + _musicPath);
                }
                return _musicPath;
            }
        }
  }
}
