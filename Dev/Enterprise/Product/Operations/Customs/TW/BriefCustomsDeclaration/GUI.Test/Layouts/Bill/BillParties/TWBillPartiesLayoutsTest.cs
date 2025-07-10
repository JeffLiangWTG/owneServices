using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillPartiesLayouts))]
	sealed class TWBillPartiesLayoutsTest : LayoutsAbstractTest
	{
		public void TestMainGroupBoxText()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			manifest.AMA_ManifestType = TWManifestTypes.Codes.ImportLowValueDutiableGoods;
			manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			manifest.Bills.AddNew();

			using (var form = new BriefDeclarationForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>("billsAndPacksTabControl");
				var billPartiesTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>("billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billPartiesTabPage;
				var asycudaBillPartiesUserControl = billsAndPacksTabControl.FindSingle<AsycudaBillPartiesUserControl>("asycudaBillPartiesUserControl");
				var shipperAddressUserControl = asycudaBillPartiesUserControl.FindSingle<ShipperAddressUserControl>("ShipperAddressUserControl");
				var consigneeAddressUserControl = asycudaBillPartiesUserControl.FindSingle<ConsigneeAddressUserControl>("ConsigneeAddressUserControl");
				var shipperMainGroupBox = shipperAddressUserControl.FindSingle<ZGroupBox>("MainGroupBox");
				var consigneeMainGroupBox = consigneeAddressUserControl.FindSingle<ZGroupBox>("MainGroupBox");

				CombineAssertions(() =>
				{
					AssertEquals("shipperMainGroupBox.Text", "Supplier", shipperMainGroupBox.CaptionResourceString.Caption);
					AssertEquals("consigneeMainGroupBox.Text", "Consignee", consigneeMainGroupBox.CaptionResourceString.Caption);
				});

				manifest.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				CombineAssertions(() =>
				{
					AssertEquals("shipperMainGroupBox Text", "Exporter", shipperMainGroupBox.CaptionResourceString.Caption);
					AssertEquals("consigneeMainGroupBox Text", "Buyer", consigneeMainGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TWBillPartiesLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TWBillPartiesControlBag.Instance.ShipperAddressUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TWBillPartiesControlBag.Instance.ConsigneeAddressUserControl, ControlWidthClass.LongNoCaption);
			}
		}
	}
}
