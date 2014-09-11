using Rust;
using Fougerite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Recorder {
	public class Recorder : Fougerite.Module {

        public override string Name {
			get { return "Recorder"; }
        }
        public override string Author {
            get { return "balu92"; }
        }
        public override string Description {
            get { return "Record buildings"; }
        }
        public override Version Version {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

		public static Recorder GetInstance() { return instance; }
		private static Recorder instance;

		public Conf config;

		public DirectoryInfo SavedBuildings;
		public Dictionary<string, Builder> builders;
		public Dictionary<string, Building> buildings;
		public Dictionary<string, List<object>> buildhistory;
		// Entity.Name, PrefabName, Cost
		public String[,] prefabNames = new string[,] {
			{"SingleBed", ";deploy_singlebed", "Cloth:40|Metal Fragments:100"},
			{"MetalCeiling", ";struct_metal_ceiling", "Low Quality Metal:6"},
			{"MetalDoorFrame", ";struct_metal_doorframe", "Low Quality Metal:4"},
			{"MetalFoundation", ";struct_metal_foundation", "Low Quality Metal:8"},
			{"MetalPillar", ";struct_metal_pillar", "Low Quality Metal:2"},
			{"MetalRamp", ";struct_metal_ramp", "Low Quality Metal:5"},
			{"MetalStairs", ";struct_metal_stairs", "Low Quality Metal:5"},
			{"MetalWall", ";struct_metal_wall", "Low Quality Metal:4"},
			{"MetalWindowFrame", ";struct_metal_windowframe", "Low Quality Metal:4"},
			{"WoodCeiling", ";struct_wood_ceiling", "Wood Planks:6"},
			{"WoodDoorFrame", ";struct_wood_doorway", "Wood Planks:4"},
			{"WoodFoundation", ";struct_wood_foundation", "Wood Planks:8"},
			{"WoodPillar", ";struct_wood_pillar", "Wood Planks:2"},
			{"WoodRamp", ";struct_wood_ramp", "Wood Planks:5"},
			{"WoodStairs", ";struct_wood_stairs", "Wood Planks:5"},
			{"WoodWall", ";struct_wood_wall", "Wood Planks:4"},
			{"WoodWindowFrame", ";struct_wood_windowframe", "Wood Planks:4"},
			{"Campfire", ";deploy_camp_bonfire", "Wood:5"},
			{"Furnace", ";deploy_furnace", "Stones:15|Wood:20|Low Grade Fuel:10"},
			{"LargeWoodSpikeWall", ";deploy_largewoodspikewall", "Wood:200"},
			{"WoodBoxLarge", ";deploy_wood_storage_large", "Wood:60"},
			{"MetalDoor", ";deploy_metal_door", "Metal Fragments:200"},
			{"MetalBarsWindow", ";deploy_metalwindowbars", "Metal Fragments:100"},
			{"RepairBench", ";deploy_repairbench", "Stones:12|Wood:60|Low Grade Fuel:6|Metal Fragments:50"},
			{"SleepingBagA", ";deploy_camp_sleepingbag", "Cloth:15"},
			{"SmallStash", ";deploy_small_stash", "Leather:10"},
			{"WoodSpikeWall", ";deploy_woodspikewall", "Wood:100"},
			{"Barricade_Fence_Deployable", ";deploy_wood_barricade", "Wood:30"},
			{"WoodGate", ";deploy_woodgate", "Wood:120"},
			{"WoodGateway", ";deploy_woodgateway", "Wood:400"},
			{"Wood_Shelter", ";deploy_wood_shelter", "Wood:50"},
			{"WoodBox", ";deploy_wood_box", "Wood:30"},
			{"WoodenDoor", ";deploy_wood_door", "Wood:30"},
			{"Workbench", ";deploy_workbench", "Wood:50|Stones:8"}
		};

		public string msgName = "Recorder";

        public override void Initialize() {
			instance = this;

			SavedBuildings = new DirectoryInfo(ModuleFolder);
			if (!Directory.Exists(SavedBuildings.FullName)) {
				Directory.CreateDirectory(SavedBuildings.FullName);
			}

			builders = new Dictionary<string, Builder>();
			buildings = new Dictionary<string, Building>();
			buildhistory = new Dictionary<string, List<object>>();
			Hooks.OnCommand -= OnCommandHandler;
			Hooks.OnEntityDeployed -= Record;
			Hooks.OnCommand += OnCommandHandler;
			Hooks.OnEntityDeployed += Record;
			LoadBuildings();
		}

		public override void DeInitialize() {
			foreach (Building b in buildings.Values) {
				b.ToIni();
			}
			builders.Clear();
			buildings.Clear();
			Hooks.OnCommand -= OnCommandHandler;
			Hooks.OnEntityDeployed -= Record;
		}

        void Record(Fougerite.Player player, Entity entity) {
			if (builders.ContainsKey(player.SteamID)) {

				Vector3 ev3 = Vector3.zero;
				Quaternion eq = Quaternion.identity;

				Inventory inv = new Inventory();

				if (entity.Object is DeployableObject) {
					var dep = entity.Object as DeployableObject;
					ev3 = dep.transform.position;
					eq = dep.transform.rotation;
					inv = dep.GetComponent<Inventory>();
				} else if (entity.Object is StructureComponent) {
					var dep = entity.Object as StructureComponent;
					ev3 = dep.transform.position;
					eq = dep.transform.rotation;
				}

				Builder b = builders[player.SteamID];
				if (b.building == null) {
					b.building = new Building(b.buildingName, ev3, eq);
				}

				b.building.Add(ev3, eq, getPrefabName(entity.Name), inv);

				builders[player.SteamID] = b;
			}
        }

        void OnCommandHandler(Fougerite.Player player, string text, string[] args) {
			if (player.Admin) {
				if (text.ToLower() == "record" || text.ToLower() == "rec") {
					if (builders.ContainsKey(player.SteamID)) {
						player.MessageFrom(msgName, String.Format("You are already building: {0}", builders[player.SteamID].building.name));
					} else if (args.Length == 0) {
						player.MessageFrom(msgName, "You need to specify a name for your building.");
					} else if (buildings.ContainsKey(String.Join(" ", args))) {
						player.MessageFrom(msgName, "There is already a building called: " + String.Join(" ", args));
					} else {
						var builder = new Builder(player.SteamID, String.Join(" ", args));
						builders.Add(player.SteamID, builder);
					}

				} else if (text.ToLower() == "stop") {
					if (builders.ContainsKey(player.SteamID)) {
						Builder b = builders[player.SteamID];
						if (b.building == null) {
							Logger.LogDebug("[Builder] There is no building to save.");
							return;
						}

						foreach (BuildingPart bp in b.building.parts.Values) {
							bp.UpdateInv();
						}
						buildings.Add(b.buildingName, b.building);
						b.building.ToIni();
						builders.Remove(player.SteamID);
						player.MessageFrom(msgName, "Your building is saved successfully.");
					}
				} else if (text.ToLower() == "build") {
					Building b;
					if (args.Length == 0) {
						player.MessageFrom(msgName, "You need to specify the name of the building you want to build!");
						return;
					} else if (buildings.ContainsKey(String.Join(" ", args))) {
						b = buildings[String.Join(" ", args)];
						if (b == null)
							return;
					} else {
						player.MessageFrom(msgName, String.Format("{0} is not a valid building name.", String.Join(" ", args)));
						return;
					}

					Vector3 front = Util.GetUtil().Infront(player, 10);
					front.y = (float)(GetGround(front.x, front.z) - 3.7F);

					b.Build(front, player);

				} else if (text.ToLower() == "buildlook") {
					Building b;
					if (args.Length == 0) {
						player.MessageFrom(msgName, "You need to specify the name of the building you want to build!");
						return;
					} else if (buildings.ContainsKey(String.Join(" ", args))) {
						b = buildings[String.Join(" ", args)];
						if (b == null)
							return;
					} else {
						player.MessageFrom(msgName, String.Format("{0} is not a valid building name.", String.Join(" ", args)));
						return;
					}

					Vector3 lookat = PlayerIsLookingAt(player);
					lookat.y -= 3.7F;

					b.Build(lookat, player);
				} else if (text.ToLower() == "undo") {
					if (buildhistory.ContainsKey(player.SteamID)) {
						foreach(object obj in buildhistory[player.SteamID]) {
							Entity ent = new Entity(obj);
							ent.Destroy();
						}
						buildhistory[player.SteamID] = new List<object>();
					}
				} else if (text.ToLower() == "myy"){
					player.Message ("Your Y rotation: " + player.PlayerClient.controllable.transform.rotation.eulerAngles.y);
				}
			}
        }

		private IEnumerable<DirectoryInfo> GetBuildingPaths() {
			foreach (DirectoryInfo dirInfo in SavedBuildings.GetDirectories()) {
				string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".ini");
				if (File.Exists(path)) yield return dirInfo;
			}
		}

		public void LoadBuildings() {
			buildings.Clear();

			foreach (DirectoryInfo bPath in GetBuildingPaths()) {
				IniParser ini = new IniParser(Path.Combine(bPath.FullName, bPath.Name + ".ini"));
				Building building = new Building(bPath.Name, Vector3.zero, Quaternion.identity);
				int count = ini.Count();

				string prefab = string.Empty;
				Vector3 v3 = Vector3.zero;
				Quaternion q = Quaternion.identity;
				Dictionary<int, DeployedInvItem> items = new Dictionary<int, DeployedInvItem>();
				string itemName = String.Empty;
				int itemQty = 0;
				int itemslot = 0;

				for (int i = 0; i < count; i++) {
					prefab = ini.GetSetting(i.ToString(), "prefab");
					v3 = StringToV3(ini.GetSetting(i.ToString(), "localPos"));
					q = StringToQuat(ini.GetSetting(i.ToString(), "localRot"));

					if (ini.GetSetting(i.ToString(), "hasInventory") == true.ToString()) {
						items.Clear();
						IniParser invIni = new IniParser(Path.Combine(bPath.FullName, bPath.Name + "_Part" + i + "_inv") + ".ini");
						for (int j = 0; j < invIni.Count(); j++) {
							itemName = invIni.GetSetting(j.ToString(), "Name");
							itemQty = int.Parse(invIni.GetSetting(j.ToString(), "Quantity"));
							itemslot = int.Parse(invIni.GetSetting(j.ToString(), "Slot"));
							items[i] = new DeployedInvItem(itemName, itemQty, itemslot);
						}
					}
					DeployedInv inv = new DeployedInv(items);

					building.Add(v3, q, prefab, inv);
				}
				buildings.Add(bPath.Name, building);
			}
		}

		/*********
		 * UTILS *
		 *********/

		public float GetGround(float x, float z) {
			RaycastHit hit;
			var orig = new Vector3(x, 1000.0F, z);
			if (Physics.Raycast(orig, Vector3.down, out hit, 1500.0F, 1 << 19)) {
				return hit.point.y;
			}
			return 1000.0F;
		}

		public Vector3 PlayerIsLookingAt(Fougerite.Player player) {
			RaycastHit hit;
			var orig = player.PlayerClient.controllable.eyesRay;
			if (Physics.Raycast(orig, out hit, 500.0F, 1 << 19)) {
				return hit.point;
			}
			return Vector3.zero;
		}

		// TODO
		/*public string BuildingToString(Building building) {

		}

		public Building StringToBuilding(string building) {

		}*/

		public static string V3ToString(Vector3 v3) {
			return String.Format("{0}|{1}|{2}", new string[] { v3.x.ToString("G9"), v3.y.ToString("G9"), v3.z.ToString("G9") });
		}

		public static Vector3 StringToV3(string str) {
			try {
				string[] nums = str.Split(new char[] { '|' });
				return new Vector3(
					Single.Parse(nums[0]),
					Single.Parse(nums[1]),
					Single.Parse(nums[2]));
			} catch {
				return Vector3.zero;
			}
		}

		public static string QuatToString(Quaternion quat) {
			return String.Format("{0}|{1}|{2}|{3}", new object[] { quat.x.ToString("G9"), quat.y.ToString("G9"), quat.z.ToString("G9"), quat.w.ToString("G9") });
		}

		public static Quaternion StringToQuat(string str) {
			try {
				string[] nums = str.Split(new char[] { '|' });
				return new Quaternion(
					Single.Parse(nums[0]),
					Single.Parse(nums[1]),
					Single.Parse(nums[2]),
					Single.Parse(nums[3]));
			} catch {
				return Quaternion.identity;
			}
		}

		public string getPrefabName(string buildPartName) {
			for (var i = 0; i < prefabNames.Length; i++) {
				if (prefabNames[i,0] == buildPartName) {
					return prefabNames[i,1];
				}
			}
			Logger.LogDebug("[Builder] PrefabName: " + buildPartName + " not found!");
			return "<PrefabName Not Found>";
		}
    }
}
