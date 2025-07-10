using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NCTSHeaderProviderTest : TestCaseWithFactory
	{
		[TestDate(2012, 12, 21, 12, 22, 35)]
		public void TestNCTSHeaderInterfaceMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				CombineAssertions("Ncts header level with no tags", () =>
				{
					AssertEquals("SyntaxIdentifier", NCTSMessageProviderConstants.NCTSHeader.SyntaxIdentifier, nctsHeaderProvider.SyntaxIdentifier);
					AssertEquals("SyntaxVersionNumber", NCTSMessageProviderConstants.NCTSHeader.SyntaxVersionNumber, nctsHeaderProvider.SyntaxVersionNumber);
					AssertEquals("MessageSender", NCTSMessageProviderConstants.Messages.MessageSender, nctsHeaderProvider.MessageSender);
					AssertEquals("MessageRecipient", NCTSMessageProviderConstants.Messages.MessageRecipient, nctsHeaderProvider.MessageRecipient);
					AssertEquals("DateOfPreparation", "121221", nctsHeaderProvider.DateOfPreparation);
					AssertEquals("TimeOfPreparation", 1222, nctsHeaderProvider.TimeOfPreparation);
					AssertEquals("InterchangeControlReference", "202200000231", nctsHeaderProvider.InterchangeControlReference);
					AssertEquals("AcknowledgementRequest", ZInt.Zero, nctsHeaderProvider.AcknowledgementRequest);
					AssertEquals("TestIndicator", ZInt.Zero, nctsHeaderProvider.TestIndicator);
					AssertEquals("MessageIdentification", "202200000231LPK", nctsHeaderProvider.MessageIdentification);
					AssertEquals("MessageType", NCTSMessageProviderConstants.Messages.MessageType, nctsHeaderProvider.MessageType);
					AssertEquals("CommunicationsAgreementId", NCTSMessageProviderConstants.NCTSHeader.ZeroValue, nctsHeaderProvider.CommunicationsAgreementId);
					AssertNotNull("Principal", nctsHeaderProvider.Principal);
					AssertNotNull("Consignor", nctsHeaderProvider.Consignor);
					AssertNotNull("Consignee", nctsHeaderProvider.Consignee);
					AssertNotNull("Carrier", nctsHeaderProvider.Carrier);
					AssertEquals("CustomsOfficeDepartureCode", "TR066666", nctsHeaderProvider.CustomsOfficeDepartureCode);
					AssertEquals("CustomsOfficeDestinationCode", "TR350300", nctsHeaderProvider.CustomsOfficeDestinationCode);
					AssertEquals("SignatureStaffName", "ilker pakten", nctsHeaderProvider.SignatureStaffName);
				});

				CombineAssertions("Ncts header level with HEAHEA tags", () =>
				{
					AssertEquals("ReferenceNumber", 2, nctsHeaderProvider.ReferenceNumber);
					AssertEquals("TypeOfDeclaration", "A", nctsHeaderProvider.TypeOfDeclaration);
					AssertEquals("DestinationCountryCode", "TR", nctsHeaderProvider.DestinationCountryCode);
					AssertEquals("AgreedLocationOfGoodsCode", "CCC", nctsHeaderProvider.AgreedLocationOfGoodsCode);
					AssertEquals("AgreedLocationOfGoods", "DDD", nctsHeaderProvider.AgreedLocationOfGoods);
					AssertEquals("AgreedLocationOfGoodsLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.AgreedLocationOfGoodsLNG);
					AssertEquals("AuthorisedLocationOfGoodsCode", "2", nctsHeaderProvider.AuthorisedLocationOfGoodsCode);
					AssertEquals("PlaceOfLoadingCode", "ISTANBUL", nctsHeaderProvider.PlaceOfLoadingCode);
					AssertEquals("CountryOfDispatchExportCode", "TR", nctsHeaderProvider.CountryOfDispatchExportCode);
					AssertEquals("CustomsSubPlace", "12345678901234567", nctsHeaderProvider.CustomsSubPlace);
					AssertEquals("InlandTransportMode", "30", nctsHeaderProvider.InlandTransportMode);
					AssertEquals("TransportModeAtBorder", "20", nctsHeaderProvider.TransportModeAtBorder);
					AssertEquals("IdentityOfMeansOfTransportAtDeparture", "EEE", nctsHeaderProvider.IdentityOfMeansOfTransportAtDeparture);
					AssertEquals("IdentityOfMeansOfTransportAtDepartureLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.IdentityOfMeansOfTransportAtDepartureLNG);
					AssertEquals("NationalityOfMeansOfTransportAtDeparture", "GB", nctsHeaderProvider.NationalityOfMeansOfTransportAtDeparture);
					AssertEquals("IdentityOfMeansOfTransportCrossingBorder", "YGT", nctsHeaderProvider.IdentityOfMeansOfTransportCrossingBorder);
					AssertEquals("IdentityOfMeansOfTransportCrossingBorderLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.IdentityOfMeansOfTransportCrossingBorderLNG);
					AssertEquals("NationalityofMeansOfTransportCrossingBorder", "L", nctsHeaderProvider.NationalityofMeansOfTransportCrossingBorder);
					AssertEquals("TypeOfMeansOfTransportCrossingBorder", "20", nctsHeaderProvider.TypeOfMeansOfTransportCrossingBorder);
					AssertEquals("ContainerisedIndicator", "1", nctsHeaderProvider.ContainerisedIndicator);
					AssertEquals("DialogLanguageIndicatorAtDeparture", Core.Constants.CountryCodes.Turkey, nctsHeaderProvider.DialogLanguageIndicatorAtDeparture);
					AssertEquals("DialogLanguageIndicatorAtDepartureLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.DialogLanguageIndicatorAtDepartureLNG);
					AssertEquals("TotalNumberofCusInBondCargoDescItems", 2, nctsHeaderProvider.TotalNumberofCusInBondCargoDescItems);
					AssertEquals("TotalNumberofPackages", 21, nctsHeaderProvider.TotalNumberofPackages);
					AssertEquals("TotalGrossMass", 250.67m, nctsHeaderProvider.TotalGrossMass);
					AssertEquals("DeclarationDate", "20121221", nctsHeaderProvider.DeclarationDate);
					AssertEquals("DeclarationPlace", "Istanbul", nctsHeaderProvider.DeclarationPlace);
					AssertEquals("DeclarationPlaceLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.DeclarationPlaceLNG);
					AssertEquals("TransportChargesOfMethodOfPayment", "X", nctsHeaderProvider.TransportChargesOfMethodOfPayment);
					AssertEquals("CommercialReferenceNumber", "additional text", nctsHeaderProvider.CommercialReferenceNumber);
					AssertEquals("SecurityEnable", "1", nctsHeaderProvider.SecurityEnable);
					AssertEquals("ConveyanceReferenceNumber", "4", nctsHeaderProvider.ConveyanceReferenceNumber);
					AssertEquals("PlaceOfUnloadingCode", "Test", nctsHeaderProvider.PlaceOfUnloadingCode);
					AssertEquals("PlaceOfUnloadingCodeLNG", TRMessageConstants.LanguageCode, nctsHeaderProvider.PlaceOfUnloadingCodeLNG);
					AssertEquals("TruckId2", "AB1234CD", nctsHeaderProvider.TruckId2);
					AssertEquals("TruckId3", "AB5678CD", nctsHeaderProvider.TruckId3);
					AssertEquals("StampTax", 150.55m, nctsHeaderProvider.StampTax);
					AssertEquals("ReferenceNumberCustomsData", "TR310100", nctsHeaderProvider.ReferenceNumberCustomsData);
					AssertEquals("Tanker", "0", nctsHeaderProvider.Tanker);
					AssertEquals("StampDutyStatus", "5", nctsHeaderProvider.StampDutyStatus);
					AssertEquals("StampDutyLedgerDate", ZDate.Today.ToString(), nctsHeaderProvider.StampDutyLedgerDate);
				});

				CombineAssertions("Ncts header level with guarantee tags", () =>
				{
					AssertEquals("BondCodeType", "D", nctsHeaderProvider.BondCodeType);
					AssertEquals("BondDescription", "DENIZ", nctsHeaderProvider.BondDescription);
					AssertEquals("AccCodREF6", "1234", nctsHeaderProvider.BondPinCode);
					AssertEquals("BondCurrencyCode", "TRY", nctsHeaderProvider.BondCurrencyCode);
					AssertEquals("BondAmount", 1.00m, nctsHeaderProvider.BondAmount);
				});

				CombineAssertions("Ncts header level other members with BS tags", () =>
				{
					AssertStartsWith("DeclarantName", "ULUKOM LOGISTICS", nctsHeaderProvider.DeclarantName);
					AssertLessThanOrEqualTo("DeclarantName Length", nctsHeaderProvider.DeclarantName.Length, 35);
					AssertStartsWith("DeclarantAddress", "1. TAŞOCAĞI CAD. BURÇ SK. ULUKOM İŞ", nctsHeaderProvider.DeclarantAddress);
					AssertLessThanOrEqualTo("DeclarantAddress Length", nctsHeaderProvider.DeclarantAddress.Length, 35);
					AssertEquals("DeclarantCity", "İSTANBUL", nctsHeaderProvider.DeclarantCity);
					AssertEquals("DeclarantVATID", "8890376405", nctsHeaderProvider.DeclarantVATID);
					AssertEquals("ConsultantTaxNo", "8890376405", nctsHeaderProvider.ConsultantTaxNo);
					AssertStartsWith("ConsultantName", "ULUKOM LOGISTICS", nctsHeaderProvider.ConsultantName);
					AssertLessThanOrEqualTo("ConsultantName Length", nctsHeaderProvider.ConsultantName.Length, 35);
				});

				header.MovementHeader.BM_LocationOfGoodsCode = string.Empty;
				header.MovementHeader.BM_InlandTransportMode = string.Empty;
				header.MovementHeader.BM_ExportTransportMode = string.Empty;
				header.MovementHeader.BM_TransportAtDeparture = string.Empty;
				header.MovementHeader.BM_TOLCarrierID = string.Empty;
				header.MovementHeader.BM_RL_NKForeignDestPort = string.Empty;
				header.MovementHeader.BM_PlaceOfUnloading = string.Empty;

				var headerContainers = header.DepartureHeaderContainers;
				headerContainers.AddNew();

				nctsHeaderProvider = new NCTSHeaderProvider(header);

				CombineAssertions("Ncts header level HEAHEA tags with conditions", () =>
				{
					AssertEquals("AgreedLocationOfGoodsLNG", string.Empty, nctsHeaderProvider.AgreedLocationOfGoodsLNG);
					AssertEquals("InlandTransportMode", string.Empty, nctsHeaderProvider.InlandTransportMode);
					AssertEquals("TransportModeAtBorder", string.Empty, nctsHeaderProvider.TransportModeAtBorder);
					AssertEquals("IdentityOfMeansOfTransportAtDepartureLNG", string.Empty, nctsHeaderProvider.IdentityOfMeansOfTransportAtDepartureLNG);
					AssertEquals("IdentityOfMeansOfTransportCrossingBorderLNG", string.Empty, nctsHeaderProvider.IdentityOfMeansOfTransportCrossingBorderLNG);
					AssertEquals("TypeOfMeansOfTransportCrossingBorder", string.Empty, nctsHeaderProvider.TypeOfMeansOfTransportCrossingBorder);
					AssertEquals("ContainerisedIndicator", "1", nctsHeaderProvider.ContainerisedIndicator);
					AssertEquals("DeclarationPlaceLNG", string.Empty, nctsHeaderProvider.DeclarationPlaceLNG);
					AssertEquals("PlaceOfUnloadingCodeLNG", string.Empty, nctsHeaderProvider.PlaceOfUnloadingCodeLNG);
				});
			}
		}

		public void TestMoveToFTZFields()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var movement = header.MovementHeader;
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				movement.BM_CustomsOfficeAtBorder = "CODE";
				movement.MoveToFTZ = true;

				AssertEquals("CHKGIK117 returns EVET when MoveToFTZ is true.", "EVET", nctsHeaderProvider.CHKGIK117);
				AssertEquals("LOCGIK117 returns BM_CustomsOfficeAtBorder when MoveToFTZ is true.", "CODE", nctsHeaderProvider.LOCGIK117);

				movement.MoveToFTZ = false;
				AssertEquals("CHKGIK117 returns EVET when MoveToFTZ is false.", "HAYIR", nctsHeaderProvider.CHKGIK117);
				AssertEquals("LOCGIK117 returns empty when MoveToFTZ is false.", string.Empty, nctsHeaderProvider.LOCGIK117);
			}
		}

		public void TestCustomsOfficeDepartureCode()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				header.CustomsOffices.RemoveAndDelete(header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfTransit));
				var nctsHeaderProvider = new NCTSHeaderProvider(header);
				AssertEquals("CustomsOfficeDepartureCode returns DepartureCustomsOfficeCode when TRA office missing.", "TR066666", nctsHeaderProvider.CustomsOfficeDepartureCode);
			}
		}
	}
}
