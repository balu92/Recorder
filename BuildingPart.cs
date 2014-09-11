using System;
using UnityEngine;

namespace Recorder {
	public class BuildingPart {

		public string prefab;
		public Vector3 localPosition;
		public Quaternion localRotation;
		public bool hasInventory = false;
		public DeployedInv Inv;
		public Inventory InternalInv;

		public BuildingPart(Vector3 pos, Quaternion rot, string pref, DeployedInv inv) {
			if (inv != null) {
				Inv = inv;
				hasInventory = true;
			}

			prefab = pref;
			localPosition = pos;
			localRotation = rot;
		}

		public BuildingPart(Vector3 pos, Quaternion rot, string pref, Inventory inv) {
			if (inv != null) {
				Inv = new DeployedInv(inv);
				InternalInv = inv;
				hasInventory = true;
			}

			prefab = pref;
			localPosition = pos;
			localRotation = rot;
		}

		public void UpdateInv() {
			if (InternalInv == null)
				return;

			Inv = new DeployedInv(InternalInv);
		}
	}
}

