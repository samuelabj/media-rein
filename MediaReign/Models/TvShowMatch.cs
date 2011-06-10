using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign.Models {
	public class TvShowMatch {

		public TvShowMatch(string name, int? season, int? episode) {
			Name = name;
			Season = season;
			Episode = episode;
		}

		public string Name { get; private set; }
		public int? Season { get; private set; }
		public int? Episode { get; private set; }
	}
}
