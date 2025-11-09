using UnityEngine;
using System;
using TouristMap.Data;

namespace TouristMap.Storage
{
    [Serializable]
    public class ChunkData
    {
        // Массив пинов в этом чанке (по умолчанию пустой)
        public PinData[] pins = new PinData[0];
    }
}
