using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaReign.Models {
	public interface ITvShowRegexRepo {
		 LinkedList<Regex> Matches { get; private set; }
		 Regex Cleanup { get; private set; }
		 Regex Separator { get; private set; }

		 string ShowGroup { get; }
		 string SeasonGroup { get; }
		 string EpisodeGroup { get; }
	}
}
