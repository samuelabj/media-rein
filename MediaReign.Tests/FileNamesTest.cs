using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using MediaReign.Models;

namespace MediaReign.Tests {
	public class FileNamesTest {

		[Fact]
		public void MatchTvShows() {
			var matcher = new TvShowMatcher();
			matcher.RegexRepo = new TvShowRegexRepo();

			foreach(var show in Helper.Shows) {
				var match = matcher.Match(show.File);
				Assert.Equal(show.IsDummy, match == null);

				if(match != null) {
					Assert.Equal(show.Name, match.Name, StringComparer.OrdinalIgnoreCase);
					Assert.Equal(show.Season, match.Season);
					Assert.Equal(show.Episode, match.Episode);
				}
			}
		}
	}
}
