using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _src.CodeBase.UI
{
    public class VariantButton : MonoBehaviour
    {
        [SerializeField] 
        private Button _button;


        [SerializeField] 
        private TextMeshProUGUI _variantText;


        public Button Button => _button;

        public void SetText(string text) => 
            _variantText.text = text;

        public void SetSelectedColor() =>
            _button.image.color = Color.green;

        public void SetUnselectable() => 
            _button.interactable = false;
    }
}