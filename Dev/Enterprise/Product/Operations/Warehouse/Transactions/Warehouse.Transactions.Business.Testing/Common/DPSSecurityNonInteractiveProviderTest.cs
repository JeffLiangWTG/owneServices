using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DPSSecurityNonInteractiveProviderTest : TestCaseWithFactory
	{
		public void TestDPSValidation_StatusMatched()
		{
			TestDPSValidationCore(ScreeningStatusesList.Codes.Matched, expectedResult: false);
		}

		public void TestDPSValidation_StatusClear()
		{
			TestDPSValidationCore(ScreeningStatusesList.Codes.Clear, expectedResult: true);
		}

		void TestDPSValidationCore(string screeningStatus, bool expectedResult)
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, "ALL"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50);
				receive.AllocateLocationsWithMock();
				receive.WD_ScreeningStatus = screeningStatus;

				var dpsValidation = new DPSSecurityNonInteractiveProvider();
				AssertEquals(expectedResult, dpsValidation.ValidateDPS(receive));
				Assert("There should be no error message.", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
