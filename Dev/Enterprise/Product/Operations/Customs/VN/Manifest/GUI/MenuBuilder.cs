using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.VN.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ZMenuItem[] BuildMenu()
		{
			return Array.Empty<ZMenuItem>();
		}

		public override ResourceString MenuCaption =>
			ResString.GetMultilingualString("9E3B5899-7A47-475A-BC22-F45CD14263E9", "VN Manifest");
	}
}
