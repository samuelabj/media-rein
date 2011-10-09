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
using MediaReign.Data;

namespace MediaReign {
	/// <summary>
	/// Interaction logic for DiscoverWindow.xaml
	/// </summary>
	public partial class DiscoverWindow : Window {
		private class TvFileItem {
			public FileInfo File { get; set; }
			public TvMatch Match { get; set; }
			public string EpisodesDisplay { get { return Match.Episode + (Match.ToEpisode.HasValue ? " - " + Match.ToEpisode : null); }}
			public string Message { get; set; }
			public TvDbSeries Series { get; set; }
			public List<TvDbEpisode> Episodes { get; set; }
			public bool IsIdle { get; set; }
			public string FormattedName { get; set; }
			public string OverrideName { get; set; }
			public int? OverrideSeriesId { get; set; }
		}

		private Dictionary<string, int> seriesIdCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<int, TvDbSeries> seriesCache = new Dictionary<int, TvDbSeries>();
		private HashSet<string> failed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private LinkedList<TvFileItem> items;

		public DiscoverWindow() {
			InitializeComponent();

			moveBtn.Click += new RoutedEventHandler(moveBtn_Click);
			renameBtn.Click += new RoutedEventHandler(renameBtn_Click);
			filesListView.Loaded += new RoutedEventHandler(filesListView_Loaded);
		}

		protected override void OnInitialized(EventArgs e) {
			FetchFiles();
			
			base.OnInitialized(e);
		}

		void filesListView_Loaded(object sender, RoutedEventArgs e) {
			foreach(var item in items) {
				var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
				var change = container.FindControl<Button>("seriesChangeBtn");
				var temp = item;

				change.Click += new RoutedEventHandler((o, args) => {
					var dialog = new SpecifySeriesWindow();
					if(dialog.ShowDialog().Value) {
						var sameItems = items.Where(i => i.Match.Name.Equals(temp.Match.Name, StringComparison.OrdinalIgnoreCase)).ToList();
						sameItems.ForEach(i => {
							i.OverrideName = dialog.Name;
							i.OverrideSeriesId = dialog.TvDbId;

							var c = filesListView.ItemContainerGenerator.ContainerFromItem(i) as ListViewItem;
							var name = container.FindControl<TextBlock>("name");
							name.Text = i.OverrideSeriesId.ToString() ?? i.OverrideName;
						});

						ProcessFiles(sameItems);
					}					
				});
			}
		}

		private void FetchFiles() {
			var root = new DirectoryInfo(Settings.FetchPath);
			var matcher = new TvMatcher();
			matcher.RegexRepo = new TvRegexRepo();

			items = new LinkedList<TvFileItem>();
			var files = root.GetFiles().Where(f => Settings.MediaExtensions.Contains(f.Extension));

			foreach(var f in files) {
				var match = matcher.Match(f.Name);
				if(match == null) continue;

				items.AddLast(new TvFileItem { 
					File = f,
					Match = match 
				});
			}

			filesListView.ItemsSource = items;
			ProcessFiles(items);
		}

		void ProcessFiles(IEnumerable<TvFileItem> items) {
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += new DoWorkEventHandler(worker_DoWork);
			worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

			worker.RunWorkerAsync(items);
		}

		void worker_DoWork(object sender, DoWorkEventArgs e) {
			var worker = sender as BackgroundWorker;
			var tvdb = new TvDbRequest(Settings.TvDbApiKey);
			var items = e.Argument as IEnumerable<TvFileItem>;
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

				var seriesId = item.OverrideSeriesId;
				item.OverrideSeriesId = null;

				try {
					if(!seriesId.HasValue) {
						var name = item.OverrideName ?? item.Match.Name;
						item.OverrideName = null;

						if(failed.Contains(name)) {
							report("Found no results", true);
							continue;
						}

						int sid;
						if(!seriesIdCache.TryGetValue(name, out sid)) {
							// Check if we've already recorded its alias
							using(var db = DataHelper.Context()) {
								sid = db.Series_Aliases.Where(a => a.Alias == name).Select(a => a.SeriesId).SingleOrDefault();
							}

							if(sid == default(int)) {
								var results = tvdb.Search(name);

								if(!results.Any()) {
									failed.Add(name);
									report("Found no results", true);
									continue;
								}

								sid = results.First().Id;
							}

							seriesIdCache.Add(name, sid);
							report("Found match, getting series information", false);
						}

						seriesId = sid;
					}

					TvDbSeries series;
					if(!seriesCache.TryGetValue(seriesId.Value, out series)) {
						series = tvdb.Series(seriesId.Value);
						seriesCache.Add(seriesId.Value, series);
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

					item.FormattedName = String.Format("{0} - S{1:00}E{2:00}{3} - {4}{5}",
						item.Series.Name,
						item.Episodes.First().Season,
						item.Episodes.First().Number,
						item.Match.ToEpisode.HasValue ? String.Format("-E{0:00}", item.Match.ToEpisode) : null,
						String.Join(" + ", item.Episodes.Select(ep => ep.Name)),
						item.File.Extension);

					report(item.FormattedName, true);

				} catch(Exception ex) {
					Console.WriteLine(ex);
					report("A problem occurred", true);
				}
			}

			e.Result = true;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			var item = filesListView.Items[(int)e.UserState] as TvFileItem;
			var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			
			var status = container.FindControl<TextBlock>("status");
			var progress = container.FindControl<AnimatedImage>("progress");
			var name = container.FindControl<TextBlock>("name");

			if(item.Series != null) {
				name.Text = item.Series.Name;
			}

			status.Text = item.Message;			
			progress.Visibility = item.IsIdle ? Visibility.Collapsed : Visibility.Visible;
		}

