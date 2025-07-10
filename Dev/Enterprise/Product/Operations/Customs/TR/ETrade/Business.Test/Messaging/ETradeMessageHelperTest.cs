using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business.Testing.Messaging
{
	class ETradeMessageHelperTest : TestCaseWithFactory
	{
		public void TestVehicleTransportType()
		{
			CombineAssertions(() =>
			{
				ZString emptyVehicleType = ETradeMessageHelper.VehicleTransportType(ZString.Empty, true);
				AssertEquals(ZString.Empty, emptyVehicleType);
				emptyVehicleType = ETradeMessageHelper.VehicleTransportType(ZString.Empty, false);
				AssertEquals(ZString.Empty, emptyVehicleType);

				ZString airModeVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Air, true);
				AssertEquals("5", airModeVehicleType);
				ZString airModeNonVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Air, false);
				AssertEquals("40", airModeNonVehicleType);

				ZString seaModeVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Sea, true);
				AssertEquals("3", seaModeVehicleType);
				ZString seaModeNonVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Sea, false);
				AssertEquals("10", seaModeNonVehicleType);

				ZString roadModeVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Road, true);
				AssertEquals("4", roadModeVehicleType);
				ZString roadModeNonVehicleType = ETradeMessageHelper.VehicleTransportType(TransportTypeList.Codes.Road, false);
				AssertEquals("30", roadModeNonVehicleType);
			});
		}

		public void TestCountryCodeOfTRCustoms()
		{
			CombineAssertions(() =>
			{
				ZString trCountryCode = ETradeMessageHelper.CountryCodeOfTRCustoms(Factory, ZString.Empty, ZDateTime.UtcToday);
				AssertEquals(ZString.Empty, trCountryCode);

				trCountryCode = ETradeMessageHelper.CountryCodeOfTRCustoms(Factory, "TR", ZDateTime.UtcToday);
				AssertEquals("052", trCountryCode);

				trCountryCode = ETradeMessageHelper.CountryCodeOfTRCustoms(Factory, "ES", ZDateTime.UtcToday);
				AssertEquals("011", trCountryCode);
			});
		}

		public void TestVerficationCode()
		{
			CombineAssertions(() =>
			{
				ZString verficationCode = ETradeMessageHelper.VerficationCode(ZString.Empty);
				AssertEquals("Y", verficationCode);

				verficationCode = ETradeMessageHelper.VerficationCode("XXX");
				AssertEquals("Y", verficationCode);

				verficationCode = ETradeMessageHelper.VerficationCode("EXS");
				AssertEquals("V", verficationCode);
			});
		}

		public void TestGetLastCredentials()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "KNZ";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";

			var outgoingMessage1 = CreateEdiMessage(TRMessageTypes.Codes.TRE, header);
			outgoingMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 13, 30, 38);
			outgoingMessage1.EM_MessageOwner = "";

			var outgoingMessage2 = CreateEdiMessage(TRMessageTypes.Codes.TRE, header);
			outgoingMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 25, 14, 02, 51);
			outgoingMessage2.EM_MessageOwner = "KNZ";

			var outgoingMessage3 = CreateEdiMessage(TRMessageTypes.Codes.TRE);
			outgoingMessage3.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 26, 15, 43, 34);
			outgoingMessage3.EM_MessageOwner = "";

			IETradeQueryForInspectionClerk provider = new ETradeQueryForInspectionClerkMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertEquals("UserName", "20201224104", provider.UserName);
				AssertEquals("UserPassword", "25d55ad283aa400af464c76d713c07ad", provider.UserPassword);
				AssertEquals(2, header.Messages.Count);
			});
		}

		EDIMessage CreateEdiMessage(string messageType, AsycudaManifestHeader header = null)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_IsTestMessage = true;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			if (header != null)
			{
				message.EM_LinkedObject = header;
				message.EM_LinkUniqueID = header.PK;
				message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			}
			return message;
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "TR", "052", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "ES", "011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			Factory.Save();
		}
	}
}
