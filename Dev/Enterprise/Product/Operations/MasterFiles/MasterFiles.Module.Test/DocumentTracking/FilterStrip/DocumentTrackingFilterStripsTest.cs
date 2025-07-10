
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DocumentTrackingFilterStripsTest : TestCaseWithFactory
	{
		public void TestDateReceivedFilter_IHaveRequiredDocuments()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(-4));
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddDays(4));

			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, allFilters.Filter);
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should not be in the collection", !orgHeaderCollection.Contains(org3));
		}

		public void TestDateReceivedFilter_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(-4));
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddDays(4));

			TestFilter(allFilters, shipment1, shipment2, shipment3);
		}

		[TestDate(2024, 4, 7, 7, 0, 0)]
		public void TestDateReceivedFilterWithTimeZones_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			TestDateAttribute.UseUNLOCO = true;

			var sydBranchPk = new Guid("FDD429D2-648C-4895-8F9F-06E90DED2BE5");
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranchPk, Env.CurrentDepartmentPK))
			{
				CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(-4));
				CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);
				CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddDays(4));

				TestFilter(allFilters, shipment1, shipment2, shipment3);
			}

			var sinBranchPk = new Guid("EF8CBDB5-9F53-4360-921E-C2929BF77A85");
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sinBranchPk, Env.CurrentDepartmentPK))
			{
				TestFilter(allFilters, shipment1, shipment2, shipment3);
			}
		}

		void TestFilter(DummyFilterStripBusinessObject allFilters, BusinessObject shipment1, BusinessObject shipment2, BusinessObject shipment3)
		{
			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddMinutes(-1);
			dateFilter.Property2 = ZDateTime.Now.AddMinutes(1);
			dateFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should not be in the collection", !forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestValidToDateFilter_IHaveRequiredDocuments()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(-4), ZDateTime.Now.AddDays(4));
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(10));
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddDays(-40), ZDateTime.Now.AddDays(100));

			var validToDateFilter = (ModuleDateFilter)allFilters["Valid To Date"];
			validToDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			validToDateFilter.Property1 = ZDateTime.Now.AddDays(2);
			validToDateFilter.Property2 = ZDateTime.Now.AddDays(20);
			validToDateFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, allFilters.Filter);
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should not be in the collection", !orgHeaderCollection.Contains(org3));
		}

		public void TestValidToDateFilter_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddMonths(-40), ZDateTime.Now.AddDays(4));
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(365));
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddYears(-3), ZDateTime.Now.AddYears(-1));

			var validToDateFilter = (ModuleDateFilter)allFilters["Valid To Date"];
			validToDateFilter.PropertySearch = ModuleDateFilter.Past;
			validToDateFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDocTypeFilter_IHaveRequiredDocuments()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, allFilters.Filter);
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should not be in the collection", !orgHeaderCollection.Contains(org3));
		}

		public void TestDocTypeFilter_IHaveRequiredDocuments_NotOperators()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			//This is to make sure it only grabs newly created orgs to prevent the filtered collection from getting too large
			org1.OH_Category = "NAT";
			org2.OH_Category = "NAT";
			org3.OH_Category = "NAT";

			var orgQuery = new ZQuery(OrgHeaderSchema.OH_Category, OrgConstants.Category.NaturalPersonIndividual);

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			typeFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should be in the collection", orgHeaderCollection.Contains(org3));

			typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = "OA";
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			typeFilter.IsActive = true;

			orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should be in the collection", orgHeaderCollection.Contains(org3));

			typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = "P";
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			typeFilter.IsActive = true;

			orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should be in the collection", orgHeaderCollection.Contains(org3));
		}

		public void TestDocTypeFilter_IHaveRequiredDocuments_NotOperators_OtherThings()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			//This is to make sure it only grabs newly created orgs to prevent the filtered collection from getting too large
			org1.OH_Category = "NAT";
			org2.OH_Category = "NAT";
			org3.OH_Category = "NAT";

			var orgQuery = new ZQuery(OrgHeaderSchema.OH_Category, OrgConstants.Category.NaturalPersonIndividual);

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			typeFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should be not in the collection", !orgHeaderCollection.Contains(org3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should be in the collection", orgHeaderCollection.Contains(org3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should be in the collection", orgHeaderCollection.Contains(org3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;

			orgHeaderCollection = new OrgHeaderCollection(Factory, new ZQuery(allFilters.Filter, orgQuery));
			orgHeaderCollection.Load();

			Assert("Org1 should be in the collection", orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should be not in the collection", !orgHeaderCollection.Contains(org3));
		}

		public void TestDocTypeFilter_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now.AddDays(4));

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should not be in the collection", !forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDocTypeFilter_IDocsAndCartageParent_NotOperators()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Now);

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			typeFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should be in the collection", forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = "OA";
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			typeFilter.IsActive = true;

			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should be in the collection", forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = "P";
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			typeFilter.IsActive = true;

			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should be in the collection", forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDocTypeFilter_IDocsAndCartageParent_NotOperators_OtherThings()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			////This is to make sure it only grabs newly created shipments to prevent the filtered collection from getting too large
			//shipment1.OH_Category = "NAT";
			//shipment2.OH_Category = "NAT";
			//shipment3.OH_Category = "NAT";
			//
			//var shipmentQuery = new ZQuery(OrgHeaderSchema.OH_Category, OrgConstants.Category.NaturalPersonIndividual);

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			typeFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), /*new ZQuery(*/allFilters.Filter/*, shipmentQuery)*/);

			Assert("Shipment1 should be in the collection", forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be not in the collection", !forwardingShipmentCollection.Contains(shipment3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), /*new ZQuery(*/allFilters.Filter/*, shipmentQuery)*/);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), /*new ZQuery(*/allFilters.Filter/*, shipmentQuery)*/);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;

			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), /*new ZQuery(*/allFilters.Filter/*, shipmentQuery)*/);

			Assert("Shipment1 should be in the collection", forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be not in the collection", !forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDateReceivedAndDocTypeFilter_IHaveRequiredDocuments()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(4));

			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateFilter.IsActive = true;

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, allFilters.Filter);
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should be in the collection", orgHeaderCollection.Contains(org2));
			Assert("Org3 should not be in the collection", !orgHeaderCollection.Contains(org3));
		}

		public void TestDateReceivedAndDocTypeFilter_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(4));

			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateFilter.IsActive = true;

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should not be in the collection", !forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDateReceivedAndDocTypeFilter_ForwardingConsol()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingConsol>() };
			var consol1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());
			var consol2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());
			var consol3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());

			CreateJobRequiredDocument(consol1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(consol2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now);
			CreateJobRequiredDocument(consol3, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(4));

			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateFilter.IsActive = true;

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var forwardingConsolCollection = Factory.Load(ObjectFactory.GetType<IForwardingConsol>(), allFilters.Filter);

			Assert("Consol1 should not be in the collection", !forwardingConsolCollection.Contains(consol1));
			Assert("Consol2 should be in the collection", forwardingConsolCollection.Contains(consol2));
			Assert("Consol3 should not be in the collection", !forwardingConsolCollection.Contains(consol3));
		}

		public void TestCombinedAllFilters_IHaveRequiredDocuments()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobRequiredDocument(org1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(org2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(4));
			CreateJobRequiredDocument(org3, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(4), ZDateTime.Now);
			CreateJobRequiredDocument(org4, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(1));
			CreateJobRequiredDocument(org5, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(10));
			CreateJobRequiredDocument(org5, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);

			var dateReceivedFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateReceivedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateReceivedFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateReceivedFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateReceivedFilter.IsActive = true;

			var validToDateFilter = (ModuleDateFilter)allFilters["Valid To Date"];
			validToDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			validToDateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			validToDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			validToDateFilter.IsActive = true;

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var orgHeaderCollection = new OrgHeaderCollection(Factory, allFilters.Filter);
			orgHeaderCollection.Load();

			Assert("Org1 should not be in the collection", !orgHeaderCollection.Contains(org1));
			Assert("Org2 should not be in the collection", !orgHeaderCollection.Contains(org2));
			Assert("Org3 should not be in the collection", !orgHeaderCollection.Contains(org3));
			Assert("Org4 should be in the collection", orgHeaderCollection.Contains(org4));
			Assert("Org5 should not be in the collection", !orgHeaderCollection.Contains(org5));
		}

		public void TestCombinedAllFilters_IDocsAndCartageParent()
		{
			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment4 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment5 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(4));
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now.AddDays(4), ZDateTime.Now);
			CreateJobRequiredDocument(shipment4, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(1));
			CreateJobRequiredDocument(shipment5, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now.AddDays(10));
			CreateJobRequiredDocument(shipment5, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);

			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Now.AddDays(2);
			dateFilter.IsActive = true;

			var validToDateFilter = (ModuleDateFilter)allFilters["Valid To Date"];
			validToDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			validToDateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			validToDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			validToDateFilter.IsActive = true;

			var typeFilter = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should not be in the collection", !forwardingShipmentCollection.Contains(shipment3));
			Assert("Shipment4 should be in the collection", forwardingShipmentCollection.Contains(shipment4));
			Assert("Shipment5 should not be in the collection", !forwardingShipmentCollection.Contains(shipment5));
		}

		public void TestBusinessObjectNotSupported()
		{
			var supportedBusinessObject = new DummyFilterStripBusinessObject { QueryObjectType = typeof(OrgHeader) };
			AssertNotNull(supportedBusinessObject["Date Received"]);
			AssertNotNull(supportedBusinessObject["Document Type"]);

			var notSupportedBusinessObject = new DummyFilterStripBusinessObject { QueryObjectType = typeof(GlbBranch) };
			AssertNull(notSupportedBusinessObject["Date Received"]);
			AssertNull(notSupportedBusinessObject["Document Type"]);
		}

		public void TestCombinedAllFilters_MultipleDocumentTypeFilters()
		{
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now);

			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var typeFilter1 = (ModuleTextFilter)allFilters["Document Type"];
			typeFilter1.Property = Core.Constants.RefDocTypes.ExportCartageAdvice;
			typeFilter1.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			typeFilter1.IsActive = true;

			var typeFilter2 = allFilters.AddFilterStrip<ModuleTextFilter>("Document Type");
			typeFilter2.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			typeFilter2.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should not be in the collection", !forwardingShipmentCollection.Contains(shipment3));

			typeFilter2.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should not be in the collection", !forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter1.OrCategory = FilterOrCategory.Blue;
			typeFilter2.OrCategory = FilterOrCategory.Blue;
			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter1.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			typeFilter2.SqlComparisonOperator = SQLComparisonOperator.Contains;
			typeFilter2.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));

			typeFilter1.SqlComparisonOperator = SQLComparisonOperator.Contains;
			typeFilter1.Property = Core.Constants.RefDocTypes.PowerOfAttorney;
			typeFilter2.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			typeFilter2.Property = Core.Constants.RefDocTypes.ExportCartageAdvice;
			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			Assert("Shipment1 should not be in the collection", !forwardingShipmentCollection.Contains(shipment1));
			Assert("Shipment2 should be in the collection", forwardingShipmentCollection.Contains(shipment2));
			Assert("Shipment3 should be in the collection", forwardingShipmentCollection.Contains(shipment3));
		}

		public void TestDateReceivedFilter_HasDateOrNot()
		{
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment4 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, null, ZDateTime.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.PowerOfAttorney, null, ZDateTime.Now);

			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = "Has Date";
			dateFilter.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			AssertEquals("Shipment1 should be in the collection", true, forwardingShipmentCollection.Contains(shipment1));
			AssertEquals("Shipment2 should be in the collection", true, forwardingShipmentCollection.Contains(shipment2));
			AssertEquals("Shipment3 should not be in the collection", false, forwardingShipmentCollection.Contains(shipment3));
			AssertEquals("Shipment4 should not be in the collection", false, forwardingShipmentCollection.Contains(shipment4));

			dateFilter.PropertySearch = "Has No Date";
			forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			AssertEquals("Shipment1 should be in the collection", true, forwardingShipmentCollection.Contains(shipment1));
			AssertEquals("Shipment2 should not be in the collection", false, forwardingShipmentCollection.Contains(shipment2));
			AssertEquals("Shipment3 should be in the collection", true, forwardingShipmentCollection.Contains(shipment3));
			AssertEquals("Shipment4 should not be in the collection", false, forwardingShipmentCollection.Contains(shipment4));
		}

		public void TestDateReceivedAndDocTypeFilter_HasDate_IsBlank()
		{
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipment3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.ExportCartageAdvice, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment1, Core.Constants.RefDocTypes.PowerOfAttorney, null, ZDateTime.Now);
			CreateJobRequiredDocument(shipment2, Core.Constants.RefDocTypes.PowerOfAttorney, ZDateTimeOffset.Now, ZDateTime.Now);
			CreateJobRequiredDocument(shipment3, Core.Constants.RefDocTypes.PowerOfAttorney, null, ZDateTime.Now);

			var allFilters = new DummyFilterStripBusinessObject { QueryObjectType = ObjectFactory.GetType<IForwardingShipment>() };
			var dateFilter = (ModuleDateFilter)allFilters["Date Received"];
			dateFilter.PropertySearch = "Has Date";
			dateFilter.IsActive = true;

			var typeFilter2 = allFilters.AddFilterStrip<ModuleTextFilter>("Document Type");
			typeFilter2.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			typeFilter2.IsActive = true;

			var forwardingShipmentCollection = Factory.Load(ObjectFactory.GetType<IForwardingShipment>(), allFilters.Filter);

			AssertEquals("No element in the collection", false, forwardingShipmentCollection.Any());
		}

		void CreateJobRequiredDocument(BusinessObject businessObject, ZString docType, ZDateTimeOffset? dateReceived, ZDateTime? validToDate = null)
		{
			var jobRequiredDocument = !(businessObject is IHaveRequiredDocuments)
				? ((IDocsAndCartageParent)businessObject).RequiredDocumentsProvider.RequiredDocuments.AddNew()
				: ((IHaveRequiredDocuments)businessObject).RequiredDocuments.AddNew();
			jobRequiredDocument.EQ_DocType = docType;
			jobRequiredDocument.EQ_ValidToDate = validToDate.GetValueOrDefault();

			if (dateReceived != null)
			{
				jobRequiredDocument.EQ_DateReceived = dateReceived.Value;
			}

			Factory.Save();
		}

		[RequiresSTA]
		public void TestDocumentTrackingFilterStripsHelper_IndexSearch()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.FillWithValidTestData();

			var document1 = Factory.New<JobRequiredDocument>();
			document1.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			document1.EQ_DocType = RefDocTypes.WithholdingTaxExemption;
			document1.EQ_DateReceived = new ZDateTimeOffset(2019, 11, 1);
			document1.EQ_ValidToDate = new ZDate(2019, 12, 31);
			orgHeader1.RequiredDocuments.Add(document1);

			var orgHeader2 = (BusinessObject)Factory.New<IOrgHeader>();
			orgHeader2.FillWithValidTestData();

			var document2 = Factory.New<JobRequiredDocument>();
			document2.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			document2.EQ_DocType = RefDocTypes.Permit;
			document2.EQ_DateReceived = new ZDateTimeOffset(2019, 11, 1);
			document2.EQ_ValidToDate = new ZDate(2019, 12, 31);
			orgHeader1.RequiredDocuments.Add(document2);

			Factory.Save();

			using (new GlowIndexQueryEngineMock(mock =>
			{
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetTestSearchFields());
				_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(GetQueryResult(orgHeader1));
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IOrgHeader" });
			}))
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var filterBO = module.FilterBusinessObject;
				var filter1 = (IndexSearchModuleDateFilter)filterBO["DocumentReceivedDate"];
				var filter2 = (IndexSearchModuleTextFilter)filterBO["DocumentType"];
				var filter3 = (IndexSearchModuleDateFilter)filterBO["ValidToDate"];
				AssertNotNull(filter1);
				AssertNotNull(filter2);
				AssertNotNull(filter3);

				filter2.IsActive = true;
				filter2.Property = RefDocTypes.WithholdingTaxExemption;
				filter2.ComparisonOperator = "starts with";
				module.PerformSearch_ForTest();

				AssertEquals(1, module.GridCollection.Count);
				var result = module.GridCollection[0] as BusinessObject;
				AssertEquals(orgHeader1.PK, result.PK);
			}
		}
		SearchFieldCollection GetTestSearchFields()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var ret = new SearchFieldCollection("IOrgHeader", new SearchField[] { field1, field2 });
			return ret;
		}

		GlowIndexQueryResultCollection GetQueryResult(OrgHeader orgHeader)
		{
			var ret = new GlowIndexQueryResultCollection();
			ret.Status = GlowIndexQueryStatus.Success;
			ret.Results = new GlowIndexQueryResult[]
			{
				new GlowIndexQueryResult(orgHeader.PK.ToString(),"IOrgHeader")
			};
			return ret;
		}
	}
}
