using System;

namespace Recorder {
	public class DeployedInvItem {

		public string Name;
		public int Quantity;
		public int Slot;

		public DeployedInvItem(string name, int qty, int slot) {
			Name = name;
			Quantity = qty;
			Slot = slot;
		}
	}
}

