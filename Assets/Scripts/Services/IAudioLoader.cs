using System.Collections;
using UnityEngine;

namespace TouristMap.Services
{
    public interface IAudioLoader
    {
        IEnumerator LoadAudio(string path, AudioSource targetAudioSource);
    }
}

