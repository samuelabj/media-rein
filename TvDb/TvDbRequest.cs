using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace TvDb {
	public class TvDbRequest {
		private WebClient web;
		private string mirror;

		public TvDbRequest(string api) {
			Api = api;
			web = new WebClient();
		}

		public string Api { get; set; }

		private string Mirror {
			get {
				if(mirror == null) {
					mirror = "http://www.thetvdb.com/api/";
				}
				return mirror;
			}
		}

		public LinkedList<TvDbSearchResult> Search(string name, string language) {
			var xml = DownloadXml("GetSeries.php?seriesname={0}&language={1}", name, language);

			var results = from series in xml.Descendants("Series")
						  where series.HasElements
						  select new TvDbSearchResult {
							  Id = series.Get<int>("seriesid"),
							  Aired = series.Get<DateTime>("FirstAired"),
							  Language = series.Get("language"),
							  Overview = series.Get("Overview"),
							  Name = series.Get("SeriesName"),
							  IMDbId = series.Get("IMDB_ID"),
							  BannerPath = series.Get("banner")
						  };

			return new LinkedList<TvDbSearchResult>(results);
		}

		public TvDbSeries Series(int id, string language) {
			var xml = DownloadXml("{0}/series/{1}/all/{2}", Api, id, language);

			var results = from series in xml.Descendants("Series")
						  where series.HasElements
						  select new TvDbSeries {
							  Id = series.Get<int>("id"),
							  Actors = series.Split("Actors"),
							  AirsDay = series.Get<DayOfWeek>("Airs_DayOfWeek"),
							  AirsTime = series.Get<DateTime>("Airs_Time"),
							  ContentRating = series.Get("ContentRating"),
							  FirstAired = series.Get<DateTime>("FirstAired"),
							  Genre = series.Split("Genre"),
							  IMDbId = series.Get("IMDB_ID"),
							  Language = series.Get("Language"),
							  Network = series.Get("Network"),
							  Overview = series.Get("Overview"),
							  Rating = series.Get<double>("Rating"),
							  Runtime = series.Get<double>("Runtime"),
							  TvDotComId = series.Get("SeriesID"),
							  Name = series.Get("SeriesName"),
							  Status = series.Get("Status"),
							  BannerPath = series.Get("banner"),
							  FanartPath = series.Get("fanart"),
							  LastUpdated = series.Get<DateTime>("lastupdated"),
							  PosterPath = series.Get("poster"),
							  Zap2ItId = series.Get("zap2it_id"),

							  Episodes = (from ep in series.Descendants("Episodes")
										 where ep.HasElements
										 select new TvDbEpisode {
											 Id = ep.Get<int>("id"),
											 Directors = ep.Split("Director"),
											 Name = ep.Get("EpisodeName"),
											 Number = ep.Get<int>("EpisodeNumber"),
											 Aired = ep.Get<DateTime>("FirstAired"),
											 GuestStars = ep.Split("GuestStars"),
											 ImDbId = ep.Get("IMDB_ID"),
											 Language = ep.Get("Language"),
											 Overview = ep.Get("Overview"),
											 Season = ep.Get<int>("SeasonNumber"),
											 Writers = ep.Split("Writer"),
											 AbsoluteNumber = ep.Get<int>("absolute_number"),
											 Filename = ep.Get("filename"),
											 LastUpdated = ep.Get<DateTime>("lastupdated"),
											 SeasonId = ep.Get<int>("seasonid"),
											 SeriesId = ep.Get<int>("seriesId")
										 })
										 .ToList()
						  };

			return results.Single();
		}

		private XDocument DownloadXml(string request, params object[] args) {
			var data = web.DownloadString(BuildRequestPath(request, args));
			return XDocument.Parse(data);
		}

		private string BuildRequestPath(string request, params object[] args) {
			return Mirror + String.Format(request, args.Select(a => (object)Uri.EscapeDataString(a.ToString())).ToArray());
		}
	}
}
