using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPortDeliveryTimeFilterBusinessObject))]
	sealed class GlbPortDeliveryTimeBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		ModuleNkFilter GetModuleNkFilter(ZString description)
		{
			return (ModuleNkFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestDischargeUNLOCO()
		{
			ModuleNkFilter filter = GetModuleNkFilter("Discharge Port");

			filter.Property = Port1.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);

			filter.Property = Port2.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			RefUNLOCO port3 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			filter.Property = port3.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);
		}

		public void TestDestinationUNLOCO()
		{
			ModuleNkFilter filter = GetModuleNkFilter("Destination Port");

			filter.Property = Port1.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = Port2.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			RefUNLOCO port3 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			filter.Property = port3.RL_Code;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);
		}

		ModuleGuidFilter GetModuleGuidFilter(ZString description)
		{
			return (ModuleGuidFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestHiddenCompanyFilter()
		{
			ModuleGuidFilter filter = GetModuleGuidFilter("Company");

			filter.Property = Env.CurrentCompany.PK;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			filter.Property = company.PK;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);
		}

		ModuleTextFilter GetModuleTextFilter(ZString description)
		{
			return (ModuleTextFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestFreightMode()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Freight Mode");

			filter.Property = Constants.TransportModes.Rail;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = Constants.TransportModes.Air;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = Constants.TransportModes.Sea;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);
		}

		public void TestJobMode()
		{
			ModuleTextFilter filter = GetModuleTextFilter("Job Mode");

			filter.Property = "CUS";
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);

			filter.Property = "FWD";
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionContains(Delivery01, Collection);
			AssertCollectionContains(Delivery02, Collection);

			filter.Property = "ALL";
			filter.IsActive = true;
			Collection.AdditionalFilter = filter.Query;
			AssertCollectionNotContains(Delivery01, Collection);
			AssertCollectionNotContains(Delivery02, Collection);
		}

		#region Implementation

		RefUNLOCO Port1, Port2;
		GlbPortDeliveryTime Delivery01, Delivery02;
		GlbPortDeliveryTimeCollection Collection;

		protected override void SetUp()
		{
			base.SetUp();

			Collection = new GlbPortDeliveryTimeCollection(Factory);

			Port1 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);
			Port2 = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);

			Delivery01 = Factory.New<GlbPortDeliveryTime>();
			Delivery01.G1_FreightMode = Constants.TransportModes.Air;
			Delivery01.G1_RL_NKDischargePort = Port1.RL_Code;
			Delivery01.G1_RL_NKDestinationPort = Port2.RL_Code;
			Delivery01.G1_DaysDelayFromArrivalToDeliver = 1;
			Delivery01.G1_JobMode = "CUS";

			Delivery02 = Factory.New<GlbPortDeliveryTime>();
			Delivery02.G1_FreightMode = Constants.TransportModes.Rail;
			Delivery02.G1_RL_NKDischargePort = Port2.RL_Code;
			Delivery02.G1_RL_NKDestinationPort = Port1.RL_Code;
			Delivery02.G1_DaysDelayFromArrivalToDeliver = 5;
			Delivery02.G1_JobMode = "FWD";

			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbPortDeliveryTimeFilterBusinessObject();
		}

		#endregion
	}
}
