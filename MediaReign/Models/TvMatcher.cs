using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign.Models {
	public class TvMatcher {

		public ITvRegexRepo RegexRepo { get; set; }

		public TvMatch Match(string value) {
			var clean = RegexRepo.Cleanup.Replace(value, String.Empty);

			foreach(var matchreg in RegexRepo.Matches) {
				var matches = matchreg.Match(clean);

				var show = matches.Groups[RegexRepo.ShowGroup].Value;
				var season = matches.Groups[RegexRepo.SeasonGroup].Value;
				var episode = matches.Groups[RegexRepo.EpisodeGroup].Value;

				if(String.IsNullOrWhiteSpace(show)) continue;

				show = RegexRepo.Separator.Replace(show, " ").Trim();
				var s = default(int?);
				var e = default(int?);

				if(!String.IsNullOrWhiteSpace(season)) {
					s = int.Parse(season);
				} 
				
				if(!String.IsNullOrWhiteSpace(episode)) {
					e = int.Parse(episode);
				}

				if(e.HasValue) {
					return new TvMatch(show, s, e.Value);
				}
			}

			return null;
		}
	}
}
