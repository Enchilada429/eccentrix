using UnityEngine;
using UnityEditor;

public class FatTetoChecker : MonoBehaviour
{
// only check during Editor because it uses the UnityEditor package which will break the build if tried to package while building
#if UNITY_EDITOR
    void Start()
    {
        // check if the fat teto png exists and if not then quit the game (wont work in editor)
        if (!AssetDatabase.AssetPathExists("Assets/Textures/fat_teto.png"))
        {
            Debug.Log("<color=red>ERROR: MISSING FAT TETO PNG IN TEXTURES FOLDER</color>");
            Application.Quit();
        }
        else
        {
            Debug.Log("<color=green>FAT TETO PNG CONFIRMED, WELCOME</color>");
        }
    }
#endif
}
