using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaReign.Models {
	public class TvShowRegexRepo : ITvShowRegexRepo {
		private const string Show = @"^(?<Show>.*?)";
		private static string[] SeasonEpisode = new[] {
			@"[\W_]+S(?<Season>\d+)E(?<Episode>\d+)",
			@"[\W_]+(?<Season>\d+)x(?<Episode>\d+)",
			@"[\W_]+(?<Season>\d{1})(?<Episode>\d{2})[\W_]",
			@"[\W_]+?(?<Episode>\d{2})[\W_]"
		};

		public TvShowRegexRepo() {
			Separator = new Regex(@"[\W_]");
			Cleanup = new Regex(@"(^\(.*\))|\[.*?\]");
			Matches = new LinkedList<Regex>(SeasonEpisode.Select(s => new Regex(Show + s, RegexOptions.IgnoreCase)));
		}

		public LinkedList<Regex> Matches { get; private set; }
		public Regex Cleanup { get; private set; }
		public Regex Separator { get; private set; }

		public string ShowGroup { get { return "Show"; } }
		public string SeasonGroup { get { return "Season"; } }
		public string EpisodeGroup { get { return "Episode"; } }
	}
}
