﻿using System;
using System.IO;
using System.Collections.Generic;
using Fougerite;
using UnityEngine;

namespace Recorder {
	public class Building {

		public string name;
		public DirectoryInfo path;
		public Dictionary<int, BuildingPart> parts;
		public Origo origo;			// don't serialize this
		public StructureMaster sm;	// don't serialize this

		public Building(string namee, Vector3 ov3, Quaternion ovRot) {
			name = namee;
			path = new DirectoryInfo(Path.Combine (Recorder.GetInstance ().SavedBuildings.FullName, name));
			origo = new Origo(ov3, ovRot);
			parts = new Dictionary<int, BuildingPart>();
		}

		public void Add(Vector3 pos, Quaternion rot, string prefName) {
			var bp = new BuildingPart(WorldToLocalpos(pos), rot, prefName);
			Add(bp);
		}

		public void Add(BuildingPart bp) {
			parts.Add(parts.Count, bp);
		}

		public void Build(Vector3 buildTo, Fougerite.Player player) {

			Vector3 spawnPos;
			Quaternion spawnRot;

			List<object> bhistory = new List<object>();
			foreach (BuildingPart bp in parts.Values) {
				spawnPos = buildTo + bp.localPosition;
				spawnRot = bp.localRotation;

				if (sm == null) {
					sm = World.GetWorld().CreateSM(player, spawnPos.x, spawnPos.y, spawnPos.z, spawnRot);
				}
				Entity spawnedObj = (Entity)World.GetWorld().Spawn(bp.prefab, spawnPos.x, spawnPos.y, spawnPos.z, spawnRot);
				bhistory.Add (spawnedObj.Object);

				if (spawnedObj.Object is DeployableObject) {
					spawnedObj.ChangeOwner(player);
				} else if (spawnedObj.Object is StructureComponent) {
					sm.AddStructureComponent(spawnedObj.Object as StructureComponent);
				}
			}
			Recorder.buildhistory[player.SteamID] = bhistory;
		}

		public Vector3 WorldToLocalpos(Vector3 v3) {
			return v3 - origo.position;
		}

		public void ToIni() {
			if (File.Exists (path.FullName + ".ini")) {
				return;
			}
			var ini = new IniParser (path.FullName + ".ini");
			for (int i = 0; i < parts.Count; i++) {
				ini.AddSetting (i.ToString(), "prefab", parts[i].prefab);
				ini.AddSetting (i.ToString(), "localPos", Recorder.V3ToString(parts[i].localPosition));
				ini.AddSetting (i.ToString(), "localRot", Recorder.QuatToString(parts[i].localRotation));
			}
			ini.Save ();
		}
	}
}

