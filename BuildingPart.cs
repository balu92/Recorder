using System;
using UnityEngine;

namespace Recorder {
	public class BuildingPart {

		public string prefab;
		public Vector3 localPosition;
		public Quaternion localRotation;

		public BuildingPart(Vector3 pos, Quaternion rot, string pref) {
			prefab = pref;
			localPosition = pos;
			localRotation = rot;
		}
	}
}

