using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	public class TestManifestCreator : TestObjectCreator
	{
		public TestManifestCreator(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			entryHeader.CH_BGMReference = NumberFountains.ECIManifestReferencePrefix + "01010101";
		}

		public TestManifestCreator(CusEntryHeader entryHeader, ZString referenceNumber)
			: base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			entryHeader.CH_BGMReference = NumberFountains.ECIManifestReferencePrefix + referenceNumber;
		}

		public TestManifestCreator(CusEntryHeader entryHeader, ZString masterBill, ZString flightNo, ZString loading, ZString discharge, ZDateTime aTD, ZDateTime aTA)
			: this(entryHeader)
		{
			manifestMasterBill = masterBill;
			manifestFlightNo = flightNo;
			manifestCarrier = Carrier;
			manifestLoading = loading;
			manifestDischarge = discharge;
			manifestATD = aTD;
			manifestATA = aTA;
		}

		public TestManifestCreator(CusEntryHeader entryHeader, ZString masterBill, ZString flightNo, ZString loading, ZString discharge, ZDateTime aTD, ZDateTime aTA, ZString referenceNumber)
			: this(entryHeader, referenceNumber)
		{
			manifestMasterBill = masterBill;
			manifestFlightNo = flightNo;
			manifestCarrier = Carrier;
			manifestLoading = loading;
			manifestDischarge = discharge;
			manifestATD = aTD;
			manifestATA = aTA;
		}
		readonly CusEntryHeader entryHeader;

		readonly ZString manifestMasterBill;
		readonly ZString manifestFlightNo;
		readonly OrgHeader manifestCarrier;
		readonly ZString manifestLoading;
		readonly ZString manifestDischarge;
		readonly ZDateTime manifestATD;
		readonly ZDateTime manifestATA;

		public JobDeclaration AddDeclaration(ZString messageType, OrgHeader supplier, OrgHeader importer, ZString houseBill, ZString goodsDescription, ZDecimal weightInKG, ZInt packages, ZDecimal invoiceAmountInNZD)
		{
			return AddDeclaration(messageType, supplier, importer, houseBill, manifestLoading, manifestDischarge, manifestATD, manifestATA, goodsDescription, weightInKG, packages, invoiceAmountInNZD);
		}

		public JobDeclaration AddDeclaration(ZString messageType, OrgHeader supplier, OrgHeader importer, ZString houseBill, ZString origin, ZString destination, ZDateTime eTD, ZDateTime eTA,
			ZString goodsDescription, ZDecimal weightInKG, ZInt packages, ZDecimal invoiceAmountInNZD)
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			JobDeclaration declaration = AddDeclaration();

			// Message Header Type
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			// Manifest Info
			declaration.JE_MasterBill = manifestMasterBill;
			declaration.JE_VoyageFlightNo = manifestFlightNo;
			declaration.JE_OH_ShippingLine = manifestCarrier.PK;
			declaration.JE_RL_NKPortOfLoading = manifestLoading;
			declaration.JE_RL_NKPortOfArrival = manifestDischarge;
			declaration.JE_ExportDate = manifestATD;
			declaration.JE_DateOfArrival = manifestATA;

			// Declaration Info
			declaration.JE_HouseBill = houseBill;
			declaration.JE_RL_NKOrigin = origin;
			declaration.JE_RL_NKFinalDestination = destination;
			declaration.JE_DateAtOrigin = eTD;
			declaration.JE_DateAtFinalDestination = eTA;
			declaration.JE_GoodsDescription = goodsDescription;
			declaration.JE_TotalWeight = weightInKG;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_TotalNoOfPacks = packages;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_ECI_InvoiceAmount = invoiceAmountInNZD;
			declaration.JE_ECI_InvoiceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "NZD").PK;
			// Done Last to allow for setting of UNLOCOS from Orgs.
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			return declaration;
		}

		public JobDeclaration AddDeclaration()
		{
			JobDeclaration declaration = entryHeader.Declarations.AddNew();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.LinkToManifest(entryHeader, entryHeader.Declarations.Count);

			if (entryHeader.Declarations.Count == 1)
			{
				entryHeader.CH_JE = declaration.PK;
			}
			return declaration;
		}
	}
}
