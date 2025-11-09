using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TouristMap.Services
{
    public interface IImageLoader
    {
        IEnumerator LoadImage(string path, Image targetImage, Sprite fallback = null);
    }
}

