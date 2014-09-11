using System;

namespace Recorder {
	public class Conf {
		public bool Enabled;
		public bool Undo;

		public Conf (bool enabled, bool undo) {
			Enabled = enabled;
			Undo = undo;
		}
	}
}

