using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaReign.Models {
	public interface ITvRegexRepo {
		 LinkedList<Regex> Matches { get; }
		 Regex Cleanup { get; }
		 Regex Separator { get; }

		 string SeriesGroup { get; }
		 string SeasonGroup { get; }
		 string EpisodeGroup { get; }
	}
}
