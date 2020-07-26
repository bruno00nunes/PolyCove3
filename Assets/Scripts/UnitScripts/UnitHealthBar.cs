using UnityEngine;
using UnityEngine.UI;

namespace UnitScripts
{
     [RequireComponent(typeof(Slider))]
     public class UnitHealthBar : MonoBehaviour
     {
          private Slider _slider;
          public Gradient gradient;
          public Image fill;

          public void Awake()
          {
               _slider = GetComponent<Slider>();
          }

          public void SetMaxHealth(float health)
          {
               _slider.maxValue = health;
               _slider.value = health;
               fill.color = gradient.Evaluate(1f);
          }

          public void SetHealth(float health)
          {
               _slider.value = health;
               fill.color = gradient.Evaluate(_slider.normalizedValue);
          }
     }
}
