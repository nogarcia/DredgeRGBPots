using UnityEngine;

namespace DredgeRGBPots
{
	public class Loader
	{
		/// <summary>
		/// This method is run by Winch to initialize your mod
		/// </summary>
		public static void Initialize()
		{
			var gameObject = new GameObject(nameof(DredgeRGBPots));
			gameObject.AddComponent<DredgeRGBPots>();
			GameObject.DontDestroyOnLoad(gameObject);
		}
	}
}