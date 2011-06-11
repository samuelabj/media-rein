using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Diagnostics;

namespace TvDb {
	public class TvDbRequest {
		private string mirror;
		private const string language = "en";

		public TvDbRequest(string api) {
			Api = api;
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

		public LinkedList<TvDbSearchResult> Search(string name) {
			var xml = DownloadXml("GetSeries.php?seriesname={0}&language={1}", name, language);

			var results = from series in xml.Descendants("Series")
						  where series.HasElements
						  select new TvDbSearchResult {
							  Id = series.GetInt("seriesid"),
							  Aired = series.GetDateTime("FirstAired"),
							  Language = series.Get("language"),
							  Overview = series.Get("Overview"),
							  Name = series.Get("SeriesName"),
							  IMDbId = series.Get("IMDB_ID"),
							  BannerPath = series.Get("banner")
						  };

			return new LinkedList<TvDbSearchResult>(results);
		}

		public TvDbSeries Series(int id, bool zip) {
			var xml = DownloadXml("{0}/series/{1}/all/{2}", zip, Api, id, language);
			
			var results = from series in xml.Descendants("Series")
						  where series.HasElements
						  select new TvDbSeries {
							  Id = series.GetInt("id"),
							  Actors = series.Split("Actors"),
							  AirsDay = series.Get<DayOfWeek>("Airs_DayOfWeek"),
							  AirsTime = series.GetDateTime("Airs_Time"),
							  ContentRating = series.Get("ContentRating"),
							  FirstAired = series.GetDateTime("FirstAired"),
							  Genre = series.Split("Genre"),
							  IMDbId = series.Get("IMDB_ID"),
							  Language = series.Get("Language"),
							  Network = series.Get("Network"),
							  Overview = series.Get("Overview"),
							  Rating = series.GetDouble("Rating"),
							  Runtime = series.GetDouble("Runtime"),
							  TvDotComId = series.Get("SeriesID"),
							  Name = series.Get("SeriesName"),
							  Status = series.Get("Status"),
							  BannerPath = series.Get("banner"),
							  FanartPath = series.Get("fanart"),
							  LastUpdated = series.GetUnixDateTime("lastupdated"),
							  PosterPath = series.Get("poster"),
							  Zap2ItId = series.Get("zap2it_id"),

							  Episodes = (from ep in xml.Descendants("Episode")
							             where ep.HasElements
							             select new TvDbEpisode {
							                 Id = ep.GetInt("id"),
							                 Directors = ep.Split("Director"),
							                 Name = ep.Get("EpisodeName"),
							                 Number = ep.GetInt("EpisodeNumber"),
											 Aired = ep.GetDateTime("FirstAired"),
							                 GuestStars = ep.Split("GuestStars"),
							                 ImDbId = ep.Get("IMDB_ID"),
							                 Language = ep.Get("Language"),
							                 Overview = ep.Get("Overview"),
											 Season = ep.GetInt("SeasonNumber"),
							                 Writers = ep.Split("Writer"),
											 AbsoluteNumber = ep.GetInt("absolute_number"),
							                 Filename = ep.Get("filename"),
											 LastUpdated = ep.GetUnixDateTime("lastupdated"),
											 SeasonId = ep.GetInt("seasonid"),
											 SeriesId = ep.GetInt("seriesId")
							             })
							             .ToList()
						  };

			return results.Single();
		}

		public TvDbSeries Series(int id) {
			return Series(id, false);
		}

		private XDocument DownloadXml(string request, bool zip, params object[] args) {
			if(zip) {
				var data = DownloadZip(request + ".zip", args);
				return XDocument.Parse(Encoding.UTF8.GetString(data));
			} else {
				var data = new WebClient().DownloadString(BuildRequestPath(request, args));
				return XDocument.Parse(data);
			}
		}

		private XDocument DownloadXml(string request, params object[] args) {
			return DownloadXml(request, false, args);
		}

		private byte[] DownloadZip(string request, params object[] args) {
			var data = new WebClient().DownloadData(BuildRequestPath(request, args));

			using(var zip = new ZipInputStream(new MemoryStream(data))) {
				zip.GetNextEntry();
				var buffer = new byte[zip.Length];
				zip.Read(buffer, 0, buffer.Length);

				return buffer;
			}
		}

		private string BuildRequestPath(string request, params object[] args) {
			return Mirror + String.Format(request, args.Select(a => (object)Uri.EscapeDataString(a.ToString())).ToArray());
		}
	}
}
