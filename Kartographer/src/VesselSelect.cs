/*
 * Copyright 2015 SatNet 
 * This file is subject to the included LICENSE.md file. 
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace Kartographer
{
	[KSPAddon(KSPAddon.Startup.Flight,false)]
	public class VesselSelect: MonoBehaviour
	{
		static private VesselSelect _instance;
		static public VesselSelect Instance
		{
			get { return _instance; }
		}
		private const double 	SAMPLE_TIME = 1.0d;

		private Rect 		_windowPos = new Rect();
		private Vector2		_scrollPos = new Vector2();
		private GUIStyle 	_windowStyle;
		private GUIStyle 	_labelStyle;
		private GUIStyle 	_centeredLabelStyle;
		private GUIStyle	_buttonStyle;
		private GUIStyle	_scrollStyle;
		private GUIStyle	_toggleStyle;
		private int 		_winID;
		private bool 		_hasInitStyles 	= false;
		private bool		_active = false;
		private double 		_prevTime 		= 0.0d;
		private bool 		_asteroids = false;
		private bool 		_debris = false;
		private Vessel 		_krakenSacrifice = null;
		private int 		_krakenCountDown = 0;
		private bool _krakenWarn = false;
		private SortedList<double,Vessel> _vessels = new SortedList<double,Vessel>();

		public void Start()
		{
			if (_instance)
				Destroy (_instance);
			_instance = this;
			_winID = GUIUtility.GetControlID (FocusType.Passive);
			Debug.Log ("Vessel Select Start");
			PluginConfiguration config = PluginConfiguration.CreateForType<KartographSettings> ();
			config.load ();
			_windowPos = config.GetValue<Rect> ("VesselWindowPos",new Rect());

			_windowPos.width = 0.0f;
			_windowPos.height = 0.0f;

			_krakenWarn = config.GetValue<bool> ("KrakenWarn",false);

		}

		/// <summary>
		/// Callback for object destruction.
		/// </summary>
		public void OnDestroy()
		{
			RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			PluginConfiguration config = PluginConfiguration.CreateForType<KartographSettings> ();
			config.load ();
			config.SetValue ("VesselWindowPos",_windowPos);
			config.save ();
			if (_instance == this)
				_instance = null;
		}

		/// <summary>
		/// Toggles window visibility.
		/// </summary>
		public void ToggleWindow()
		{
			_active = !_active;
			if (_active) {
				RenderingManager.AddToPostDrawQueue (0, OnDraw);
			} else {
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
		}

		public bool IsUsable()
		{
			return true;
		}

		/// <summary>
		/// Called on physics update. Set the Distances.
		/// </summary>
		public void FixedUpdate()
		{
			double time = Planetarium.GetUniversalTime ();

			if (!_active || HighLogic.LoadedSceneIsEditor || time - _prevTime < SAMPLE_TIME) {
				return;
			}

			_prevTime = time;

			_vessels.Clear ();
			Vessel vessel = FlightGlobals.ActiveVessel;
			foreach (Vessel v in FlightGlobals.Vessels) {
				if (v == vessel)
					continue;
				double distance = Vector3.Distance (v.transform.position, vessel.transform.position);
				while (_vessels.ContainsKey (distance)) {
					distance += 0.001d;
				}
				_vessels.Add (distance, v);
			}

			// Destroy a random leaf part if kraken is set.
			if (_krakenSacrifice != null) {

				if (_krakenCountDown == 0) {
					int count = _krakenSacrifice.parts.Count;
					Part part = _krakenSacrifice.parts [UnityEngine.Random.Range (0, count - 1)];
					while (part.children.Count > 0) {
						part = part.children [UnityEngine.Random.Range (0, part.children.Count - 1)];
					}
					part.explode ();
					_krakenCountDown = 10;
				} else {
					_krakenCountDown--;
				}

				if (_krakenSacrifice.parts.Count == 0) {
					_krakenSacrifice = null;
				}
			}
		}

		/// <summary>
		/// Callback when a draw is requested.
		/// </summary>
		private void OnDraw()
		{
			if (!_hasInitStyles)
				InitStyles ();
			if (_active && HighLogic.LoadedSceneHasPlanetarium) {
				_windowPos = GUILayout.Window (_winID, _windowPos, OnWindow, "Vessel Select", _windowStyle);
				if (_windowPos.x == 0.0f && _windowPos.y == 0.0f) {
					_windowPos.y = Screen.height * 0.5f - Math.Max(_windowPos.height * 0.5f,150.0f);
					_windowPos.x = Screen.width * 0.5f - _windowPos.width * 0.5f;
				}
			}
		}

		/// <summary>
		/// Draws the window.
		/// </summary>
		/// <param name="windowId">Window identifier.</param>
		private void OnWindow(int windowId)
		{
			GUILayout.BeginVertical ();
			if (FlightGlobals.ActiveVessel != null) {
				GUILayout.Label (FlightGlobals.ActiveVessel.RevealName (), _centeredLabelStyle);
			}
			_asteroids = GUILayout.Toggle (_asteroids, "Include Asteroids", _toggleStyle);
			_debris = GUILayout.Toggle (_debris, "Include Debris", _toggleStyle);

			_scrollPos = GUILayout.BeginScrollView (_scrollPos, _scrollStyle, GUILayout.MinWidth (420.0f), GUILayout.Height (200.0f));
			foreach (KeyValuePair<double, Vessel> kvp in _vessels) {
				string desc = "";
				if (kvp.Value.vesselType == VesselType.Debris  && !_debris) {
					continue;
				}
				if (kvp.Value.vesselType == VesselType.SpaceObject  && !_asteroids) {
					continue;
				}
				if (kvp.Value.vesselType == VesselType.SpaceObject  && kvp.Value.DiscoveryInfo.Level < DiscoveryLevels.Name) {
					continue;
				}
				if (kvp.Value.vesselType == VesselType.Flag) {
					desc = "Flag:";
				}

				GUILayout.BeginHorizontal ();
				GUILayout.Label (desc+kvp.Value.RevealName (), _labelStyle,GUILayout.MinWidth(150.0f));
				GUILayout.Label (KartographStyle.Instance.GetNumberString (kvp.Key) + "m", _labelStyle, GUILayout.MinWidth (100.0f));
				if (GUILayout.Button("Target",_buttonStyle)) {
					FlightGlobals.fetch.SetVesselTarget (kvp.Value);
				}
				if (GUILayout.Button ("Switch", _buttonStyle)) {
					FlightGlobals.SetActiveVessel (kvp.Value);
				}

				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Close", _buttonStyle)) {
				ToggleWindow ();
			}
			if (GUILayout.Button ("Untarget", _buttonStyle)) {
				FlightGlobals.fetch.SetVesselTarget (null);
			}
			if (!KartographSettings.Instance.DisableKraken && GUILayout.Button ("Unleash the Kraken", _buttonStyle)) {
				if (_krakenWarn) {
					_krakenSacrifice = FlightGlobals.ActiveVessel;
				} else {
					ScreenMessages.PostScreenMessage ("Unleashing the Kraken will destroy the active vessel. You get only one warning.");
					_krakenWarn = true;
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
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
			_scrollStyle = KartographStyle.Instance.ScrollView;
			_toggleStyle = KartographStyle.Instance.Toggle;

			_hasInitStyles = true;
		}

	}
}

