using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Winch.Core;

namespace DredgeRGBPots
{
	public class DredgeRGBPots : MonoBehaviour
	{
		public void Awake()
		{
			WinchCore.Log.Debug($"{nameof(DredgeRGBPots)} has loaded!");

			PlacedHarvestPOIPatch.Init();
			new Harmony("nogarcia.RGBPots").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
