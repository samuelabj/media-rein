using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TvDb {
	internal static class TvDbExtensions {

		public static T Get<T>(this XElement parent, string name) {
			var el = parent.Element(name);
			if(el == null || el.Value == "") return default(T);

			return (T)Convert.ChangeType(el.Value, typeof(T));
		}
	}
}
