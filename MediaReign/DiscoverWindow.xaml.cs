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
using TvDb;
using System.IO;
using MediaReign.Models;
using System.ComponentModel;
using MediaReign.Controls;

namespace MediaReign {
	/// <summary>
	/// Interaction logic for DiscoverWindow.xaml
	/// </summary>
	public partial class DiscoverWindow : Window {
		private class TvFileItem {
			public string File { get; set; }
			public TvMatch Match { get; set; }
			public string EpisodesDisplay { get { return Match.Episode + (Match.ToEpisode.HasValue ? " - " + Match.ToEpisode : null); }}
			public string Message { get; set; }
			public TvDbSeries Series { get; set; }
			public List<TvDbEpisode> Episodes { get; set; }
			public bool IsIdle { get; set; }
		}

		private Dictionary<string, TvDbSeries> seriesCache = new Dictionary<string, TvDbSeries>(StringComparer.OrdinalIgnoreCase);

		public DiscoverWindow() {
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e) {
			FetchFiles();
			base.OnInitialized(e);
		}

		private void FetchFiles() {
			var root = new DirectoryInfo(Settings.FetchPath);
			var matcher = new TvMatcher();
			matcher.RegexRepo = new TvRegexRepo();

			var items = new LinkedList<TvFileItem>();
			var files = root.GetFiles().Where(f => Settings.MediaExtensions.Any(ext => f.Name.EndsWith(ext)));

			foreach(var f in files) {
				var match = matcher.Match(f.Name);
				if(match == null) continue;

				items.AddLast(new TvFileItem { 
					File = f.Name,
					Match = match 
				});
			}

			filesListView.ItemsSource = items;

			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += new DoWorkEventHandler(worker_DoWork);
			worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, ex) => {
				//filesListView.ItemsSource = items;
			});

			worker.RunWorkerAsync(items);
		}

		void worker_DoWork(object sender, DoWorkEventArgs e) {
			var worker = sender as BackgroundWorker;
			var items = e.Argument as LinkedList<TvFileItem>;
			var tvdb = new TvDbRequest(Settings.TvDbApiKey);
			var i = -1;

			foreach(var item in items) {
				i++;
				var progress = (int)(i / (double)items.Count() * 100);

				Action<string, bool> report = (m, idle) => {
					item.Message = m;
					item.IsIdle = idle;
					worker.ReportProgress(progress, i);
				};
				report("Searching TvDb", false);

				try {
					TvDbSeries series;
					if(!seriesCache.TryGetValue(item.Match.Name, out series)) {
						var results = tvdb.Search(item.Match.Name);

						if(!results.Any()) {
							report("Found no results", true);
							continue;
						}

						report("Found match, getting series information", false);
						var seriesId = results.First().Id;

						if(!seriesCache.TryGetValue(seriesId.ToString(), out series)) {
							series = tvdb.Series(seriesId);
							seriesCache.Add(seriesId.ToString(), series);
						}
						seriesCache.Add(item.Match.Name, series);
					}
					report("Found series, looking up episode", false);

					item.Series = series;
					if(item.Match.Season.HasValue) {
						item.Episodes = item.Series.Episodes.Where(ep =>
							ep.Season == item.Match.Season
							&& ep.Number >= item.Match.Episode
							&& ep.Number <= (item.Match.ToEpisode.HasValue ? item.Match.ToEpisode.Value : item.Match.Episode))
							.ToList();
					} else {
						item.Episodes = item.Series.Episodes.Where(ep => ep.AbsoluteNumber == item.Match.Episode).ToList();
					}

					if(!item.Episodes.Any()) {
						report("Couldn't find episode", true);
						continue;
					}

					report(String.Format("{0} - S{1:00}E{2:00}{3} - {4}{5}", 
						item.Series.Name, 
						item.Episodes.First().Season, 
						item.Episodes.First().Number,
 						item.Match.ToEpisode.HasValue ? String.Format("-E{0:00}", item.Match.ToEpisode) : null,
						String.Join(" + ", item.Episodes.Select(ep => ep.Name)), 
						System.IO.Path.GetExtension(item.File)), true);

				} catch(Exception) {
					report("A problem occurred", true);
				}
			}

			e.Result = true;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			var item = filesListView.Items[(int)e.UserState] as TvFileItem;
			var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			var status = container.FindControl<TextBlock>("status");
			status.Text = item.Message;
			var progress = container.FindControl<AnimatedImage>("progress");
			progress.Visibility = item.IsIdle ? Visibility.Collapsed : Visibility.Visible;
		}
	}
}
