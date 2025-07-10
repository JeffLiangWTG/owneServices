using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonInvoiceLineDetailsControlBag))]
	sealed class CommonInvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonInvoiceLineDetailsControlBag.EntryInstructionGuidDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.PartNoCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ProcedureCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.DescriptionLongTextControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CountryOfOriginCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CountryOfOriginDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.PrimaryPreferenceDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.WithDescriptionPrimaryPreferenceDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CommodityCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.TariffFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.WithDescriptionTariffFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.FormattedWithDescriptionTariffFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.InvoiceQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CustomsQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CustomsSecondQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CustomsThirdQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CustomsFourthQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CustomsFifthQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.LinePriceCurrencyCalcFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.TaxTypeDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.WeightCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.VolumeCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.OrderNumberTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.NetWeightCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.BondedWhsQuantityCalcDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ConcessionOrderTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.HazMatCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.HazMatCodeQualifierDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ExtraInfoForClassificationTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ContainerModeDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.CountryOfExportCodeFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.SecondaryPreferenceTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.RelatedIndicatorDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ValuationCodeDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ValuationMarkupCalcEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.UnicodeDescriptionLongTextControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.BrandNameTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ModelTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.OriginStateDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ManufacturerAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.SoldToPartyAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ExporterAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ConsigneeAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.SellerAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ShipToPartyAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.GrowerAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ProducerAddressControl);
				yield return nameof(CommonInvoiceLineDetailsControlBag.SupplierOrganisationFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.TreatmentProviderOrganisationFindBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.PreviousEntryNumberTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.PreviousEntryLineNumberCalcEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.StateOrRegionOfOriginDropEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.ValuationDateDateEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.DateForDutyDateEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.BondedWHSOrderNumberTextBox);
				yield return nameof(CommonInvoiceLineDetailsControlBag.BondedWHSOrderLineNumberCalcEdit);
				yield return nameof(CommonInvoiceLineDetailsControlBag.SerialNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonInvoiceLineDetailsControlBag.Instance;
	}
}
