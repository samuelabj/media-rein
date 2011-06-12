using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaReign.Models {
	public class TvRegexRepo : ITvRegexRepo {
		private const string Show = @"^(?<Series>.*?)";
		private static string[] SeasonEpisode = new[] {
			@"[\W_]+S(?<Season>\d+)E(?<Episode>\d+)([- :]{0,2}E?(?<ToEpisode>\d+))?",
			@"[\W_]+(?<Season>\d+)x(?<Episode>\d+)([- :]{1,2}(?<ToEpisode>\d+))?",
			@"[^\w(]+(?<Season>\d{1})(?<Episode>\d{2})([- :]{1,2}(?<ToEpisode>\d+))?[^\w)]",
			@"[\W_]+?(?<Episode>\d{2})([- :]{1,2}(?<ToEpisode>\d+))?[\W_]"
		};

		public TvRegexRepo() {
			Separator = new Regex(@"[\W_]");
			Cleanup = new Regex(@"(^\(.*?\))|\[.*?\]");
			Matches = new LinkedList<Regex>(SeasonEpisode.Select(s => new Regex(Show + s, RegexOptions.IgnoreCase)));
		}

		public LinkedList<Regex> Matches { get; private set; }
		public Regex Cleanup { get; private set; }
		public Regex Separator { get; private set; }

		public string SeriesGroup { get { return "Series"; } }
		public string SeasonGroup { get { return "Season"; } }
		public string EpisodeGroup { get { return "Episode"; } }
		public string ToEpisodeGroup { get { return "ToEpisode"; } }
	}
}