		void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			var found = items.Where(i => i.Episodes != null && i.Episodes.Any());
			Dictionary<int, Series> seriesDict;
			string lastPath = null;
			var ids = found.Select(i => i.Series.Id);

			using(var db = DataHelper.Context()) {
				seriesDict = db.Series.Where(s => s.TvDbId.HasValue && ids.Contains(s.TvDbId.Value)).ToDictionary(s => s.TvDbId.Value);
			}
			
			foreach(var item in items) {
				var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
				var saveToTxt = container.FindControl<TextBlock>("saveToTxt");
				var saveToBtn = container.FindControl<Button>("saveToBtn");
				var seriesChangeBtn = container.FindControl<Button>("seriesChangeBtn");

				seriesChangeBtn.Visibility = System.Windows.Visibility.Visible;

				if(!found.Any(i => i == item)) continue;
				saveToBtn.Visibility = System.Windows.Visibility.Visible;
				saveToTxt.Visibility = System.Windows.Visibility.Visible;

				Series series;
				if(seriesDict.TryGetValue(item.Series.Id, out series)) {
					if(Directory.Exists(series.Path)) {
						saveToTxt.Text = series.Path;
					}
				}

				saveToBtn.Click += new RoutedEventHandler((o, args) => {
					var dialog = new System.Windows.Forms.FolderBrowserDialog();
					if(lastPath != null) {
						dialog.SelectedPath = lastPath;
					}

					if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
						saveToTxt.Text = dialog.SelectedPath;
						lastPath = dialog.SelectedPath;

						foreach(var i in found.Where(i => i.Series.Id == item.Series.Id)) {
							Console.WriteLine(i.Series.Id);
							var c = filesListView.ItemContainerGenerator.ContainerFromItem(i) as ListViewItem;
							var s = c.FindControl<TextBlock>("saveToTxt");
							s.Text = dialog.SelectedPath;
						}
					}
				});
			}
		}

		void renameBtn_Click(object sender, RoutedEventArgs e) {
			throw new NotImplementedException();
		}

		void moveBtn_Click(object sender, RoutedEventArgs e) {
			foreach(var item in items.Where(i => i.Episodes != null && i.Episodes.Any())) {
				var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
				var saveToTxt = container.FindControl<TextBox>("saveToTxt");

				if(String.IsNullOrWhiteSpace(saveToTxt.Text)) continue;

				var dir = new DirectoryInfo(saveToTxt.Text);
				if(!dir.Exists) dir.Create();
				if(!dir.GetFiles().Any(f => Settings.MediaExtensions.Contains(f.Extension))) {
					dir = dir.CreateSubdirectory(item.Series.Name);
				}

				var dirPath = dir.FullName.TrimEnd(new char[] { '\\' });
				var path = dirPath + '\\' + item.FormattedName;
				if(System.IO.File.Exists(path)) continue;

				item.File.CopyTo(path);

				using(var db = DataHelper.Context()) {
					var series = db.Series.SingleOrDefault(s => s.TvDbId == item.Series.Id);
					if(series == null) {
						series = new Series();
						series.TvDbId = item.Series.Id;
						db.Series.InsertOnSubmit(series);
					}

					series.Path = dirPath;

					var file = new Data.File();
					file.Path = path;
					file.Files_Histories.Add(new Files_History {
						Date = DateTime.Now,
						Path = item.File.FullName
					});

					db.Files.InsertOnSubmit(file);
					db.SubmitChanges();
				}
			}
		}
	}
}
