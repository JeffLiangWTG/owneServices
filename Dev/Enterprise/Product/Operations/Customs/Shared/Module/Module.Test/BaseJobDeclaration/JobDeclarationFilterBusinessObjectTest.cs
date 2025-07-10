using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Module;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestInvoiceLineSpecificFieldSubGroup()
		{
			var invLineSubGroup = new JobDeclarationFilterBusinessObject.InvoiceLineSpecificFieldSubGroup();
			var query = new ZQuery();
			query.AddToFilter(JobComInvoiceLineSchema.JI_PrimaryPreference, "500");

			AssertEquals("Use ClusterKey", "JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_PrimaryPreference = '500')", invLineSubGroup.GetSubQuery(query).LiteralTextADO);
		}

		public void TestEntryInstructionSubGroup()
		{
			var entryInstructionSubGroup = new JobDeclarationFilterBusinessObject.EntryInstructionSubGroup();
			var query = new ZQuery();
			query.AddToFilter(CusEntryInstructionSchema.CEI_SubStyle, "X");

			AssertEquals("Use ClusterKey", "JE_ClusterKey IN (SELECT CEI_ClusterKey FROM dbo.CusEntryInstruction WHERE CEI_SubStyle = 'X')", entryInstructionSubGroup.GetSubQuery(query).LiteralTextADO);
		}

		#region Test Filter Max Length
		public void TestFilterMaxLength()
		{
			var filterCollection = new JobDeclarationFilterBusinessObject().ModuleFilters;
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Billing Branch Code should be set correctly.", GlbBranchSchema.GB_Code.MaxLength, filterCollection["Billing Branch Code"].MaxLength);
				AssertEquals("MaxLength of Billing Department Code should be set correctly.", GlbDepartmentSchema.GE_Code.MaxLength, filterCollection["Billing Department Code"].MaxLength);
			});
		}

		#endregion
		#region Workflow Custom Fields Filter
		public void TestCustomFieldFilter()
		{
			var filterCollection = new JobDeclarationFilterBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var template = CreateWorkflowTemplate(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode);
			AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = new JobDeclarationFilterBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion
		#region Customs Declaration Related Containers Filter
		class TestPopupFindBox : ZArchitecture.GUI.Internal.ZPopupFindBox
		{
			public IFindBoxPopup PopForm
			{
				get
				{
					return base.PopupForm;
				}
			}
		}

		public void TestAddRelatedContainersFilterWithNoRight()
		{
			AssertNoExceptionThrown(() =>
			{
				using (JobDeclarationModule module = (JobDeclarationModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration))
				using (TestPopupFindBox pop = new TestPopupFindBox())
				{
					Env.Security.CustomsDeclarationEnquiry.IsAllowed = false;
					var list = (module.FilterBusinessObject.ToList().FirstOrDefault(o => o.GetType() == typeof(RelatedContainersOfJobDeclarationFilter)) as ModuleFilterWithList).List;
					pop.ModuleID = ModuleIDs.Customs.JobDeclaration;
					pop.List = list;
					AssertNotNull(pop.List);
					var form = pop.PopForm;
				}
			});
		}

		#endregion
		#region Customs Declaration Related Transport Bookings Filter
		public void TestRelatedTransportBookingsFilter()
		{
			var dec2 = Factory.New<BaseJobDeclaration>();
			dec2.JE_MessageType = "EXP";
			var shipment = Factory.New<ForwardingShipment>();
			dec2.JE_JS = shipment.PK;
			var bookingConsolidation2 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation2.KB_ParentID = dec2.PK;
			bookingConsolidation2.KB_ParentTableCode = "JE";
			bookingConsolidation2.KB_JobDirection = "PIC";
			var bookingConsolidation3 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation3.KB_ParentID = shipment.PK;
			bookingConsolidation3.KB_ParentTableCode = "JS";
			bookingConsolidation3.KB_JobDirection = "PIC";
			var booking2 = Factory.New<DtbBooking>();
			booking2.KM_JobID = "TB002";
			booking2.KM_KB_Booking = bookingConsolidation2.PK;
			var booking3 = Factory.New<DtbBooking>();
			booking3.KM_JobID = "TB003";
			booking3.KM_KB_Booking = bookingConsolidation3.PK;
			Factory.Save();
			var filterStripBiz0 = new JobDeclarationFilterBusinessObject();
			var filter = (RelatedTransportBookingsOfJobDeclarationFilter)filterStripBiz0["Related Transport Bookings"];
			filter.IsActive = true;
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB002");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec2 }, jobDeclarations);
			filter.Clear();
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB003");
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec2 }, jobDeclarations);
		}

		#endregion
		#region Related HVLV Filters
		public void TestRelatedHVLShipmentFilter()
		{
			var standAloneDeclaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var standAloneDeclaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var standAloneDeclaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			((BusinessObject)consignment1).FillWithValidTestData();
			((BusinessObject)consignment2).FillWithValidTestData();
			((BusinessObject)consignment3).FillWithValidTestData();
			consignment1.HVC_JS_ManifestedOnShipment = shipment1.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment1.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment2.PK;
			consignment1.HVC_JE_ImportDeclaration = standAloneDeclaration1.PK;
			consignment2.HVC_JE_ImportDeclaration = standAloneDeclaration2.PK;
			consignment3.HVC_JE_ExportDeclaration = standAloneDeclaration3.PK;
			Factory.Save();
			var filterStripBiz0 = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterStripBiz0["Related HVL Shipment"];
			filter.IsActive = true;
			filter.Property = shipment1.PK;
			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { standAloneDeclaration1, standAloneDeclaration2 }, jobDeclarations);
			filter.Clear();
			filter.Property = shipment2.PK;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { standAloneDeclaration3 }, jobDeclarations);
			filter.Clear();
			filter.Property = shipment3.PK;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertEquals(0, jobDeclarations.Count);
		}

		#endregion

		#region Pickup/Delivery Drop Mode Filters

		public void TestPickupDropModeFilters()
		{
			var filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var pickupDropModefilter = (ModuleTextFilter)filterBO["Pickup Drop Mode"];
			pickupDropModefilter.IsActive = true;
			pickupDropModefilter.Property = "";

			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			shipment1.DocsAndCartage.JP_FCLPickupEquipmentNeeded = FCLEquipmentNeeded.SideLoader;

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			shipment2.DocsAndCartage.JP_FCLPickupEquipmentNeeded = FCLEquipmentNeeded.WaitForUnpack;

			BaseJobDeclaration declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.DocsAndCartage.JP_FCLPickupEquipmentNeeded = FCLEquipmentNeeded.SideLoader;

			BaseJobDeclaration declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.DocsAndCartage.JP_FCLPickupEquipmentNeeded = FCLEquipmentNeeded.LiftOffOn;

			Factory.Save();

			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should return 4 declarations", 4, collection.Count);
			AssertEquals("Should contain declaration1", true, collection.Contains(declaration1));
			AssertEquals("Should contain declaration2", true, collection.Contains(declaration2));
			AssertEquals("Should contain declaration3", true, collection.Contains(declaration3));
			AssertEquals("should contain declaration4", true, collection.Contains(declaration4));

			pickupDropModefilter.Property = "SDL";
			collection.Load(filterBO.Filter);
			AssertEquals("Should return 2 declations", 2, collection.Count);
			AssertEquals("Should contain declaration1", true, collection.Contains(declaration1));
			AssertEquals("Should contain declaration2", false, collection.Contains(declaration2));
			AssertEquals("Should contain declaration3", true, collection.Contains(declaration3));
			AssertEquals("should contain declaration4", false, collection.Contains(declaration4));
		}

		public void TestDeliveryDropModeFilters()
		{
			var filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var deliveryDropModefilter = (ModuleTextFilter)filterBO["Delivery Drop Mode"];
			deliveryDropModefilter.IsActive = true;
			deliveryDropModefilter.Property = "";

			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			shipment1.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = FCLEquipmentNeeded.WaitForUnpack;

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			shipment2.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = FCLEquipmentNeeded.WaitForUnpack;

			BaseJobDeclaration declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = FCLEquipmentNeeded.Trailer;

			BaseJobDeclaration declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = FCLEquipmentNeeded.LiftOffOn;

			Factory.Save();

			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should return 4 declarations", 4, collection.Count);
			AssertEquals("Should contain declaration1", true, collection.Contains(declaration1));
			AssertEquals("Should contain declaration2", true, collection.Contains(declaration2));
			AssertEquals("Should contain declaration3", true, collection.Contains(declaration3));
			AssertEquals("should contain declaration4", true, collection.Contains(declaration4));

			deliveryDropModefilter.Property = "WUP";
			collection.Load(filterBO.Filter);

			AssertEquals("Should return 2 declations", 2, collection.Count);
			AssertEquals("Should contain declaration1", true, collection.Contains(declaration1));
			AssertEquals("Should contain declaration2", true, collection.Contains(declaration2));
			AssertEquals("Should contain declaration3", false, collection.Contains(declaration3));
			AssertEquals("should contain declaration4", false, collection.Contains(declaration4));
		}

		#endregion

		#region Pickup/Delivery Transport Company Filters

		#region TestPickupTransportCompany

		public void TestPickupTransportCompany()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			shipment1.DocsAndCartage.PickupCartageCoPK = orgHeader1.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			declaration2.DocsAndCartage.PickupCartageCoPK = orgHeader2.PK;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment3 = Factory.New<ForwardingShipment>();
			declaration3.JE_JS = shipment3.PK;
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;
			shipment3.DocsAndCartage.PickupCartageCoPK = orgHeader3.PK;

			Factory.Save();

			var filter = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter["Pickup Transport Company"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(declaration1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(declaration3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestPickupTransportCompany_BlankOperators()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			shipment1.DocsAndCartage.PickupCartageCoPK = orgHeader1.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			shipment2.DocsAndCartage.PickupCartageCoPK = orgHeader2.PK;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.DocsAndCartage.JP_OA_PickupCartageCoAddr = ZGuid.Empty;

			Factory.Save();

			var collection = new BaseJobDeclarationCollection(Factory);
			var filter = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter["Pickup Transport Company"];
			orgFilter.IsActive = true;

			orgFilter.Property = ZGuid.Empty;
			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);
			AssertEquals("Contains 1 declaration", 1, collection.Count);

			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);
			AssertEquals("Contains declaration1 & declaration2", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, collection);
		}

		#endregion

		#region TestDeliveryTransportCompany

		public void TestDeliveryTransportCompany()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			shipment1.DocsAndCartage.DeliveryCartageCoPK = orgHeader1.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			declaration2.DocsAndCartage.DeliveryCartageCoPK = orgHeader2.PK;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment3 = Factory.New<ForwardingShipment>();
			declaration3.JE_JS = shipment3.PK;
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;
			shipment3.DocsAndCartage.DeliveryCartageCoPK = orgHeader3.PK;

			Factory.Save();

			var filter = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter["Delivery Transport Company"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(declaration1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(declaration3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestDeliveryTransportCompany_BlankOperators()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment1 = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment1.PK;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			shipment1.DocsAndCartage.DeliveryCartageCoPK = orgHeader1.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment2 = Factory.New<ForwardingShipment>();
			declaration2.JE_JS = shipment2.PK;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			shipment2.DocsAndCartage.DeliveryCartageCoPK = orgHeader2.PK;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;

			Factory.Save();

			var collection = new BaseJobDeclarationCollection(Factory);
			var filter = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter["Delivery Transport Company"];
			orgFilter.IsActive = true;

			orgFilter.Property = ZGuid.Empty;
			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);
			AssertEquals("Contains 1 declaration", 1, collection.Count);

			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);
			AssertEquals("Contains declaration1 & declaration2", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, collection);
		}

		#endregion

		#endregion

		#region Test Pickup/Delivery Organization

		public void TestPickupFromOrganization()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "PICKUP ORG";

			var anotherOrg = OrgHeader.New(Factory);
			anotherOrg.OH_Code = "OH2";
			anotherOrg.OH_FullName = "ANOTHER ORG";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.SupplierPickupAddress.OrganisationPK = org.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPickupAddress.OrganisationPK = org.PK;
			var shipmentDeclaration = BaseJobDeclaration.New(Factory);
			shipmentDeclaration.JE_JS = shipment.PK;

			var anotherDeclaration = BaseJobDeclaration.New(Factory);
			anotherDeclaration.SupplierPickupAddress.OrganisationPK = anotherOrg.PK;

			var anotherShipment = Factory.New<ForwardingShipment>();
			anotherShipment.ConsignorPickupAddress.OrganisationPK = anotherOrg.PK;
			var anotherShipmentDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentDeclaration.JE_JS = anotherShipment.PK;

			var emptyDeclaration = BaseJobDeclaration.New(Factory);

			var emptyShipment = Factory.New<ForwardingShipment>();
			var emptyShipmentDeclaration = BaseJobDeclaration.New(Factory);
			emptyShipmentDeclaration.JE_JS = emptyShipment.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.PickupFrom];
			AssertType<ConsignorCollection>("Filter's lookup list", filter.List);

			filter.IsActive = true;
			filter.Property = org.PK;
			TestPickupFromDeliveryToOrganizationCore(filter, declaration.PK, shipmentDeclaration.PK, anotherDeclaration.PK, anotherShipmentDeclaration.PK, emptyDeclaration.PK, emptyShipmentDeclaration.PK);
		}

		public void TestDeliveryToOrganization()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "DELIVERY ORG";

			var anotherOrg = OrgHeader.New(Factory);
			anotherOrg.OH_Code = "OH2";
			anotherOrg.OH_FullName = "ANOTHER ORG";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.ImporterDeliveryAddress.OrganisationPK = org.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org.PK;
			var shipmentDeclaration = BaseJobDeclaration.New(Factory);
			shipmentDeclaration.JE_JS = shipment.PK;

			var anotherDeclaration = BaseJobDeclaration.New(Factory);
			anotherDeclaration.ImporterDeliveryAddress.OrganisationPK = anotherOrg.PK;

			var anotherShipment = Factory.New<ForwardingShipment>();
			anotherShipment.ConsigneeDeliveryAddress.OrganisationPK = anotherOrg.PK;
			var anotherShipmentDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentDeclaration.JE_JS = anotherShipment.PK;

			var emptyDeclaration = BaseJobDeclaration.New(Factory);

			var emptyShipment = Factory.New<ForwardingShipment>();
			var emptyShipmentDeclaration = BaseJobDeclaration.New(Factory);
			emptyShipmentDeclaration.JE_JS = emptyShipment.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.DeliveryTo];
			AssertType<ConsigneeCollection>("Filter's lookup list", filter.List);

			filter.IsActive = true;
			filter.Property = org.PK;
			TestPickupFromDeliveryToOrganizationCore(filter, declaration.PK, shipmentDeclaration.PK, anotherDeclaration.PK, anotherShipmentDeclaration.PK, emptyDeclaration.PK, emptyShipmentDeclaration.PK);
		}

		void TestPickupFromDeliveryToOrganizationCore(ModuleGuidFilter filter, ZGuid declarationPK, ZGuid shipmentDeclarationPK, ZGuid anotherDeclarationPK, ZGuid anotherShipmentDeclarationPK, ZGuid emptyDeclarationPK, ZGuid emptyShipmentDeclarationPK)
		{
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			var declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("Equal", () =>
			{
				AssertEquals("Matched 2 Declarations", 2, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclaration", declarationPKs.Contains(shipmentDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("NotEqual", () =>
			{
				AssertEquals("Matched 4 Declarations", 4, collection.Count);
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("IsBlank", () =>
			{
				AssertEquals("Matched 2 Declarations", 2, collection.Count);
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("IsNotBlank", () =>
			{
				AssertEquals("Matched 4 Declarations", 4, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclarations", declarationPKs.Contains(shipmentDeclarationPK));
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
			});
		}

		#endregion

		#region Test Pick/Delivery Name

		public void TestPickupFromName()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "PICKUP ORG";

			var anotherOrg = OrgHeader.New(Factory);
			anotherOrg.OH_Code = "OH2";
			anotherOrg.OH_FullName = "ANOTHER ORG";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.SupplierPickupAddress.OrganisationPK = org.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPickupAddress.OrganisationPK = org.PK;
			var shipmentDeclaration = BaseJobDeclaration.New(Factory);
			shipmentDeclaration.JE_JS = shipment.PK;

			var declarationOverrided = BaseJobDeclaration.New(Factory);
			declarationOverrided.SupplierPickupAddress.E2_AddressOverride = true;
			declarationOverrided.SupplierPickupAddress.E2_CompanyName = "PICKUP ORG";

			var shipmentOverrided = Factory.New<ForwardingShipment>();
			shipmentOverrided.ConsignorPickupAddress.E2_AddressOverride = true;
			shipmentOverrided.ConsignorPickupAddress.E2_CompanyName = "PICKUP ORG";
			var shipmentOverridedDeclaration = BaseJobDeclaration.New(Factory);
			shipmentOverridedDeclaration.JE_JS = shipmentOverrided.PK;

			var anotherDeclaration = BaseJobDeclaration.New(Factory);
			anotherDeclaration.SupplierPickupAddress.OrganisationPK = anotherOrg.PK;

			var anotherShipment = Factory.New<ForwardingShipment>();
			anotherShipment.ConsignorPickupAddress.OrganisationPK = anotherOrg.PK;
			var anotherShipmentDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentDeclaration.JE_JS = anotherShipment.PK;

			var anotherDeclarationOverrided = BaseJobDeclaration.New(Factory);
			anotherDeclarationOverrided.SupplierPickupAddress.E2_AddressOverride = true;
			anotherDeclarationOverrided.SupplierPickupAddress.E2_CompanyName = "ANOTHER ORG";

			var anotherShipmentOverrided = Factory.New<ForwardingShipment>();
			anotherShipmentOverrided.ConsignorPickupAddress.E2_AddressOverride = true;
			anotherShipmentOverrided.ConsignorPickupAddress.E2_CompanyName = "ANOTHER ORG";
			var anotherShipmentOverridedDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentOverridedDeclaration.JE_JS = anotherShipmentOverrided.PK;

			var emptyDeclaration = BaseJobDeclaration.New(Factory);

			var emptyShipment = Factory.New<ForwardingShipment>();
			var emptyShipmentDeclaration = BaseJobDeclaration.New(Factory);
			emptyShipmentDeclaration.JE_JS = emptyShipment.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.PickupFromName];
			filter.IsActive = true;

			TestPickupFromDeliveryToNameCore(filter, "PICKUP ORG",
				declaration.PK, shipmentDeclaration.PK, declarationOverrided.PK, shipmentOverridedDeclaration.PK,
				anotherDeclaration.PK, anotherShipmentDeclaration.PK, anotherDeclarationOverrided.PK, anotherShipmentOverridedDeclaration.PK,
				emptyDeclaration.PK, emptyShipmentDeclaration.PK);
		}

		public void TestDeliveryToName()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "DELIVERY ORG";

			var anotherOrg = OrgHeader.New(Factory);
			anotherOrg.OH_Code = "OH2";
			anotherOrg.OH_FullName = "ANOTHER ORG";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.ImporterDeliveryAddress.OrganisationPK = org.PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org.PK;
			var shipmentDeclaration = BaseJobDeclaration.New(Factory);
			shipmentDeclaration.JE_JS = shipment.PK;

			var declarationOverrided = BaseJobDeclaration.New(Factory);
			declarationOverrided.ImporterDeliveryAddress.E2_AddressOverride = true;
			declarationOverrided.ImporterDeliveryAddress.E2_CompanyName = "DELIVERY ORG";

			var shipmentOverrided = Factory.New<ForwardingShipment>();
			shipmentOverrided.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipmentOverrided.ConsigneeDeliveryAddress.E2_CompanyName = "DELIVERY ORG";
			var shipmentOverridedDeclaration = BaseJobDeclaration.New(Factory);
			shipmentOverridedDeclaration.JE_JS = shipmentOverrided.PK;

			var anotherDeclaration = BaseJobDeclaration.New(Factory);
			anotherDeclaration.ImporterDeliveryAddress.OrganisationPK = anotherOrg.PK;

			var anotherShipment = Factory.New<ForwardingShipment>();
			anotherShipment.ConsigneeDeliveryAddress.OrganisationPK = anotherOrg.PK;
			var anotherShipmentDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentDeclaration.JE_JS = anotherShipment.PK;

			var anotherDeclarationOverrided = BaseJobDeclaration.New(Factory);
			anotherDeclarationOverrided.ImporterDeliveryAddress.E2_AddressOverride = true;
			anotherDeclarationOverrided.ImporterDeliveryAddress.E2_CompanyName = "ANOTHER ORG";

			var anotherShipmentOverrided = Factory.New<ForwardingShipment>();
			anotherShipmentOverrided.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			anotherShipmentOverrided.ConsigneeDeliveryAddress.E2_CompanyName = "ANOTHER ORG";
			var anotherShipmentOverridedDeclaration = BaseJobDeclaration.New(Factory);
			anotherShipmentOverridedDeclaration.JE_JS = anotherShipmentOverrided.PK;

			var emptyDeclaration = BaseJobDeclaration.New(Factory);

			var emptyShipment = Factory.New<ForwardingShipment>();
			var emptyShipmentDeclaration = BaseJobDeclaration.New(Factory);
			emptyShipmentDeclaration.JE_JS = emptyShipment.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.DeliveryToName];
			filter.IsActive = true;

			TestPickupFromDeliveryToNameCore(filter, "DELIVERY ORG",
				declaration.PK, shipmentDeclaration.PK, declarationOverrided.PK, shipmentOverridedDeclaration.PK,
				anotherDeclaration.PK, anotherShipmentDeclaration.PK, anotherDeclarationOverrided.PK, anotherShipmentOverridedDeclaration.PK,
				emptyDeclaration.PK, emptyShipmentDeclaration.PK);
		}

		void TestPickupFromDeliveryToNameCore(ModuleTextFilter filter, ZString fullName,
			ZGuid declarationPK, ZGuid shipmentDeclarationPK, ZGuid declarationOverridedPK, ZGuid shipmentOverridedDeclarationPK,
			ZGuid anotherDeclarationPK, ZGuid anotherShipmentDeclarationPK, ZGuid anotherDeclarationOverridedPK, ZGuid anotherShipmentOverridedDeclarationPK,
			ZGuid emptyDeclarationPK, ZGuid emptyShipmentDeclarationPK)
		{
			filter.Property = fullName.Substring(0, 1);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			var declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("StartsWith", () =>
			{
				AssertEquals("Matched 4 Declarations", 4, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclaration", declarationPKs.Contains(shipmentDeclarationPK));
				Assert("declarationOverrided", declarationPKs.Contains(declarationOverridedPK));
				Assert("shipmentOverridedDeclaration", declarationPKs.Contains(shipmentOverridedDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("DoesNotStartWith", () =>
			{
				AssertEquals("Matched 6 Declarations", 6, collection.Count);
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
				Assert("anotherDeclarationOverrided", declarationPKs.Contains(anotherDeclarationOverridedPK));
				Assert("anotherShipmentOverridedDeclaration", declarationPKs.Contains(anotherShipmentOverridedDeclarationPK));
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.Property = fullName.Substring(1, fullName.Length - 2);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("Contains", () =>
			{
				AssertEquals("Matched 4 Declarations", 4, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclaration", declarationPKs.Contains(shipmentDeclarationPK));
				Assert("declarationOverrided", declarationPKs.Contains(declarationOverridedPK));
				Assert("shipmentOverridedDeclaration", declarationPKs.Contains(shipmentOverridedDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("NotContains", () =>
			{
				AssertEquals("Matched 6 Declarations", 6, collection.Count);
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
				Assert("anotherDeclarationOverrided", declarationPKs.Contains(anotherDeclarationOverridedPK));
				Assert("anotherShipmentOverridedDeclaration", declarationPKs.Contains(anotherShipmentOverridedDeclarationPK));
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.Property = fullName;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("Equal", () =>
			{
				AssertEquals("Matched 4 Declarations", 4, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclaration", declarationPKs.Contains(shipmentDeclarationPK));
				Assert("declarationOverrided", declarationPKs.Contains(declarationOverridedPK));
				Assert("shipmentOverridedDeclaration", declarationPKs.Contains(shipmentOverridedDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("NotEqual", () =>
			{
				AssertEquals("Matched 6 Declarations", 6, collection.Count);
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
				Assert("anotherDeclarationOverrided", declarationPKs.Contains(anotherDeclarationOverridedPK));
				Assert("anotherShipmentOverridedDeclaration", declarationPKs.Contains(anotherShipmentOverridedDeclarationPK));
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("IsBlank", () =>
			{
				AssertEquals("Matched 2 Declarations", 2, collection.Count);
				Assert("emptyDeclaration", declarationPKs.Contains(emptyDeclarationPK));
				Assert("emptyShipmentDeclaration", declarationPKs.Contains(emptyShipmentDeclarationPK));
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			declarationPKs = collection.Select(d => d.PK).ToArray();
			CombineAssertions("IsNotBlank", () =>
			{
				AssertEquals("Matched 8 Declarations", 8, collection.Count);
				Assert("declaration", declarationPKs.Contains(declarationPK));
				Assert("shipmentDeclaration", declarationPKs.Contains(shipmentDeclarationPK));
				Assert("declarationOverrided", declarationPKs.Contains(declarationOverridedPK));
				Assert("shipmentOverridedDeclaration", declarationPKs.Contains(shipmentOverridedDeclarationPK));
				Assert("anotherDeclaration", declarationPKs.Contains(anotherDeclarationPK));
				Assert("anotherShipmentDeclaration", declarationPKs.Contains(anotherShipmentDeclarationPK));
				Assert("anotherDeclarationOverrided", declarationPKs.Contains(anotherDeclarationOverridedPK));
				Assert("anotherShipmentOverridedDeclaration", declarationPKs.Contains(anotherShipmentOverridedDeclarationPK));
			});
		}

		#endregion

		public void TestDefaultCreatedTimeFilter()
		{
			var decFilterBO = new JobDeclarationFilterBusinessObject();
			decFilterBO.QueryObjectType = typeof(BaseJobDeclaration);
			var filter = decFilterBO.ModuleFilters["Created Time"] as ModuleDateFilter;
			AssertEquals(true, filter.Visible);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, filter.PropertySearch);
		}

		public void TestManifestNumberFilter_Disabled()
		{
			AssertNull((ModuleTextFilter)GetFilterBusinessObject(false).ModuleFilters[DeclarationFilterConstants.NumberFilterTypes.ManifestNumber]);
		}

		public void TestManifestNumberFilter_Enabled()
		{
			var filterBusinessObject = GetFilterBusinessObject(true);
			filterBusinessObject.QueryObjectType = typeof(BaseJobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[DeclarationFilterConstants.NumberFilterTypes.ManifestNumber];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_ManifestNumber = "123456";
			Factory.Save();
			CombineAssertions(() =>
			{
				textFilter.Property = "123";
				AssertEquals("find for StartsWith:123", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "369";
				AssertEquals("No result for StartsWith:369", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				textFilter.Property = "234";
				AssertEquals("find for Contains:234", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "468";
				AssertEquals("No result for Contains:468", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestClientAssignedStaffFilter()
		{
			var asserter = new FilterStripAsserter<BaseJobDeclaration>(Factory, (d) => d.JE_DeclarationReference);
			var controllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			controllingBranch.GB_Code = "XYZ";
			controllingBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
			declaration1.JE_OH_Importer = importer.PK;
			var importerStaff = Factory.NewWithValidTestData<GlbStaff>();
			importerStaff.GS_Code = "XES";
			var importerAssignment = importer.StaffAssignments.AddNew();
			importerAssignment.O8_Role = "ACT";
			importerAssignment.O8_GS_NKPersonResponsible = importerStaff.GS_Code;
			importerAssignment.O8_Department = "SEA";
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
			declaration2.JE_OH_Supplier = supplier.PK;
			var supplierStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			supplierStaff1.GS_Code = "JBB";
			var supplierAssignment1 = supplier.StaffAssignments.AddNew();
			supplierAssignment1.O8_Role = "ACT";
			supplierAssignment1.O8_GS_NKPersonResponsible = supplierStaff1.GS_Code;
			supplierAssignment1.O8_Department = "RAI";
			var supplierStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			supplierStaff2.GS_Code = "CJY";
			var supplierAssignment2 = supplier.StaffAssignments.AddNew();
			supplierAssignment2.O8_Role = "SAL";
			supplierAssignment2.O8_GS_NKPersonResponsible = supplierStaff2.GS_Code;
			supplierAssignment2.O8_Department = "ROD";
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration3.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			declaration3.Job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			var localClientAssignment = localClient.StaffAssignments.AddNew();
			localClientAssignment.O8_Department = "AIR";
			localClientAssignment.O8_Role = "SAL";
			localClientAssignment.O8_GS_NKPersonResponsible = "ABC";
			declaration3.JE_IsCancelled = false;
			Factory.Save();
			asserter.AddToScope(declaration1);
			asserter.AddToScope(declaration2);
			asserter.AddToScope(declaration3);
			var filter = (JobDeclarationClientAssignedStaffModuleFilter)filterBO["Client Assigned Staff"];
			filter.IsActive = true;
			asserter.AssertMatches("Matches all since client type not specified", filter, declaration1, declaration2, declaration3);
			filter.ClientType = "IMP";
			filter.StaffRole = "ACT";
			filter.AssignedStaff = "XES";
			filter.Department = "SEA";
			filter.ControllingBranch = controllingBranch.PK;
			asserter.AssertMatches("Matches importer", filter, declaration1);
			filter.ClientType = "SUP";
			filter.StaffRole = "ACT";
			filter.AssignedStaff = "JBB";
			filter.Department = "RAI";
			filter.ControllingBranch = controllingBranch.PK;
			asserter.AssertMatches("Matches supplier/accountant", filter, declaration2);
			filter.ClientType = "SUP";
			filter.StaffRole = "SAL";
			filter.AssignedStaff = "CJY";
			filter.Department = "ROD";
			filter.ControllingBranch = controllingBranch.PK;
			asserter.AssertMatches("Matches supplier/sales rep", filter, declaration2);
			filter.ClientType = "LOC";
			filter.StaffRole = "SAL";
			filter.AssignedStaff = ZString.Empty;
			filter.Department = "AIR";
			filter.ControllingBranch = ZGuid.Empty;
			asserter.AssertMatches("Matches local client", filter, declaration3);
		}

		public void TestCompareCustomAndCommonFilters()
		{
			ModuleNumberFilter customFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.Common];
			customFilter.Property = "1234";
			customFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			customFilter.IsActive = true;
			ModuleNumberFilter declarationFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.DeclarationReference];
			ModuleNumberFilter entryNumberFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			ModuleNumberFilter houseBillFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.HouseBill];
			declarationFilter.IsCommon = true;
			declarationFilter.IsActive = true;
			entryNumberFilter.IsCommon = true;
			entryNumberFilter.IsActive = true;
			houseBillFilter.IsCommon = true;
			houseBillFilter.IsActive = true;
			ModuleNumberFilter commonFilter = (ModuleNumberFilter)filterBO["Common Numbers and References"];
			customFilter.Property = "1'234";
			customFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			commonFilter.IsActive = true;
			customFilter.Property = "1234";
			AssertContainsExactElementsInAnyOrder(customFilter, commonFilter);
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(35), customFilter.MaxLength);
		}

		public void TestNoExceptionInCommonFilter()
		{
			BaseJobDeclaration[] retrievedDeclarations;
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var filter = (ModuleNumberFilter)filterBO["Common Numbers and References"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "1'23";
			var retrievingFactory = new BusinessObjectFactory();
			AssertNoExceptionThrown("Should have no nasty SQL injection exceptions", delegate
			{
				retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			});
			filter.Property = "123";
			retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found NO declarations on exact search", 0, retrievedDeclarations.Length);
		}

		public void TestGetCompanyQuery()
		{
			JobDeclarationFilterBusinessObject bizObj = new JobDeclarationFilterBusinessObject();
			ZQuery companyQuery = bizObj.Filter;
			AssertContains(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK).LiteralTextADO, companyQuery.LiteralTextADO);
			bizObj.CountryCode = GlbCompany.CurrentCompany.Country.Code;
			companyQuery = bizObj.Filter;
			AssertContains(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK).LiteralTextADO, companyQuery.LiteralTextADO);
			bizObj.CountryCode = CountryCodes.Bangladesh;
			companyQuery = bizObj.Filter;
			AssertNotContains(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK).LiteralTextADO, companyQuery.LiteralTextADO);
		}

		public void TestWorkflowFiltersPresent()
		{
			JobDeclarationFilterBusinessObject milestoneFilter = new JobDeclarationFilterBusinessObject();
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}

		public void TestAdditionalReferenceNumberFilterExists()
		{
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull((filter[DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber]));
		}

		public void TestAdditionalReferenceNumberIssueDateFilterExists()
		{
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull((filter[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]));
		}

		public void TestContructorNullInstanceReference()
		{
			var code = "AU";
			SetCurrentCountry(code);
			Factory.Save();
			AssertEquals(code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			GlbCompany.CurrentCompany.Delete();
			Factory.Save();
			AssertEquals(code, GetCountry());
			var declaration = new JobDeclarationFilterBusinessObject();
			AssertEquals(code, declaration.CountryCode);
		}

		ZString GetCountry()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		void SetCurrentCountry(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = code;
		}

		public void TestGetInvoiceNumberQueryFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoiceHeader1 = declaration1.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "test1";
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "test2";
			Factory.Save();
			var invoiceNumberQueryFilter = (ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber];
			invoiceNumberQueryFilter.Property = "test";
			invoiceNumberQueryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			invoiceNumberQueryFilter.Property = "ALL INVOICES";
			invoiceNumberQueryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
		}

		public void TestGetInvoiceAmountQueryFilter()
		{
			RunAmountFilterTest("Invoice Amount", (BaseJobComInvoiceHeader jz, ZDecimal amount) =>
			{
				jz.JZ_InvoiceAmount = amount;
			});
		}

		void RunAmountFilterTest(string filterName, Action<BaseJobComInvoiceHeader, ZDecimal> amountSetter)
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[filterName]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoiceHeader1 = declaration1.Invoices.AddNew();
			amountSetter(invoiceHeader1, 0m);
			invoiceHeader1.JZ_InvoiceNumber = "test1";
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			amountSetter(invoiceHeader2, 87m);
			invoiceHeader2.JZ_InvoiceNumber = "test2";
			Factory.Save();
			var amountQueryFilter = (ModuleNumberRangeFilter)filter[filterName];
			amountQueryFilter.Decimals = 2;
			amountQueryFilter.Property1 = 0m;
			amountQueryFilter.Property2 = 0m;
			amountQueryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			var amountQueryFilter2 = (ModuleNumberRangeFilter)filter[filterName];
			amountQueryFilter2.Decimals = 2;
			amountQueryFilter2.BetweenDefaultProperty1 = 50m;
			amountQueryFilter2.BetweenDefaultProperty2 = 100m;
			amountQueryFilter2.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			var amountQueryFilter3 = (ModuleNumberRangeFilter)filter[filterName];
			amountQueryFilter3.Decimals = 2;
			amountQueryFilter3.BetweenDefaultProperty1 = 100m;
			amountQueryFilter3.BetweenDefaultProperty2 = 200m;
			amountQueryFilter3.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
		}

		public void TestARTransactionFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter["AR Transaction #"]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration1.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = declaration1;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001005";
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001005";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "B00009999";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_LineType = TransactionLineTypes.WIP;
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			ModuleFountainFilter fountainFilter = (ModuleFountainFilter)filter["AR Transaction #"];
			fountainFilter.Property = "00001005";
			fountainFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			fountainFilter.Property = "00001001";
			fountainFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			fountainFilter.Property = "";
			fountainFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestCommodityCode()
		{
			RefCommodityCode commodityCode1 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			RefCommodityCode commodityCode2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";
			RefCommodityCode commodityCode3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode3.RH_Code = "COM3";
			BaseJobDeclaration declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header1 = declaration1.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_RH_NKCommodity_Code = commodityCode1.RH_Code;
			BaseJobDeclaration declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header2 = declaration2.Invoices.AddNew();
			BaseJobComInvoiceLine line21 = header2.JobComInvoiceLines.AddNew();
			line21.JI_RH_NKCommodity_Code = commodityCode1.RH_Code;
			BaseJobComInvoiceLine line22 = header2.JobComInvoiceLines.AddNew();
			line22.JI_RH_NKCommodity_Code = commodityCode2.RH_Code;
			BaseJobDeclaration declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header3 = declaration3.Invoices.AddNew();
			BaseJobComInvoiceLine line3 = header3.JobComInvoiceLines.AddNew();
			line3.JI_RH_NKCommodity_Code = commodityCode3.RH_Code;
			Factory.Save();
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			((ModuleNkFilter)filter["Commodity Code"]).Property = commodityCode1.RH_Code;
			((ModuleNkFilter)filter["Commodity Code"]).IsActive = true;
			BaseJobDeclarationCollection declarationCollection = new BaseJobDeclarationCollection(Factory);
			declarationCollection.Load(filter.Filter);
			AssertEquals(2, declarationCollection.Count);
			AssertCollectionContains(declaration1, declarationCollection);
			AssertCollectionContains(declaration2, declarationCollection);
			AssertCollectionNotContains(declaration3, declarationCollection);
			((ModuleNkFilter)filter["Commodity Code"]).Property = commodityCode2.RH_Code;
			declarationCollection.Load(filter.Filter);
			AssertEquals(1, declarationCollection.Count);
			AssertCollectionContains(declaration2, declarationCollection);
			AssertCollectionNotContains(declaration1, declarationCollection);
			AssertCollectionNotContains(declaration3, declarationCollection);
		}

		public void TestCustomFields()
		{
			BaseJobDeclaration declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header1 = declaration1.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_CustomAttrib1 = "blh";
			BaseJobDeclaration declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobComInvoiceHeader header2 = declaration2.Invoices.AddNew();
			BaseJobComInvoiceLine line2 = header2.JobComInvoiceLines.AddNew();
			line2.JI_CustomAttrib1 = "hlb";
			Factory.Save();
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			((ModuleTextFilter)filter["ComInvoiceLine.CustomAttribute1"]).Property = "hl";
			((ModuleTextFilter)filter["ComInvoiceLine.CustomAttribute1"]).IsActive = true;
			BusinessObject[] filteredCommercialInvoices = Factory.Load(typeof(BaseJobDeclaration), filter.Filter);
			AssertEquals(1, filteredCommercialInvoices.Length);
			AssertEquals(declaration2, filteredCommercialInvoices[0]);
		}

		public void TestJobStatusFilterDoesNotFilterOutJobDeclarationsWithShipment()
		{
			BaseJobDeclaration standaloneDeclaration = Factory.New<BaseJobDeclaration>();
			JobHeader standaloneDeclarationHeader = Factory.NewJobForTesting<JobHeader>();
			standaloneDeclarationHeader.JH_JobNum = "J2";
			standaloneDeclarationHeader.JH_ParentID = standaloneDeclaration.PK;
			standaloneDeclarationHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			standaloneDeclarationHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			standaloneDeclarationHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			standaloneDeclarationHeader.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			standaloneDeclarationHeader.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobHeader shipmentLinkedHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentLinkedHeader.JH_JobNum = "J1";
			shipmentLinkedHeader.JH_ParentID = shipment.PK;
			shipmentLinkedHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			shipmentLinkedHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			shipmentLinkedHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentLinkedHeader.JH_Status = JobHeaderStatus.Working.Code;
			shipmentLinkedHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			BaseJobDeclaration shipmentLinkedDeclaration = Factory.New<BaseJobDeclaration>();
			shipmentLinkedDeclaration.JE_JS = shipment.PK;
			Factory.Save();
			AssertSame(shipmentLinkedDeclaration, shipment.GetDeclaration());
			AssertEquals("declaration.JE_DeclarationReference", "B00001000", standaloneDeclaration.JE_DeclarationReference);
			AssertEquals("shipmentDeclaration.JE_DeclarationReference", "S00001000", shipmentLinkedDeclaration.JE_DeclarationReference);
			BaseJobDeclarationCollection allDeclarations = new BaseJobDeclarationCollection(Factory);
			allDeclarations.Load();
			AssertCollectionContains("standaloneDeclaration", standaloneDeclaration, allDeclarations);
			AssertCollectionContains("shipmentLinkedDeclaration", shipmentLinkedDeclaration, allDeclarations);
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			((ModuleTextFilter)filter["Job Status"]).Property = JobHeaderStatus.Working.Code;
			((ModuleTextFilter)filter["Job Status"]).IsActive = true;
			BaseJobDeclarationCollection filteredDeclarations = new BaseJobDeclarationCollection(Factory);
			filteredDeclarations.Load(filter.Filter);
			AssertCollectionNotContains("declaration", standaloneDeclaration, filteredDeclarations);
			AssertCollectionContains("shipmentDeclaration", shipmentLinkedDeclaration, filteredDeclarations);
			((ModuleTextFilter)filter["Job Status"]).Property = JobHeaderStatus.JobReadyForCostPosting.Code;
			filteredDeclarations.Load(filter.Filter);
			AssertCollectionContains("declaration", standaloneDeclaration, filteredDeclarations);
			AssertCollectionNotContains("shipmentDeclaration", shipmentLinkedDeclaration, filteredDeclarations);
		}

		public void TestAdditionalNumbersFilterFunction()
		{
			BaseJobDeclaration declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobDeclaration declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			CusEntryNumber number1 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number2 = Factory.NewWithValidTestData<CusEntryNumber>();
			//make sure the dates are not equal
			number1.CE_IssueDate = new ZDateTime(2008, 6, 17);
			number1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number2.CE_IssueDate = new ZDateTime(2008, 6, 19);
			number2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number1.CE_EntryNum = "EN000001";
			number1.CE_EntryNum = "EN000002";
			declaration1.AdditionalReferenceNumbers.Add(number1);
			declaration2.AdditionalReferenceNumbers.Add(number2);
			Factory.Save();
			JobDeclarationFilterBusinessObject filter1 = new JobDeclarationFilterBusinessObject();
			((ModuleTextBaseFilter)filter1[DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber]).Property = number1.CE_EntryNum;
			((ModuleTextBaseFilter)filter1[DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber]).IsActive = true;
			JobDeclarationFilterBusinessObject filter2 = new JobDeclarationFilterBusinessObject();
			((ModuleDateFilter)filter2[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter2[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]).Property1 = number2.CE_IssueDate;
			((ModuleDateFilter)filter2[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]).Property2 = number2.CE_IssueDate;
			((ModuleDateFilter)filter2[CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate]).IsActive = true;
			BaseJobDeclarationCollection declarationCollection1 = new BaseJobDeclarationCollection(Factory);
			BaseJobDeclarationCollection declarationCollection2 = new BaseJobDeclarationCollection(Factory);
			declarationCollection1.Load(filter1.Filter);
			AssertCollectionContains(declaration1, declarationCollection1);
			AssertCollectionNotContains(declaration2, declarationCollection1);
			declarationCollection2.Load(filter2.Filter);
			AssertCollectionNotContains(declaration1, declarationCollection2);
			AssertCollectionContains(declaration2, declarationCollection2);
		}

		public void TestETAFilter()
		{
			BaseJobDeclaration declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			BaseJobDeclaration declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_DateAtFinalDestination = new ZDateTime(2005, 1, 2);
			declaration2.JE_DateAtFinalDestination = new ZDateTime(2005, 2, 1);
			Factory.Save();
			JobDeclarationFilterBusinessObject filter = new JobDeclarationFilterBusinessObject();
			((ModuleDateFilter)filter[DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter[DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter[DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter[DeclarationFilterConstants.DateFilterTypes.EstimatedTimeOfArrival]).IsActive = true;
			BaseJobDeclarationCollection declarationCollection = new BaseJobDeclarationCollection(Factory);
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains(declaration1, declarationCollection);
			AssertCollectionNotContains(declaration2, declarationCollection);
		}

		public virtual void TestAgentReferenceFilters()
		{
			ModuleTextFilter agentsReference = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.AgentsReference];
			agentsReference.IsActive = true;
			agentsReference.Property = "XXX";
			agentsReference.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			BaseJobDeclaration testDec1 = Factory.New<BaseJobDeclaration>();
			testDec1.JE_AgentsReference = "Test 1";
			BaseJobDeclaration testDec2 = Factory.New<BaseJobDeclaration>();
			testDec2.JE_AgentsReference = "Test 2";
			Factory.Save();
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);
			agentsReference.Property = "Test";
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 2, collection.Count);
			agentsReference.SqlComparisonOperator = SQLComparisonOperator.Equal;
			agentsReference.Property = "XXX";
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 0, collection.Count);
			agentsReference.Property = "Test 1";
			collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
		}

		public virtual void TestApplicationCodeFilters()
		{
			var filterBO = (JobDeclarationFilterBusinessObject)Activator.CreateInstance(typeof(JobDeclarationFilterBusinessObject));
			if (filterBO != null)
			{
				ModuleTextFilter applicationCode = (ModuleTextFilter)filterBO[filterBO.ApplicationCodeFilterCaption];
				applicationCode.IsActive = true;
				applicationCode.Property = "CCC";
				applicationCode.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				BaseJobDeclaration testDec1 = Factory.New<BaseJobDeclaration>();
				testDec1.JE_ApplicationCode = "AAA";
				BaseJobDeclaration testDec2 = Factory.New<BaseJobDeclaration>();
				testDec2.JE_ApplicationCode = "BBB";
				Factory.Save();
				BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
				collection.Load(filterBO.Filter);
				AssertEquals("Not decs", 0, collection.Count);
				applicationCode.Property = "AAA";
				collection = new BaseJobDeclarationCollection(Factory);
				collection.Load(filterBO.Filter);
				AssertEquals("Decs", 1, collection.Count);
			}
		}

		public void TestFilterWithCountryCode()
		{
			GlbCompany nZCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch branch = nZCompany.Branches.AddNew();
			branch.GB_RL_NKHomePort = filterBO.CountryCode + "XXX";
			BaseJobDeclaration dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_GB = branch.PK;
			Factory.Save();
			ZString literalText = filterBO.Filter.LiteralTextADO;
			AssertEquals("Contains only one JE_GB query", 1, literalText.OccurrencesIgnoringCase("JE_GB in"));
		}

		public void TestCountryCode()
		{
			filterBO.CountryCode = "AU";
			AssertEquals("Country Code", "AU", filterBO.CountryCode);
		}

		public void TestOnlyLoadJobsCreatedForCurrentCompanyWhenMultipleCompaniesInSameCountry()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";
			otherCompany.GC_RN_NKCountryCode = currentCompany.GC_RN_NKCountryCode; // create another company in the same country
			GlbBranch branchInOtherCompany = otherCompany.Branches.AddNew();
			branchInOtherCompany.GB_Code = "OTH";
			branchInOtherCompany.GB_BranchName = "Other Company Branch";
			branchInOtherCompany.GB_RL_NKHomePort = currentCompany.GC_RN_NKCountryCode + "XXX";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branchInOtherCompany.GB_OH_OrgProxy = orgProxy.PK;
			GlbBranch alternateBranchCurrentCompany = Factory.NewWithValidTestData<GlbBranch>();
			alternateBranchCurrentCompany.GB_GC = currentCompany.PK;
			alternateBranchCurrentCompany.GB_Code = "ALT";
			alternateBranchCurrentCompany.GB_RL_NKHomePort = currentCompany.GC_RN_NKCountryCode + "ZZZ";
			alternateBranchCurrentCompany.GB_OH_OrgProxy = orgProxy.PK;
			GlbCompany companyInOtherCountry = Factory.New<GlbCompany>();
			companyInOtherCountry.GC_Code = "OTC";
			companyInOtherCountry.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, currentCompany.GC_RN_NKCountryCode)).RN_Code; // create another company in the another country
			GlbBranch branchInOtherCountry = companyInOtherCountry.Branches.AddNew();
			branchInOtherCountry.GB_Code = "OTC";
			branchInOtherCountry.GB_BranchName = "Other Country Company Branch";
			branchInOtherCountry.GB_RL_NKHomePort = companyInOtherCountry.GC_RN_NKCountryCode + "XXX";
			branchInOtherCountry.GB_OH_OrgProxy = orgProxy.PK;
			BaseJobDeclaration jobInThisCompany = BaseJobDeclaration.New(Factory);
			jobInThisCompany.JE_GB = currentBranch.PK;
			jobInThisCompany.JE_DeclarationReference = "X0000";
			jobInThisCompany.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInAlternateBranch = BaseJobDeclaration.New(Factory);
			jobInAlternateBranch.JE_GB = alternateBranchCurrentCompany.PK;
			jobInAlternateBranch.JE_DeclarationReference = "Z0000100";
			jobInAlternateBranch.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInOtherCompany = BaseJobDeclaration.New(Factory);
			jobInOtherCompany.JE_GB = branchInOtherCompany.PK;
			jobInOtherCompany.JE_DeclarationReference = "X0001";
			jobInOtherCompany.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInOtherCountry = BaseJobDeclaration.New(Factory);
			jobInOtherCountry.JE_GB = branchInOtherCountry.PK;
			jobInOtherCountry.JE_DeclarationReference = "C0001";
			jobInOtherCountry.DontReAssignReferenceNoForUnitTest = true;
			Factory.Save();
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			filterBO.CountryCode = ZString.Empty;
			ZQuery filter = filterBO.Filter;
			collection.Load(filter);
			AssertEquals("No jobs as country has not been specified", 0, collection.Count);
			filterBO.CountryCode = GlbCompany.CurrentCompany.Country.Code;
			filter = filterBO.Filter;
			filter.OrderBy = BaseJobDeclaration.Schema.JE_DeclarationReference;
			AssertEquals(BaseJobDeclaration.Schema.JE_DeclarationReference, filter.OrderBy);
			collection.Load(filter);
			AssertEquals("Only records relating to the branches for the company logged into should display", 2, collection.Count);
			AssertEquals("This company's main branch job", "X0000", collection[0].JE_DeclarationReference);
			AssertEquals("This company's alternate branch job", "Z0000100", collection[1].JE_DeclarationReference);
			filterBO.CountryCode = companyInOtherCountry.GC_RN_NKCountryCode;
			filter = filterBO.Filter;
			collection.Load(filter);
			AssertEquals("Only records relating to the branches for the other country", 1, collection.Count);
			AssertEquals("C0001", collection[0].JE_DeclarationReference);
		}

		public void TestLoadJobsFromAllCountriesInWebTracker()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";
			otherCompany.GC_RN_NKCountryCode = currentCompany.GC_RN_NKCountryCode; // create another company in the same country
			GlbBranch branchInOtherCompany = otherCompany.Branches.AddNew();
			branchInOtherCompany.GB_Code = "OTH";
			branchInOtherCompany.GB_BranchName = "Other Company Branch";
			branchInOtherCompany.GB_RL_NKHomePort = currentCompany.GC_RN_NKCountryCode + "XXX";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branchInOtherCompany.GB_OH_OrgProxy = orgProxy.PK;
			GlbBranch alternateBranchCurrentCompany = Factory.NewWithValidTestData<GlbBranch>();
			alternateBranchCurrentCompany.GB_GC = currentCompany.PK;
			alternateBranchCurrentCompany.GB_Code = "ALT";
			alternateBranchCurrentCompany.GB_RL_NKHomePort = currentCompany.GC_RN_NKCountryCode + "ZZZ";
			alternateBranchCurrentCompany.GB_OH_OrgProxy = orgProxy.PK;
			GlbCompany companyInOtherCountry = Factory.New<GlbCompany>();
			companyInOtherCountry.GC_Code = "OTC";
			companyInOtherCountry.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, currentCompany.GC_RN_NKCountryCode)).RN_Code; // create another company in the another country
			GlbBranch branchInOtherCountry = companyInOtherCountry.Branches.AddNew();
			branchInOtherCountry.GB_Code = "OTC";
			branchInOtherCountry.GB_BranchName = "Other Country Company Branch";
			branchInOtherCountry.GB_RL_NKHomePort = companyInOtherCountry.GC_RN_NKCountryCode + "XXX";
			branchInOtherCountry.GB_OH_OrgProxy = orgProxy.PK;
			BaseJobDeclaration jobInThisCompany = BaseJobDeclaration.New(Factory);
			jobInThisCompany.JE_GB = currentBranch.PK;
			jobInThisCompany.JE_DeclarationReference = "X0000";
			jobInThisCompany.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInAlternateBranch = BaseJobDeclaration.New(Factory);
			jobInAlternateBranch.JE_GB = alternateBranchCurrentCompany.PK;
			jobInAlternateBranch.JE_DeclarationReference = "Z0000100";
			jobInAlternateBranch.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInOtherCompany = BaseJobDeclaration.New(Factory);
			jobInOtherCompany.JE_GB = branchInOtherCompany.PK;
			jobInOtherCompany.JE_DeclarationReference = "X0001";
			jobInOtherCompany.DontReAssignReferenceNoForUnitTest = true;
			BaseJobDeclaration jobInOtherCountry = BaseJobDeclaration.New(Factory);
			jobInOtherCountry.JE_GB = branchInOtherCountry.PK;
			jobInOtherCountry.JE_DeclarationReference = "C0001";
			jobInOtherCountry.DontReAssignReferenceNoForUnitTest = true;
			Factory.Save();
			//Test for Web
			Globals.IsWeb = true;
			filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject(); // need to get new filterBO otherwise moduleFilters stay the same as IsWeb = false
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			// No Country Filter
			filterBO.CountryCode = ZString.Empty;
			ZQuery filter = filterBO.Filter;
			collection.Load(filter);
			AssertEquals("For Web, all 4 jobs should be shown even no country is specified", 4, collection.Count);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInAlternateBranch, collection);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInOtherCountry, collection);
			// Country Filter 1
			filterBO.CountryCode = GlbCompany.CurrentCompany.Country.Code;
			filter = filterBO.Filter;
			collection.Load(filter);
			AssertEquals("For Web, all 4 jobs should be shown regardless of the country code", 4, collection.Count);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInAlternateBranch, collection);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInOtherCountry, collection);
			// Country Filter 2
			filterBO.CountryCode = companyInOtherCountry.GC_RN_NKCountryCode;
			filter = filterBO.Filter;
			collection.Load(filter);
			AssertEquals("For Web, all 4 jobs should be shown regardless of the country code", 4, collection.Count);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInAlternateBranch, collection);
			AssertCollectionContains(jobInThisCompany, collection);
			AssertCollectionContains(jobInOtherCountry, collection);
		}

		public virtual void TestJE_EntryStatus_NotSentFilter()
		{
			Assert("Filter does not contain JE_EntryStatus = '' when entry status empty", filterBO.Filter.LiteralTextADO.IndexOf("JE_EntryStatus = ''") == -1);
			var entryStatus = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			entryStatus.IsActive = true;
			entryStatus.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			entryStatus.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert("Filter contains JE_EntryStatus = '' when NotSent is filtered on", filterBO.Filter.LiteralTextADO.IndexOf("JE_EntryStatus = ''") != -1);
		}

		public virtual void TestJE_EntryStatusFilterProperties()
		{
			var entryStatus = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			entryStatus.IsActive = true;
			AssertEquals(BaseJobDeclaration.Schema.JE_EntryStatusMaxLength, entryStatus.MaxLength);
			AssertEquals(true, entryStatus.ShowComparisonOperator);

			var shouldShowFilterType = DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code, GlbCompany.CurrentCompany.PK);
			AssertEquals(shouldShowFilterType, entryStatus.ShowFilterType);
		}

		public void TestJE_HouseBill()
		{
			BaseJobDeclaration decl = BaseJobDeclaration.New(Factory);
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.HouseBill];
			filter.IsActive = true;
			filter.Property = "Test";
			decl.JE_HouseBill = "test";
			Factory.Save();
			BaseJobDeclarationCollection retrievedBOs = new BaseJobDeclarationCollection(Factory);
			retrievedBOs.Load(filterBO.Filter);
			AssertEquals("JE_HouseBill", "test", retrievedBOs[0].JE_HouseBill.ToLower());
		}

		public void TestSecondHouseBill()
		{
			BaseJobDeclaration decl = BaseJobDeclaration.New(Factory);
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.HouseBill];
			filter.IsActive = true;
			filter.Property = "Test";
			Bill bill = decl.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "Test";
			Factory.Save();
			BaseJobDeclarationCollection retrievedBOs = new BaseJobDeclarationCollection(Factory);
			retrievedBOs.Load(filterBO.Filter);
			AssertEquals(decl, retrievedBOs[0]);
		}

		public void TestHouseBillIsBlank()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var bill2 = declaration2.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var bill3 = declaration3.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_HouseBill = "Test";
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.HouseBill];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Factory.Save();
			var retrievedBOs = new BaseJobDeclarationCollection(Factory);
			retrievedBOs.Load(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, retrievedBOs);
		}

		/// <summary>
		///
		///		"B00001910";
		///		"B00001911";
		///		"B00001912";
		///		"B00001916";
		///
		///		"S00001911";
		///		"S00001912";
		///		"S00001913";
		///		"S00001915";
		///
		///		"G00001915";

		/// </summary>
		public void TestJobNumber()
		{
			BusinessObject[] filteredDecs = null;
			AddTestShipmentData();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.DeclarationReference];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
			filter.Property = "S";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find >= 4 records but found " + filteredDecs.Length, filteredDecs.Length >= 4);
			filter.Property = "S1912";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "1912";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);
			filter.Property = "B";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find >= 4 records but found " + filteredDecs.Length, filteredDecs.Length >= 4);
			filter.Property = "1915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 2 records but found " + filteredDecs.Length, filteredDecs.Length == 2);
			filter.Property = "G1915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "G00001915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = " G00001915 , S1912 ";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 2 record but found " + filteredDecs.Length, filteredDecs.Length == 2);
			filter.Property = "1M40570281D";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should not find any record", true, filteredDecs.Length == 0);
			filter.Property = "13A";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "G13A";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "G0001756";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "1756";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.Property = "GG00001915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should not find any record", true, filteredDecs.Length == 0);
			filter.Property = "GC00001915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 1 record but found " + filteredDecs.Length, filteredDecs.Length == 1);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "B0000";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Records Expected for " + filter.SqlComparisonOperator.ToString() + " [" + filter.Property + "]", 4, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "G0000191";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Records Expected for " + filter.SqlComparisonOperator.ToString() + " [" + filter.Property + "]", 1, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "915";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Records Expected for " + filter.SqlComparisonOperator.ToString() + " [" + filter.Property + "]", 3, filteredDecs.Length);
		}

		/// <summary>
		/// EntryNum1 = "1M15353189291";
		/// EntryNum2 = "1M41818302981";
		/// EntryNum3 = "1S11818192873";
		/// EntryNum4 = "1A15353192273";
		/// EntryNum5 = "1M17222222222";
		/// EntryNum6 = "1S18222222222";
		/// EntryNum7 = "2M15353222222";
		/// EntryNum8 = "3S13333333333";
		/// </summary>
		public void TestEntryNumberFilter()
		{
			BusinessObject[] filteredDecs = null;
			AddTestEntryNumberData();
			Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
			filter.Property = "1M1";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(35), filter.MaxLength);
			filter.Property = "M1";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 3, filteredDecs.Length);
			filter.Property = "1M1M";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 0, filteredDecs.Length);
			filter.Property = "222222";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			filter.Property = "3S13333333333";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			filter.Property = "1818";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Record", 2, filteredDecs.Length);
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records", filteredDecs.Length >= 8);
		}

		[TestDate(2023, 12, 31)]
		public void TestEntryNumberWithNotFilter()
		{
			var filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			BusinessObject[] filteredDecs = null;
			var factory = new BusinessObjectFactory();
			var testDec1 = AddJobDeclaration("A1", factory);
			var testDec2 = AddJobDeclaration("A2", factory);
			var testDec3 = AddJobDeclaration("A3", factory);
			var testDec4 = AddJobDeclaration("A4", factory);
			_ = AddJobDeclaration("A5", factory);
			var testDecWithHeader1 = AddJobDeclaration("B1", factory);
			var testDecWithHeader2 = AddJobDeclaration("B2", factory);
			var testDecWithHeader3 = AddJobDeclaration("B3", factory);
			var testDecWithHeader4 = AddJobDeclaration("B4", factory);
			var testHead1 = AddCusEntryHeader(testDecWithHeader1, factory);
			var testHead2 = AddCusEntryHeader(testDecWithHeader2, factory);
			var testHead3 = AddCusEntryHeader(testDecWithHeader3, factory);
			var testHead4 = AddCusEntryHeader(testDecWithHeader4, factory);
			_ = AddCusEntryNumber("AA1", testDec1, factory);
			_ = AddCusEntryNumber("AA2", testDec2, factory);
			_ = AddCusEntryNumber("AA3", testDec3, factory);
			_ = AddCusEntryNumber("AA4", testDec4, factory);
			_ = AddCusEntryNumber("BB1", testHead1, factory);
			_ = AddCusEntryNumber("BB2", testHead2, factory);
			_ = AddCusEntryNumber("BB3", testHead3, factory);
			_ = AddCusEntryNumber("BB4", testHead4, factory);
			_ = AddCusEntryNumber("", testHead4, factory);
			Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(factory);
			factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "AA1";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 8 Records", 8, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "BB";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);
			filter.Property = "AA";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "4";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 7 Records", 7, filteredDecs.Length);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.Property = "";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.Property = "";
			filteredDecs = factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 8 Records", 8, filteredDecs.Length);
		}

		/// <summary>
		/// MasterBillNum1 = "999-00119291";
		/// MasterBillNum2 = "999-01192918";
		/// </summary>
		public void TestMasterBillSearch()
		{
			BusinessObject[] filteredDecs = null;
			AddTestMasterBillData();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.MasterBill];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
			filter.Property = "999-";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should find 8 Records", 8, filteredDecs.Length);
			filter.Property = "999-00119291";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should find 5 Records", 5, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should find 5 Records", 5, filteredDecs.Length);
			filter.Property = "999-0119291";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should find 0 Records", 0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should find 3 Records", 3, filteredDecs.Length);
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
		}

		public void TestMasterBillIsBlank()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var bill2 = declaration2.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var bill3 = declaration3.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_MasterBill = "Test";
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.MasterBill];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Factory.Save();
			var retrievedBOs = new BaseJobDeclarationCollection(Factory);
			retrievedBOs.Load(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, retrievedBOs);
		}

		public void TestContainerNoSearch()
		{
			BusinessObject[] filteredDecs = null;
			BaseJobDeclaration bO = BaseJobDeclaration.New(Factory);
			bO.JE_TransportMode = TransportModes.Sea;
			bO.DisableDefaultPackingInformation = true;
			BaseCusContainer container1 = bO.CusContainers.AddNew();
			container1.CO_ContainerNumber = "contfortest1";
			BaseCusContainer container2 = bO.CusContainers.AddNew();
			container2.CO_ContainerNumber = "contfortest2";
			BaseJobDeclaration noMatch = BaseJobDeclaration.New(Factory);
			noMatch.DisableDefaultPackingInformation = true;
			BaseCusContainer noMatchContainer = noMatch.CusContainers.AddNew();
			noMatchContainer.CO_ContainerNumber = "nomatchcont";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.ContainerNumber];
			filter.IsActive = true;
			filter.Property = "contfortest1";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, filteredDecs.Length);
		}

		public void TestOwnerRefSearch()
		{
			BaseJobDeclaration[] retrievedDeclarations;
			var declarationABC = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declarationABCD = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration123 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration456ABC = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declarationEmpty = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			PrepareFilter(filter, SQLComparisonOperator.Equal, "A'BC");
			var retrievingFactory = new BusinessObjectFactory();
			AssertNoExceptionThrown("Should have no nasty SQL injection exceptions", delegate
			{
				retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			});
			PrepareFilter(filter, SQLComparisonOperator.Equal, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string>(), "EQUAL ABC => no matching OwnerRef");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string>(), "STARTS WITH ABC => no matching OwnerRef");
			declarationABC.JE_OwnerRef = "ABC";
			declarationABCD.JE_OwnerRef = "ABCD";
			declaration123.JE_OwnerRef = "123";
			declaration456ABC.JE_OwnerRef = "456ABC";
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER1";
			var order = declaration456ABC.AttachedOrders.AddNew();
			order.JD_OrderNumber = "999";
			order.BuyerPK = buyer.PK;
			Factory.Save();
			PrepareFilter(filter, SQLComparisonOperator.Equal, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef }, "EQUAL ABC => 1 matching Declaration");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef }, "STARTS WITH ABC => 2 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "1");
			AssertFilteredElements(retrievingFactory, new List<string> { declaration123.JE_OwnerRef }, "STARTS WITH 1 => 1 matching Declaration");
			PrepareFilter(filter, SQLComparisonOperator.DoesNotStartWith, "C");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration123.JE_OwnerRef, declaration456ABC.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "DOES NOT START WITH C => 4 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.DoesNotStartWith, "A");
			AssertFilteredElements(retrievingFactory, new List<string> { declaration123.JE_OwnerRef, declaration456ABC.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "DOES NOT START WITH A => 3 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.Contains, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration456ABC.JE_OwnerRef }, "CONTAINS ABC => 3 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.NotContains, "ABC");
			AssertFilteredElements(retrievingFactory, new List<string> { declaration123.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "NOT CONTAINS ABC => 2 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.Equal, "ABCD");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABCD.JE_OwnerRef }, "EQUAL ABCD => 1 matching Declaration");
			var secondOwnerFilter = (ModuleTextFilter)filterBO.ModuleFilters.AddNewDuplicateFilter(filter);
			PrepareFilter(filter, SQLComparisonOperator.NotContains, "ABCD");
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.NotContains, "456");
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declaration123.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "NOT CONTAINS ABCD AND NOT CONTAINS 456 => 3 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "456");
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.StartsWith, "999");
			AssertFilteredElements(retrievingFactory, new List<string> { }, "STARTS WITH 456 AND STARTS WITH 999 => 0 matching Declaration");
			secondOwnerFilter.GroupName = "test";
			AssertFilteredElements(retrievingFactory, new List<string> { declaration456ABC.JE_OwnerRef }, "STARTS WITH 456 AND STARTS WITH 999 (grouping) => 1 matching Declaration");
			secondOwnerFilter.GroupName = "";
			PrepareFilter(filter, SQLComparisonOperator.DoesNotStartWith, "ABC");
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.DoesNotStartWith, "456");
			AssertFilteredElements(retrievingFactory, new List<string> { declaration123.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "DOES NOT START WITH ABC AND DOES NOT START WITH 456 => 2 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "ABC", FilterOrCategory.Aqua);
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef }, "STARTS WITH ABC (as a group) => 2 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "ABC", FilterOrCategory.Aqua);
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.DoesNotStartWith, "123", FilterOrCategory.Aqua);
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration456ABC.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "STARTS WITH ABC OR DOES NOT START WITH 123 => 4 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.StartsWith, "ABC", FilterOrCategory.Aqua);
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.StartsWith, "123", FilterOrCategory.Aqua);
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration123.JE_OwnerRef }, "STARTS WITH ABC OR START WITH 123 => 3 matching Declarations");
			PrepareFilter(filter, SQLComparisonOperator.DoesNotStartWith, "456", FilterOrCategory.Aqua);
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.DoesNotStartWith, "999", FilterOrCategory.Aqua);
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration123.JE_OwnerRef, declaration456ABC.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "DOES NOT STARTS WITH 456 OR DOES NOT START WITH 999 => 4 matching Declarations");
			secondOwnerFilter.GroupName = "test";
			PrepareFilter(filter, SQLComparisonOperator.DoesNotStartWith, "456", FilterOrCategory.Aqua);
			PrepareFilter(secondOwnerFilter, SQLComparisonOperator.DoesNotStartWith, "999", FilterOrCategory.Aqua);
			AssertFilteredElements(retrievingFactory, new List<string> { declarationABC.JE_OwnerRef, declarationABCD.JE_OwnerRef, declaration123.JE_OwnerRef, declarationEmpty.JE_OwnerRef }, "DOES NOT STARTS WITH 456 OR DOES NOT START WITH 999 (grouping) => 4 matching Declarations");
		}

		void PrepareFilter(ModuleTextFilter filter, SQLComparisonOperator op, ZString value, FilterOrCategory? orCategory = null)
		{
			filter.SqlComparisonOperator = op;
			filter.IsActive = true;
			if (orCategory != null)
			{
				filter.OrCategory = orCategory.Value;
			}

			filter.Property = value;
		}

		void AssertFilteredElements(BusinessObjectFactory factory, IEnumerable<string> expected, ZString comment)
		{
			AssertContainsExactElementsInAnyOrder(comment, expected, factory.Load(typeof(BaseJobDeclaration), filterBO.Filter)?.Select(d => ((BaseJobDeclaration)d).JE_OwnerRef));
		}

		public void TestOrderNumberOnInvoicesLineSearch()
		{
			BusinessObject[] filteredDecs = null;
			BaseJobDeclaration declaration1 = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader1 = declaration1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobDeclaration declaration2 = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader2 = declaration2.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			line1.JI_OrderNumber = "1234";
			declaration1.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			BaseJobComInvoiceLine line2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			line2.JI_OrderNumber = "333";
			BaseJobComInvoiceLine line3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			line3.JI_OrderNumber = "333";
			BaseJobComInvoiceLine line4 = invoiceHeader2.JobComInvoiceLines.AddNew();
			line4.JI_OrderNumber = "12345";
			declaration1.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			declaration2.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "333";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 declarations", 2, filteredDecs.Length);
			filter.Property = "1234";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 declaration", 1, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1234";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 declarations", 2, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1234567";
			filteredDecs = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found no declarations", 0, filteredDecs.Length);
		}

		public void TestOrderNumberAttachedToDeclarationSearch()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "xxx";
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			BaseJobDeclaration[] retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have read none initially", 0, retrievedDeclarations.Length);
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "xxx";
			declaration.AttachedOrders.Add(order);
			Factory.Save();
			retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have read 1", 1, retrievedDeclarations.Length);
			AssertEquals("Should have read the correct one", "xxx", retrievedDeclarations[0].AttachedOrders[0].JD_OrderNumber);
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "yyy";
			declaration.AttachedOrders.Add(order);
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "zzz";
			declaration.AttachedOrders.Add(order);
			Factory.Save();
			filter.Property = " xxx , yyy ";
			retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have read 2", 2, retrievedDeclarations.Length);
			var orderNumbers = retrievedDeclarations.Select(x => x.AttachedOrders.First().JD_OrderNumber).ToList();
			orderNumbers.Sort();
			var str = orderNumbers.Aggregate((x, y) => x + ", " + y);
			AssertEquals("xxx, yyy", str);
		}

		public void TestOrderReferencesSearch()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "xxx";
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			BaseJobDeclaration[] retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have read none initially", 0, retrievedDeclarations.Length);
			OrderItem item = declaration.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "xxx";
			Factory.Save();
			retrievedDeclarations = (BaseJobDeclaration[])retrievingFactory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have read 1", 1, retrievedDeclarations.Length);
			AssertEquals("Should have read the correct one", "xxx", retrievedDeclarations[0].DocsAndCartage.OrderItems[0].JT_OrderReference);
		}

		public void TestOrderReferencesSearchWhenDeclarationAttachedToShipment()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "xxx";
			var retrievingFactory = new BusinessObjectFactory();
			var retrievedDeclarations = retrievingFactory.Load<BaseJobDeclaration>(filterBO.Filter);
			AssertEquals("Should have read none initially", 0, retrievedDeclarations.Length);
			var item = declaration.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "xxx";
			Factory.Save();
			retrievingFactory = new BusinessObjectFactory();
			retrievedDeclarations = retrievingFactory.Load<BaseJobDeclaration>(filterBO.Filter);
			AssertEquals("Should have read 1", 1, retrievedDeclarations.Length);
			AssertEquals("Should have read the correct one", "xxx", retrievedDeclarations[0].DocsAndCartage.OrderItems[0].JT_OrderReference);
		}

		#region Importer

		public void TestImporterSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Importer, 1, DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Importer, 1, DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, false);
		}

		public virtual void TestImporterNameSearch()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "IMP ORG";
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_OH_Importer = org.PK;
			var noMatchDec = BaseJobDeclaration.New(Factory);
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterName];
			filter.IsActive = true;
			filter.Property = "I";
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched 1 Declaration on Importer Name", 1, collection.Count);
			AssertEquals("Matched Declaration", declaration.PK, collection[0].PK);
		}

		#endregion

		public void TestDepartmentBranch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = companyPK;
			job.JH_GE = departmentPK;
			job.JH_GS_NKRepOps = newStaff1.GS_Code;
			Factory.Save();
			var filter1 = (ModuleGuidFilter)filterBO["Declaration Branch"];
			filter1.IsActive = true;
			filter1.Property = branchPK;
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JE_GB", 1, collection.Count);
			AssertEquals("JE_GB", branchPK, collection[0].JE_GB);
		}

		public void TestFilterByBillingBranchWithShipments()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GB = branch.PK;
			jobHeader1.JH_ParentID = declaration.PK;
			var shipmentDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentDeclaration.JE_JS = shipment.PK;
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_GB = branch.PK;
			jobHeader2.JH_ParentID = shipment.PK;
			Factory.Save();
			var filter = (ModuleGuidFilter)filterBO["Billing Branch"];
			filter.Property = branch.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filter.Query);
			AssertEquals("Both declaration and shipmentDeclaration should exist in the result", 2, collection.Count);
			AssertNotNull("The second result should have a shipment", collection[1].Shipment);
		}

		public void TestBillingBranch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			declaration.JE_IsCancelled = false;
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			inactiveHeaderDeclaration.JE_IsCancelled = false;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = branch.GB_GC;
			job.JH_GE = departmentPK;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = branch.GB_GC;
			inactiveJob.JH_GE = departmentPK;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				Factory.Save();
			}

			var filter = (ModuleGuidFilter)filterBO["Billing Branch"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = branchPK;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GB", 1, collection.Count);
			AssertEquals("JH_GB", branchPK, collection[0].Job.JH_GB);
			AssertEquals("inactive header declaration not in collection", false, collection.Contains(inactiveHeaderDeclaration));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("No Matched JH_GB", 0, collection.Count);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var nonCurrentBranch = Factory.New<GlbBranch>();
			nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			nonCurrentBranch.GB_Code = "NCB";
			job.JH_GB = nonCurrentBranch.PK;
			Factory.Save();
			var collection2 = new BaseJobDeclarationCollection(Factory);
			collection2.Load(filterBO.Filter);
			AssertEquals("No Match JH_GB in other company", 0, collection2.Count);
		}

		public void TestBillingDepartmentSearch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = companyPK;
			job.JH_GE = departmentPK;
			job.JH_GS_NKRepOps = newStaff1.GS_Code;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = companyPK;
			inactiveJob.JH_GE = departmentPK;
			inactiveJob.JH_GS_NKRepOps = newStaff1.GS_Code;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				job2.JH_GS_NKRepOps = newStaff1.GS_Code;
				Factory.Save();
			}

			var filter = (ModuleGuidFilter)filterBO["Billing Department"];
			filter.IsActive = true;
			filter.Property = departmentPK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GE", 1, collection.Count);
			AssertEquals("JH_GE", departmentPK, collection[0].Job.JH_GE);
			AssertEquals("inactive header declaration not in collection", false, collection.Contains(inactiveHeaderDeclaration));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GE", 0, collection.Count);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var nonCurrentDept = Factory.New<GlbDepartment>();
			nonCurrentDept.GE_Code = "CC";
			job.JH_GE = nonCurrentDept.PK;
			Factory.Save();
			collection.Load(filterBO.Filter);
			AssertEquals("No Match JH_GE in other company", 0, collection.Count);
		}

		public void TestBillingBranchCodeSearch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var branchCode = branch.GB_Code;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			declaration.JE_IsCancelled = false;
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			inactiveHeaderDeclaration.JE_IsCancelled = false;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = branch.GB_GC;
			job.JH_GE = departmentPK;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = branch.GB_GC;
			inactiveJob.JH_GE = departmentPK;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				Factory.Save();
			}

			var filter = (ModuleTextFilter)filterBO["Billing Branch Code"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = branchCode;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched GB_Code", 1, collection.Count);
			AssertEquals("JH_GB", branchPK, collection[0].Job.JH_GB);
			AssertEquals("inactive header declaration not in collection", false, collection.Contains(inactiveHeaderDeclaration));
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched GB_Code", 1, collection.Count);
			AssertEquals("JH_GB", branchPK, collection[0].Job.JH_GB);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("No Matched GB_Code", 0, collection.Count);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var nonCurrentBranch = Factory.New<GlbBranch>();
			nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			nonCurrentBranch.GB_Code = "NCB";
			job.JH_GB = nonCurrentBranch.PK;
			Factory.Save();
			var collection2 = new BaseJobDeclarationCollection(Factory);
			collection2.Load(filterBO.Filter);
			AssertEquals("No Match GB_Code in other company", 0, collection2.Count);
		}

		public void TestBillingDepartmentCodeSearch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = companyPK;
			job.JH_GE = departmentPK;
			job.JH_GS_NKRepOps = newStaff1.GS_Code;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = companyPK;
			inactiveJob.JH_GE = departmentPK;
			inactiveJob.JH_GS_NKRepOps = newStaff1.GS_Code;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				job2.JH_GS_NKRepOps = newStaff1.GS_Code;
				Factory.Save();
			}

			var filter = (ModuleTextFilter)filterBO["Billing Department Code"];
			filter.IsActive = true;
			filter.Property = departmentCode;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched GE_Code", 1, collection.Count);
			AssertEquals("JH_GE", departmentPK, collection[0].Job.JH_GE);
			AssertEquals("inactive header declaration not in collection", false, collection.Contains(inactiveHeaderDeclaration));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GE", 0, collection.Count);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched GE_Code", 1, collection.Count);
			AssertEquals("JH_GE", departmentPK, collection[0].Job.JH_GE);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var nonCurrentDept = Factory.New<GlbDepartment>();
			nonCurrentDept.GE_Code = "CC";
			job.JH_GE = nonCurrentDept.PK;
			Factory.Save();
			collection.Load(filterBO.Filter);
			AssertEquals("No Match GE_Code in other company", 0, collection.Count);
		}

		public void TestBillingOperatorSearch()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var newStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST2";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = companyPK;
			job.JH_GE = departmentPK;
			job.JH_GS_NKRepOps = newStaff1.GS_Code;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = companyPK;
			inactiveJob.JH_GE = departmentPK;
			inactiveJob.JH_GS_NKRepOps = newStaff1.GS_Code;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				job2.JH_GS_NKRepOps = newStaff2.GS_Code;
				Factory.Save();
			}

			var filter4 = (ModuleNkFilter)filterBO["Billing Operator"];
			filter4.IsActive = true;
			filter4.Property = newStaff1.GS_Code;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GS_NKRepOps", 1, collection.Count);
			AssertEquals("JH_GS_NKRepOps", newStaff1.GS_Code, collection[0].Job.JH_GS_NKRepOps);
			AssertEquals("inactive header declaration not in collection", false, collection.Contains(inactiveHeaderDeclaration));
			filter4.Property = newStaff2.GS_Code;
			var collection2 = new BaseJobDeclarationCollection(Factory);
			collection2.Load(filterBO.Filter);
			AssertEquals("No Match JH_GS_NKRepOps in other company", 0, collection2.Count);
		}

		public void TestBillingTaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var branch = GlbBranch.CurrentBranch;
			var branchPK = branch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;
			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";
			declaration.JE_IsCancelled = false;
			var inactiveHeaderDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			inactiveHeaderDeclaration.JE_GB = branchPK;
			inactiveHeaderDeclaration.JE_DeclarationReference = "B01234568";
			inactiveHeaderDeclaration.JE_IsCancelled = false;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = branch.GB_GC;
			job.JH_GE = departmentPK;
			job.JH_GB_TaxBranch = branchPK;
			var inactiveJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			inactiveJob.JH_ParentID = inactiveHeaderDeclaration.PK;
			inactiveJob.JH_ParentTableCode = "JE";
			inactiveJob.JH_GB = branchPK;
			inactiveJob.JH_GC = branch.GB_GC;
			inactiveJob.JH_GE = departmentPK;
			inactiveJob.JH_GB_TaxBranch = branchPK;
			Factory.Save();

			inactiveJob.MarkAsInactive();
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration.PK;
				job2.JH_ParentTableCode = "JE";
				job2.JH_GB = branchPK;
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GE = departmentPK;
				job2.JH_GB_TaxBranch = branchPK;
				Factory.Save();
			}

			var filter = (ModuleGuidFilter)filterBO["Billing Tax Branch"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = branchPK;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched JH_GB_TaxBranch", 1, collection.Count);
			AssertEquals("JH_GB_TaxBranch", branchPK, collection[0].Job.JH_GB_TaxBranch);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterBO.Filter);
			AssertEquals("No Matched JH_GB_TaxBranch", 0, collection.Count);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var nonCurrentBranch = Factory.New<GlbBranch>();
			nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			nonCurrentBranch.GB_Code = "NCB";
			job.JH_GB_TaxBranch = nonCurrentBranch.PK;
			Factory.Save();
			var collection2 = new BaseJobDeclarationCollection(Factory);
			collection2.Load(filterBO.Filter);
			AssertEquals("No Match JH_GB_TaxBranch in other company", 0, collection2.Count);
		}

		#region Supplier

		public virtual void TestSupplierSearch()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "addr1";
			org.OH_Code = "-1-";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_OH_Supplier = org.PK;
			BaseJobDeclaration noMatchDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader noMatchInvoice = noMatchDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			noMatchInvoice.JZ_JE = noMatchDec.PK;
			OrgHeader decSupplier = OrgHeader.New(Factory);
			decSupplier.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			decSupplier.OH_Code = "DECSUP";
			declaration.JE_OH_Supplier = decSupplier.PK;
			Factory.Save();
			ModuleGuidsFilter filter = (ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property2 = org.PK;
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Invoice Header", 1, collection.Count);
			AssertEquals("Matched supplier on Invoice Header", org.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);
			filter.Property2 = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);
			filter.Property2 = decSupplier.PK;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, collection.Count);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, collection[0].JE_OH_Supplier);
		}

		public virtual void TestSupplierNameSearch()
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = "OH1";
			org.OH_FullName = "SUP ORG";
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_OH_Supplier = org.PK;
			var noMatchDec = BaseJobDeclaration.New(Factory);
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.SupplierName];
			filter.IsActive = true;
			filter.Property = "S";
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched 1 Declaration on Supplier Name", 1, collection.Count);
			AssertEquals("Matched Declaration", declaration.PK, collection[0].PK);
		}

		#endregion

		public void TestShippingLineSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ShippingLine, 1, DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ShippingLine, 1, DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder, false);
		}

		public void TestForwarderSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Forwarder, 2, DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Forwarder, 2, DeclarationFilterConstants.OrgFilterTypes.ShippingLineForwarder, false);
		}

		public void TestExternalBrokerSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ExternalBroker, 1, DeclarationFilterConstants.OrgFilterTypes.ExternalBroker, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ExternalBroker, 1, DeclarationFilterConstants.OrgFilterTypes.ExternalBroker, false);
		}

		public void TestControllingAgentSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ControllingAgent, 1, DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ControllingAgent, 1, DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, false);
		}

		public void TestControllingCustomerSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ControllingCustomer, 1, DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, true);
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_ControllingCustomer, 1, DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, false);
		}

		protected void TestOrganisationSearch(string declPropName, int property1Or2, string orgFilterType, bool isOrgActive)
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = isOrgActive ? "_!_!_!" : "ABC";
			org.OH_IsActive = isOrgActive;
			var declaration = BaseJobDeclaration.New(Factory);
			declaration[declPropName] = org.PK;
			Factory.Save();
			var filter = filterBO[orgFilterType];
			if (filter is ModuleGuidsFilter guidsFilter)
			{
				if (property1Or2 == 1)
				{
					guidsFilter.Property1 = org.PK;
				}
				else
				{
					guidsFilter.Property2 = org.PK;
				}
			}
			else if (filter is ModuleGuidFilter guidFilter)
			{
				guidFilter.Property = org.PK;
			}

			filter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			if (isOrgActive)
			{
				if (filter is ModuleGuidsFilter moduleGuidsFilter)
				{
					if (property1Or2 == 1)
					{
						AssertNoWarning(moduleGuidsFilter.Property1Info, "Organization is in-active.");
					}
					else
					{
						AssertNoWarning(moduleGuidsFilter.Property2Info, "Organization is in-active.");
					}
				}
				else if (filter is ModuleGuidFilter moduleGuidFilter)
				{
					AssertNoWarning(moduleGuidFilter.PropertyInfo, "Organization is in-active.");
				}
			}
			else
			{
				if (filter is ModuleGuidsFilter moduleGuidsFilter)
				{
					if (property1Or2 == 1)
					{
						AssertHasWarning(moduleGuidsFilter.Property1Info, "Organization is in-active.");
					}
					else
					{
						AssertHasWarning(moduleGuidsFilter.Property2Info, "Organization is in-active.");
					}
				}
				else if (filter is ModuleGuidFilter moduleGuidFilter)
				{
					AssertHasWarning(moduleGuidFilter.PropertyInfo, "Organization is in-active.");
				}
			}

			AssertEquals("Matched " + orgFilterType + " on declaration", 1, collection.Count);
			AssertEquals("Matched " + orgFilterType + " on declaration", org.PK, collection[0][declPropName]);
		}

		public void TestPortOfLoadingSearch()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.LoadDischarge];
			filter.IsActive = true;
			filter.Property1 = "NZAKL";
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
			filter.Property1 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestPortOfArrivalSearch()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.LoadDischarge];
			filter.IsActive = true;
			filter.Property2 = "NZAKL";
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
			filter.Property2 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestOriginPortSearch()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.OriginDestination];
			filter.IsActive = true;
			filter.Property1 = "NZAKL";
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
			filter.Property1 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestDestinationPortSearch()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.OriginDestination];
			filter.IsActive = true;
			filter.Property2 = "NZAKL";
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
			filter.Property2 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestCommercialInvoicePaymentDate()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_JE = declaration.PK;
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.CommercialInvoicePaymentDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2003, 1, 1);
			filter.Property2 = new ZDateTime(2003, 1, 30);
			invoice.JZ_PaymentDate = new ZDateTime(2003, 1, 15);
			groupHeader.JZ_PaymentDate = new ZDateTime(2003, 1, 30);
			Factory.Save();
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
			filter.Property1 = new ZDateTime(2003, 1, 30);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have no matches", 0, collection.Count);
		}

		public void TestCommercialInvoiceInvoiceDate()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_JE = declaration.PK;
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.CommercialInvoiceDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2003, 1, 1);
			filter.Property2 = new ZDateTime(2003, 1, 30);
			invoice.JZ_InvoiceDate = new ZDateTime(2003, 1, 15);
			groupHeader.JZ_InvoiceDate = new ZDateTime(2003, 1, 30);
			Factory.Save();
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
			filter.Property1 = new ZDateTime(2003, 1, 30);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have no matches", 0, collection.Count);
		}

		public virtual void TestPartAttrib()
		{
			BaseJobDeclaration bO1 = BaseJobDeclaration.New(Factory);
			bO1.JE_DeclarationReference = "B00000001";
			BaseJobComInvoiceHeader invoiceHeader1 = bO1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_JE = bO1.PK;
			BaseJobComInvoiceLine line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			line1.JI_PartAttrib1 = "P1";
			line1.JI_PartAttrib2 = "P2";
			line1.JI_PartAttrib3 = "P3";
			Factory.Save();
			AssertPartAttrib(DeclarationFilterConstants.NumberFilterTypes.PartAttribute1, "P1");
			AssertPartAttrib(DeclarationFilterConstants.NumberFilterTypes.PartAttribute2, "P2");
			AssertPartAttrib(DeclarationFilterConstants.NumberFilterTypes.PartAttribute3, "P3");
		}

		protected void AssertPartAttrib(string numberFilterTypes, string matchingValue)
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[numberFilterTypes];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = matchingValue;
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals(numberFilterTypes + " - Should have found 1 declaration", 1, collection.Count);
			filter.Property = "NO MATCH";
			collection.Load(filterBO.Filter);
			AssertEquals(numberFilterTypes + " - Should have found 0 declaration", 0, collection.Count);
			filter.IsActive = false;
			filter.Property = "";
		}

		#region Test Cartage Dates
		public void TestCartageAdvisedDateFilter()
		{
			AddDateValueRecords();
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.CartageAdvised];
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertProperDeclarationsLoaded(filter, declaration1, declaration2, declaration3);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertProperDeclarationsLoaded(filter, declaration4, declaration5);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 30);
			AssertProperDeclarationsLoaded(filter, declaration2);
		}

		public void TestGoodsDeliveredDateFilter()
		{
			AddDateValueRecords();
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.GoodsDelivered];
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertProperDeclarationsLoaded(filter, declaration1, declaration2, declaration3);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertProperDeclarationsLoaded(filter, declaration4, declaration5);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2004, 1, 1);
			filter.Property2 = new ZDateTime(2004, 1, 30);
			AssertProperDeclarationsLoaded(filter, declaration1, declaration3);
		}

		void AssertProperDeclarationsLoaded(ModuleDateFilter filter, params BaseJobDeclaration[] expectedDeclarations)
		{
			collection.Load(filter.Query);
			AssertEquals("Proper quantity of declarations loaded", expectedDeclarations.Length, collection.Count);
			foreach (var declaration in expectedDeclarations)
			{
				Assert("Declaration should be loaded", collection.Contains(declaration.PK));
			}
		}

		#endregion
		public void TestCartageControllerFilter()
		{
			GlbStaff cartageController = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader importAirConsigneeWithCC = Factory.NewWithValidTestData<OrgHeader>();
			importAirConsigneeWithCC.StaffAssignments.ImportAirCartageCordinator = cartageController.GS_Code;
			OrgHeader importAirConsigneeNoCC = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importSeaConsigneeWithCC = Factory.NewWithValidTestData<OrgHeader>();
			importSeaConsigneeWithCC.StaffAssignments.ImportSeaCartageCordinator = cartageController.GS_Code;
			OrgHeader importSeaConsigneeNoCC = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader exportAirConsignorWithCC = Factory.NewWithValidTestData<OrgHeader>();
			exportAirConsignorWithCC.StaffAssignments.ExportAirCartageCordinator = cartageController.GS_Code;
			OrgHeader exportAirConsignorNoCC = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader exportSeaConsignorWithCC = Factory.NewWithValidTestData<OrgHeader>();
			exportSeaConsignorWithCC.StaffAssignments.ExportSeaCartageCordinator = cartageController.GS_Code;
			OrgHeader exportSeaConsignorNoCC = Factory.NewWithValidTestData<OrgHeader>();
			BaseJobDeclaration importAirCCDeclaration = BaseJobDeclaration.New(Factory);
			importAirCCDeclaration.JE_TransportMode = TransportModes.Air;
			importAirCCDeclaration.JE_RL_NKOrigin = "IDJKT";
			importAirCCDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			importAirCCDeclaration.JE_OH_Importer = importAirConsigneeWithCC.PK;
			BaseJobDeclaration importAirNoCCDeclaration = BaseJobDeclaration.New(Factory);
			importAirNoCCDeclaration.JE_TransportMode = TransportModes.Air;
			importAirNoCCDeclaration.JE_RL_NKOrigin = "IDJKT";
			importAirNoCCDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			importAirNoCCDeclaration.JE_OH_Importer = importAirConsigneeNoCC.PK;
			BaseJobDeclaration importSeaCCDeclaration = BaseJobDeclaration.New(Factory);
			importSeaCCDeclaration.JE_TransportMode = TransportModes.Sea;
			importSeaCCDeclaration.JE_RL_NKOrigin = "IDJKT";
			importSeaCCDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			importSeaCCDeclaration.JE_OH_Importer = importSeaConsigneeWithCC.PK;
			BaseJobDeclaration importSeaNoCCDeclaration = BaseJobDeclaration.New(Factory);
			importSeaNoCCDeclaration.JE_TransportMode = TransportModes.Sea;
			importSeaNoCCDeclaration.JE_RL_NKOrigin = "IDJKT";
			importSeaNoCCDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			importSeaNoCCDeclaration.JE_OH_Importer = importSeaConsigneeNoCC.PK;
			BaseJobDeclaration exportAirCCDeclaration = BaseJobDeclaration.New(Factory);
			exportAirCCDeclaration.JE_TransportMode = TransportModes.Air;
			exportAirCCDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportAirCCDeclaration.JE_RL_NKFinalDestination = "IDJKT";
			exportAirCCDeclaration.JE_OH_Supplier = exportAirConsignorWithCC.PK;
			BaseJobDeclaration exportAirNoCCDeclaration = BaseJobDeclaration.New(Factory);
			exportAirNoCCDeclaration.JE_TransportMode = TransportModes.Air;
			exportAirNoCCDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportAirNoCCDeclaration.JE_RL_NKFinalDestination = "IDJKT";
			exportAirNoCCDeclaration.JE_OH_Supplier = exportAirConsignorNoCC.PK;
			BaseJobDeclaration exportSeaCCDeclaration = BaseJobDeclaration.New(Factory);
			exportSeaCCDeclaration.JE_TransportMode = TransportModes.Sea;
			exportSeaCCDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportSeaCCDeclaration.JE_RL_NKFinalDestination = "IDJKT";
			exportSeaCCDeclaration.JE_OH_Supplier = exportSeaConsignorWithCC.PK;
			BaseJobDeclaration exportSeaNoCCDeclaration = BaseJobDeclaration.New(Factory);
			exportSeaNoCCDeclaration.JE_TransportMode = TransportModes.Sea;
			exportSeaNoCCDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportSeaNoCCDeclaration.JE_RL_NKFinalDestination = "IDJKT";
			exportSeaNoCCDeclaration.JE_OH_Supplier = exportSeaConsignorNoCC.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			BaseJobDeclarationCollection collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			Assert("Expect collection to contain all Declarations", collection.Contains(importAirCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(importSeaCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(exportAirCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(exportSeaCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(importAirNoCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(importSeaNoCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(exportAirNoCCDeclaration));
			Assert("Expect collection to contain all Declarations", collection.Contains(exportSeaNoCCDeclaration));
			filter.Property = cartageController.PK;
			collection.Load(filterBO.Filter);
			Assert("Expect collection to contain matching Declarations", collection.Contains(importAirCCDeclaration));
			Assert("Expect collection to contain matching Declarations", collection.Contains(importSeaCCDeclaration));
			Assert("Expect collection to contain matching Declarations", collection.Contains(exportAirCCDeclaration));
			Assert("Expect collection to contain matching Declarations", collection.Contains(exportSeaCCDeclaration));
			Assert("Expect collection not to contain nonmatching Declarations", !collection.Contains(importAirNoCCDeclaration));
			Assert("Expect collection not to contain nonmatching Declarations", !collection.Contains(importSeaNoCCDeclaration));
			Assert("Expect collection not to contain nonmatching Declarations", !collection.Contains(exportAirNoCCDeclaration));
			Assert("Expect collection not to contain nonmatching Declarations", !collection.Contains(exportSeaNoCCDeclaration));
		}

		public void TestShowJobsAffectedByFCAAndFIFTApportionments()
		{
			AssertNull("The filter should not be added to start with", filterBO[DeclarationFilterConstants.ShowJobsFCA_FIFTApportionment]);
			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_IncoTerm = IncoTerms.FreeCarrier;
			BaseApportionedCharge charge1 = invoice1.GroupCharges.AddNew();
			charge1.J7_ChargeType = Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_IsApportionedCharge = true;
			declaration1.ApportionmentDirty = false;
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_IncoTerm = IncoTerms.FreeCarrier;
			BaseApportionedCharge charge2 = invoice2.GroupCharges.AddNew();
			charge2.J7_ChargeType = Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			charge2.J7_IsIncludedInITOT = false;
			declaration2.ApportionmentDirty = false;
			BaseJobDeclaration declaration3 = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice3 = declaration3.Invoices.AddNew();
			invoice3.JZ_IncoTerm = IncoTerms.FreeOnBoard;
			BaseApportionedCharge charge3 = invoice3.GroupCharges.AddNew();
			charge3.J7_ChargeType = Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			charge3.J7_IsIncludedInITOT = true;
			declaration3.ApportionmentDirty = false;
			StmData stmData = Factory.New<StmData>();
			stmData.SD_Name = "JobsWithFCAAndFIFTApportionmentErrorsExist";
			stmData.SD_Owner = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			JobDeclarationFilterBusinessObject bizObj = new JobDeclarationFilterBusinessObject();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.ShowJobsFCA_FIFTApportionment];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals(true, declaration1.MatchesFilter(bizObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(bizObj.Filter));
			AssertEquals(false, declaration3.MatchesFilter(bizObj.Filter));
		}

		[TestDate(2021, 8, 3, 23, 0, 0)]
		[TestUtcOffset(-11, 0, 0)]
		public void TestAuditDate()
		{
			AddAuditData();
			ModuleDateFilter filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday;
			collection.Load(filterBO.Filter);
			AssertEquals("Only 1 declaration should be returned", 1, collection.Count);
			AssertEquals("Search Result returns Declaration3", true, collection.Contains(declaration3.PK));
			filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate];
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("1 declarations should be returned", 1, collection.Count);
			AssertEquals("Search Result returns Declaration4", true, collection.Contains(declaration4.PK));
			filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate];
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("1 declarations should be returned", 1, collection.Count);
			AssertEquals("Search Result returns Declaration5", true, collection.Contains(declaration5.PK));
			filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate];
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			AssertEquals("Search Result returns Declaration3", true, collection.Contains(declaration3.PK));
			AssertEquals("Search Result returns Declaration4", true, collection.Contains(declaration4.PK));
			AssertEquals("Search Result returns Declaration5", true, collection.Contains(declaration5.PK));
			filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditDate];
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			AssertEquals("Search Result returns Declaration1", true, collection.Contains(declaration1.PK));
			AssertEquals("Search Result returns Declaration2", true, collection.Contains(declaration2.PK));
			AssertEquals("Search Result returns Declaration6", true, collection.Contains(declaration6.PK));
		}

		public void TestAuditReference()
		{
			AddAuditData();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "Audit";
			collection.Load(filterBO.Filter);
			AssertEquals("2 declarations should be returned", 2, collection.Count);
			AssertEquals("Search Result returns Declaration4", true, collection.Contains(declaration4.PK));
			AssertEquals("Search Result returns Declaration5", true, collection.Contains(declaration5.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "Declaration3 audited.";
			collection.Load(filterBO.Filter);
			AssertEquals("1 declaration should be returned", 1, collection.Count);
			AssertEquals("Search Result returns Declaration3", true, collection.Contains(declaration3.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference];
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			AssertEquals("Search Result returns Declaration1", true, collection.Contains(declaration1.PK));
			AssertEquals("Search Result returns Declaration2", true, collection.Contains(declaration2.PK));
			AssertEquals("Search Result returns Declaration6", true, collection.Contains(declaration6.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditReference];
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			AssertEquals("Search Result returns Declaration3", true, collection.Contains(declaration3.PK));
			AssertEquals("Search Result returns Declaration4", true, collection.Contains(declaration4.PK));
			AssertEquals("Search Result returns Declaration5", true, collection.Contains(declaration5.PK));
		}

		public void TestAuditUser()
		{
			AddAuditData();
			var filter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUser];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = ZString.Empty;
			collection.Load(filterBO.Filter);
			AssertEquals("All declarations should be returned", 6, collection.Count);
			filter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUser];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = staff.GS_Code;
			collection.Load(filterBO.Filter);
			AssertEquals("1 declaration should be returned", 1, collection.Count);
			AssertEquals("Search Result returns Declaration3", true, collection.Contains(declaration3.PK));
		}

		public void TestAuditUserName()
		{
			AddAuditData();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "Test User";
			collection.Load(filterBO.Filter);
			AssertEquals("1 declaration should be returned", 1, collection.Count);
			Assert("Search Result returns Declaration3", collection.Contains(declaration3.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "user";
			collection.Load(filterBO.Filter);
			AssertEquals("0 declarations should be returned", 0, collection.Count);
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName];
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			Assert("Search Result returns Declaration3", collection.Contains(declaration3.PK));
			Assert("Search Result returns Declaration4", collection.Contains(declaration4.PK));
			Assert("Search Result returns Declaration5", collection.Contains(declaration5.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName];
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			collection.Load(filterBO.Filter);
			AssertEquals("3 declarations should be returned", 3, collection.Count);
			Assert("Search Result returns Declaration1", collection.Contains(declaration1.PK));
			Assert("Search Result returns Declaration2", collection.Contains(declaration2.PK));
			Assert("Search Result returns Declaration6", collection.Contains(declaration6.PK));
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LastAuditFilterTypes.LastAuditLogUserName];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = "";
			collection.Load(filterBO.Filter);
			AssertEquals("6 declarations should be returned", 6, collection.Count);
			Assert("Search Result returns Declaration1", collection.Contains(declaration1.PK));
			Assert("Search Result returns Declaration2", collection.Contains(declaration2.PK));
			Assert("Search Result returns Declaration3", collection.Contains(declaration3.PK));
			Assert("Search Result returns Declaration4", collection.Contains(declaration4.PK));
			Assert("Search Result returns Declaration5", collection.Contains(declaration5.PK));
			Assert("Search Result returns Declaration6", collection.Contains(declaration6.PK));
		}

		public void TestPaymentAmountFilter()
		{
			RunAmountFilterTest("Payment Amount", (BaseJobComInvoiceHeader jz, ZDecimal amount) =>
			{
				jz.JZ_PaymentAmount = amount;
			});
		}

		public void TestCombinedEntryStatus()
		{
			if (DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code, GlbCompany.CurrentCompany.PK))
			{
				var declaration0 = Factory.New<BaseJobDeclaration>();
				declaration0.JE_EntryStatus = "1";
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_EntryStatus = "1";
				var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "6";
				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_EntryStatus = "1";
				var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "6";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_EntryStatus = "1";
				var declaration3 = Factory.New<BaseJobDeclaration>();
				declaration3.JE_EntryStatus = "1";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_EntryStatus = "6";
				var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader4.CH_EntryStatus = "2";
				var declaration4 = Factory.New<BaseJobDeclaration>();
				var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader5.CH_EntryStatus = ZString.Empty;
				var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader6.CH_EntryStatus = ZString.Empty;
				declaration4.JE_EntryStatus = "1";
				Factory.Save();
				var filter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
				filter.IsActive = true;
				filter.Property = "1";
				var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration0.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				AssertEquals(declaration4.PK, filteredDecs[2].PK);
				filter.Property = "6";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration1.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				filter.Property = "2";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration3.PK, filteredDecs[0].PK);
			}
			else
			{
				Assert("For other countries, result will be different.", true);
			}
		}

		public virtual void TestCombinedMessageStatus()
		{
			if (DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(GlbCompany.CurrentCompany.Country.RN_Code))
			{
				var declaration0 = Factory.New<BaseJobDeclaration>();
				declaration0.JE_MessageStatus = "AWO";
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_MessageStatus = "AWO";
				var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_Status = "AWC";
				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_MessageStatus = "AWO";
				var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_Status = "AWC";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_Status = "AWO";
				var declaration3 = Factory.New<BaseJobDeclaration>();
				declaration3.JE_MessageStatus = "AWO";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_Status = "CLO";
				var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader4.CH_Status = "AWC";
				var declaration4 = Factory.New<BaseJobDeclaration>();
				var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader5.CH_Status = ZString.Empty;
				var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader6.CH_Status = ZString.Empty;
				declaration4.JE_MessageStatus = "AWO";
				Factory.Save();
				var filter = (ModuleTextFilter)filterBO[filterBO.MessageStatusText];
				filter.IsActive = true;
				filter.Property = "AWO";
				var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration0.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				AssertEquals(declaration4.PK, filteredDecs[2].PK);
				filter.Property = "AWC";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration1.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				AssertEquals(declaration3.PK, filteredDecs[2].PK);
				filter.Property = "CLO";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration3.PK, filteredDecs[0].PK);
			}
			else
			{
				Assert("For other countries, result will be different.", true);
			}
		}

		public virtual void TestEntryStatusForIntegratedCountry()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			if (IntegratedCountryHelper.IsInterfaceEnabledCompany(GlbCompany.CurrentCompany.PK, countryCode) && DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(countryCode, GlbCompany.CurrentCompany.PK))
			{
				var declaration0 = Factory.New<BaseJobDeclaration>();
				declaration0.JE_EntryStatus = "SUB";
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_EntryStatus = "SUB";
				var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "DUT";
				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_EntryStatus = "SUB";
				var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "DUT";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_EntryStatus = "SUB";
				var declaration3 = Factory.New<BaseJobDeclaration>();
				declaration3.JE_EntryStatus = "SUB";
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_EntryStatus = "DUT";
				var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader4.CH_EntryStatus = "CSN";
				var declaration4 = Factory.New<BaseJobDeclaration>();
				var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader5.CH_EntryStatus = ZString.Empty;
				var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader6.CH_EntryStatus = ZString.Empty;
				declaration4.JE_EntryStatus = "SUB";
				Factory.Save();
				AssertEquals("SUB", declaration4.JE_EntryStatus);
				var filter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
				filter.IsActive = true;
				filter.Property = "SUB";
				var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration0.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				AssertEquals(declaration4.PK, filteredDecs[2].PK);
				filter.Property = "DUT";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration1.PK, filteredDecs[0].PK);
				AssertEquals(declaration2.PK, filteredDecs[1].PK);
				filter.Property = "CSN";
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertEquals(declaration3.PK, filteredDecs[0].PK);
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				AssertEquals("ComparisonOperator should have been reset.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Exact);
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				AssertEquals("ComparisonOperator should have been changed.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.NotEqual);
			}
			else
			{
				Assert("For non-integrated countries, result will be different.", true);
			}
		}

		public void TestEntryStatus_WithComparisonOperator()
		{
			var entryStatusFilter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			if (entryStatusFilter.Visibility != FilterVisibility.AlwaysAppliedAndHidden)
			{
				var declaration1 = Factory.New<BaseJobDeclaration>();
				declaration1.JE_EntryStatus = "11";
				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_EntryStatus = "12";
				Factory.Save();
				entryStatusFilter.IsActive = true;
				entryStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				entryStatusFilter.Property = "11";
				var collection = new BaseJobDeclarationCollection(Factory);
				collection.Load(filterBO.Filter);
				AssertEquals("Matched EntryStatus", 1, collection.Count);
				AssertEquals("JE_EntryStatus", "11", collection[0].JE_EntryStatus);
				entryStatusFilter.IsActive = true;
				entryStatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				entryStatusFilter.Property = "11";
				collection.Load(filterBO.Filter);
				AssertEquals("Matched EntryStatus", 1, collection.Count);
				AssertEquals("JE_EntryStatus", "12", collection[0].JE_EntryStatus);
			}
			else
			{
				Assert("For US, entryStatusFilter is AlwaysAppliedAndHidden.", true);
			}
		}

		public void TestServiceTypeFilters()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var service1 = declaration1.Services.AddNew();
			service1.ES_ServiceCode = "FUM";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var service2 = declaration2.Services.AddNew();
			service2.ES_ServiceCode = "XIN";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ServiceType];
			filter.IsActive = true;
			filter.Property = "XIN";
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched ES_ServiceCode", 1, collection.Count);
			AssertEquals("ES_ServiceCode", "XIN", collection[0].Services[0].ES_ServiceCode);
			filter.Property = "XXX";
			collection.Load(filterBO.Filter);
			AssertEquals("No Matched ES_ServiceCode", 0, collection.Count);
		}

		public void TestServiceDateBookedFilters()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var service1 = declaration1.Services.AddNew();
			service1.ES_ServiceCode = "FUM";
			service1.ES_Booked = new ZDateTime("2020-06-01");
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var service2 = declaration2.Services.AddNew();
			service2.ES_ServiceCode = "XIN";
			service2.ES_Booked = new ZDateTime("2020-05-01");
			var service3 = declaration2.Services.AddNew();
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.ServiceDateBooked];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 5, 10);
			filter.Property2 = new ZDateTime(2020, 6, 10);
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
			AssertEquals("ES_ServiceCode", "FUM", collection[0].Services[0].ES_ServiceCode);
			filter.Property1 = new ZDateTime(2023, 1, 30);
			filter.Property2 = new ZDateTime(2023, 6, 10);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have no matches", 0, collection.Count);

			AssertEquals("'Has Date' filter exists", true, filter.PropertySearch_List.ContainsCode(ModuleDateFilter.HasDateEntered));
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 2 match", 2, collection.Count);

			AssertEquals("'Has No Date' filter exists", true, filter.PropertySearch_List.ContainsCode(ModuleDateFilter.HasNoDateEntered));
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
		}

		public void TestServiceCompletedFilters()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var service1 = declaration1.Services.AddNew();
			service1.ES_ServiceCode = "FUM";
			service1.ES_Completed = new ZDateTime("2020-06-09");
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var service2 = declaration2.Services.AddNew();
			service2.ES_ServiceCode = "XIN";
			service2.ES_Completed = new ZDateTime("2020-05-09");
			var service3 = declaration2.Services.AddNew();
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.ServiceCompleted];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 5, 10);
			filter.Property2 = new ZDateTime(2020, 6, 10);
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
			AssertEquals("ES_ServiceCode", "FUM", collection[0].Services[0].ES_ServiceCode);
			filter.Property1 = new ZDateTime(2023, 1, 30);
			filter.Property2 = new ZDateTime(2023, 6, 10);
			collection.Load(filterBO.Filter);
			AssertEquals("Should have no matches", 0, collection.Count);

			AssertEquals("'Has Date' filter exists", true, filter.PropertySearch_List.ContainsCode(ModuleDateFilter.HasDateEntered));
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 2 match", 2, collection.Count);

			AssertEquals("'Has No Date' filter exists", true, filter.PropertySearch_List.ContainsCode(ModuleDateFilter.HasNoDateEntered));
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.Load(filterBO.Filter);
			AssertEquals("Should have 1 match", 1, collection.Count);
		}

		public void TestGetTariffInvLineQueryFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[DeclarationFilterConstants.NumberFilterTypes.TariffInvLine]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "123456789";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "234567890";
			Factory.Save();
			var tariffInvLineQueryFilter = (ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.TariffInvLine];
			tariffInvLineQueryFilter.Property = "12";
			tariffInvLineQueryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			tariffInvLineQueryFilter.Property = "23";
			tariffInvLineQueryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestGetDescriptionInvLineQueryFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "IAN TEST 1";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "2 IAN TEST";
			Factory.Save();
			var invoiceNumberQueryFilter = (ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine];
			invoiceNumberQueryFilter.Property = "IAN TEST";
			invoiceNumberQueryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			invoiceNumberQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestServiceLevelModeFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[DeclarationFilterConstants.ServiceLevel]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_RS_NKServiceLevel = "STD";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_RS_NKServiceLevel = "DEF";
			Factory.Save();
			var serviceLevelModeFilter = (ModuleNkFilter)filter[DeclarationFilterConstants.ServiceLevel];
			serviceLevelModeFilter.Property = "STD";
			serviceLevelModeFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
		}

		#region CRM Security
		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<BaseJobDeclaration>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.CustomsDeclarationEnquiryCRMSecurity);
		}

		#endregion
		public void TestGetCustomFilterStripsHelpersCore()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var task1 = declaration1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Test Task Declaration";
			var shipment = Factory.New<ForwardingShipment>();
			var task2 = shipment.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Test Task Shipment";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment.PK;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var task3 = declaration3.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "~Test Task Declaration";
			Factory.Save();
			var declarationFilterBizo = new JobDeclarationFilterBusinessObject();
			var filter = declarationFilterBizo.AddFilterStrip<TasksModuleFilter>("Tasks");
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test Task");
			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task1, task2 }, subFilterResult);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<BaseJobDeclaration>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextSqlFormatted, new[] { declaration1.PK, declaration2.PK }, result.Select(x => x.PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<BaseJobDeclaration>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextSqlFormatted, new[] { declaration3.PK }, result.Select(x => x.PK));
		}

		public void TestQueryForParentBusinessObjects_TasksFilterWithMatch()
		{
			var (declaration1, declaration2, declaration3, filter) = SetupDeclarationsWithProcessTasksAndFilterStrip();
			var query = filter.Query;
			CombineAssertions(() =>
			{
				AssertEquals("Declaration 1 No Match", false, declaration1.MatchesFilter(query));
				AssertEquals("Declaration 2 Matches", true, declaration2.MatchesFilter(query));
				AssertEquals("Declaration 3 Matches", true, declaration3.MatchesFilter(query));
			});
		}

		public void TestQueryForParentBusinessObjects_WithEmptyList()
		{
			var (declaration1, declaration2, declaration3, filter) = SetupDeclarationsWithProcessTasksAndFilterStrip();
			var query = filter.GetQueryForParentBusinessObjects(new List<BusinessObject>()
			{ });
			CombineAssertions(() =>
			{
				AssertEquals("Declaration 1 No Match", false, declaration1.MatchesFilter(query));
				AssertEquals("Declaration 2 No Match", false, declaration2.MatchesFilter(query));
				AssertEquals("Declaration 3 No Match", false, declaration3.MatchesFilter(query));
			});
		}

		public void TestQueryForParentBusinessObjects_NonMatchingTaskOnDeclaration()
		{
			var (declaration1, declaration2, declaration3, filter) = SetupDeclarationsWithProcessTasksAndFilterStrip();
			var query = filter.GetQueryForParentBusinessObjects(new List<BusinessObject>()
			{ declaration1 });
			CombineAssertions(() =>
			{
				AssertEquals("Declaration 1 No Match", false, declaration1.MatchesFilter(query));
				AssertEquals("Declaration 2 No Match", false, declaration2.MatchesFilter(query));
				AssertEquals("Declaration 3 No Match", false, declaration3.MatchesFilter(query));
			});
		}

		public void TestQueryForParentBusinessObjects_MatchingTaskDeclarationsAlsoLinkedToShipment()
		{
			var (declaration1, declaration2, declaration3, filter) = SetupDeclarationsWithProcessTasksAndFilterStrip();
			var query = filter.GetQueryForParentBusinessObjects(new List<BusinessObject>()
			{ declaration1, declaration2, declaration3 });
			CombineAssertions(() =>
			{
				AssertEquals("Declaration 1 No Match", false, declaration1.MatchesFilter(query));
				AssertEquals("Declaration 2 Matches", true, declaration2.MatchesFilter(query));
				AssertEquals("Declaration 3 Matches", true, declaration3.MatchesFilter(query));
			});
		}

		public void TestFlightVoyageVesselFilter()
		{
			AssertNotNull(filterBO[DeclarationFilterConstants.FlightVoyageVessel]);
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_TransportMode = "SEA";
			declaration1.JE_VoyageFlightNo = "12345";
			declaration1.JE_VesselName = "The Spitfire";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_TransportMode = "SEA";
			declaration2.JE_VoyageFlightNo = "6789";
			declaration2.JE_VesselName = "Flying Dutchman";
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_TransportMode = "SEA";
			declaration3.JE_VoyageFlightNo = "25874";
			declaration3.JE_VesselName = "Boaty McBoatface";
			Factory.Save();
			var voyageAndVesselFilter = (ModuleTextAndNkFilter)filterBO[DeclarationFilterConstants.FlightVoyageVessel];
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals(string.Format("declaration1 is in Collection, dec = {0}", declarations.Count), true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals(string.Format("declaration1 is in Collection, dec = {0}", declarations.Count), true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "Flying Dutchman";
			voyageAndVesselFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "2";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "c";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			declarations = new BaseJobDeclarationCollection(Factory, filterBO.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			AssertEquals(JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, voyageAndVesselFilter.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VesselName.MaxLength, voyageAndVesselFilter.NkMaxLength);
		}

		public void TestRelatedContainersFilter()
		{
			var jobDeclaration1 = Factory.New<BaseJobDeclaration>();
			var cusContainer1 = jobDeclaration1.CusContainers.AddNew();
			var jobContainer1 = Factory.New<CommonContainer>();
			jobContainer1.JC_ContainerNum = "CON001";
			cusContainer1.CO_JC = jobContainer1.PK;
			var cusContainer2 = jobDeclaration1.CusContainers.AddNew();
			var jobContainer2 = Factory.New<CommonContainer>();
			jobContainer2.JC_ContainerNum = "CON002";
			cusContainer2.CO_JC = jobContainer2.PK;
			var jobDeclaration2 = Factory.New<BaseJobDeclaration>();
			var cusContainer3 = jobDeclaration2.CusContainers.AddNew();
			var jobContainer3 = Factory.New<CommonContainer>();
			jobContainer3.JC_ContainerNum = "CON001";
			cusContainer3.CO_JC = jobContainer3.PK;
			var cusContainer4 = jobDeclaration2.CusContainers.AddNew();
			var jobContainer4 = Factory.New<CommonContainer>();
			jobContainer4.JC_ContainerNum = "XXX002";
			cusContainer4.CO_JC = jobContainer4.PK;
			var jobDeclaration3 = Factory.New<BaseJobDeclaration>();
			var cusContainer5 = jobDeclaration3.CusContainers.AddNew();
			var jobContainer5 = Factory.New<CommonContainer>();
			jobContainer5.JC_ContainerNum = "XXX001";
			cusContainer5.CO_JC = jobContainer5.PK;
			var cusContainer6 = jobDeclaration3.CusContainers.AddNew();
			var jobContainer6 = Factory.New<CommonContainer>();
			jobContainer6.JC_ContainerNum = "XXX002";
			cusContainer6.CO_JC = jobContainer6.PK;
			Factory.Save();
			var filterStripBiz0 = new JobDeclarationFilterBusinessObject();
			var relatedContainerFilter = (RelatedContainersOfJobDeclarationFilter)filterStripBiz0[DeclarationFilterConstants.RelatedContainers];
			relatedContainerFilter.IsActive = true;
			relatedContainerFilter.SelectedFilters.AddTextFilterStrip("Container #", "CON");
			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertEquals("Should have 2 Job Declarations", 2, jobDeclarations.Count);
		}

		public void TestEstimateDateFilter_DeclarationTransports()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_RL_NKPortOfArrival = "AUSYD";
			declaration1.JE_RL_NKPortOfLoading = "SGSIN";
			var transport1 = declaration1.Transports.AddNew();
			transport1.FillWithValidTestData();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKLoadPort = "SGSIN";
			transport1.JW_ETA = new ZDateTime(2018, 1, 1);
			transport1.JW_ETD = new ZDateTime(2018, 1, 10);
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_RL_NKPortOfArrival = "CNSHA";
			declaration2.JE_RL_NKPortOfLoading = "AUSYD";
			var transport2 = declaration2.Transports.AddNew();
			transport2.FillWithValidTestData();
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2018, 1, 5);
			transport2.JW_ETD = new ZDateTime(2018, 1, 10);
			Factory.Save();
			CombineAssertFilter(declaration1.PK, declaration2.PK);
		}

		public void TestEstimateDateFilter_ConsolTransports()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_RL_NKPortOfArrival = "AUSYD";
			declaration1.JE_RL_NKPortOfLoading = "SGSIN";
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			declaration1.JE_JS = shipment1.PK;
			var transport1 = consol1.Transports.AddNew();
			transport1.FillWithValidTestData();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKLoadPort = "SGSIN";
			transport1.JW_ETA = new ZDateTime(2018, 1, 1);
			transport1.JW_ETD = new ZDateTime(2018, 1, 10);
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_RL_NKPortOfArrival = "CNSHA";
			declaration2.JE_RL_NKPortOfLoading = "AUSYD";
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			declaration2.JE_JS = shipment2.PK;
			var transport2 = consol2.Transports.AddNew();
			transport2.FillWithValidTestData();
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2018, 1, 5);
			transport2.JW_ETD = new ZDateTime(2018, 1, 10);
			Factory.Save();
			CombineAssertFilter(declaration1.PK, declaration2.PK);
		}

		public void TestEstimateDateFilter_ShipmentTransports()
		{
			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_RL_NKPortOfArrival = "AUSYD";
			declaration1.JE_RL_NKPortOfLoading = "SGSIN";
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			declaration1.JE_JS = shipment1.PK;
			var transport1 = shipment1.Transports.AddNew();
			transport1.FillWithValidTestData();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKLoadPort = "SGSIN";
			transport1.JW_ETA = new ZDateTime(2018, 1, 1);
			transport1.JW_ETD = new ZDateTime(2018, 1, 10);
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_RL_NKPortOfArrival = "CNSHA";
			declaration2.JE_RL_NKPortOfLoading = "AUSYD";
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			declaration2.JE_JS = shipment2.PK;
			var transport2 = shipment2.Transports.AddNew();
			transport2.FillWithValidTestData();
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2018, 1, 5);
			transport2.JW_ETD = new ZDateTime(2018, 1, 10);
			Factory.Save();
			CombineAssertFilter(declaration1.PK, declaration2.PK);
		}

		void CombineAssertFilter(ZGuid declaration1Pk, ZGuid declaration2Pk)
		{
			var expectedPks = new[] { declaration1Pk };
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge, new ZDateTime(2018, 1, 1), new ZDateTime(2018, 1, 2), expectedPks);
			expectedPks = new[] { declaration1Pk, declaration2Pk };
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ETAOfDischarge, new ZDateTime(2018, 1, 1), new ZDateTime(2018, 1, 6), expectedPks);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ETDOfLoading, new ZDateTime(2018, 1, 10), new ZDateTime(2018, 1, 10), expectedPks);
			expectedPks = Array.Empty<ZGuid>();
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ETDOfLoading, new ZDateTime(2018, 1, 1), new ZDateTime(2018, 1, 8), expectedPks);
		}

		void AssertDateFilter(string filterDes, ZDateTime value1, ZDateTime value2, ZGuid[] expectedPks)
		{
			var filter = (ModuleDateFilter)filterBO[filterDes];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = value1;
			filter.Property2 = value2;
			filter.IsActive = true;
			var collection = new BaseJobDeclarationCollection(Factory);
			collection.Load(filterBO.Filter);
			var actualPks = collection.Select(c => c.PK);
			AssertContainsExactElementsInAnyOrder("Should find these expected declarations.", expectedPks, actualPks);
		}

		public void TestCommonNumbersAndReferencesFilter_MultiSearch()
		{
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.Common];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "A,B";
			Assert(filterBO.Filter.LiteralTextSqlFormatted, filterBO.Filter.LiteralTextSqlFormatted.Contains("'A', 'B'"));
			AssertNoExceptionThrown(() => Factory.Load<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestCommonNumbersAndReferencesFilter_MultiSearch_NoExceptionWhenHuge()
		{
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.Common];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "A,B";
			var description = filter.Description;
			filter = (ModuleNumberFilter)filterBO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "C,D";
			filter = (ModuleNumberFilter)filterBO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "E,F";
			filter = (ModuleNumberFilter)filterBO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "G,H,I";
			filter = (ModuleNumberFilter)filterBO.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(description);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "J,K,L";
			Assert(filterBO.Filter.LiteralTextSqlFormatted, filterBO.Filter.LiteralTextSqlFormatted.Contains("'G', 'H', 'I'"));
			AssertNoExceptionThrown(() => Factory.Load<BaseJobDeclaration>(filterBO.Filter));
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var declaration3 = Factory.New<BaseJobDeclaration>();

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = declaration1.PK;
			job1.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = declaration2.PK;
			job2.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentID = declaration3.PK;
			job3.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filter["Profit/Loss Reason"];
			var declarationCollection = new BaseJobDeclarationCollection(Factory);
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration2, declaration3 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration2, declaration3 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration2, declaration3 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration3 }, declarationCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			declarationCollection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, declarationCollection);
		}

		public void TestDeclarantFilter_Disabled()
		{
			var jobDeclarationFilterBusinessObjectMock = new Mock<JobDeclarationFilterBusinessObject> { CallBase = true };
			jobDeclarationFilterBusinessObjectMock.Protected().Setup<bool>("ShowDeclarantFilter").Returns(false);

			AssertNull(filterBO[DeclarationFilterConstants.OrgFilterTypes.Declarant]);
		}

		public void TestDeclarantFilter_Properties()
		{
			var jobDeclarationFilterBusinessObjectMock = new Mock<JobDeclarationFilterBusinessObject> { CallBase = true };
			jobDeclarationFilterBusinessObjectMock.Protected().Setup<bool>("ShowDeclarantFilter").Returns(true);
			var filterBusinessObject = jobDeclarationFilterBusinessObjectMock.Object;
			var declarantFilter = (ModuleGuidFilterForOrg)filterBusinessObject[DeclarationFilterConstants.OrgFilterTypes.Declarant];
			declarantFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Declarant", declarantFilter.Description);
				AssertEquals("Category", FilterCategories.Organisations, declarantFilter.Category);
			});
		}

		public virtual void TestDeclarantFilter()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "ORG2";
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_OA_DeclarantAddress = orgHeader2.MainAddress.PK;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			Factory.Save();

			var jobDeclarationFilterBusinessObjectMock = new Mock<JobDeclarationFilterBusinessObject> { CallBase = true };
			jobDeclarationFilterBusinessObjectMock.Protected().Setup<bool>("ShowDeclarantFilter").Returns(true);

			var filterBusinessObject = jobDeclarationFilterBusinessObjectMock.Object;
			var filter = (ModuleGuidFilterForOrg)filterBusinessObject[DeclarationFilterConstants.OrgFilterTypes.Declarant];
			filter.IsActive = true;

			CombineAssertions(() =>
			{
				filter.Property = orgHeader1.PK;
				AssertMatch("orgHeader1", true, false, true);
				filter.Property = orgHeader2.PK;
				AssertMatch("orgHeader2", false, true, false);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3)
			{
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filterBusinessObject.Filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filterBusinessObject.Filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filterBusinessObject.Filter));
			}
		}

		public void TestDocTypeFilter_CanSearchShipmentsInCustomsDeclarations()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobRequiredDocument2 = shipment2.DocsAndCartage.RequiredDocuments.AddNew();
			jobRequiredDocument2.EQ_DocType = Core.Constants.RefDocTypes.ExportCartageAdvice;
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobRequiredDocument3 = shipment3.DocsAndCartage.RequiredDocuments.AddNew();
			jobRequiredDocument3.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.JE_JS = shipment3.PK;
			Factory.Save();

			var filterBO = GetNewFilterStripBusinessObject();
			filterBO.QueryObjectType = typeof(BaseJobDeclaration);
			var filter = filterBO.AddFilterStrip<ModuleTextFilter>("Document Type");
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = Core.Constants.RefDocTypes.ExportCartageAdvice;

			var filteredJobDeclarations = Factory.Load(typeof(BaseJobDeclaration), filterBO.Filter);
			AssertEquals("Should have found the shipment with specific document type", 1, filteredJobDeclarations.Length);
			AssertEquals("Should be declaration2", declaration2.PK, filteredJobDeclarations[0].PK);
		}

		public virtual void TestFilterHandlers()
		{
			var expectedHandlers = new[]
			{
				typeof(CurrentCompanyFilterHandler),
				typeof(AgentsReferenceFilterHandler),
				typeof(ContainerNumberFilterHandler),
				typeof(InvoiceNumberFilterHandler),
			};

			var filterStripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertContainsExactElementsInAnyOrder(expectedHandlers, filterStripBO.FilterHandlers.Select(h => h.GetType()));
		}

		public void TestGetIndexSearchFilter_ShouldNotThrowExceptions()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
				filterStrip.SearchType = SearchType.Index;

				var searchFields = new List<SearchField>();

				foreach (var filterHandler in filterStrip.FilterHandlers)
				{
					foreach (var requiredIndexSearchField in filterHandler.RequiredIndexSearchFields)
					{
						var sf = SearchField.Create(requiredIndexSearchField, requiredIndexSearchField);
						searchFields.Add(sf);
					}
				}

				var sfc = new SearchFieldCollection(string.Empty, searchFields.ToArray());
				filterStrip.IndexSearchFields = sfc;

				AssertNoExceptionThrown(() => filterStrip.LoadModuleFilters());
			}
		}

		#region Constants
		protected const string EntryNum1 = "1M15353189291";
		protected const string EntryNum2 = "1M41818302981";
		protected const string EntryNum3 = "1S11818192873";
		protected const string EntryNum4 = "1A15353192273";
		protected const string EntryNum5 = "1M17222222222";
		protected const string EntryNum6 = "1S18222222222";
		protected const string EntryNum7 = "2M15353222222";
		protected const string EntryNum8 = "3S13333333333";
		protected const string MasterBillNum1 = "999-00119291";
		protected const string MasterBillNum2 = "999-01192918";
		protected const string DecNum1 = "DEC NUM 1";
		protected const string DecNum2 = "DEC NUM 2";
		protected const string DecNum3 = "DEC NUM 3";
		protected const string DecNum4 = "DEC NUM 4";
		protected const string DecNum5 = "DEC NUM 5";
		protected const string DecNum6 = "DEC NUM 6";
		protected const string DecNum7 = "DEC NUM 7";
		protected const string DecNum8 = "DEC NUM 8";
		protected const string DeclarationReference1 = "B00001910";
		protected const string DeclarationReference2 = "B00001911";
		protected const string DeclarationReference3 = "B00001912";
		protected const string DeclarationReference4 = "B00001916";
		protected const string DeclarationReference5 = "G0001756";
		protected const string DeclarationReference6 = "1756";
		protected const string DeclarationReference7 = "G00001915";
		protected const string DeclarationReference8 = "GC00001915";
		protected const string DeclarationReference9 = "G13A";
		protected const string DeclarationReference10 = "13A";
		protected const string UniqueReference1 = "S00001911";
		protected const string UniqueReference2 = "S00001912";
		protected const string UniqueReference3 = "S00001913";
		protected const string UniqueReference4 = "S00001915";
		protected const string DecWithHeader1 = "DEC NUM HEAD 1";
		protected const string DecWithHeader2 = "DEC NUM HEAD 2";
		protected const string DecWithHeader3 = "DEC NUM HEAD 3";
		protected const string DecWithHeader4 = "DEC NUM HEAD 4";
		protected const string HeadNum1 = "HEAD ENTRY 1";
		protected const string HeadNum2 = "HEAD ENTRY 2";
		protected const string HeadNum3 = "HEAD ENTRY 3";
		protected const string HeadNum4 = "HEAD ENTRY 4";
		protected const string WithHeader = "W/HEAD";
		protected const string ShipNum1_1 = "SHIP 11";
		protected const string ShipNum1_2 = "SHIP 12";
		protected const string ShipNum1_3 = "SHIP 13";
		protected const string ShipNum1_4 = "SHIP 14";
		protected const string ShipNum2_1 = "SHIP 21";
		protected const string ShipNum2_2 = "SHIP 22";
		protected const string ShipNum2_3 = "SHIP 23";
		protected const string ShipNum2_4 = "SHIP 24";
		protected const string ConsolNum1 = "CONSOL 1";
		protected const string ConsolNum2 = "CONSOL 2";
		#endregion
		protected BaseJobDeclaration declaration1;
		protected BaseJobDeclaration declaration2;
		protected BaseJobDeclaration declaration3;
		protected BaseJobDeclaration declaration4;
		protected BaseJobDeclaration declaration5;
		protected BaseJobDeclaration declaration6;
		protected BaseJobDeclarationCollection collection;
		protected JobDocsAndCartage docsAndCartage1;
		protected JobDocsAndCartage docsAndCartage2;
		protected JobDocsAndCartage docsAndCartage3;
		protected GlbStaff staff;
		protected JobDeclarationFilterBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}

		protected sealed override FilterStripBusinessObject GetNewFilterStripBusinessObject() => (FilterStripBusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType());

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			// Uses raw SQL
			exclusions.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.Common));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.Common));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.Common));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.Common));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode));
			// Uses raw SQL:
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobDocsAndCartageSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobOrderHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(JobOrderItemSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.OrderNumberOwnersRef));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, "Additional Reference Number"));
			// Workflow, billing, audit, etc...  just exclde all
			exclusions.Add(TableFilter(GlbBranchSchema.Constants.TableName, "Country/Region"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Branch"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Department"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Operator"));
			exclusions.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Cartage Coordinator"));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Cartage Coordinator"));
			exclusions.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Cartage Coordinator"));
			exclusions.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Client Assigned Staff"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Job Status"));
			exclusions.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AP Invoice #"));
			exclusions.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AP Invoice #"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AP Invoice #"));
			exclusions.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(JobChargeSchema.Constants.TableName, "Supplier Cost Reference"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Supplier Cost Reference"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Date"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Date"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Completed"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Completed"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Milestone"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Milestone"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Last Completed Milestone"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Last Completed Milestone"));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, "Entry Status"));
			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, "Common Numbers and References"));
			exclusions.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, "Master Bill"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Invoice #"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Order # / Owner's Reference"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Payment #"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Order # / Owner's Reference"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute1"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Department"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Operator"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Job Status"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AP Invoice #"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Supplier Cost Reference"));
			exclusions.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Client Assigned Staff"));
			exclusions.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AR Transaction #"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Completed"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Milestone"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Last Completed Milestone"));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, "Common Numbers and References"));
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, "Entry Status"));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, "Common Numbers and References"));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, "Additional Reference Number"));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Common Numbers and References"));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Order # / Owner's Reference"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Completed"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Milestone"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Last Completed Milestone"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Invoice Amount"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Payment Amount"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Tariff - Inv Line"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute4"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Tariff - Inv Line"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute4"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Description - Inv Line"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute5"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Description - Inv Line"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute5"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute6"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute6"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomText1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomText1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal1"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal1"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal2"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal2"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal3"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal3"));
			exclusions.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Any Commercial Invoice Text Attribute"));
			exclusions.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Any Commercial Invoice Text Attribute"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			exclusions.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			return exclusions;
		}

		protected void AddTestShipmentData()
		{
			BaseJobDeclaration testDecWithHeader1 = AddJobDeclaration(DeclarationReference1);
			BaseJobDeclaration testDecWithHeader2 = AddJobDeclaration(DeclarationReference2);
			BaseJobDeclaration testDecWithHeader3 = AddJobDeclaration(DeclarationReference3);
			BaseJobDeclaration testDecWithHeader4 = AddJobDeclaration(DeclarationReference4);
			BaseJobDeclaration testDecWithHeader5 = AddJobDeclaration(DeclarationReference5);
			BaseJobDeclaration testDecWithHeader6 = AddJobDeclaration(DeclarationReference6);
			BaseJobDeclaration testDecWithHeader7 = AddJobDeclaration(DeclarationReference7);
			BaseJobDeclaration testDecWithHeader8 = AddJobDeclaration(DeclarationReference8);
			BaseJobDeclaration testDecWithHeader9 = AddJobDeclaration(DeclarationReference9);
			BaseJobDeclaration testDecWithHeader10 = AddJobDeclaration(DeclarationReference10);
			BaseJobDeclaration testShipmentDecWithHeader1 = AddJobDeclaration(UniqueReference1);
			BaseJobDeclaration testShipmentDecWithHeader2 = AddJobDeclaration(UniqueReference2);
			BaseJobDeclaration testShipmentDecWithHeader3 = AddJobDeclaration(UniqueReference3);
			BaseJobDeclaration testShipmentDecWithHeader4 = AddJobDeclaration(UniqueReference4);
			CommonShipment shipment1 = AddShipment(UniqueReference1, testShipmentDecWithHeader1);
			CommonShipment shipment2 = AddShipment(UniqueReference2, testShipmentDecWithHeader2);
			CommonShipment shipment3 = AddShipment(UniqueReference3, testShipmentDecWithHeader3);
			CommonShipment shipment4 = AddShipment(UniqueReference4, testShipmentDecWithHeader4);
			AssertNotNull(shipment1);
		}

		protected void AddTestEntryNumberData()
		{
			BaseJobDeclaration testDec1 = AddJobDeclaration(DecNum1);
			BaseJobDeclaration testDec2 = AddJobDeclaration(DecNum2);
			BaseJobDeclaration testDec3 = AddJobDeclaration(DecNum3);
			BaseJobDeclaration testDec4 = AddJobDeclaration(DecNum4);
			BaseJobDeclaration testDecWithHeader1 = AddJobDeclaration(DecWithHeader1);
			BaseJobDeclaration testDecWithHeader2 = AddJobDeclaration(DecWithHeader2);
			BaseJobDeclaration testDecWithHeader3 = AddJobDeclaration(DecWithHeader3);
			BaseJobDeclaration testDecWithHeader4 = AddJobDeclaration(DecWithHeader4);
			CusEntryHeader testHead1 = AddCusEntryHeader(testDecWithHeader1);
			CusEntryHeader testHead2 = AddCusEntryHeader(testDecWithHeader2);
			CusEntryHeader testHead3 = AddCusEntryHeader(testDecWithHeader3);
			CusEntryHeader testHead4 = AddCusEntryHeader(testDecWithHeader4);
			CusEntryNumber testEntryNumber1 = AddCusEntryNumber(EntryNum1, testDec1);
			CusEntryNumber testEntryNumber2 = AddCusEntryNumber(EntryNum2, testDec2);
			CusEntryNumber testEntryNumber3 = AddCusEntryNumber(EntryNum3, testDec3);
			CusEntryNumber testEntryNumber4 = AddCusEntryNumber(EntryNum4, testDec4);
			CusEntryNumber testEntryNumber5 = AddCusEntryNumber(EntryNum5, testHead1);
			CusEntryNumber testEntryNumber6 = AddCusEntryNumber(EntryNum6, testHead2);
			CusEntryNumber testEntryNumber7 = AddCusEntryNumber(EntryNum7, testHead3);
			CusEntryNumber testEntryNumber8 = AddCusEntryNumber(EntryNum8, testHead4);
		}

		protected void AddTestMasterBillData()
		{
			BaseJobDeclaration testDec1 = AddJobDeclaration(DecNum1);
			BaseJobDeclaration testDec2 = AddJobDeclaration(DecNum2);
			BaseJobDeclaration testDec3 = AddJobDeclaration(DecNum3);
			BaseJobDeclaration testDec4 = AddJobDeclaration(DecNum4);
			BaseJobDeclaration testDec5 = AddJobDeclaration(DecNum5);
			testDec1.JE_MasterBill = MasterBillNum1;
			testDec2.JE_MasterBill = MasterBillNum1;
			testDec3.JE_MasterBill = MasterBillNum1;
			testDec4.JE_MasterBill = MasterBillNum1;
			testDec5.JE_MasterBill = MasterBillNum1;
			BaseJobDeclaration testDec6 = AddJobDeclaration(DecNum6);
			BaseJobDeclaration testDec7 = AddJobDeclaration(DecNum7);
			BaseJobDeclaration testDec8 = AddJobDeclaration(DecNum8);
			testDec6.JE_MasterBill = MasterBillNum2;
			testDec7.JE_MasterBill = MasterBillNum2;
			testDec8.JE_MasterBill = MasterBillNum2;
		}

		protected BaseJobDeclaration AddJobDeclaration(string decRef)
		{
			BaseJobDeclaration result = BaseJobDeclaration.New(Factory);
			result.JE_DeclarationReference = decRef;
			result.DontReAssignReferenceNoForUnitTest = true;
			return result;
		}

		protected CusEntryHeader AddCusEntryHeader(BusinessObject parent)
		{
			CusEntryHeader result = Factory.New<CusEntryHeader>();
			result.CH_JE = parent.PK;
			return result;
		}

		protected CusEntryNumber AddCusEntryNumber(ZString entryNumber, BusinessObject parent)
		{
			CusEntryNumber result = Factory.New<CusEntryNumber>();
			result.CE_EntryNum = entryNumber;
			result.CE_ParentID = parent.PK;
			result.CE_ParentTable = parent.TableName;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return result;
		}

		protected BaseJobDeclaration AddJobDeclaration(string decRef, BusinessObjectFactory factory)
		{
			BaseJobDeclaration result = BaseJobDeclaration.New(factory);
			result.JE_DeclarationReference = decRef;
			result.DontReAssignReferenceNoForUnitTest = true;
			return result;
		}

		protected CusEntryHeader AddCusEntryHeader(BusinessObject parent, BusinessObjectFactory factory)
		{
			CusEntryHeader result = factory.New<CusEntryHeader>();
			result.CH_JE = parent.PK;
			return result;
		}

		protected CusEntryNumber AddCusEntryNumber(ZString entryNumber, BusinessObject parent, BusinessObjectFactory factory)
		{
			CusEntryNumber result = factory.New<CusEntryNumber>();
			result.CE_EntryNum = entryNumber;
			result.CE_ParentID = parent.PK;
			result.CE_ParentTable = parent.TableName;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return result;
		}

		protected CommonShipment AddShipment(ZString uniqueConsignRef, BaseJobDeclaration dec)
		{
			CommonShipment result = CommonShipment.New(Factory);
			result.JS_UniqueConsignRef = uniqueConsignRef;
			dec.JE_JS = result.PK;
			return result;
		}

		protected void AddDateValueRecords()
		{
			declaration1 = BaseJobDeclaration.New(Factory);
			declaration2 = BaseJobDeclaration.New(Factory);
			declaration3 = BaseJobDeclaration.New(Factory);
			declaration3.JE_JS = Factory.New<ForwardingShipment>().PK;
			declaration4 = BaseJobDeclaration.New(Factory);
			declaration4.JE_JS = Factory.New<ForwardingShipment>().PK;
			declaration5 = BaseJobDeclaration.New(Factory);
			collection = new BaseJobDeclarationCollection(Factory);
			docsAndCartage1 = declaration1.DocsAndCartage;
			docsAndCartage2 = declaration2.DocsAndCartage;
			docsAndCartage3 = declaration3.DocsAndCartage;
			docsAndCartage1.JP_DeliveryCartageAdvised = new ZDateTime(2004, 1, 1);
			docsAndCartage1.JP_DeliveryCartageCompleted = new ZDateTime(2004, 1, 5);
			docsAndCartage2.JP_DeliveryCartageAdvised = new ZDateTime(2005, 1, 1);
			docsAndCartage2.JP_DeliveryCartageCompleted = new ZDateTime(2005, 1, 5);
			docsAndCartage3.JP_DeliveryCartageAdvised = new ZDateTime(2004, 1, 1);
			docsAndCartage3.JP_DeliveryCartageCompleted = new ZDateTime(2004, 1, 5);
			Factory.Save();
		}

		protected void AddAuditData()
		{
			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CCC";
			staff.GS_FullName = "Test User";
			staff.GS_LoginName = "Test User";
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "DDD";
			staff1.GS_FullName = "DTest User";
			staff1.GS_LoginName = "DTest User";
			declaration1 = Factory.New<BaseJobDeclaration>();
			declaration2 = Factory.New<BaseJobDeclaration>();
			declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_AuditDateUtc = ZDateTime.BrettsBirthday.AddHours(12);
			declaration3.JE_AuditReference = "Declaration3 audited.";
			declaration3.JE_GS_NKAuditUser = "CCC";
			declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.JE_AuditDateUtc = ZDateTime.UtcNow;
			declaration4.JE_AuditReference = "Audit For Declaration4 completed.";
			declaration4.JE_GS_NKAuditUser = "DDD";
			declaration5 = Factory.New<BaseJobDeclaration>();
			declaration5.JE_AuditDateUtc = ZDateTime.UtcNow.AddHours(13);
			declaration5.JE_AuditReference = "Audit.";
			declaration5.JE_GS_NKAuditUser = "DDD";
			declaration6 = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			collection = new BaseJobDeclarationCollection(Factory);
		}

		(BaseJobDeclaration Declaration1, BaseJobDeclaration Declaration2, BaseJobDeclaration Declaration3, TasksModuleFilter Filter) SetupDeclarationsWithProcessTasksAndFilterStrip()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var task1 = declaration1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "No Match";
			var shipment = Factory.New<ForwardingShipment>();
			var task2 = shipment.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Match";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment.PK;
			var declaration3 = Factory.New<BaseJobDeclaration>();
			var task3 = declaration3.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "Match";
			Factory.Save();
			var declarationFilterBizo = new JobDeclarationFilterBusinessObject();
			var filter = declarationFilterBizo.AddFilterStrip<TasksModuleFilter>("Tasks");
			filter.SelectedFilters.AddTextFilterStrip("Description", "Match");
			return (declaration1, declaration2, declaration3, filter);
		}

		static JobDeclarationFilterBusinessObject GetFilterBusinessObject(bool showManifestNumberFilter)
		{
			var mock = new Mock<JobDeclarationFilterBusinessObject>();
			mock.CallBase = true;
			mock.Protected()
				.Setup<bool>("ShowManifestNumberFilter")
				.Returns(showManifestNumberFilter);
			return mock.Object;
		}
	}
}
