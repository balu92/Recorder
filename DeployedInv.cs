using System;
using System.IO;
using System.Collections.Generic;

namespace Recorder {
	public class DeployedInv {

		public Dictionary<int, DeployedInvItem> Items;

		public DeployedInv(Dictionary<int, DeployedInvItem> items) {
			Items = items;
		}

		public DeployedInv(Inventory inv) {
			Items = new Dictionary<int, DeployedInvItem>(inv.slotCount);
			IInventoryItem item;
			for (int i = 0; i < inv.slotCount; i++) {
				if (inv.GetItem(i, out item)) {
					Items[i] = new DeployedInvItem(item.datablock.name, item.uses, item.slot);
				}
			}
		}
		
		public void ToIni(string path) {
			if (File.Exists(path)) {
				return;
			}
			File.AppendAllText(path, "");

			IniParser ini = new IniParser(path);

			int itemCount = 0;

			foreach (DeployedInvItem item in Items.Values) {
				if (item != null) {
					ini.AddSetting(itemCount.ToString(), "Name", item.Name);
					ini.AddSetting(itemCount.ToString(), "Slot", item.Slot.ToString());
					ini.AddSetting(itemCount.ToString(), "Quantity", item.Quantity.ToString());
					itemCount++;
				}
			}
			ini.Save();
		}
	}
}

// TODO: fix item quantity for torch, maybe weapons?