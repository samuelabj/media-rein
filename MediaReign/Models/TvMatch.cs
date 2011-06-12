using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign.Models {
	public class TvMatch {

		public TvMatch(string name, int? season, int episode, int? toEpisode) {
			Name = name;
			Season = season;
			Episode = episode;
			ToEpisode = toEpisode;
		}

		public string Name { get; private set; }
		public int? Season { get; private set; }
		public int Episode { get; private set; }
		public int? ToEpisode { get; private set; }
	}
}
