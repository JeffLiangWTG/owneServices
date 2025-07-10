using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	public class MacroImageMenuGroupInitEventArgs : EventArgs
	{
		public MacroImageMenuGroup Menus;
	}

	public class UploadLocalImageEventArgs : EventArgs
	{
		public IOpenFileDialog Dialog;
	}

	public interface IOpenFileDialog : IDisposable
	{
		string Filter { get; set; }
		int FilterIndex { get; set; }
		bool RestoreDirectory { get; set; }
		string UnmappedFileName { get; }
		string FileName { get; set; }
		string InitialDirectory { get; set; }
		DialogResult ShowDialog(IWin32Window owner);
	}

	public class MacroImageMenuGroup : IMacroImageMenu
	{
		public string MenuText { get; }
		Type IMacroImageMenu.MacroImageMenuType { get { return typeof(MacroImageMenuGroup); } }
		public MacroImageMenuGroup(string menuText)
		{
			MenuText = menuText;
		}

		public MacroImageMenuGroup AddMenuGroup(string menuText)
		{
			var newGroup = new MacroImageMenuGroup(menuText);
			InternalMenus.Add(newGroup);
			return newGroup;
		}

		public MacroImageMenu AddMenu(string menuText, Image placeholderImage, string imageMacro, string imageText)
		{
			var newMenu = new MacroImageMenu(menuText, placeholderImage, imageMacro, imageText);
			InternalMenus.Add(newMenu);
			return newMenu;
		}

		readonly List<IMacroImageMenu> InternalMenus = new List<IMacroImageMenu>();

		public IEnumerable<IMacroImageMenu> Menus
		{
			get
			{
				return InternalMenus;
			}
		}
	}

	public class MacroImageMenu : IMacroImageMenu
	{
		public readonly Image PlaceholderImage;
		public readonly string Macro;
		public readonly string ImageText;

		public string MenuText { get; private set; }
		Type IMacroImageMenu.MacroImageMenuType { get { return typeof(MacroImageMenu); } }

		internal MacroImageMenu(string menuText, Image placeholderImage, string imageMacro, string imageText)
		{
			MenuText = menuText;
			PlaceholderImage = placeholderImage;
			Macro = imageMacro;
			ImageText = imageText;
		}
	}

	public interface IMacroImageMenu
	{
		Type MacroImageMenuType { get; }
		string MenuText { get; }
	}
}
