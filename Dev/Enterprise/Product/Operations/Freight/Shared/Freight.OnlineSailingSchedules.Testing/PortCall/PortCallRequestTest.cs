using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(PortCallRequest))]
	public class PortCallRequestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestVoyageParameter()
		{
			var request = new PortCallRequest(Factory);
			request.Voyage = "ABC123";

			request.RequestType = PortCallRequestType.Load;
			AssertRequestString("VoyageNumberOut:ABC123", request);

			request.RequestType = PortCallRequestType.Discharge;
			AssertRequestString("VoyageNumberIn:ABC123", request);
		}

		public void TestReplaceEscapeCharacters()
		{
			var request = new PortCallRequest(Factory);
			request.CallSign = @"A\B*C""";

			AssertRequestString(@"Vessel.CallSign:A\\B\*C\""", request);
		}

		public void TestDateRangeParameter_Load()
		{
			var request = new PortCallRequest(Factory);
			request.RequestType = PortCallRequestType.Load;

			request.StartEstimatedTime = new ZDateTime(2017, 8, 12);
			request.EndEstimatedTime = ZDateTime.Empty;
			AssertRequestString(@"Etd:[2017-08-12 TO *]", request);

			request.StartEstimatedTime = ZDateTime.Empty;
			request.EndEstimatedTime = new ZDateTime(2017, 8, 20);
			AssertRequestString(@"Etd:[* TO 2017-08-20]", request);

			request.StartEstimatedTime = ZDateTime.Empty;
			request.EndEstimatedTime = ZDateTime.Empty;
			AssertRequestString(string.Empty, request);

			request.StartEstimatedTime = new ZDateTime(2017, 8, 12);
			request.EndEstimatedTime = new ZDateTime(2017, 8, 20);
			AssertRequestString(@"Etd:[2017-08-12 TO 2017-08-20]", request);
		}

		public void TestDateRangeParameter_Discharge()
		{
			var request = new PortCallRequest(Factory);
			request.RequestType = PortCallRequestType.Discharge;

			request.StartEstimatedTime = new ZDateTime(2017, 8, 12);
			request.EndEstimatedTime = ZDateTime.Empty;
			AssertRequestString(@"Eta:[2017-08-12 TO *]", request);

			request.StartEstimatedTime = ZDateTime.Empty;
			request.EndEstimatedTime = new ZDateTime(2017, 8, 20);
			AssertRequestString(@"Eta:[* TO 2017-08-20]", request);

			request.StartEstimatedTime = ZDateTime.Empty;
			request.EndEstimatedTime = ZDateTime.Empty;
			AssertRequestString(string.Empty, request);

			request.StartEstimatedTime = new ZDateTime(2017, 8, 12);
			request.EndEstimatedTime = new ZDateTime(2017, 8, 20);
			AssertRequestString(@"Eta:[2017-08-12 TO 2017-08-20]", request);
		}

		public void TestAddDoubleQuotationMarksForSpecialCharacters()
		{
			var request = new PortCallRequest(Factory);

			request.CallSign = @"A+B";
			AssertRequestString(@"Vessel.CallSign:""A+B""", request);

			request.CallSign = @"A-B";
			AssertRequestString(@"Vessel.CallSign:""A-B""", request);

			request.CallSign = @"A<B";
			AssertRequestString(@"Vessel.CallSign:""A<B""", request);

			request.CallSign = @"A>B";
			AssertRequestString(@"Vessel.CallSign:""A>B""", request);

			request.CallSign = @"A=B";
			AssertRequestString(@"Vessel.CallSign:""A=B""", request);

			request.CallSign = @"A(B";
			AssertRequestString(@"Vessel.CallSign:""A(B""", request);

			request.CallSign = @"A)B";
			AssertRequestString(@"Vessel.CallSign:""A)B""", request);

			request.CallSign = @"A!B";
			AssertRequestString(@"Vessel.CallSign:""A!B""", request);

			request.CallSign = @"A B";
			AssertRequestString(@"Vessel.CallSign:""A B""", request);

			request.CallSign = @"A+-<>=( )!B";
			AssertRequestString(@"Vessel.CallSign:""A+-<>=( )!B""", request);
		}

		public void TestCombineParameters()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "JON";
			var cusCode = carrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "SCAC";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = "CCC";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "GOINGMERRY";

			Factory.Save();

			var request = new PortCallRequest(Factory);
			request.RequestType = PortCallRequestType.Load;
			request.Port = "USLAC";
			request.CarrierPK = carrier.PK;
			request.VesselPK = vessel.PK;
			request.CallSign = "TOM";
			request.Voyage = "K569";
			request.IMO = "967666";
			request.StartEstimatedTime = new ZDateTime(2017, 8, 12);
			request.EndEstimatedTime = new ZDateTime(2017, 8, 20);

			AssertRequestString(@"Vessel.VesselName:GOINGMERRY AND Vessel.ImoNumber:967666 AND Vessel.CallSign:TOM AND Port.Unloco:USLAC AND Carrier.Code:SCAC AND VoyageNumberOut:K569 AND Etd:[2017-08-12 TO 2017-08-20]", request);
		}

		void AssertRequestString(ZString expected, PortCallRequest request)
		{
			AssertEquals(expected, request.GetRequestString(out var errorMessage));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortCallRequest(Factory);
		}

		#endregion
	}
}
