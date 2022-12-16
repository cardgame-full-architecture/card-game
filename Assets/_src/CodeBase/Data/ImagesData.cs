using System.Collections.Generic;
using UnityEngine;

namespace _src.CodeBase.Data
{
    [CreateAssetMenu(fileName = "ImagesDataConfig", menuName = "Configs/ImagesDataConfig")]
    public class ImagesData : ScriptableObject
    {
        [SerializeField]
        private List<Sprite> _spritesImages;


        public List<Sprite> SpritesImages => _spritesImages;
    }
}