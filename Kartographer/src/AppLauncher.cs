/*
 * Copyright 2015 SatNet 
 * This file is subject to the included LICENSE.md file. 
 */

using System;
using UnityEngine;

namespace Kartographer
{
	[KSPAddonImproved(KSPAddonImproved.Startup.Flight | KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TrackingStation, false)]
	public class AppLauncher: MonoBehaviour
	{

		const float AUTO_HIDE_TIME = 5.0f;

		static public AppLauncher Instance
		{
			get { return _instance; }
		}
		static private AppLauncher _instance = null;
		private bool 		_active = false;
		private Rect 		_windowPos = new Rect();
		private GUIStyle 	_windowStyle;
		private GUIStyle 	_labelStyle;
		private GUIStyle 	_centeredLabelStyle;
		private GUIStyle	_buttonStyle;
		private bool 		_hasInitStyles 	= false;
		private int 		_winID;
		private float 		_autoHideTime = 0.0f;
		public ApplicationLauncherButton _toolbarButton;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			if (_instance)
				Destroy (_instance);
			_instance = this;
			_winID = GUIUtility.GetControlID (FocusType.Passive);
			GameEvents.onGUIApplicationLauncherReady.Add (OnAppLaunchReady);
			GameEvents.onGameSceneSwitchRequested.Add (OnSceneChange);
			GameEvents.OnMapEntered.Add (Resize);
			GameEvents.OnMapExited.Add (Resize);

			if (ApplicationLauncher.Ready) {
				OnAppLaunchReady ();
			}
		}

