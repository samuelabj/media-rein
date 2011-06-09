using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace MediaReign.Tests {
	public class FileNamesTest {

		[Fact]
		public void DiscoverTvShowName() {
			foreach(var info in Helper.Files) {
				var hasmatch = false;
				var file = Regex.Replace(info.File, Helper.RemoveRegex, "");

				foreach(var ser in Helper.FileNameSeasonEpisodeRegex) {
					var regex = new Regex(Helper.FileNameShowRegex + ser, RegexOptions.IgnoreCase);
					var matches = regex.Match(file);

					var show = matches.Groups["Show"].Value;
					var season = matches.Groups["Season"].Value;
					var episode = matches.Groups["Episode"].Value;

					if(!String.IsNullOrWhiteSpace(show)) {
						Assert.Equal(info.Show, Regex.Replace(show, Helper.SeperatorRegex, " ").Trim(), StringComparer.OrdinalIgnoreCase);
					}

					if(!String.IsNullOrWhiteSpace(season)) {
						Assert.Equal(info.Season, int.Parse(season));
						Assert.Equal(info.Episode, int.Parse(episode));
						hasmatch = true;
					} else if(!String.IsNullOrWhiteSpace(episode)) {
						Assert.Equal(info.Episode, int.Parse(episode));
						hasmatch = true;
					}
				}

				Assert.NotEqual(hasmatch, info.IsDummy);
			}
		}
	}
}
