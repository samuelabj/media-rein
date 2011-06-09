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
	public partial class FindSeriesWindow : Window {

		public FindSeriesWindow() {
			InitializeComponent();
		}

		private void findBtn_Click(object sender, RoutedEventArgs e) {
			var dialog = new Forms.FolderBrowserDialog();
			dialog.SelectedPath = @"\\candy-mountain\E\TV Shows\";

			if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				statusBar.Visibility = System.Windows.Visibility.Visible;

				var worker = new BackgroundWorker();
				worker.WorkerReportsProgress = true;
				worker.DoWork += new DoWorkEventHandler(Download_DoWork);
				worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
				
				worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, ex) => {
					statusBar.Visibility = System.Windows.Visibility.Hidden;
					var data = ex.Result as Dictionary<DirectoryInfo, LinkedList<TvDbSearchResult>>;
					foldersLst.ItemsSource = data;
				});

				worker.RunWorkerAsync(dialog.SelectedPath);
			}
		}

		void Download_DoWork(object sender, DoWorkEventArgs e) {
			var worker = sender as BackgroundWorker;
			var tvdb = new TvDbRequest("A1DA4CF74415C72E");
			var root = new DirectoryInfo(e.Argument as string);

			var i = 0;
			var dirs = root.GetDirectories().Take(1);
			var data = new Dictionary<DirectoryInfo, LinkedList<TvDbSearchResult>>();

			using(var db = DataHelper.Context()) {
				foreach(var dir in dirs) {
					if(!db.Series.Any(s => s.Path == dir.FullName)) {
						worker.ReportProgress((int)(i / (double)dirs.Count() * 100), "Downloading: " + dir.Name);

						LinkedList<TvDbSearchResult> results = new LinkedList<TvDbSearchResult>();
						try {
							results = tvdb.Search(dir.Name, "en");
						} catch { }
						data.Add(dir, results);				
					}
					i++;
				}
			}

			e.Result = data;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			progressBar.Value = e.ProgressPercentage;
			statusTb.Text = e.UserState as string;
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
						var sr = new Series {
							Name = result.Name,
							Path = dir.FullName,
							TvDbId = result.Id
						};
						series.Add(sr);
					}
				}

				db.Series.InsertAllOnSubmit(series);
				db.SubmitChanges();
				MessageBox.Show("Added " + series.Count);

				var find = new FindFilesWindow();
				find.Show();
				this.Close();
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
