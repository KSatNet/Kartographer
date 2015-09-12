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
	public class TimeControl
	{
		public double UT { get { return _UT; } }

		const int			MAX_TIME_GRAN = 2;

		private GUIStyle 	_labelStyle;
		private GUIStyle	_buttonStyle;
		private GUIStyle	_scrollStyle;
		private double 		_UT = 0.0d;
		private int 		_timeGranularity = 1;


		public TimeControl ()
		{
			InitStyles ();
		}


		public double TimeGUI(double ut)
		{
			_UT = ut;
			GUILayout.Label ("Time Controls", _labelStyle);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Finer", _buttonStyle)) {
				_timeGranularity--;
				if (_timeGranularity < 0)
					_timeGranularity = 0;
			}
			if (GUILayout.Button ("Coarser", _buttonStyle)) {
				_timeGranularity++;
				if (_timeGranularity > MAX_TIME_GRAN)
					_timeGranularity = MAX_TIME_GRAN;
			}
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			if (_timeGranularity == 0) {
				if (GUILayout.Button ("+.01sec", _buttonStyle)) {
					_UT = _UT + (0.01);
				}
				if (GUILayout.Button ("+.1sec", _buttonStyle)) {
					_UT = _UT + (0.1);
				}
				if (GUILayout.Button ("+1sec", _buttonStyle)) {
					_UT = _UT + (1.0);
				}
				if (GUILayout.Button ("+10sec", _buttonStyle)) {
					_UT = _UT + (10.0);
				}
			} else if (_timeGranularity == 1) {
				if (GUILayout.Button ("+1min", _buttonStyle)) {
					_UT = _UT + (60.0);
				}
				if (GUILayout.Button ("+10min", _buttonStyle)) {
					_UT = _UT + (10.0 * 60.0);
				}
				if (GUILayout.Button ("+1hr", _buttonStyle)) {
					_UT = _UT + (Util.ONE_KHOUR);
				}
				if (GUILayout.Button ("+1d", _buttonStyle)) {
					_UT = _UT + (Util.ONE_KDAY);
				}
			} else {
				if (GUILayout.Button ("+10d", _buttonStyle)) {
					_UT = _UT + (10.0 * Util.ONE_KDAY);
				}
				if (GUILayout.Button ("+100d", _buttonStyle)) {
					_UT = _UT + (100.0 * Util.ONE_KDAY);
				}
				if (GUILayout.Button ("+1yr", _buttonStyle)) {
					_UT = _UT + (Util.ONE_KYEAR);
				}
				if (GUILayout.Button ("+10yr", _buttonStyle)) {
					_UT = _UT + (10.0 * Util.ONE_KYEAR);
				}
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (_timeGranularity == 0) {
				if (GUILayout.Button ("-.01sec", _buttonStyle)) {
					_UT = _UT - (0.01);
				}
				if (GUILayout.Button ("-.1sec", _buttonStyle)) {
					_UT = _UT - (0.1);
				}
				if (GUILayout.Button ("-1sec", _buttonStyle)) {
					_UT = _UT - (1.0);
				}
				if (GUILayout.Button ("-10sec", _buttonStyle)) {
					_UT = _UT - (10.0);
				}
			} else if (_timeGranularity == 1) {
				if (GUILayout.Button ("-1min", _buttonStyle) ) {
					_UT = _UT - (60.0);
				}
				if (GUILayout.Button ("-10min", _buttonStyle) ) {
					_UT = _UT - (10.0 * 60.0);
				}
				if (GUILayout.Button ("-1hr", _buttonStyle)) {
					_UT = _UT - (Util.ONE_KHOUR);
				}
				if (GUILayout.Button ("-1d", _buttonStyle) ) {
					_UT = _UT - (Util.ONE_KDAY);
				}
			} else {
				if (GUILayout.Button ("-10d", _buttonStyle) ) {
					_UT = _UT - (10.0 * Util.ONE_KDAY);
				}
				if (GUILayout.Button ("-100d", _buttonStyle) ) {
					_UT = _UT - (100.0 * Util.ONE_KDAY);
				}
				if (GUILayout.Button ("-1yr", _buttonStyle) ) {
					_UT = _UT - (Util.ONE_KYEAR);
				}
				if (GUILayout.Button ("-10yr", _buttonStyle) ) {
					_UT = _UT - (10.0 * Util.ONE_KYEAR);
				}
			}
			GUILayout.EndHorizontal ();

			return _UT;
		}



		/// <summary>
		/// Initializes the styles.
		/// </summary>
		private void InitStyles()
		{
			_labelStyle = KartographStyle.Instance.Label;
			_buttonStyle = KartographStyle.Instance.Button;
			_scrollStyle = KartographStyle.Instance.ScrollView;
		}
	}
}

