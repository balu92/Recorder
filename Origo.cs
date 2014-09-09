using System;
using UnityEngine;

namespace Recorder {
	public class Origo {

		public Vector3 position;
		public Quaternion rotation;

		public Origo(Vector3 originV3, Quaternion originRot) {
			position = originV3;
			rotation = originRot;
		}
	}
}

