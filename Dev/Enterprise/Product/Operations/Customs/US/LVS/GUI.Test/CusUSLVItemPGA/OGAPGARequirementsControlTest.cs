using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(OGAPGARequirementsControl))]
	public sealed class OGAPGARequirementsControlTest : ZControlBaseTestCase<OGAPGARequirementsControl>
	{
		public void TestDataSourceType()
		{
			AssertEquals("Should bind to Enterprise.Customs.US.LVS.Business.CusUSLVItem", typeof(CusUSLVItem), Control.BindingSource.DataSourceType);
		}

		public void TestPGARequirementsTab()
		{
			AssertNoExceptionThrown("PGA requirements tab doesn't exist or control was renamed", () => _ = PGARequirementsTabPage);
			AssertEquals("PGA requirements tab caption should be 'PGA Requirements'", "PGA Requirements", PGARequirementsTabPage.Text);
			AssertNoExceptionThrown("PGA requirements grid doesn't exist or control was renamed", () => _ = PGARequirementsTabPageGrid);
			AssertEquals("Should bind to ItemPGAWrapperCollection", "ItemPGAWrapperCollection", PGARequirementsTabPageGrid.GetBindingMember());
		}

		protected override bool UsesControlDataBindings => false;

		protected override bool RequiresTypeDescriptor => false;

		BaseDeclarationTabPage PGARequirementsTabPage => Control.Controls.Find("pgaRequirementsTabPage", true).OfType<BaseDeclarationTabPage>().Single();

		ZGrid PGARequirementsTabPageGrid => PGARequirementsTabPage.Controls.Find("OGAPGARequirementsGrid", false).OfType<ZGrid>().Single();

		protected override DummyBusinessObject GetDummyForBinding()
		{
			return Factory.New<DummyConsignmentItemLine>();
		}
	}

	class DummyConsignmentItemLine : DummyBusinessObject
	{
		public DummyConsignmentItemLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			consignmentItemLine = factory.New<CusUSLVItem>();
		}
		readonly CusUSLVItem consignmentItemLine;

		public CusUSLVItemPGAWrapperCollection ItemPGAWrapperCollection => consignmentItemLine.ItemPGAWrapperCollection;
	}
}
