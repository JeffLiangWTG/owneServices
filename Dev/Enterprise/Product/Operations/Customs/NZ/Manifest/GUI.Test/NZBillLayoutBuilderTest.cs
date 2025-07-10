using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	[TestedType(typeof(NZBillLayoutBuilder))]
	sealed class NZBillLayoutBuilderTest : BillLayoutBuilderAbstractTest<NZBillLayoutBuilder, AsycudaBill>
	{
		public void TestControlVisibility_ManifestTypeICR()
		{
			AssertControlVisibility_ForManifestType(NZManifestTypes.Codes.ICR);
		}

		public void TestControlVisibility_ManifestTypeOCR()
		{
			AssertControlVisibility_ForManifestType(NZManifestTypes.Codes.OCR);
		}

		protected override int ExpectedMaxColumns => 4;

		protected override NZBillLayoutBuilder GetColumnLayoutBuilderForTesting() => new NZBillLayoutBuilder();

		void AssertControlVisibility_ForManifestType(string manifestType)
		{
			var manifest = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, manifestType, ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine);
			manifest.AMA_ManifestType = manifestType;
			manifest.Bills.RemoveAndDeleteAll();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingle<AsycudaBillUserControl>("asycudaBillUserControl");
				var commonBag = new BillLayoutBuilder<AsycudaBill>().CommonBag;
				var customsEntryNumberTextBox = asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.CustomsEntryNumberTextBox.Name);
				var shipmentTypeDropEdit = asycudaBillUserControl.FindSingleOrDefault<Control>(commonBag.ShipmentTypeDropEdit.Name);
				if (manifestType == NZManifestTypes.Codes.ICR)
				{
					AssertEquals("CustomsEntryNumberTextBox visibility", false, customsEntryNumberTextBox.Visible);
					AssertEquals("ShipmentTypeDropEdit visibility", true, shipmentTypeDropEdit.Visible);
				}
				else if (manifestType == NZManifestTypes.Codes.OCR)
				{
					AssertEquals("CustomsEntryNumberTextBox visibility", true, customsEntryNumberTextBox.Visible);
					AssertEquals("ShipmentTypeDropEdit visibility", false, shipmentTypeDropEdit.Visible);
				}
			}
		}
	}
}
