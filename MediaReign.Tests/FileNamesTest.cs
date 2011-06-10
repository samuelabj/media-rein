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
			matcher.RegexRepo = new RegexRepo();

			foreach(var info in Helper.Files) {
				var match = matcher.Match(info.File);
				Assert.Equal(info.IsDummy, match == null);

				if(match != null) {
					Assert.Equal(info.Show, match.Name, StringComparer.OrdinalIgnoreCase);
					Assert.Equal(info.Season, match.Season);
					Assert.Equal(info.Episode, match.Episode);
				}
			}
		}
	}
}
