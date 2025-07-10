using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingConsolidationModule))]
	public class DtbBookingConsolidationModuleTest : ZModuleBasherTest
	{
		public void TestFilterControl()
		{
			using (var module = new DtbBookingConsolidationModuleForTest())
			using (var filterControl = (DtbBookingConsolidationFilterControl)module.GetNewFilterControlForTest())
			{
				AssertEquals(true, filterControl.FilterBusinessObject is DtbBookingConsolidationFilterBusinessObject);
				AssertEquals(true, filterControl.GridCollection is DtbBookingMultiJobConsolidationCollection);
				filterControl.Dispose();
			}
		}

		public void TestGridCollection_HasFormStateOfBooking()
		{
			using (var module = new DtbBookingConsolidationModule())
			{
				AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(module.GridCollection.Factory));
			}
		}

		public void TestOnFactorySwapped()
		{
			using (var module = new DtbBookingConsolidationModule())
			{
				AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(module.GridCollection.Factory));

				((IFilterModuleInternalsForTesting)module).PerformSearch(); // Perform Search swaps Factory
				AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(module.GridCollection.Factory));
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = new DtbBookingConsolidationModuleForTest())
			{
				AssertEquals(Env.Licence.TransportBookings, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new DtbBookingConsolidationModuleForTest())
			{
				AssertEquals(Env.Security.DtbBookingConsolidation, module.SecurityCheckpoint);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = new DtbBookingConsolidationModuleForTest())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = new DtbBookingConsolidationModuleForTest())
			{
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("The 'DtbConsolidation' business context should be returned", BusinessContext.DtbConsolidation, module.BusinessContexts[0]);
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			Helper.CreateConsolidationMultiJob(Helper.CreateOrganisation("ORG1"));
			Helper.CreateConsolidationMultiJob(Helper.CreateOrganisation("ORG2"));
			Helper.CreateConsolidationMultiJob(Helper.CreateOrganisation("ORG3"));

			Factory.Save();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbBookingConsolidation;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
