using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingRequest))]
	public class RoutingRequestTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestOriginUNLOCO()
		{
			RoutingRequest request = new RoutingRequest(Factory)
			{
				OriginUNLOCOCode = "AUSYD"
			};
			AssertEquals("AUSYD", request.OriginUNLOCO.RL_Code);

			request.OriginUNLOCOCode = "ZUBIN";
			AssertNull(request.OriginUNLOCO);
		}

		public void TestDestinationUNLOCO()
		{
			RoutingRequest request = new RoutingRequest(Factory)
			{
				DestinationUNLOCOCode = "AUSYD"
			};
			AssertEquals("AUSYD", request.DestinationUNLOCO.RL_Code);

			request.DestinationUNLOCOCode = "ZUBIN";
			AssertNull(request.DestinationUNLOCO);
		}

		#endregion

		#region Build Message

		[TestDate(2008, 12, 14, 9, 32, 0)]
		public void TestGenerateRequestString()
		{
			RoutingRequest request = new RoutingRequest(Factory)
			{
				OriginUNLOCOCode = "AUSYD",
				DestinationUNLOCOCode = "USLAX",
				DepartureDate = new ZDateTime(2008, 6, 11),
				AirlineCode = "QF",

				CodeShareInterlineOption = "X",
				MinimumConnectionTime = 3,
				CargoPassengerFlightOption = "C",
				EquipmentType = "W"
			};

			string expectedMessage = "R 08/12/14 09:32:00 (CargoWise One) SSIM SYD LAX 08/06/11 .X2.H...3CW QF . . . .";
			AssertEquals(expectedMessage, request.GenerateRequestString());

			request.CodeShareInterlineOption = "";
			request.MinimumConnectionTime = 0;
			request.CargoPassengerFlightOption = "";
			request.EquipmentType = "";
			request.AirlineCode = "";

			expectedMessage = "R 08/12/14 09:32:00 (CargoWise One) SSIM SYD LAX 08/06/11 ..2.H...0.. . . . . .";
			AssertEquals(expectedMessage, request.GenerateRequestString());

			request.IncludeWeeklyTimetable = true;
			expectedMessage = "R 08/12/14 09:32:00 (CargoWise One) SSIM SYD LAX 08/06/11 ..2.T...0.. . . . . .";
			AssertEquals(expectedMessage, request.GenerateRequestString());

			request.ConnectionsCount = "2";
			expectedMessage = "R 08/12/14 09:32:00 (CargoWise One) SSIM SYD LAX 08/06/11 ..2.T2..0.. . . . . .";
			AssertEquals(expectedMessage, request.GenerateRequestString());

			request.IncludeCO2EmissionValue = true;
			expectedMessage = "R 08/12/14 09:32:00 (CargoWise One) SSIM SYD LAX 08/06/11 ..2.T2..0.. . . . . . C";
			AssertEquals(expectedMessage, request.GenerateRequestString());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RoutingRequest(Factory);
		}

		#endregion
	}
}
