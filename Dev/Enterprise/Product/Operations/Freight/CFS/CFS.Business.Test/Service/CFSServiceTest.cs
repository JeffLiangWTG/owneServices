using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSService))]
	public class CFSServiceTest : EnterpriseBusinessObjectTestCase
	{
		#region Property Overrides

		#region TestES_ServiceCode

		public void TestES_ServiceCode()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;
			AssertEquals("Service code on dependent service should be updated", parentService.ES_ServiceCode, childService.ES_ServiceCode);
		}

		#endregion

		#region TestES_Booked

		public void TestES_Booked()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_Booked = ZDateTime.Now;
			AssertEquals("Booked date on dependent service should be updated", parentService.ES_Booked, childService.ES_Booked);
		}

		#endregion

		#region TestES_Calc_LocationCode

		public void TestES_Calc_LocationCode()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			var address = Factory.NewWithValidTestData<OrgAddress>();
			parentService.ES_Calc_LocationCode = address.OA_Code;
			AssertEquals("Location code on dependent service should be updated", parentService.ES_Calc_LocationCode, childService.ES_Calc_LocationCode);
		}

		#endregion

		#region TestES_Completed

		public void TestES_Completed()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_Completed = ZDateTime.Now;
			AssertEquals("Completed date on dependent service should be updated", parentService.ES_Completed, childService.ES_Completed);
		}

		#endregion

		#region TestES_Duration

		public void TestES_Duration()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_Duration = ZDateTime.Now;
			AssertEquals("Duration on dependent service should be updated", parentService.ES_Duration, childService.ES_Duration);
		}

		#endregion

		#region TestES_OA_Location

		public void TestES_OA_Location()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			OrgAddress location = Factory.New<OrgAddress>();
			parentService.ES_OA_Location = location.PK;
			AssertEquals("Location on dependent service should be updated", parentService.ES_OA_Location, childService.ES_OA_Location);
		}

		#endregion

		#region TestES_OH_Contractor

		public void TestES_OH_Contractor()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			OrgHeader contractor = Factory.New<OrgHeader>();
			parentService.ES_OH_Contractor = contractor.PK;
			AssertEquals("Contractor on dependent service should be updated", parentService.ES_OH_Contractor, childService.ES_OH_Contractor);
		}

		#endregion

		#region TestES_References

		public void TestES_References()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_References = "AX10293840";
			AssertEquals("Reference on dependent service should be updated", parentService.ES_References, childService.ES_References);
		}

		#endregion

		#region TestES_ServiceCount

		public void TestES_ServiceCount()
		{
			var parentService = Parent.Services.AddNew();
			parentService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var dependentShipment = Parent.PackUnpackShipments.AddNew();
			AssertEquals("DependentShipment should have matching services with parent", 1, dependentShipment.DocsAndCartage.Services.Count);

			var childService = dependentShipment.DocsAndCartage.Services[0];
			CombineAssertions("ChildService should match parent service", () =>
			{
				AssertEquals("ES_ServiceCode", parentService.ES_ServiceCode, childService.ES_ServiceCode);
				AssertEquals("ES_ServiceId/ES_ExternalServiceId", parentService.ES_ServiceId, childService.ES_ExternalServiceId);
			});

			Factory.Save();
			AssertEquals("Precondition - Parent should have one dependent", 1, Parent.DependentServiceParents.Length);

			parentService.ES_ServiceCount = new ZDecimal(23);
			AssertEquals("Service count on dependent service should be updated", parentService.ES_ServiceCount, childService.ES_ServiceCount);
		}

		#endregion

		#endregion

		#region TestContainerServicesGetCopiedToShipmentsOnSave
		public void TestContainerServicesGetCopiedToShipmentsOnSave()
		{
			CFSShipment dependentShipment = Parent.PackUnpackShipments.AddNew();
			JobService fumigation = dependentShipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			JobService quarantine = dependentShipment.DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;

			JobService parentFumigation = Parent.Services.AddNew();
			parentFumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			JobService parentCustomsHold = Parent.Services.AddNew();
			parentCustomsHold.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;

			Factory.Save();

			AssertEquals("Precondition - parent should have 1 dependent shipment", 1, Parent.DependentServiceParents.Length);
			IHaveServices dependent = Parent.DependentServiceParents[0];

			Assert("Parent's dependent should still have fumigation", dependent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
			Assert("Parent's dependent should still have quarantine", dependent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.QuarantineInspection));
			Assert("Parent's dependent should have inherited customs hold from parent", dependent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.CustomsHold));

			Assert("Parent should still have fumigation", Parent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
			Assert("Parent should not have quarantine", !Parent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.QuarantineInspection));
			Assert("Parent should still have customs hold", Parent.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.CustomsHold));
		}

		#endregion

		#region TestGetTypeFromPrefix

		public void TestGetTypeFromPrefix()
		{
			JobServiceWithExposedHashTable service = Factory.New<JobServiceWithExposedHashTable>();
			AssertEquals("Hashtable should return correct type", typeof(CFSDocsAndCartage), service.GetTypeFromPrefix("JP"));
			AssertEquals("Hashtable should return correct type", typeof(CFSContainer), service.GetTypeFromPrefix("JC"));
		}

		class JobServiceWithExposedHashTable : CFSService
		{
			public JobServiceWithExposedHashTable(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Type GetTypeFromPrefix(string prefix)
			{
				return base.GetTypeFromPrefix(prefix);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CFSContainer container = factory.New<CFSContainer>();
			return container.Services.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Parent = Factory.New<CFSContainer>();
			Services = Parent.Services;
		}

		protected CFSContainer Parent;
		protected JobServiceDependentCollection Services;

		#endregion

	}
}