		/// <summary>
		/// Called when this object is destroyed.
		/// </summary>
		public void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLaunchReady);
			GameEvents.onGameSceneSwitchRequested.Remove (OnSceneChange);
			GameEvents.OnMapEntered.Remove (Resize);
			GameEvents.OnMapExited.Remove (Resize);
			RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			DestroyButtons ();
			ControlUnlock ();
			if (_instance == this)
				_instance = null;
		}

		/// <summary>
		/// Callback when the scene changes.
		/// </summary>
		/// <param name="evt">Evt.</param>
		public void OnSceneChange(GameEvents.FromToAction<GameScenes,GameScenes> evt)
		{
			Resize ();
		}

		/// <summary>
		/// Callback when the app launcher bar is ready.
		/// </summary>
		public void OnAppLaunchReady()
		{
			_toolbarButton = ApplicationLauncher.Instance.AddModApplication (
					ToggleWindow,
					ToggleWindow,
					noOp,
					noOp,
					noOp,
					noOp,
					ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | 
					ApplicationLauncher.AppScenes.TRACKSTATION,
					(Texture)GameDatabase.Instance.GetTexture ("Kartographer/Textures/sat_map", false)
				);
		}

		/// <summary>
		/// Destroy the buttons.
		/// </summary>
		public void DestroyButtons()
		{
			ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
		}

		/// <summary>
		/// No op.
		/// </summary>
		public void noOp() {}

		/// <summary>
		/// Force a window resize.
		/// </summary>
		public void Resize()
		{
			_windowPos.height = 0.0f;
		}

		/// <summary>
		/// Toggles the window.
		/// </summary>
		public void ToggleWindow()
		{
			if (_active)
				onDeactivate ();
			else
				onActivate ();
		}

		/// <summary>
		/// Display the app launcher.
		/// </summary>
		public void onActivate()
		{
			Debug.Log ("Kartographer App Launch Activate - Pos:"+_windowPos.x +" "+_windowPos.y);
			_windowPos.height = 0.0f;
			_active = true;
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		/// <summary>
		/// Hide the launcher.
		/// </summary>
		public void onDeactivate()
		{
			Debug.Log ("Kartographer App Launch Deactivate");
			_active = false;
			_autoHideTime = 0.0f;
			RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			ControlUnlock ();
		}


		/// <summary>
		/// Lock the Controls.
		/// </summary>
		private void ControlLock()
		{
			InputLockManager.SetControlLock (ControlTypes.EDITOR_LOCK, "Kartographer_Launch");
		}
		/// <summary>
		/// Unlock the Controls.
		/// </summary>
		private void ControlUnlock()
		{
			InputLockManager.RemoveControlLock("Kartographer_Launch");
		}

		public void Update()
		{
			if (KartographSettings.Instance.AutoHide && _autoHideTime != 0.0f && Time.time > _autoHideTime &&
				_active && !_windowPos.Contains (Event.current.mousePosition)) {
				_toolbarButton.enabled = false;
				onDeactivate ();
			}
		}

		/// <summary>
		/// Callback when a draw is requested.
		/// </summary>
		public void OnDraw()
		{
			if (!_hasInitStyles)
				InitStyles ();
			if (_active) {
				_windowPos = GUILayout.Window (_winID, _windowPos, OnWindow, "Kartograher", _windowStyle);
				if ((_windowPos.x == 0.0f && _windowPos.y == 0.0f) || _windowPos.yMax > Screen.height) {
					Vector3 toolPos = Camera.current.WorldToScreenPoint (_toolbarButton.GetAnchor ());
					_windowPos.x = toolPos.x - _windowPos.width*0.5f;
					_windowPos.y = (Screen.height - toolPos.y);
					if (!ApplicationLauncher.Instance.IsPositionedAtTop) {
						_windowPos.y -= _windowPos.height;
					}
				}
				if (_windowPos.xMax + 5.0f > Screen.width) {
					_windowPos.x -= _windowPos.xMax - Screen.width - 5.0f;
				}
			}
			if (_active && _windowPos.Contains (Event.current.mousePosition)) {
				ControlLock ();
			} else {
				ControlUnlock();
			}
		}

		/// <summary>
		/// Draws the window.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void OnWindow(int windowId)
		{
			int buttons = 0;
			GUILayout.BeginVertical (GUILayout.Width(150.0f));
			if (KartographSettings.Instance != null && GUILayout.Button ("Settings",_buttonStyle)) {
				KartographSettings.Instance.ToggleWindow ();
				_autoHideTime = Time.time + AUTO_HIDE_TIME;
			}
	
			if (WarpTo.Instance != null && GUILayout.Button ("Warp To",_buttonStyle)) {
				WarpTo.Instance.ToggleWindow ();
				_autoHideTime = Time.time + AUTO_HIDE_TIME;
			}
			if (FocusSelect.Instance != null && FocusSelect.Instance.IsUsable()) {
				if (GUILayout.Button ("Focus Select", _buttonStyle)) {
					FocusSelect.Instance.ToggleWindow ();
					_autoHideTime = Time.time + AUTO_HIDE_TIME;
				}
			}
			if (VesselSelect.Instance != null && VesselSelect.Instance.IsUsable()) {
				if (GUILayout.Button ("Vessel Select", _buttonStyle)) {
					VesselSelect.Instance.ToggleWindow ();
					_autoHideTime = Time.time + AUTO_HIDE_TIME;
				}
			}
			if (ManeuverEditor.Instance != null && ManeuverEditor.Instance.IsUsable()	) {
				buttons++;
				if (GUILayout.Button ("Maneuver Editor",_buttonStyle)) {
					ManeuverEditor.Instance.ToggleWindow ();
					_autoHideTime = Time.time + AUTO_HIDE_TIME;
				}
			}
			GUILayout.EndVertical ();

		}

		/// <summary>
		/// Initializes the styles.
		/// </summary>
		private void InitStyles()
		{
			_windowStyle = KartographStyle.Instance.Window;
			_labelStyle = KartographStyle.Instance.Label;
			_centeredLabelStyle = KartographStyle.Instance.CenteredLabel;
			_buttonStyle = KartographStyle.Instance.Button;
			_hasInitStyles = true;
		}

	}
}

