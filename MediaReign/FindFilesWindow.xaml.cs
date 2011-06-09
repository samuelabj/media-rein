using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using TvDb;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace MediaReign {
	/// <summary>
	/// Interaction logic for FindFilesWindow.xaml
	/// </summary>
	public partial class FindFilesWindow : Window {
		private struct FoundFile {
			public string Original { get; set; }
			public string New { get; set; }
		}

		public FindFilesWindow() {
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e) {
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += new DoWorkEventHandler(worker_DoWork);
			worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, ex) => {
				var data = ex.Result as List<FoundFile>;
				filesLst.ItemsSource = data;
			});

			worker.RunWorkerAsync();

			base.OnInitialized(e);
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			
		}

		void worker_DoWork(object sender, DoWorkEventArgs e) {
			var tvdb = new TvDbRequest("A1DA4CF74415C72E");
			var fileRegex = new List<Regex>();
			fileRegex.Add(new Regex(@"S(?<s>\d+)E(?<e>\d+)"));
			
			var foundFiles = new List<FoundFile>();
			var watch = System.Diagnostics.Stopwatch.StartNew();

			using(var db = DataHelper.Context()) {
				foreach(var sr in db.Series.ToList()) {					
					var results = tvdb.Series(sr.TvDbId.Value, "en");
					foundFiles.Add(new FoundFile { Original = watch.Elapsed.ToString() });
					break;
					var dir = new DirectoryInfo(sr.Path);
					IEnumerable<FileInfo> files;
					var seasons = dir.GetDirectories("Season*");

					foreach(var sdir in seasons) {
						foreach(var file in sdir.GetFiles()) {
							if(!Settings.MediaExtensions.Contains(file.Extension)) continue;

							foundFiles.Add(new FoundFile {
								Original = file.Name,
								New = "",
							});								
						}
					}

					//if(seasons.Any()) {
					//    files = (from sn in seasons
					//             select (from f in sn.GetFiles()
					//                     where Settings.MediaExtensions.Contains(f.Extension)
					//                     select f))
					//                    .SelectMany(f => f);
					//} else {
					//    files = from f in dir.GetFiles()
					//            where Settings.MediaExtensions.Contains(f.Extension)
					//            select f;
					//}

					//foreach(var file in files) {
					//    Match m = null;
					//    foreach(var regex in fileRegex) {
					//        if((m = regex.Match(file.Name)).Success) {
					//            break;
					//        }
					//    }

					//    TvDbEpisode episode = null;
					//    if(m != null && m.Success) {
					//        var sn = int.Parse(m.Groups["s"].Value);
					//        var en = int.Parse(m.Groups["e"].Value);

					//        episode = results.Episodes.SingleOrDefault(ep => ep.Season == sn && ep.Number == en);
					//    }

					//    foundFiles.Add(new FoundFile {
					//        Original = file.Name,
					//        New = episode != null ? String.Format("{0} - S{1:00}E{2:00} - {3}", sr.Name, episode.Season, episode.Number, episode.Name) : "not found"
					//    });
					//}
				}
			}

			e.Result = foundFiles;
		}
	}
}
