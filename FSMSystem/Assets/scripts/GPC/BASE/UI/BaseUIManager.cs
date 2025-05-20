using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Harris.GPC
{

	public class UIEventTypes : EnumBase
    {
        public static readonly UIEventTypes Button1Click = new UIEventTypes("BUTTON1CLICK");
        public static readonly UIEventTypes Button2Click = new UIEventTypes("BUTTON2CLICK");

        private UIEventTypes (string value)
            : base(value)
        {
        }
    }
	
	public class BaseUIManager : MonoBehaviour
	{
		public GameObject[] UICanvas;

		public void ShowUI(string GOName)
		{
			for (int i = 0; i < UICanvas.Length; i++)
			{

			}
		}
	}
}