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
        
        public static AudioClip GetAudioClip(string name)
        {
            // Ensure the Music directory exists.
            Directory.CreateDirectory(MusicPath);

            foreach (string file in Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories))
            {
                if (name.Equals(Path.GetFileNameWithoutExtension(file)))
                {
                    string ext = Path.GetExtension(file);
                    INSTANTIATOR.Log("Found " + name + ", with extension " + ext);
                    switch (ext.ToUpperInvariant())
                    {
                        case ".WAV":
                        case ".OGG":
                            return LoadUnityAudioClip(file);
                        default:
                            INSTANTIATOR.Log("What kind of file are you even trying to play? What's a " + ext + "file?");
                            break;
                    }
                }
            }
            //LEts load the GameDtabase instead
             AudioClip databaseClip = GameDatabase.Instance.GetAudioClip(name);
        }   
    }
}
