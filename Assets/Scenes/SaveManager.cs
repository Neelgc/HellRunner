using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string saveFolder = Application.persistentDataPath + "/Saves/";

    /// <summary>
    /// Sauvegarde un objet sérialisable dans un fichier JSON.
    /// </summary>
    public static void Save<T>(T data, string fileName)
    {
        // Crée le dossier s’il n’existe pas
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string json = JsonUtility.ToJson(data, true);
        string path = saveFolder + fileName + ".json";

        File.WriteAllText(path, json);
        Debug.Log($"Sauvegarde réussie : {path}");
    }

    /// <summary>
    /// Charge un objet depuis un fichier JSON.
    /// </summary>
    public static T Load<T>(string fileName)
    {
        string path = saveFolder + fileName + ".json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);
            Debug.Log($"Chargement réussi : {path}");
            return data;
        }
        else
        {
            Debug.LogWarning($"Fichier de sauvegarde introuvable : {path}");
            return default;
        }
    }

    /// <summary>
    /// Supprime un fichier de sauvegarde spécifique.
    /// </summary>
    public static void Delete(string fileName)
    {
        string path = saveFolder + fileName + ".json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Fichier supprimé : {path}");
        }
    }

    /// <summary>
    /// Supprime toutes les sauvegardes.
    /// </summary>
    public static void DeleteAll()
    {
        if (Directory.Exists(saveFolder))
        {
            Directory.Delete(saveFolder, true);
            Debug.Log("Toutes les sauvegardes ont été supprimées !");
        }
    }


    public class PlayerData
    {
        public int score;
    }
}
