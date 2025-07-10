using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PE.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("78A57A89-1979-4204-A57C-C372C8AC61DA", "PE Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			//TODO: Build menu as required
			return Array.Empty<ZMenuItem>();
		}
	}
}
