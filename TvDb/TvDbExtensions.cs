using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel;

namespace TvDb {
	internal static class TvDbExtensions {

		public static T Get<T>(this XElement parent, string name) {
			var el = parent.Element(name);
			if(el == null || el.Value == "") return default(T);
			try {
				return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(el.Value);
			} catch {
				return default(T);
			}
		}

		public static T Get<T>(this XElement parent, string name, T defaultValue) {
			var val = Get<T>(parent, name);
			if(val.Equals(default(T))) return defaultValue;
			return val;
		}

		public static string Get(this XElement parent, string name) {
			return Get<string>(parent, name);
		}

		public static string[] Split(this XElement parent, string name) {
			var val = Get(parent, name, "");
			return val.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
