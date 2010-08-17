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
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace MediaReign {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void findBtn_Click(object sender, RoutedEventArgs e) {
			var dialog = new FolderBrowserDialog();
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

			foreach(var dir in data.Keys) {
				var results = data[dir];

				foldersLst.Items.Add(new {
					Folder = dir.Name,
					Results = results
				});
			}
		}

		void worker_DoWork(object sender, DoWorkEventArgs e) {
			var worker = sender as BackgroundWorker;
			var tvdb = new TvDbRequest("A1DA4CF74415C72E");
			var root = new DirectoryInfo(e.Argument as string);

			var i = 0;
			var dirs = root.GetDirectories();
			var data = new Dictionary<DirectoryInfo, LinkedList<TvDbSearchResult>>();

			foreach(var dir in dirs) {
				var results = tvdb.Search(dir.Name, "en");
				data.Add(dir, results);
				worker.ReportProgress((int)(i++ / (double)dirs.Count() * 100), dir.Name);
			}

			e.Result = data;
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			progressBar.Value = e.ProgressPercentage;
			statusTb.Text = "Downloading: " + e.UserState as string;
		}
	}
}
