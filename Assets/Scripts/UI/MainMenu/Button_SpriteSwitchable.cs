using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace septim.ui
{
    public class Button_SpriteSwitchable
    {

        public Image images;
        public Sprite[] sprites;
        public void OnTriggerButton(int i)
        {
            if(sprites == null || sprites.Length <= 0 || i < 0 || i > sprites.Length)
            {
                return;
            }
            images.sprite = sprites[i];
        }
    }
}

