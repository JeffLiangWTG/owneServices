using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonInvoiceLineDetailsControlBag : ControlBag
	{
		public static CommonInvoiceLineDetailsControlBag Instance => instance ?? (instance = new CommonInvoiceLineDetailsControlBag());

		[ThreadStatic]
		static CommonInvoiceLineDetailsControlBag instance;

		CommonInvoiceLineDetailsControlBag()
		{
			EntryInstructionGuidDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.EntryInstructionGuidDropEdit));
			PartNoCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.PartNoCodeFindBox));
			ProcedureCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ProcedureCodeFindBox));
			DescriptionLongTextControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.DescriptionLongTextControl));
			CountryOfOriginCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CountryOfOriginCodeFindBox));
			CountryOfOriginDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CountryOfOriginDropEdit));
			PrimaryPreferenceDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.PrimaryPreferenceDropEdit));
			WithDescriptionPrimaryPreferenceDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.WithDescriptionPrimaryPreferenceDropEdit));
			CommodityCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CommodityCodeFindBox));
			TariffFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.TariffFindBox));
			WithDescriptionTariffFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.WithDescriptionTariffFindBox));
			FormattedWithDescriptionTariffFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.FormattedWithDescriptionTariffFindBox));

			InvoiceQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.InvoiceQuantityCalcDropEdit));
			CustomsQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CustomsQuantityCalcDropEdit));
			CustomsSecondQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CustomsSecondQuantityCalcDropEdit));
			CustomsThirdQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CustomsThirdQuantityCalcDropEdit));
			CustomsFourthQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CustomsFourthQuantityCalcDropEdit));
			CustomsFifthQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CustomsFifthQuantityCalcDropEdit));
			LinePriceCurrencyCalcFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.LinePriceCurrencyCalcFindBox));
			TaxTypeDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.TaxTypeDropEdit));
			WeightCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.WeightCalcDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.VolumeCalcDropEdit));

			OrderNumberTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.OrderNumberTextBox));
			NetWeightCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.NetWeightCalcDropEdit));
			BondedWhsQuantityCalcDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.BondedWhsQuantityCalcDropEdit));
			ConcessionOrderTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ConcessionOrderTextBox));
			HazMatCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.HazMatCodeFindBox));
			HazMatCodeQualifierDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.HazMatCodeQualifierDropEdit));
			ExtraInfoForClassificationTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ExtraInfoForClassificationTextBox));
			ContainerModeDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ContainerModeDropEdit));

			CountryOfExportCodeFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.CountryOfExportCodeFindBox));
			SecondaryPreferenceTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.SecondaryPreferenceTextBox));
			RelatedIndicatorDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.RelatedIndicatorDropEdit));
			ValuationCodeDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ValuationCodeDropEdit));
			ValuationMarkupCalcEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ValuationMarkupCalcEdit));
			UnicodeDescriptionLongTextControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.UnicodeDescriptionLongTextControl));
			BrandNameTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.BrandNameTextBox));
			ModelTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ModelTextBox));

			OriginStateDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.OriginStateDropEdit));
			ManufacturerAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ManufacturerAddressControl));
			SoldToPartyAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.SoldToPartyAddressControl));
			ExporterAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ExporterAddressControl));
			ConsigneeAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ConsigneeAddressControl));
			SellerAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.SellerAddressControl));
			ShipToPartyAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ShipToPartyAddressControl));
			GrowerAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.GrowerAddressControl));

			ProducerAddressControl = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ProducerAddressControl));
			SupplierOrganisationFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.SupplierOrganisationFindBox));
			TreatmentProviderOrganisationFindBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.TreatmentProviderOrganisationFindBox));

			PreviousEntryNumberTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.PreviousEntryNumberTextBox));
			PreviousEntryLineNumberCalcEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.PreviousEntryLineNumberCalcEdit));
			StateOrRegionOfOriginDropEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.StateOrRegionOfOriginDropEdit));

			ValuationDateDateEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.ValuationDateDateEdit));
			DateForDutyDateEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.DateForDutyDateEdit));

			BondedWHSOrderLineNumberCalcEdit = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.BondedWHSOrderLineNumberCalcEdit));
			BondedWHSOrderNumberTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.BondedWHSOrderNumberTextBox));

			SerialNumberTextBox = RegisterControl(nameof(CommonInvoiceLineDetailsUserControl.SerialNumberTextBox));
		}

		protected override Control CreateTemplate() => new CommonInvoiceLineDetailsUserControl();

		public ControlReference EntryInstructionGuidDropEdit { get; }
		public ControlReference PartNoCodeFindBox { get; }
		public ControlReference ProcedureCodeFindBox { get; }
		public ControlReference TariffFindBox { get; }
		public ControlReference WithDescriptionTariffFindBox { get; }
		public ControlReference FormattedWithDescriptionTariffFindBox { get; }
		public ControlReference DescriptionLongTextControl { get; }
		public ControlReference CountryOfOriginCodeFindBox { get; }
		public ControlReference PrimaryPreferenceDropEdit { get; }
		public ControlReference WithDescriptionPrimaryPreferenceDropEdit { get; }
		public ControlReference CommodityCodeFindBox { get; }

		public ControlReference InvoiceQuantityCalcDropEdit { get; }
		public ControlReference CustomsQuantityCalcDropEdit { get; }
		public ControlReference CustomsSecondQuantityCalcDropEdit { get; }
		public ControlReference CustomsThirdQuantityCalcDropEdit { get; }
		public ControlReference CustomsFourthQuantityCalcDropEdit { get; }
		public ControlReference CustomsFifthQuantityCalcDropEdit { get; }
		public ControlReference LinePriceCurrencyCalcFindBox { get; }
		public ControlReference TaxTypeDropEdit { get; }
		public ControlReference WeightCalcDropEdit { get; }
		public ControlReference VolumeCalcDropEdit { get; }

		public ControlReference OrderNumberTextBox { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference BondedWhsQuantityCalcDropEdit { get; }
		public ControlReference ConcessionOrderTextBox { get; }
		public ControlReference HazMatCodeFindBox { get; }
		public ControlReference HazMatCodeQualifierDropEdit { get; }
		public ControlReference ExtraInfoForClassificationTextBox { get; }
		public ControlReference ContainerModeDropEdit { get; }

		public ControlReference CountryOfExportCodeFindBox { get; }
		public ControlReference SecondaryPreferenceTextBox { get; }
		public ControlReference RelatedIndicatorDropEdit { get; }
		public ControlReference ValuationCodeDropEdit { get; }
		public ControlReference ValuationMarkupCalcEdit { get; }
		public ControlReference UnicodeDescriptionLongTextControl { get; }
		public ControlReference BrandNameTextBox { get; }
		public ControlReference ModelTextBox { get; }

		public ControlReference OriginStateDropEdit { get; }
		public ControlReference ManufacturerAddressControl { get; }
		public ControlReference SoldToPartyAddressControl { get; }
		public ControlReference ExporterAddressControl { get; }
		public ControlReference ConsigneeAddressControl { get; }
		public ControlReference SellerAddressControl { get; }
		public ControlReference ShipToPartyAddressControl { get; }
		public ControlReference GrowerAddressControl { get; }

		public ControlReference ProducerAddressControl { get; }
		public ControlReference SupplierOrganisationFindBox { get; }
		public ControlReference TreatmentProviderOrganisationFindBox { get; }

		public ControlReference PreviousEntryNumberTextBox { get; }
		public ControlReference PreviousEntryLineNumberCalcEdit { get; }
		public ControlReference StateOrRegionOfOriginDropEdit { get; }

		public ControlReference ValuationDateDateEdit { get; }
		public ControlReference DateForDutyDateEdit { get; }

		public ControlReference BondedWHSOrderLineNumberCalcEdit { get; }
		public ControlReference BondedWHSOrderNumberTextBox { get; }

		public ControlReference SerialNumberTextBox { get; }

		public ControlReference CountryOfOriginDropEdit { get; }
	}
}
