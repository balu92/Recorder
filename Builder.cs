using System;

namespace Recorder {
	public class Builder {
		public string owner;
		public Building building;
		public string buildingName;

		public Builder(string steamid, string name) {
			owner = steamid;
			buildingName = name;
		}
	}
}

