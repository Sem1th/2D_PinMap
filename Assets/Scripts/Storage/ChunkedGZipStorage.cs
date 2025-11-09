using System.Collections.Generic;
using UnityEngine;
using TouristMap.Data;

namespace TouristMap.Storage
{
    public class ChunkedGZipStorage
    {
        public int ChunkSize { get; set; } = 500;

        public Vector2Int GetChunkCoord(Vector2 pos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / ChunkSize),
                Mathf.FloorToInt(pos.y / ChunkSize)
            );
        }

        public void SavePin(PinData pin)
        {
            var coord = GetChunkCoord(pin.position);
            var chunk = GZipHelper.Load(coord);
            var list = new List<PinData>(chunk.pins);
            list.RemoveAll(p => p.id == pin.id);
            list.Add(pin);
            GZipHelper.Save(new ChunkData { pins = list.ToArray() }, coord);
        }

        public void DeletePin(PinData pin)
        {
            var coord = GetChunkCoord(pin.position);
            var chunk = GZipHelper.Load(coord);
            var list = new List<PinData>(chunk.pins);
            list.RemoveAll(p => p.id == pin.id);
            GZipHelper.Save(new ChunkData { pins = list.ToArray() }, coord);
        }

        public PinData[] GetPinsInBounds(Vector2 min, Vector2 max)
        {
            var result = new List<PinData>();
            var minC = GetChunkCoord(min);
            var maxC = GetChunkCoord(max);

            for (int x = minC.x; x <= maxC.x; x++)
            for (int y = minC.y; y <= maxC.y; y++)
            {
                var coord = new Vector2Int(x, y);
                var chunk = GZipHelper.Load(coord);
                foreach (var pin in chunk.pins)
                {
                    if (pin.position.x >= min.x && pin.position.x <= max.x &&
                        pin.position.y >= min.y && pin.position.y <= max.y)
                    {
                        result.Add(pin);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
