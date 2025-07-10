using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class Commodity : ICommodity
	{
		public Commodity(CusInBondMoveLineItem moveLineItem)
		{
			this.moveLineItem = moveLineItem;
		}

		readonly CusInBondMoveLineItem moveLineItem;

		public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

		public ZString CommercialCategorizationID => ZString.Empty;

		public ZString Description => ZString.Empty;

		public ZString GoodsGroupNameCode => ZString.Empty;

		public ZString Name => ZString.Empty;

		public ZString BarCode => ZString.Empty;

		public ZString ChineseDescription => ZString.Empty;

		public ZString EnglishDescription => ZString.Empty;

		public ZString CITESImportPermitID => ZString.Empty;

		public ZString FTATariffCode => ZString.Empty;

		public ZString SHTCImportPermitID => ZString.Empty;

		public ZString TariffCodeExtensionCode => ZString.Empty;

		public IEnumerable<IClassification> Classifications => null;

		public ICommodityRelatedPackaging CommodityRelatedPackaging => null;

		public IConstituent Constituent => null;

		public ICommodityDutyTaxFee DutyTaxFee => null;

		public IGovernmentProcedure GovernmentProcedure => null;

		public IEnumerable<ZString> HandlingInstructionsCodes => null;

		public IInvoiceLine InvoiceLine => null;

		public IPreviousDocument PreviousDocument => null;

		public IEnumerable<ICommodityNumber> CommodityNumbers => null;

		public IEnumerable<IDutyOtherTaxFee> DutyOtherTaxFees => null;

		public IDutyTaxFeeAmount DutyTaxFeeAmount => null;

		public IDutyTaxFeeQuantity DutyTaxFeeQuantity => null;

		public IFood Food => null;

		public IQuarantine Quarantine => null;

		public IVehicle Vehicle => null;

		public IWine Wine => null;

		public ZString CargoDescription => moveLineItem?.BI_Description ?? ZString.Empty;

		public ZString BondedNoteCode => ZString.Empty;

		public IEnumerable<ZString> VehicleIDs => null;

		IClassification ICommodity.Classification => null;

		public IInvoice Invoice => null;

		public ZString PrintingTariffCode => null;
	}
}
