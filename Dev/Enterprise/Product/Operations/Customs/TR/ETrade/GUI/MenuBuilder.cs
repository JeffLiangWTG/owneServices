using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1048")]
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("ACE852A4-B717-4DAA-ABE0-1F3AA4E95459", "TR E-Trade");

		public override ZMenuItem[] BuildMenu()
		{
			//TODO please build your 
			return System.Array.Empty<ZMenuItem>();
		}
	}
}
