using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Shipments;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class ShipmentJobDeclarationFilterDecoratorTest : TestCaseWithFactory
	{
		public void TestDecorate_FilterDescriptionHasWebSuffixForDuplicates()
		{
			var filterObject = GetDecoratedFilterObject();
			var filters = filterObject.ModuleFilters;
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.DateAtOrigin);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.DomesticInternational);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.ImporterCompanyName);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.SupplierCompanyName);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.SendReceiveForwarders);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.Status);
			AssertHasFilterAndNoWebFilter(filters, TrackingDeclarationFilterConstants.LastEditTime);

			filterObject = GetDecoratedFilterObject(false);
			filters = filterObject.ModuleFilters;

			var testColumn = new SchemaStringColumn(Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, null, true, 0);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.DateAtOrigin, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.DomesticInternational, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.ImporterCompanyName, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.SupplierCompanyName, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.SendReceiveForwarders, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.Status, testColumn);
			filters.AddTextFilter(TrackingDeclarationFilterConstants.LastEditTime, testColumn);

			ShipmentJobDeclarationFilterDecorator.Decorate(filterObject);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.DateAtOrigin);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.DomesticInternational);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.ImporterCompanyName);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.SupplierCompanyName);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.SendReceiveForwarders);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.Status);
			AssertHasFilterAndWebFilter(filters, TrackingDeclarationFilterConstants.LastEditTime);
		}

		void AssertHasFilterAndNoWebFilter(ModuleFilterCollection filters, string description)
		{
			AssertNotNull(filters[description]);
			AssertNull(filters[description + " (Web)"]);
		}

		void AssertHasFilterAndWebFilter(ModuleFilterCollection filters, string description)
		{
			AssertNotNull(filters[description]);
			AssertNotNull(filters[description + " (Web)"]);
		}

		public void TestDecorate_CreatedTimeFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_SystemCreateTimeUtc = new DateTime(2009, 01, 02);
			declaration2.JE_SystemCreateTimeUtc = new DateTime(2009, 01, 25);

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var dateFilter = (ModuleDateFilter)filter[TrackingDeclarationFilterConstants.CreatedTime];

			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new DateTime(2009, 01, 01);
			dateFilter.Property2 = new DateTime(2009, 01, 03);
			dateFilter.IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);

			dateFilter.Property2 = new DateTime(2009, 02, 01);
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
		}

		public void TestDecorate_ETDFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_DateAtOrigin = new DateTime(2009, 01, 02);
			declaration2.JE_DateAtOrigin = new DateTime(2009, 01, 25);

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var dateFilter = (ModuleDateFilter)filter[TrackingDeclarationFilterConstants.DateAtOrigin];

			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = new DateTime(2009, 01, 01);
			dateFilter.Property2 = new DateTime(2009, 01, 03);
			dateFilter.IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);

			dateFilter.Property2 = new DateTime(2009, 02, 01);
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
		}

		public void TestDecorate_DomesticInternationalFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_RL_NKOrigin = "AUSYD";
			declaration1.JE_RL_NKFinalDestination = "AUMEL";
			declaration2.JE_RL_NKOrigin = "AUSYD";
			declaration2.JE_RL_NKFinalDestination = "UAIEV";
			declaration3.JE_RL_NKOrigin = "AUSYD";
			declaration3.JE_RL_NKFinalDestination = "NZZZZ";

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var moduleFilter = (ModuleTextFilter)filter[TrackingDeclarationFilterConstants.DomesticInternational];
			moduleFilter.IsActive = true;

			moduleFilter.Property = JobShipmentFilterBusinessObject.DomesticInternationalFilterItems.Domestic;
			collection.Load(filter.Filter);

			AssertEquals("Domestic", 1, collection.Count);

			moduleFilter.Property = JobShipmentFilterBusinessObject.DomesticInternationalFilterItems.International;
			collection.Load(filter.Filter);

			AssertEquals("International", 2, collection.Count);

			moduleFilter.Property = JobShipmentFilterBusinessObject.DomesticInternationalFilterItems.All;
			collection.Load(filter.Filter);

			AssertEquals("All", 3, collection.Count);
		}

		public void TestDecorate_ImporterCompanyNameFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration1.Importer.OH_FullName = "Microsoft";

			declaration2.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration2.Importer.OH_FullName = "McDonalds";

			Factory.Save();

			AssertCompanyNameFilter(TrackingDeclarationFilterConstants.ImporterCompanyName);
		}

		public void TestDecorate_SupplierCompanyNameFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration1.Supplier.OH_FullName = "Microsoft";

			declaration2.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration2.Supplier.OH_FullName = "McDonalds";

			Factory.Save();

			AssertCompanyNameFilter(TrackingDeclarationFilterConstants.SupplierCompanyName);
		}

		void AssertCompanyNameFilter(string filterName)
		{
			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var moduleFilter = (ModuleTextFilter)filter[filterName];

			moduleFilter.Property = "Micro";
			moduleFilter.IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);

			moduleFilter.Property = "M";
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
		}

		public void TestDecorate_SendReceiveForwardersFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			declaration1.JE_OH_Forwarder = sendingForwarder.PK;
			declaration1.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			declaration2.JE_OH_Forwarder = sendingForwarder.PK;
			declaration2.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			declaration3.JE_OH_Forwarder = receivingForwarder.PK;
			declaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var moduleFilter = (ModuleGuidsFilter)filter[TrackingDeclarationFilterConstants.SendReceiveForwarders];
			moduleFilter.IsActive = true;

			moduleFilter.Property1 = sendingForwarder.PK;
			moduleFilter.Property2 = ZGuid.Empty;
			collection.Load(filter.Filter);

			AssertEquals("Sending forwarders", 2, collection.Count);

			moduleFilter.Property1 = ZGuid.Empty;
			moduleFilter.Property2 = receivingForwarder.PK;
			collection.Load(filter.Filter);

			AssertEquals("Receiving forwarders", 1, collection.Count);

			moduleFilter.Property1 = sendingForwarder.PK;
			moduleFilter.Property2 = receivingForwarder.PK;
			collection.Load(filter.Filter);

			AssertEquals("Declaration can have either sending forwarder or receiving forwarder but not both together", 0, collection.Count);
		}

		public void TestDecorate_ServiceLevelFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var serviceLevels = new ActiveServiceLevelCollection(Factory);
			declaration1.JE_RS_NKServiceLevel = serviceLevels[0].RS_Code;
			declaration2.JE_RS_NKServiceLevel = serviceLevels[1].RS_Code;

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var moduleFilter = (ModuleNkFilter)filter[DeclarationFilterConstants.ServiceLevel];

			moduleFilter.Property = serviceLevels[0].RS_Code;
			moduleFilter.IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("Expected service level", serviceLevels[0].RS_Code, collection[0].JE_RS_NKServiceLevel);
		}

		public void TestStatusFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.DocsAndCartage.JP_DeliveryCartageCompleted = DateTime.Now.AddDays(-1);
			((IBusinessObjectInternals)declaration2.DocsAndCartage).Row["JP_DeliveryCartageCompleted"] = DBNull.Value;
			((IBusinessObjectInternals)declaration3.DocsAndCartage).Row["JP_DeliveryCartageCompleted"] = DBNull.Value;

			Factory.Save();

			var filter = GetDecoratedFilterObject();
			var collection = new BaseJobDeclarationCollection(new BusinessObjectFactory());

			var moduleFilter = (ModuleTextFilter)filter[TrackingDeclarationFilterConstants.Status];
			moduleFilter.IsActive = true;

			moduleFilter.Property = ShipmentStatus.Codes.Delivered;
			collection.Load(filter.Filter);

			AssertEquals("Delivered", 1, collection.Count);

			moduleFilter.Property = ShipmentStatus.Codes.Undelivered;
			collection.Load(filter.Filter);

			AssertEquals("Undelivered", 2, collection.Count);

			moduleFilter.Property = ShipmentStatus.Codes.All;
			collection.Load(filter.Filter);

			AssertEquals("All", 3, collection.Count);
		}

		//protected override bool ShouldBeLocalizable
		//{
		//	get { return false; }
		//}

		#region Implementation

		JobDeclarationFilterBusinessObject GetDecoratedFilterObject(bool decorate = true)
		{
			var factory = new TrackingJobDeclarationFilterBusinessObjectFactory();
			var filterObject = factory.GetJobDeclarationFilterBusinessObject("AU");
			if (decorate)
			{
				ShipmentJobDeclarationFilterDecorator.Decorate(filterObject);
			}

			return filterObject;
		}

		#endregion
	}
}
