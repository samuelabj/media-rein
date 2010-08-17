using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaReign {
	public class DataHelper {
		public static string ConnectionString = "Data Source=" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\MediaReign.sdf;Persist Security Info=False";
		public static MediaReignDataContext Context() {
			return new MediaReignDataContext(ConnectionString);
		}
	}
}
