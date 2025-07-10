using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommonInvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestEntryInstructionGuidDropEdit()
		{
			AssertType<ZGuidDropEdit>(control.EntryInstructionGuidDropEdit);
		}

		public void TestPartNoCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.PartNoCodeFindBox);
		}

		public void TestProcedureCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ProcedureCodeFindBox);
		}

		public void TestDescriptionLongTextControl()
		{
			AssertType<LongTextControl>(control.DescriptionLongTextControl);
		}

		public void TestCountryOfOriginCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CountryOfOriginCodeFindBox);
		}

		public void TestCountryOfOriginDropEdit()
		{
			AssertType<ZDropEdit>(control.CountryOfOriginDropEdit);
		}

		public void TestPrimaryPreferenceDropEdit()
		{
			AssertType<ZDropEdit>(control.PrimaryPreferenceDropEdit);
		}

		public void TestCommodityCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CommodityCodeFindBox);
		}

		public void TestTariffFindBox()
		{
			AssertType<Universal.GUI.TariffFindBox>(control.TariffFindBox);
		}

		public void TestInvoiceQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.InvoiceQuantityCalcDropEdit);
		}

		public void TestCustomsQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.CustomsQuantityCalcDropEdit);
		}

		public void TestCustomsSecondQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.CustomsSecondQuantityCalcDropEdit);
		}

		public void TestCustomsThirdQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.CustomsThirdQuantityCalcDropEdit);
		}

		public void TestLinePriceCurrencyCalcFindBox()
		{
			AssertType<ZCalcFindBox>(control.LinePriceCurrencyCalcFindBox);
		}

		public void TestTaxTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.TaxTypeDropEdit);
		}

		public void TestWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.WeightCalcDropEdit);
		}

		public void TestVolumeCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.VolumeCalcDropEdit);
		}

		public void TestOrderNumberTextBox()
		{
			AssertType<ZTextBox>(control.OrderNumberTextBox);
		}

		public void TestNetWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.NetWeightCalcDropEdit);
		}

		public void TestBondedWhsQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.BondedWhsQuantityCalcDropEdit);
		}

		public void TestConcessionOrderTextBox()
		{
			AssertType<ZTextBox>(control.ConcessionOrderTextBox);
		}

		public void TestHazMatCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.HazMatCodeFindBox);
		}

		public void TestHazMatCodeQualifierDropEdit()
		{
			AssertType<ZDropEdit>(control.HazMatCodeQualifierDropEdit);
		}

		public void TestExtraInfoForClassificationTextBox()
		{
			AssertType<ZTextBox>(control.ExtraInfoForClassificationTextBox);
		}

		public void TestContainerModeDropEdit()
		{
			AssertType<ZDropEdit>(control.ContainerModeDropEdit);
		}

		public void TestCountryOfExportCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CountryOfExportCodeFindBox);
		}

		public void TestSecondaryPreferenceTextBox()
		{
			AssertType<ZTextBox>(control.SecondaryPreferenceTextBox);
		}

		public void TestRelatedIndicatorDropEdit()
		{
			AssertType<ZDropEdit>(control.RelatedIndicatorDropEdit);
		}

		public void TestValuationCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ValuationCodeDropEdit);
		}

		public void TestValuationMarkupCalcEdit()
		{
			AssertType<ZCalcEdit>(control.ValuationMarkupCalcEdit);
		}

		public void TestOriginStateDropEdit()
		{
			AssertType<ZDropEdit>(control.OriginStateDropEdit);
		}

		public void TestBrandNameTextBox()
		{
			AssertType<ZTextBox>(control.BrandNameTextBox);
		}

		public void TestModelTextBox()
		{
			AssertType<ZTextBox>(control.ModelTextBox);
		}

		public void TestUnicodeDescriptionLongTextControl()
		{
			AssertType<LongTextControl>(control.UnicodeDescriptionLongTextControl);
		}

		public void TestSupplierOrganisationFindBox()
		{
			AssertType<ZOrganisationFindBox>(control.SupplierOrganisationFindBox);
		}

		public void TestTreatmentProviderOrganisationFindBox()
		{
			AssertType<ZOrganisationFindBox>(control.TreatmentProviderOrganisationFindBox);
		}

		public void TestManufacturerAddressControl()
		{
			AssertType<ZAddressControl>(control.ManufacturerAddressControl);
		}

		public void TestSoldToPartyAddressControl()
		{
			AssertType<ZAddressControl>(control.SoldToPartyAddressControl);
		}

		public void TestExporterAddressControl()
		{
			AssertType<ZAddressControl>(control.ExporterAddressControl);
		}

		public void TestConsigneeAddressControl()
		{
			AssertType<ZAddressControl>(control.ConsigneeAddressControl);
		}

		public void TestSellerAddressControl()
		{
			AssertType<ZAddressControl>(control.SellerAddressControl);
		}

		public void TestShipToPartyAddressControl()
		{
			AssertType<ZAddressControl>(control.ShipToPartyAddressControl);
		}

		public void TestGrowerAddressControl()
		{
			AssertType<ZAddressControl>(control.GrowerAddressControl);
		}

		public void TestProducerAddressControl()
		{
			AssertType<ZAddressControl>(control.ProducerAddressControl);
		}

		public void TestWithDescriptionTariffFindBox()
		{
			AssertType<Universal.GUI.TariffFindBox>(control.WithDescriptionTariffFindBox);
		}

		public void TestFormattedWithDescriptionTariffFindBox()
		{
			AssertType<Universal.GUI.TariffFindBox>(control.FormattedWithDescriptionTariffFindBox);
		}

		public void TestWithDescriptionPrimaryPreferenceDropEdit()
		{
			AssertType<ZDropEdit>(control.WithDescriptionPrimaryPreferenceDropEdit);
		}

		public void TestPreviousEntryNumberTextBox()
		{
			AssertType<ZTextBox>(control.PreviousEntryNumberTextBox);
		}

		public void TestPreviousEntryLineNumberCalcEdit()
		{
			AssertType<ZCalcEdit>(control.PreviousEntryLineNumberCalcEdit);
		}

		public void TestStateOrRegionOfOriginDropEdit()
		{
			AssertType<ZDropEdit>(control.StateOrRegionOfOriginDropEdit);
		}

		public void TestCustomsFifthQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.CustomsFifthQuantityCalcDropEdit);
		}

		public void TestBondedWHSOrderLineNumberCalcEdit()
		{
			AssertType<ZCalcEdit>(control.BondedWHSOrderLineNumberCalcEdit);
		}

		public void TestBondedWHSOrderNumberTextBox()
		{
			AssertType<ZTextBox>(control.BondedWHSOrderNumberTextBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommonInvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CommonInvoiceLineDetailsUserControl control;
	}
}
