#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.Customs.US.ISF.GUI
{
	public static class ExtensionMethods
	{
		public static IEnumerable<Control> Find(this Control control, Func<Control, bool> predicate)
		{
			var controls = control.Controls.Cast<Control>().Where(predicate);
			return controls.Union(control.Controls.Cast<Control>().SelectMany(c => c.Find(predicate)));
		}

		public static IEnumerable<T> FindAll<T>(this Control control, int maxLevelsDeep = -1)
		{
			var controls = control.Controls.OfType<T>();
			if (maxLevelsDeep-- != 0)
			{
				controls = controls.Union(control.Controls.Cast<Control>().SelectMany(c => c.FindAll<T>(maxLevelsDeep)));
			}

			return controls;
		}

		public static T FindParent<T>(this Control control)
			where T : Control
		{
			if (control.Parent == null)
			{
				return null;
			}
			else
			{
				var parent = control.Parent as T;
				if (parent != null)
				{
					return parent;
				}
				else
				{
					return control.Parent.FindParent<T>();
				}
			}
		}

		public static Control FindFocusedControl(Control control)
		{
			var container = control as ContainerControl;
			return container != null && container.ActiveControl != null ? FindFocusedControl(container.ActiveControl) : control;
		}

		public static IEnumerable<Control> GetAllFocusedControlsUpTheTree(Control control)
		{
			var container = control as ContainerControl;
			if (container != null)
			{
				yield return container;
			}

			if (container != null && container.ActiveControl != null)
			{
				foreach (var item in GetAllFocusedControlsUpTheTree(container.ActiveControl))
				{
					yield return item;
				}
			}
		}

		public static void PerformClickCF(this Control control)
		{
			var onClickMethodInfo = control.GetType().GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);
			onClickMethodInfo.Invoke(control, new object[] { EventArgs.Empty });
		}
	}
}

#endif