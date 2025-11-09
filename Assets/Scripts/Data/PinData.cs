using UnityEngine;
using System;

namespace TouristMap.Data
{
    [Serializable]
    public class PinData
    {
        public string id; // Уникальный идентификатор пина
        public string name; // Название места
        public Vector2 position; // Позиция на карте
        public string imagePath; // Путь к изображению (опционально)
        public string description; // Текст-описание
        public string audioPath;  // Путь к аудио (опционально)

        public PinData()
        {
            id = Guid.NewGuid().ToString();
            name = "";
            description = "";
        }

        /// <summary>
        /// Создаёт глубокую копию пина (для безопасного редактирования).
        /// </summary>
        public PinData Clone()
        {
            return new PinData
            {
                id = this.id,
                name = this.name,
                position = this.position,
                imagePath = this.imagePath,
                description = this.description,
                audioPath = this.audioPath
            };
        }
    }
}
