using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvDb {
	public class TvDbSeries {

		public int Id { get; internal set; }
		public string[] Actors { get; internal set; }
		public DayOfWeek AirsDay { get; internal set; }
		public DateTime AirsTime { get; internal set; }
		public string ContentRating { get; internal set; }
		public DateTime FirstAired { get; internal set; }
		public string[] Genre { get; internal set; }
		public string IMDbId { get; internal set; }
		public string Language { get; internal set; }
		public string Network { get; internal set; }
		public string Overview { get; internal set; }
		public double Rating { get; internal set; }
		public double Runtime { get; internal set; }
		public string TvDotComId { get; internal set; }
		public string Name { get; internal set; }
		public string Status { get; internal set; }
		public string BannerPath { get; internal set; }
		public string FanartPath { get; internal set; }
		public DateTime LastUpdated { get; internal set; }
		public string PosterPath { get; internal set; }
		public string Zap2ItId { get; internal set; }

		public List<TvDbEpisode> Episodes { get; internal set; }
	}
}
