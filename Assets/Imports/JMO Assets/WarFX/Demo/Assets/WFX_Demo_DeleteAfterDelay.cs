using UnityEngine;

namespace Imports.JMO_Assets.WarFX.Demo.Assets
{
	public class WFX_Demo_DeleteAfterDelay : MonoBehaviour
	{
		public float delay = 1.0f;
	
		void Update ()
		{
			delay -= Time.deltaTime;
			if(delay < 0f)
				GameObject.Destroy(this.gameObject);
		}
	}
}
