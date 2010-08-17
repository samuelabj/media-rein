using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvDb {
	public class TvDbSearchResult {

		public int Id { get; internal set; }
		public string Language { get; internal set; }
		public string Name { get; internal set; }
		public string BannerPath { get; internal set; }
		public string Overview { get; internal set; }
		public DateTime Aired { get; internal set; }
		public string IMDbId { get; internal set; }
	}
}
