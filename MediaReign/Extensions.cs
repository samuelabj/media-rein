using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaReign {
	public static class Extensions {

		public static T FindControl<T>(this DependencyObject source, string name) where T : DependencyObject {
			if(source is T && ((Control)source).Name == name) return source as T;

			for(var i = 0; i < VisualTreeHelper.GetChildrenCount(source); i++) {
				var result = FindControl<T>(VisualTreeHelper.GetChild(source, i), name);
				if(result != null) return result as T;
			}

			return null;
		}
	}
}
