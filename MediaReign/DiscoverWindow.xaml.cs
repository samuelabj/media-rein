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

namespace MediaReign {
	/// <summary>
	/// Interaction logic for DiscoverWindow.xaml
	/// </summary>
	public partial class DiscoverWindow : Window {
		private class TvFileItem {
			public string File { get; set; }
			public TvMatch Match { get; set; }
			public string Message { get; set; }
		}

		public DiscoverWindow() {
			InitializeComponent();

			FetchFiles();
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

			foreach(var f in root.GetFiles()) {
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

				Action<string> report = (m) => {
					item.Message = m;
					worker.ReportProgress(progress, i);
				};
				report("Searching TvDb");

				var results = tvdb.Search(item.Match.Name);

				if(!results.Any()) {
					report("Found no results");
					continue;
				}
				report("Found show, looking up episode");

				var series = tvdb.Series(results.First().Id);
				var episode = series.Episodes.SingleOrDefault(ep => ep.Season == item.Match.Season && ep.Number == item.Match.Episode);

				if(episode == null) {
					report("Couldn't find episode");
				}

				report(episode.Name);

			}

			e.Result = true;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			var item = filesListView.Items[(int)e.UserState] as TvFileItem;
			var container = filesListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			var status = container.FindControl<Label>("Status");
			status.Content = item.Message;
		}
	}
}
