using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	sealed class ManifestBillFilterStripControlForTest : ManifestBillFilterStripControl
	{
		public ManifestBillFilterStripControlForTest(BusinessObjectFactory factory) : base(new ManifestBillModuleCollection(factory), new ManifestBillFilterStrip())
		{
		}

		public new ZGrid grid => base.grid;
	}
}
