using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.NZ.Manifest.Business.AsycudaContainer;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			manifest.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifest.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			var bill1 = manifest.Bills.AddNew();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("ABL_UCRNumber IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_UCRNumber).IsUnavailable);
					AssertEquals("CustomsJobNumber IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.CustomsJobNumber).IsUnavailable);
					AssertEquals("ABL_PrepaidCollect IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_PrepaidCollect).IsUnavailable);
					AssertEquals("ABL_CarrierReference IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_CarrierReference).IsUnavailable);
					AssertEquals("ABL_CustomsValue IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_CustomsValue).IsUnavailable);
					AssertEquals("ABL_RX_NKCustomsValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).IsUnavailable);
					AssertEquals("DiscountValue IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.DiscountValue).IsUnavailable);
					AssertEquals("DiscountValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.DiscountValueCurrency).IsUnavailable);
					AssertEquals("ABL_FreightValue IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
					AssertEquals("ABL_RX_NKFreightValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
					AssertEquals("ABL_TransportValue IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue).IsUnavailable);
					AssertEquals("ABL_RX_NKTransportValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).IsUnavailable);
					AssertEquals("ABL_InsuranceValue IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_InsuranceValue).IsUnavailable);
					AssertEquals("ABL_RX_NKInsuranceValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsUnavailable);
					AssertEquals("OtherChargesValue IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.OtherChargesValue).IsUnavailable);
					AssertEquals("OtherChargesValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.OtherChargesValueCurrency).IsUnavailable);
					AssertEquals("ABL_CargoStatus IsUnavailable", true, billsGrid.GetColumnStyle(ManifestBase.AutoAsycudaBill.Schema.ABL_CargoStatus).IsUnavailable);
					AssertEquals("CustomsEntryNumber IsAvailable", false, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.CustomsEntryNumber).IsUnavailable);
				});
				manifest.AMA_ManifestType = NZManifestTypes.Codes.ICR;
				AssertEquals("CustomsEntryNumber IsUnavailable", true, billsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaBill.Schema.CustomsEntryNumber).IsUnavailable);
			}
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

		protected override Type ExpectedContainerCountrySpecificUserControlType => typeof(NZContainerSpecificUserControl);

		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[] { typeof(NZManifestSpecificUserControl) };

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(NZBillLayouts);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			return header;
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			CombineAssertions(() =>
			{
				AssertEquals("NZ additional Container columns", 11, columnInfos.Length);
				foreach (var columnName in new[] { AsycudaContainer.Schema.SendMCDInformation, AsycudaContainer.Schema.HasMPIQD, AsycudaContainer.Schema.IsContainerClean, AsycudaContainer.Schema.IsPackingContaminated, AsycudaContainer.Schema.IsWoodPackingUsed, AsycudaContainer.Schema.IsWoodPackingTreated, AsycudaContainer.Schema.HasWoodPackingTreatmentCert })
				{
					AssertEquals($"{columnName} has MPI Quarantine Declaration Grouping", "199FE51D-C695-4E75-B652-13AD2DE78C1F", columnInfos.Single(x => x.ColumnName == columnName).GroupName.Key);
				}

				AssertEquals("PackLocationOrgPK is part of the Pack Location Group", "7086F5DA-21C9-4A52-B6EA-6BB94DAB81C1", columnInfos.Single(x => x.ColumnName == AsycudaContainer.Schema.PackLocationOrgPK).GroupName.Key);
				var packLocationAddress = columnInfos.Single(x => x.ColumnName == ManifestBase.AutoAsycudaContainer.Schema.ACN_OA_PackLocation);
				AssertEquals("PackLocation Address is part of the Pack Location Group", "7086F5DA-21C9-4A52-B6EA-6BB94DAB81C1", packLocationAddress.GroupName.Key);
				AssertEquals("PackLocation Address is upper case", System.Windows.Forms.CharacterCasing.Upper, packLocationAddress.CharacterCasing);
				AssertEquals("DeliveryDestinationOrgPK is part of the Delivery Destination Group", "9638DF2B-F880-4098-8463-F129BF0A98B6", columnInfos.Single(x => x.ColumnName == AsycudaContainer.Schema.DeliveryDestinationOrgPK).GroupName.Key);
				var deliveryDestinationAddress = columnInfos.Single(x => x.ColumnName == AsycudaContainer.Schema.DeliveryDestination);
				AssertEquals("DeliveryDestination Address is part of the Delivery Destination Group", "9638DF2B-F880-4098-8463-F129BF0A98B6", deliveryDestinationAddress.GroupName.Key);
				AssertEquals("DeliveryDestination Address is upper case", System.Windows.Forms.CharacterCasing.Upper, deliveryDestinationAddress.CharacterCasing);
			});
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertEquals(1, columnsVisiblility.Count);
			AssertContainsExactElementsInExactOrder(new[] { ASYCUDA.Business.AsycudaBill.Schema.CustomsEntryNumber }, columnsVisiblility.Keys);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] { "CustomsEntryNumber" }, columnInfos.Select(s => s.ColumnName));
		}
	}
}
