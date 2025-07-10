using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX101 : INXDeclaration
	{
		INX101Consignment Consignment { get; }

		IGoodsShipment GoodsShipment { get; }

		IEnumerable<IGovernmentProcedure> GovernmentProcedure { get; }

		IPackaging Packaging { get; }

		IPreviousDocument PreviousDocument { get; }

		new INX101Application Application { get; }

		IPartyDetails COImporter { get; }
	}

	public interface INX101Consignment
	{
		IEnumerable<IAdditionalDocument> AdditionalDocument { get; }

		INX101GovernmentAgencyGoodsItem GovernmentAgencyGoodsItem { get; }

		ILocation UnloadingLocation { get; }
	}

	public interface INX101GovernmentAgencyGoodsItem
	{
		IEnumerable<IPartyDetails> Manufacturers { get; }

		IEnumerable<IOrigin> Origins { get; }

		IEnumerable<IPreviousDocument> PreviousDocuments { get; }
	}

	public interface INX101Application
	{
		ZString AdhocCode { get; }

		ZString AdhocProcessNumber { get; }

		ZInt CopyQuantity { get; }

		ZString DescriptionTooLong { get; }

		ZString ECFAPrintingDescription { get; }

		ZString EUSteelDeclarationCode { get; }

		ZString EUSteelPhaseCode { get; }

		ZString FishingBoatName { get; }

		ZString FishingCONoExport { get; }

		ZString GoodsReleaseCode { get; }

		ZString GoodsReleaseReasonCode { get; }

		ZString ManufacturerPrintingCode { get; }

		ZString Observations { get; }

		ZInt OriginalCopyQuantity { get; }

		ZString PreviousCORenderCode { get; }

		ZString PrintingCode { get; }

		ZString TriangularTradeCode { get; }

		ZString TypeCode { get; }

		IPartyDetails Agent { get; }

		ZString ContactOffice { get; }

		IPartyDetails Applicant { get; }
	}
}
