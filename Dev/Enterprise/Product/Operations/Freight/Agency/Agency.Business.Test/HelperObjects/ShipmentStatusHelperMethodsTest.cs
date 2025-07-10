using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ShipmentStatusHelperMethodsTest : TestCaseWithFactory
	{
		public void TestCountsTowardsAllocations_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				List<string> list = new List<string>();
				foreach (string status in ShipmentStatusHelperMethods.GetBookingStageStatus())
				{
					if (!ShipmentStatusHelperMethods.CountsTowardsAllocations(status))
					{
						list.Add(status);
					}
				}

				foreach (string status in ShipmentStatusHelperMethods.GetBillOfLadingStageStatus())
				{
					if (!ShipmentStatusHelperMethods.CountsTowardsAllocations(status))
					{
						list.Add(status);
					}
				}

				AssertContainsExactElementsInAnyOrder("Status that do NOT count towards allocations",
					new string[]
					{
					ShipmentStatusList.Codes.ElectronicBooking,
					ShipmentStatusList.Codes.EBookingCancellationRequest,
					ShipmentStatusList.Codes.BookingCancelled,
					ShipmentStatusList.Codes.BookingRejected,
					ShipmentStatusList.Codes.WebBooking,
					ShipmentStatusList.Codes.WaitListed
					},
					list);
			}
		}

		public void TestCountsTowardsAllocations_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				List<string> list = new List<string>();
				foreach (string status in ShipmentStatusHelperMethods.GetBookingStageStatus())
				{
					if (!ShipmentStatusHelperMethods.CountsTowardsAllocations(status))
					{
						list.Add(status);
					}
				}

				foreach (string status in ShipmentStatusHelperMethods.GetBillOfLadingStageStatus())
				{
					if (!ShipmentStatusHelperMethods.CountsTowardsAllocations(status))
					{
						list.Add(status);
					}
				}

				AssertContainsExactElementsInAnyOrder("Status that do NOT count towards allocations",
					new string[]
					{
					ShipmentStatusList.Codes.ElectronicBooking,
					ShipmentStatusList.Codes.WebBooking,
					ShipmentStatusList.Codes.WaitListed
					},
					list);
			}
		}

		public void TestIsBookingStage()
		{
			List<string> falsePositive = new List<string>();
			List<string> falseNegative = new List<string>();
			foreach (string status in ShipmentStatusHelperMethods.GetBookingStageStatus())
			{
				if (!ShipmentStatusHelperMethods.IsBookingStage(status))
				{
					falseNegative.Add(status);
				}
			}

			foreach (string status in ShipmentStatusHelperMethods.GetBillOfLadingStageStatus())
			{
				if (ShipmentStatusHelperMethods.IsBookingStage(status))
				{
					falsePositive.Add(status);
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (falseNegative.Count > 0)
			{
				results.Add("Status returning a false negative", falseNegative);
			}

			if (falsePositive.Count > 0)
			{
				results.Add("Status returning a false positive", falsePositive);
			}

			AssertGroupedErrorList(results);
		}

		public void TestIsBillOfLadingStage()
		{
			List<string> falsePositive = new List<string>();
			List<string> falseNegative = new List<string>();
			foreach (string status in ShipmentStatusHelperMethods.GetBookingStageStatus())
			{
				if (ShipmentStatusHelperMethods.IsBillOfLadingStage(status))
				{
					falseNegative.Add(status);
				}
			}

			foreach (string status in ShipmentStatusHelperMethods.GetBillOfLadingStageStatus())
			{
				if (!ShipmentStatusHelperMethods.IsBillOfLadingStage(status))
				{
					falsePositive.Add(status);
				}
			}

			Dictionary<string, IList<string>> results = new Dictionary<string, IList<string>>();
			if (falseNegative.Count > 0)
			{
				results.Add("Status returning a false negative", falseNegative);
			}

			if (falsePositive.Count > 0)
			{
				results.Add("Status returning a false positive", falsePositive);
			}

			AssertGroupedErrorList(results);
		}

		public void TestGetBookingStageStatus_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContainsExactElementsInAnyOrder("Match Booking Stage Statusses",
				new AgencyShipmentStatusList(false).GetAllCodes(),
				ShipmentStatusHelperMethods.GetBookingStageStatus());
			}
		}

		public void TestGetBookingStageStatus_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertContainsExactElementsInAnyOrder("Match Booking Stage Statusses",
				new AgencyShipmentStatusList(false).GetAllCodes(),
				ShipmentStatusHelperMethods.GetBookingStageStatus());
			}
		}

		public void TestGetBillOfLadingStageStatus_ElectronicBookingAndShippingInstructions_true()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				List<string> list = new List<string>();
				foreach (ICodeDescription pair in new AgencyShipmentStatusList(true))
				{
					list.Add(pair.Code);
				}

				AssertContainsExactElementsInAnyOrder(list, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
			}
		}

		public void TestGetBillOfLadingStageStatus_ElectronicBookingAndShippingInstructions_false()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				List<string> list = new List<string>();
				foreach (ICodeDescription pair in new AgencyShipmentStatusList(true))
				{
					list.Add(pair.Code);
				}

				AssertContainsExactElementsInAnyOrder(list, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
			}
		}
	}
}
