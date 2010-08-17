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
						  Id = series.Get<int>("seriesId"),
						  Aired = series.Get<DateTime>("FirstAired"),
						  Language = series.Get<string>("language"),
						  Overview = series.Get<string>("Overview"),
						  Name = series.Get<string>("SeriesName"),
						  IMDbId = series.Get<string>("IMDB_ID"),
						  BannerPath = series.Get<string>("banner")
					  };

			return new LinkedList<TvDbSearchResult>(results);
		}

		private XDocument DownloadXml(string request, params string[] args) {
			var data = web.DownloadString(BuildRequestPath(request, args));
			return XDocument.Parse(data);
		}

		private string BuildRequestPath(string request, params string[] args) {
			return Mirror + String.Format(request, args.Select(a => (object)Uri.EscapeDataString(a)).ToArray());
		}
	}
}
