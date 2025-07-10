using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class eAdaptorUserContextTest : eAdaptorEndToEndTest
	{
		public void TestEventBranchUsedForUserContext()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, "EDI", "DAT", branch.Company.GC_Code, branch.GB_Code);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.CustomisableEvent00Code);

			PostRequest(universalEvent);

			var shipmentBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK) as BusinessObject;
			var log = shipmentBizo.GetLogs().Find(l => l.SL_SE_NKEvent == universalEvent.EventType.Value).FirstOrDefault();

			AssertEquals(branch.GB_Code, log.SL_GB_NKBranch);
		}

		public void TestEventBranchUsedForUserContext_NoDataContextCompany()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, "EDI", "DAT", null, branch.GB_Code);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.CustomisableEvent00Code);

			PostRequest(universalEvent);

			var shipmentBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK) as BusinessObject;
			var log = shipmentBizo.GetLogs().Find(l => l.SL_SE_NKEvent == universalEvent.EventType.Value).FirstOrDefault();

			AssertEquals(branch.GB_Code, log.SL_GB_NKBranch);
		}

		public void TestEventBranchIsInactive()
		{
			eAdaptorTestHelper.TurnOnVerboseLogging();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_GC = company.PK;
			inactiveBranch.GB_IsActive = false;

			var activeBranch = Factory.NewWithValidTestData<GlbBranch>();
			activeBranch.GB_GC = company.PK;

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, "EDI", "DAT", inactiveBranch.Company.GC_Code, inactiveBranch.GB_Code);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.CustomisableEvent00Code);

			var response = PostRequest(universalEvent);

			var shipmentBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK) as BusinessObject;
			var log = shipmentBizo.GetLogs().Find(l => l.SL_SE_NKEvent == universalEvent.EventType.Value).FirstOrDefault();

			AssertEquals("Should not use <EventBranch> since it is inactive", activeBranch.GB_Code, log.SL_GB_NKBranch);

			AssertContains(UniversalXmlUserContextLogging.TargetingOnlyActiveBranch(company.GC_Code, activeBranch.GB_Code), response);
		}

		public void TestEventBranchDoesntBelongToCompany()
		{
			eAdaptorTestHelper.TurnOnVerboseLogging();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var company = Factory.New<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "YYY";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = company2.PK;

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, "EDI", "DAT", company2.GC_Code, branch.GB_Code);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.CustomisableEvent00Code);

			var response = PostRequest(universalEvent);

			var shipmentBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK) as BusinessObject;
			var log = shipmentBizo.GetLogs().Find(l => l.SL_SE_NKEvent == universalEvent.EventType.Value).FirstOrDefault();

			AssertEquals("Should not use <EventBranch> since it doesn not belong to the specified company in data context", company2.FirstActiveBranchCode, log.SL_GB_NKBranch);

			AssertContains(UniversalXmlUserContextLogging.EventBranchIsNotActiveOnDataContextCompany(company2.GC_Code, branch.GB_Code), response);
			AssertContains(UniversalXmlUserContextLogging.TargetingFirstActiveBranch(company2.GC_Code, company2.FirstActiveBranchCode), response);
		}

		public void TestEventBranchWhenServerIDIsMissing()
		{
			eAdaptorTestHelper.TurnOnVerboseLogging();

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			var company = Factory.New<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, shipment.JS_UniqueConsignRef, null, null, branch.Company.GC_Code, branch.GB_Code);
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.CustomisableEvent00Code);

			var response = PostRequest(universalEvent);

			AssertContains(UniversalXmlUserContextLogging.TargetingOnlyActiveBranch(branch.Company.GC_Code, branch.GB_Code), response);
		}
	}
}
