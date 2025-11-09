using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace TouristMap.Storage
{   
    /// <summary>
    /// Утилита для сохранения/загрузки чанков в GZip.
    /// </summary>
    public static class GZipHelper
    {
        private static readonly string ChunkFolder = Path.Combine(Application.persistentDataPath, "MapChunks");

        public static string GetChunkPath(Vector2Int coord)
        {
            Directory.CreateDirectory(ChunkFolder);
            return Path.Combine(ChunkFolder, $"chunk_{coord.x}_{coord.y}.json.gz");
        }

        public static void Save(ChunkData data, Vector2Int coord)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            string path = GetChunkPath(coord);
            using var fs = new FileStream(path, FileMode.Create);
            using var gzip = new GZipStream(fs, System.IO.Compression.CompressionLevel.Optimal);
            gzip.Write(bytes, 0, bytes.Length);
        }

        public static ChunkData Load(Vector2Int coord)
        {
            string path = GetChunkPath(coord);
            if (!File.Exists(path)) return new ChunkData();

            try
            {
                using var fs = File.OpenRead(path);
                using var gzip = new GZipStream(fs, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, Encoding.UTF8);
                string json = reader.ReadToEnd();
                return JsonUtility.FromJson<ChunkData>(json) ?? new ChunkData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load chunk {coord}: {e.Message}");
                return new ChunkData();
            }
        }
    }
}
