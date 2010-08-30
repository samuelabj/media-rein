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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDb;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace MediaReign {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class FindSeries : Window {

		public FindSeries() {
			InitializeComponent();
		}

		private void findBtn_Click(object sender, RoutedEventArgs e) {
			var dialog = new Forms.FolderBrowserDialog();
			dialog.SelectedPath = @"\\candy-mountain\E\TV Shows\";

			if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				statusBar.Visibility = System.Windows.Visibility.Visible;

				var worker = new BackgroundWorker();
				worker.WorkerReportsProgress = true;
				worker.DoWork += new DoWorkEventHandler(worker_DoWork);
				worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
				worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
				worker.RunWorkerAsync(dialog.SelectedPath);
			}
		}

		void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			statusBar.Visibility = System.Windows.Visibility.Hidden;
			var data = e.Result as Dictionary<DirectoryInfo, LinkedList<TvDbSearchResult>>;

			foldersLst.ItemsSource = data;
		}

		void worker_DoWork(object sender, DoWorkEventArgs e) {
			var worker = sender as BackgroundWorker;
			var tvdb = new TvDbRequest("A1DA4CF74415C72E");
			var root = new DirectoryInfo(e.Argument as string);

			var i = 0;
			var dirs = root.GetDirectories();
			var data = new Dictionary<DirectoryInfo, LinkedList<TvDbSearchResult>>();

			using(var db = DataHelper.Context()) {
				foreach(var dir in dirs) {
					if(db.Series.Any(s => s.Path == dir.FullName)) continue;
					var results = tvdb.Search(dir.Name, "en");
					data.Add(dir, results);
					worker.ReportProgress((int)(i++ / (double)dirs.Count() * 100), dir.Name);
				}
			}

			e.Result = data;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			progressBar.Value = e.ProgressPercentage;
			statusTb.Text = "Downloading: " + e.UserState as string;
		}

		private void addBtn_Click(object sender, RoutedEventArgs e) {
			using(var db = DataHelper.Context()) {
				var series = new List<Series>();

				foreach(KeyValuePair<DirectoryInfo, LinkedList<TvDbSearchResult>> item in foldersLst.Items) {
					var container = foldersLst.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
					var combo = FindControl<ComboBox>(container);
					var result = combo.SelectedItem as TvDbSearchResult;
					var dir = item.Key;

					if(result != null) {
						IEnumerable<File> files;
						var seasons = dir.GetDirectories("Season*");
						if(seasons.Any()) { 
							files = (from sn in seasons
									select (from f in sn.GetFiles()
											where Settings.MediaExtensions.Contains(f.Extension)
											select new File {
												Path = f.FullName,
											}))
											.SelectMany(f => f);
						} else {
							files = from f in dir.GetFiles()
									where Settings.MediaExtensions.Contains(f.Extension)
									select new File {
										Path = f.FullName,
									};
						}

						var sr = new Series {
							Name = result.Name,
							Path = dir.FullName,
							TvDbId = result.Id
						};
						sr.Files.AddRange(files);

						series.Add(sr);
					}
				}

				db.Series.InsertAllOnSubmit(series);
				db.SubmitChanges();
				MessageBox.Show("Added " + db.Series.Count());
			}
		}

		private T FindControl<T>(DependencyObject obj) where T : DependencyObject {
			if(obj is T) return obj as T;

			for(var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
				var result = FindControl<T>(VisualTreeHelper.GetChild(obj, i));
				if(result != null) return result as T;
			}

			return null;
		}
	}
}
