using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobServiceDependentCollection))]
	sealed class JobServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestIsServiceRequired

		public void TestGetRequiredService()
		{
			AssertNull(Services.GetRequiredService(null));

			var otherServices = Factory.New<DummyWithServices>().Services;
			var otherService1 = otherServices.AddNew();
			otherService1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			var otherService2 = otherServices.AddNew();
			otherService2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Services.SetServicesOfThisToMatchOther(otherServices);
			AssertEquals(2, Services.Count);
			AssertEquals("The service Fumigation should be returned", Core.Constants.FreightServiceType.Codes.Fumigation, Services.GetRequiredService(otherService1)?.ES_ServiceCode);
			AssertEquals("The service Tailgate should be returned", Core.Constants.FreightServiceType.Codes.Tailgate, Services.GetRequiredService(otherService2)?.ES_ServiceCode);
		}

		public void TestIsServiceRequired()
		{
			Services.Load();
			Assert("Expect fumigation not to be required", !Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));

			JobService fumigation = Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Expect fumigation to be required", Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
		}

		#endregion

		#region TestIsServiceCompleted

		public void TestIsServiceCompleted()
		{
			Services.Load();
			Assert("Expect fumigation not to be completed", !Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Fumigation));

			JobService fumigation = Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Expect fumigation not to be completed", !Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Fumigation));

			fumigation.ES_Completed = ZDateTime.Now;
			Assert("Expect fumigation to be completed", Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Fumigation));
		}

		#endregion

		#region TestServiceCompletionDate

		public void TestServiceCompletionDate()
		{
			Services.Load();
			AssertEquals("Expect fumigation date to be empty", ZDateTime.Empty, Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation));

			JobService fumigation = Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertEquals("Expect fumigation date to be empty", ZDateTime.Empty, Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation));

			ZDateTime completionDate = ZDateTime.Now;
			fumigation.ES_Completed = completionDate;
			AssertEquals("Expect fumigation date to be correct", completionDate, Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation));
		}

		#endregion

		#region TestSetServicesOfThisToMatchOther

		public void TestSetServicesOfThisToMatchOther()
		{
			Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;

			var populatedDummy = GetNewDummyWithPopulatedServices();
			Services.SetServicesOfThisToMatchOther(populatedDummy.Services);
			AssertEquals("Dummy services should now contain 2 fumigation", 2, Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).Count());
			AssertEquals("Dummy services should still contain 1 customs hold", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.CustomsHold).Count());
			AssertEquals("Dummy services should now contain 1 quarantine", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.QuarantineInspection).Count());
			AssertEquals("Dummy services should now contain 1 washing", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.Washing).Count());
			AssertEquals("populatedDummy services should still contain 1 fumigation", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).Count());
			AssertEquals("populatedDummy services should still contain 1 quarantine", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.QuarantineInspection).Count());
			AssertEquals("populatedDummy services should still contain 1 washing", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Washing).Count());
			AssertEquals("populatedDummy services should not contain customs hold", 0, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.CustomsHold).Count());
		}

		public void TestSetServicesOfThisToMatchOther_ContractorNotOverridden()
		{
			var contractor1 = Factory.New<OrgHeader>();
			contractor1.OH_Code = "FUMORG1";
			var contractor2 = Factory.New<OrgHeader>();
			contractor2.OH_Code = "FUMORG2";

			var otherServices = Factory.New<DummyWithServices>().Services;
			var service = otherServices.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_OH_Contractor = contractor1.PK;

			Services.SetServicesOfThisToMatchOther(otherServices);
			AssertEquals("Service has been added", 1, Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, Services[0].ES_ServiceCode);
			AssertEquals("Contractor is not empty", "FUMORG1", Services[0].Contractor.OH_Code);

			Services[0].ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Services[0].ES_OH_Contractor = contractor2.PK;
			Services.SetServicesOfThisToMatchOther(otherServices);
			AssertEquals("Service has been updated", 1, Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Tailgate, Services[0].ES_ServiceCode);
			AssertEquals("Contractor is not empty", "FUMORG2", Services[0].Contractor.OH_Code);
		}

		#endregion

		#region TestSetServicesOfOtherToMatchThis

		public void TestSetServicesOfOtherToMatchThis()
		{
			Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;

			var populatedDummy = GetNewDummyWithPopulatedServices();
			populatedDummy.Services.SetServicesOfOtherToMatchThis(Services);
			AssertEquals("Dummy services should now contain 2 fumigation", 2, Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).Count());
			AssertEquals("Dummy services should still contain 1 customs hold", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.CustomsHold).Count());
			AssertEquals("Dummy services should now contain 1 quarantine", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.QuarantineInspection).Count());
			AssertEquals("Dummy services should now contain 1 washing", 1, Services.GetServices(Core.Constants.FreightServiceType.Codes.Washing).Count());
			AssertEquals("populatedDummy services should still contain 1 fumigation", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).Count());
			AssertEquals("populatedDummy services should still contain 1 quarantine", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.QuarantineInspection).Count());
			AssertEquals("populatedDummy services should still contain 1 washing", 1, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Washing).Count());
			AssertEquals("populatedDummy services should not contain customs hold", 0, populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.CustomsHold).Count());
		}

		#endregion

		#region TestSetServicesForContainerUnpack

		public void TestSetServicesForContainerUnpack()
		{
			var populatedDummy = GetNewDummyWithPopulatedServices();
			Services.SetServicesOfThisToMatchOther(populatedDummy.Services);
			Services.SetServicesForContainerUnpack(populatedDummy);
			AssertEquals(0, Services.Count);
		}

		#endregion

		#region TestAddIfNotExists

		public void TestAddIfNotExists()
		{
			AssertEquals(0, Services.Count);

			var firstFumigation = Services.AddIfNotExists(Core.Constants.FreightServiceType.Codes.Fumigation).First();
			CombineAssertions("Shipment now has fumigation", () =>
			{
				AssertEquals(1, Services.Count);
				AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, firstFumigation.ES_ServiceCode);
			});

			var secondFumigation = Services.AddIfNotExists(Core.Constants.FreightServiceType.Codes.Fumigation).First();
			CombineAssertions("The original fumigation object should be returned", () =>
			{
				AssertEquals(1, Services.Count);
				AssertEquals(firstFumigation, secondFumigation);
			});
		}

		#endregion

		#region TestRemoveIfExists

		public void TestRemoveIfExists()
		{
			var otherServices = Factory.New<DummyWithServices>().Services;
			var otherService = otherServices.AddNew();
			otherService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Services.SetServicesOfThisToMatchOther(otherServices);
			AssertEquals(1, Services.Count);

			Services.RemoveIfExists(otherService);
			AssertEquals(0, Services.Count);
		}

		#endregion

		#region TestUpdateDefaultContractor

		public void TestUpdateDefaultContractor()
		{
			var fumigation1 = Services.AddNew();
			fumigation1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Precondition - fumigation1 contractor is not set", fumigation1.ES_OH_Contractor.IsEmpty);

			var fumigation2 = Services.AddNew();
			fumigation2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Precondition - fumigation2 contractor is not set", fumigation2.ES_OH_Contractor.IsEmpty);

			Services.UpdateDefaultContractor(Core.Constants.FreightServiceType.Codes.Fumigation);
			AssertNotNull("fumigation1 Contractor is set to default", fumigation1.ES_OH_Contractor);
			AssertNotNull("fumigation2 Contractor is set to default", fumigation2.ES_OH_Contractor);
		}

		#endregion

		#region TestGetService

		public void TestGetService()
		{
			var otherServices = Factory.New<DummyWithServices>().Services;
			var otherService1 = otherServices.AddNew();
			otherService1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			var otherService2 = otherServices.AddNew();
			otherService2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Services.SetServicesOfThisToMatchOther(otherServices);
			AssertEquals(2, Services.Count);
			AssertEquals("The service Fumigation should be returned", Core.Constants.FreightServiceType.Codes.Fumigation, Services.GetService(otherService1)?.ES_ServiceCode);
			AssertEquals("The service Tailgate should be returned", Core.Constants.FreightServiceType.Codes.Tailgate, Services.GetService(otherService2)?.ES_ServiceCode);
		}

		public void TestGetServices()
		{
			Assert("Precondition - shipment doesn't have fumigation", !Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
			AssertEquals("No service exists so empty list should be returned", 0, Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).Count());

			JobService fumigation1 = Services.AddNew();
			fumigation1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertContainsExactElementsInAnyOrder("The same service row should be returned", new [] { fumigation1 }, Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation));

			JobService fumigation2 = Services.AddNew();
			fumigation2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertContainsExactElementsInAnyOrder("The same service row should be returned", new [] { fumigation1, fumigation2 }, Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation));
		}

		public void TestGetServiceInfos()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			var infos = Services.GetServiceInfos(Core.Constants.FreightServiceType.Codes.Fumigation).ToList();
			AssertEquals(0, infos.Count);

			JobService fumigation1 = Services.AddNew();
			fumigation1.ES_ServiceId = "AAA";
			fumigation1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigation1.ES_ServiceCount = 3m;
			fumigation1.ES_OH_Contractor = org.PK;
			fumigation1.ES_Completed = ZDateTime.Now;
			fumigation1.ES_OA_Location = org.MainAddress.PK;

			infos = Services.GetServiceInfos(Core.Constants.FreightServiceType.Codes.Fumigation).ToList();
			AssertEquals(1, infos.Count);
			AssertEquals(true, infos[0].IsEnabled);
			AssertEquals(org.PK, infos[0].Contractor.PK);
			AssertEquals(3m, infos[0].ServiceCount);
			AssertEquals(org.MainAddress.OA_RN_NKCountryCode, infos[0].LocationCountryCode);
			AssertEquals("AAA", infos[0].ServiceId);

			JobService fumigation2 = Services.AddNew();
			fumigation2.ES_ServiceId = "BBB";
			fumigation2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigation2.ES_ServiceCount = 5m;
			fumigation2.ES_OH_Contractor = org.PK;
			fumigation2.ES_Completed = ZDateTime.Now;
			fumigation2.ES_OA_Location = org.MainAddress.PK;

			infos = Services.GetServiceInfos(Core.Constants.FreightServiceType.Codes.Fumigation).ToList();
			AssertEquals(2, infos.Count);
			AssertEquals(true, infos[0].IsEnabled);
			AssertEquals(true, infos[1].IsEnabled);
			AssertEquals(org.PK, infos[0].Contractor.PK);
			AssertEquals(org.PK, infos[1].Contractor.PK);
			AssertEquals(3m, infos[0].ServiceCount);
			AssertEquals(5m, infos[1].ServiceCount);
			AssertEquals(org.MainAddress.OA_RN_NKCountryCode, infos[0].LocationCountryCode);
			AssertEquals(org.MainAddress.OA_RN_NKCountryCode, infos[1].LocationCountryCode);
			AssertEquals("AAA", infos[0].ServiceId);
			AssertEquals("BBB", infos[1].ServiceId);
		}

		#endregion

		#region TestServiceLocationOrganisation

		public void TestServiceLocationOrganisation()
		{
			var populatedDummyFumigationOrg = Factory.New<OrgHeader>();
			var dummyFumigationOrg = Factory.New<OrgHeader>();

			var populatedDummy = GetNewDummyWithPopulatedServices();
			populatedDummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).First().ES_OH_Contractor = populatedDummyFumigationOrg.PK;
			Dummy.Services.SetServicesOfThisToMatchOther(populatedDummy.Services);

			Dummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).First().ES_OH_Contractor = dummyFumigationOrg.PK;
			Dummy.Services.SetServicesOfThisToMatchOther(populatedDummy.Services);

			AssertEquals("Shipment fumigation location code is empty", ZString.Empty, Dummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).First().ES_Calc_LocationCode);
			AssertEquals("Shipment's location org should remain the same", dummyFumigationOrg.PK, Dummy.Services.GetServices(Core.Constants.FreightServiceType.Codes.Fumigation).First().ES_OH_Contractor);
		}

		#endregion

		#region TestAreAnyServicesIncomplete

		public void TestAreAnyServicesIncomplete()
		{
			AssertEquals("Precondition - no services on shipment", 0, Services.Count);
			Assert("There should be no incomplete services", !Services.AreAnyServicesIncomplete);

			JobService fumigation = Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			JobService quarantine = Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert("There should be incomplete services", Services.AreAnyServicesIncomplete);

			fumigation.ES_Completed = ZDateTime.Now;
			Assert("There should be incomplete services", Services.AreAnyServicesIncomplete);

			quarantine.ES_Completed = ZDateTime.Now;
			Assert("There should be no incomplete services", !Services.AreAnyServicesIncomplete);
		}

		#endregion

		#region TestOnAdded

		public void TestOnAdded()
		{
			var dummyParent = Factory.New<DummySubClassWithServices>();
			var service1 = dummyParent.Services.AddNew();
			AssertEquals(dummyParent, service1.Parent);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyParentInNewFactory = newFactory.Load<DummySubClassWithServices>(dummyParent.PK);
			dummyParentInNewFactory.Services.Load();
			AssertEquals(dummyParentInNewFactory, dummyParentInNewFactory.Services[0].Parent);
		}

		#endregion

		#region Implementation

		DummyWithServices Dummy;
		JobServiceDependentCollection Services;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<DummyWithServices>().Services;
		}

		protected override void SetUp()
		{
			Dummy = Factory.New<DummyWithServices>();
			Services = Dummy.Services;
		}

		DummyWithServices GetNewDummyWithPopulatedServices()
		{
			DummyWithServices result = Factory.New<DummyWithServices>();

			result.Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			result.Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			result.Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Washing;

			return result;
		}

		#endregion
	}
}
