using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module
{
	[TestedType(typeof(JobTradeLaneFilterBusinessObject))]
	sealed class JobTradeLaneFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTradeLaneDescriptionFilter()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();

			tradeLane1.EJ_Code = "AAAA";
			tradeLane1.EJ_Description = "AAAA";
			tradeLane1.EJ_Location1 = "AUSYD";
			tradeLane1.EJ_Location2 = "AUBNE";
			tradeLane1.EJ_OH_RelatedOrg = orgHeader.PK;

			tradeLane2.EJ_Code = "BBBB";
			tradeLane2.EJ_Description = "BBBB";
			tradeLane2.EJ_Location1 = "AUSYD";
			tradeLane2.EJ_Location2 = "AUBNE";
			tradeLane2.EJ_OH_RelatedOrg = orgHeader.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[JobTradeLaneFilterBusinessObject.Descriptions.TradeLaneDescription];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			JobTradeLane[] lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);

			filter.Property = "AAAA";

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionNotContains(tradeLane2, lanes);
		}

		public void TestTradeLaneCodeFilter()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();

			tradeLane1.EJ_Code = "AAAA";
			tradeLane1.EJ_Location1 = "AUSYD";
			tradeLane1.EJ_Location2 = "AUBNE";
			tradeLane1.EJ_OH_RelatedOrg = orgHeader.PK;

			tradeLane2.EJ_Code = "BBBB";
			tradeLane2.EJ_Location1 = "AUSYD";
			tradeLane2.EJ_Location2 = "AUBNE";
			tradeLane2.EJ_OH_RelatedOrg = orgHeader.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[JobTradeLaneFilterBusinessObject.Descriptions.TradeLaneCode];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			JobTradeLane[] lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);

			filter.Property = "AAAA";

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionNotContains(tradeLane2, lanes);
		}

		public void TestDirectionTypeFilter()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();

			tradeLane1.EJ_Code = "AAAA";
			tradeLane1.EJ_Location1 = "AUSYD";
			tradeLane1.EJ_Location2 = "AUBNE";
			tradeLane1.EJ_Direction = DirectionTypeList.Codes.OneWay;
			tradeLane1.EJ_OH_RelatedOrg = orgHeader.PK;

			tradeLane2.EJ_Code = "BBBB";
			tradeLane2.EJ_Location1 = "AUSYD";
			tradeLane2.EJ_Location2 = "AUBNE";
			tradeLane2.EJ_Direction = DirectionTypeList.Codes.BothWays;
			tradeLane2.EJ_OH_RelatedOrg = orgHeader.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[JobTradeLaneFilterBusinessObject.Descriptions.TradeLaneDirectionType];
			filter.IsActive = true;
			filter.Property = ZString.Empty;

			JobTradeLane[] lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);

			filter.Property = DirectionTypeList.Codes.OneWay;

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionNotContains(tradeLane2, lanes);

			filter.Property = DirectionTypeList.Codes.BothWays;

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionNotContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);
		}

		public void TestTradeLaneLocationsFilter()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();

			tradeLane1.EJ_Code = "AAAA";
			tradeLane1.EJ_Location1 = "AUSYD";
			tradeLane1.EJ_Location2 = "AUBNE";
			tradeLane1.EJ_Direction = DirectionTypeList.Codes.OneWay;
			tradeLane1.EJ_OH_RelatedOrg = orgHeader.PK;

			tradeLane2.EJ_Code = "BBBB";
			tradeLane2.EJ_Location1 = "AUMEL";
			tradeLane2.EJ_Location2 = "AUPER";
			tradeLane2.EJ_Direction = DirectionTypeList.Codes.BothWays;
			tradeLane2.EJ_OH_RelatedOrg = orgHeader.PK;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[JobTradeLaneFilterBusinessObject.Descriptions.TradeLaneLocations];
			filter.IsActive = true;
			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;

			JobTradeLane[] lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionNotContains(tradeLane2, lanes);

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUPER";

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionNotContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);
		}

		public void TestPrincipalFilter()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader1.OH_IsShippingProvider = true;
			orgHeader1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgHeader2.OH_IsShippingProvider = true;
			orgHeader2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();

			tradeLane1.EJ_Code = "AAAA";
			tradeLane1.EJ_Location1 = "AUSYD";
			tradeLane1.EJ_Location2 = "AUBNE";
			tradeLane1.EJ_Direction = DirectionTypeList.Codes.OneWay;
			tradeLane1.EJ_OH_RelatedOrg = orgHeader1.PK;

			tradeLane2.EJ_Code = "BBBB";
			tradeLane2.EJ_Location1 = "AUMEL";
			tradeLane2.EJ_Location2 = "AUPER";
			tradeLane2.EJ_Direction = DirectionTypeList.Codes.BothWays;
			tradeLane2.EJ_OH_RelatedOrg = orgHeader2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[JobTradeLaneFilterBusinessObject.Descriptions.Principal];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			JobTradeLane[] lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);

			filter.Property = orgHeader1.PK;

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionContains(tradeLane1, lanes);
			AssertCollectionNotContains(tradeLane2, lanes);

			filter.Property = orgHeader2.PK;

			lanes = Factory.Load<JobTradeLane>(filter.Query);

			AssertCollectionNotContains(tradeLane1, lanes);
			AssertCollectionContains(tradeLane2, lanes);
		}

		JobTradeLaneFilterBusinessObject FilterStrip
		{
			get { return filterStrip ?? (filterStrip = new JobTradeLaneFilterBusinessObject()); }
		}
		JobTradeLaneFilterBusinessObject filterStrip;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobTradeLaneFilterBusinessObject();
		}
	}
}
