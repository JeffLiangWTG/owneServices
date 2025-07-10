using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public abstract class OrdersBaseFilterBusinessObjectTestCase : FilterStripBusinessObjectTestCase
	{
		#region Not published on Web Filters

		public virtual void TestNotPublishedOnWebFilters()
		{
			if (FilterStripBizO["Registered Staff"] != null)
			{
				AssertEquals(false, FilterStripBizO["Registered Staff"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Creating User"] != null)
			{
				AssertEquals(false, FilterStripBizO["Creating User"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Last Edit User"] != null)
			{
				AssertEquals(false, FilterStripBizO["Last Edit User"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Any Open Task Assigned To"] != null)
			{
				AssertEquals(false, FilterStripBizO["Any Open Task Assigned To"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Next Task Assigned To"] != null)
			{
				AssertEquals(false, FilterStripBizO["Next Task Assigned To"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Pre Advice #"] != null)
			{
				AssertEquals(false, FilterStripBizO["Pre Advice #"].IsPublishedOnWeb);
			}
		}

		#endregion

		#region Attribute Filters

		void AssertOrderAttributeFilter(ZString description, SchemaColumn column)
		{
			AssertEquals("Filter Category", FilterStripBizO[description].Category, FilterCategories.AttributeSearch);
			if (column is SchemaStringColumn)
			{
				AssertOrderTextOrNumberFilter(description, (SchemaStringColumn)column);
			}
			else if (column is SchemaBoolColumn)
			{
				AssertOrderFlagsFilter(description, (SchemaBoolColumn)column);
			}
			else if (column is SchemaDecimalColumn)
			{
				AssertOrderDecimalFilter(description, (SchemaDecimalColumn)column);
			}
			else if (column is SchemaDateTimeColumn)
			{
				AssertOrderDateFilter(description, (SchemaDateTimeColumn)column);
			}
			else
			{
				Assert("Not supported columnType is being tested", false);
			}
		}

		void AssertOrderLineAttributeFilter(ZString description, SchemaColumn column)
		{
			AssertEquals("Filter Category", FilterStripBizO[description].Category, FilterCategories.AttributeSearch);
			if (column is SchemaStringColumn)
			{
				AssertOrderLineTextOrNumberFilter(description, (SchemaStringColumn)column);
			}
			else if (column is SchemaBoolColumn)
			{
				AssertOrderLineFlagsFilter(description, (SchemaBoolColumn)column);
			}
			else if (column is SchemaDecimalColumn)
			{
				AssertOrderLineDecimalFilter(description, (SchemaDecimalColumn)column);
			}
			else if (column is SchemaDateTimeColumn)
			{
				AssertOrderLineDateFilter(description, (SchemaDateTimeColumn)column);
			}
			else
			{
				Assert("Not supported columnType is being tested", false);
			}
		}

		public void TestAttributeFilters()
		{
			Assert("Precondition", !Globals.IsWeb);
			//Order Line filters
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute1, JobOrderLineSchema.JO_CustomAttrib1);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute2, JobOrderLineSchema.JO_CustomAttrib2);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute3, JobOrderLineSchema.JO_CustomAttrib3);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute4, JobOrderLineSchema.JO_CustomAttrib4);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute5, JobOrderLineSchema.JO_CustomAttrib5);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomAttribute6, JobOrderLineSchema.JO_CustomAttrib6);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomText1, JobOrderLineSchema.JO_CustomTextBlob1);
			AssertOrderLineAttributeFilter("Part Attribute 1", JobOrderLineSchema.JO_PartAttrib1);
			AssertOrderLineAttributeFilter("Part Attribute 2", JobOrderLineSchema.JO_PartAttrib2);
			AssertOrderLineAttributeFilter("Part Attribute 3", JobOrderLineSchema.JO_PartAttrib3);
			AssertOrderLineAttributeFilter("Serial Number", JobOrderLineSchema.JO_SerialNumber);

			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDate1, JobOrderLineSchema.JO_CustomDate1);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDate2, JobOrderLineSchema.JO_CustomDate2);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDate3, JobOrderLineSchema.JO_CustomDate3);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDate4, JobOrderLineSchema.JO_CustomDate4);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDate5, JobOrderLineSchema.JO_CustomDate5);

			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDecimal1, JobOrderLineSchema.JO_CustomDecimal1);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDecimal2, JobOrderLineSchema.JO_CustomDecimal2);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDecimal3, JobOrderLineSchema.JO_CustomDecimal3);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDecimal4, JobOrderLineSchema.JO_CustomDecimal4);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomDecimal5, JobOrderLineSchema.JO_CustomDecimal5);

			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomFlag1, JobOrderLineSchema.JO_CustomFlag1);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomFlag2, JobOrderLineSchema.JO_CustomFlag2);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomFlag3, JobOrderLineSchema.JO_CustomFlag3);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomFlag4, JobOrderLineSchema.JO_CustomFlag4);
			AssertOrderLineAttributeFilter(Constants.CustomLabels.OrderLine.CustomFlag5, JobOrderLineSchema.JO_CustomFlag5);

			//Order Header filters
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomAttribute1, JobOrderHeaderSchema.JD_CustomAttrib1);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomAttribute2, JobOrderHeaderSchema.JD_CustomAttrib2);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomAttribute3, JobOrderHeaderSchema.JD_CustomAttrib3);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomAttribute4, JobOrderHeaderSchema.JD_CustomAttrib4);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomAttribute5, JobOrderHeaderSchema.JD_CustomAttrib5);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomContact1, JobOrderHeaderSchema.JD_FirstBuyerContact);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomContact2, JobOrderHeaderSchema.JD_SecondBuyerContact);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.GoodsOrigin, JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.GoodsDestination, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo);

			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDate1, JobOrderHeaderSchema.JD_CustomDate1);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDate2, JobOrderHeaderSchema.JD_CustomDate2);

			AssertOrderAttributeFilter(string.Format("Estimated {0}", Constants.CustomLabels.Order.UserTrackDate1), JobOrderHeaderSchema.JD_EstimateUserDate1);
			AssertOrderAttributeFilter(string.Format("Actual {0}", Constants.CustomLabels.Order.UserTrackDate1), JobOrderHeaderSchema.JD_ActualUserDate1);
			AssertOrderAttributeFilter(string.Format("Estimated {0}", Constants.CustomLabels.Order.UserTrackDate2), JobOrderHeaderSchema.JD_EstimateUserDate2);
			AssertOrderAttributeFilter(string.Format("Actual {0}", Constants.CustomLabels.Order.UserTrackDate2), JobOrderHeaderSchema.JD_ActualUserDate2);
			AssertOrderAttributeFilter(string.Format("Estimated {0}", Constants.CustomLabels.Order.UserTrackDate3), JobOrderHeaderSchema.JD_EstimateUserDate3);
			AssertOrderAttributeFilter(string.Format("Actual {0}", Constants.CustomLabels.Order.UserTrackDate3), JobOrderHeaderSchema.JD_ActualUserDate3);
			AssertOrderAttributeFilter(string.Format("Estimated {0}", Constants.CustomLabels.Order.UserTrackDate4), JobOrderHeaderSchema.JD_EstimateUserDate4);
			AssertOrderAttributeFilter(string.Format("Actual {0}", Constants.CustomLabels.Order.UserTrackDate4), JobOrderHeaderSchema.JD_ActualUserDate4);

			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDecimal1, JobOrderHeaderSchema.JD_CustomDecimal1);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDecimal2, JobOrderHeaderSchema.JD_CustomDecimal2);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDecimal3, JobOrderHeaderSchema.JD_CustomDecimal3);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDecimal4, JobOrderHeaderSchema.JD_CustomDecimal4);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomDecimal5, JobOrderHeaderSchema.JD_CustomDecimal5);

			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomFlag1, JobOrderHeaderSchema.JD_CustomFlag1);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomFlag2, JobOrderHeaderSchema.JD_CustomFlag2);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomFlag3, JobOrderHeaderSchema.JD_CustomFlag3);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomFlag4, JobOrderHeaderSchema.JD_CustomFlag4);
			AssertOrderAttributeFilter(Constants.CustomLabels.Order.CustomFlag5, JobOrderHeaderSchema.JD_CustomFlag5);

			ResetModuleFilters();
			((ModuleTextBaseFilter)FilterStripBizO["Any Text Attribute"]).IsActive = true;
			((ModuleTextBaseFilter)FilterStripBizO["Any Text Attribute"]).Property = "123";
			AssertSearchResults();

			Globals.IsWeb = true;
			try
			{
				fFilterStripBizO = null;
				OrgHeader testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				FilterStripBizO.LoggedInWebUsersOrg = testLoggedInOrg;

				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();

				foreach (ModuleFilter filter in FilterStripBizO)
				{
					AssertNotEquals("Any attribute filter should not exist", FilterCategories.AttributeSearch, filter.Category);
				}

				OrgCustomLabels opCA1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				opCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
				opCA1.OT_Caption = "OrgProxysCA1";

				GlbCompany.CurrentCompany.OrgProxy.MiscServ[OrgMiscServSchema.OM_IMPartAttrib1Name.Name] = "OrgProxysPA1";

				OrgCustomLabels loCA2 = testLoggedInOrg.CustomLabels.AddNew();
				loCA2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
				loCA2.OT_Caption = "LoggedInOrgsCA2";

				Factory.Save();
				fFilterStripBizO = null;
				FilterStripBizO.LoggedInWebUsersOrg = testLoggedInOrg;

				AssertNotNull("OrgProxy`s filter is here", FilterStripBizO["OrgProxysCA1 - CA"]);
				AssertNotNull("LoggedInOrg`s filter for CustomAttribute2 is here", FilterStripBizO["LoggedInOrgsCA2 - CA"]);
				AssertNull("OrgProxy`s filter for PartAttribute1 was not inherited", FilterStripBizO["OrgProxysPA1 - CA"]);

				OrgCustomLabels loCA1 = testLoggedInOrg.CustomLabels.AddNew();
				loCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
				loCA1.OT_Caption = "LoggedInOrgsCA1";

				testLoggedInOrg.MiscServ[OrgMiscServSchema.OM_IMPartAttrib1Name.Name] = "LoggedInOrgsPA1";

				Factory.Save();
				fFilterStripBizO = null;
				FilterStripBizO.LoggedInWebUsersOrg = testLoggedInOrg;

				AssertNotNull("LoggedInOrg`s filter for CustomAttribute1 has overriden OrgProxy`s one", FilterStripBizO["LoggedInOrgsCA1 - CA"]);
				AssertNull("OrgProxy`s filter for CustomAttribute1 was overriden", FilterStripBizO["OrgProxysCA1 - CA"]);
				AssertNotNull("LoggedInOrg`s filter for CustomAttribute2 is here", FilterStripBizO["LoggedInOrgsCA2 - CA"]);
				AssertNull("OrgProxy`s filter for PartAttribute1 was not inherited", FilterStripBizO["OrgProxysPA1 - CA"]);
				AssertNotNull("LoggedInOrg`s filter for PartAttribute1 is here", FilterStripBizO["LoggedInOrgsPA1 - PA1"]);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#endregion

		#region Workflow

		public virtual void TestWorkflowFiltersPresent()
		{
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", FilterStripBizO["Milestone Date"]);
		}

		public virtual void TestDoesntFilterByCompanyForAnyOpenTask()
		{
			Assert(FilterStripBizO.ShouldAddWorkflowFilters);
			GlbStaff user = Factory.New<GlbStaff>();
			user.GS_Code = "ZZZ";
			user.GS_LoginName = "ZZZ";

			GlbCompany company = Factory.New<GlbCompany>();
			Factory.Save();

			ProcessTask task1 = (ExpectSearchResultsToContain as IWorkflowProvider).WorkflowItems.Tasks.AddNew();

			task1.P9_GC = company.PK;
			task1.P9_GS_NKAssignedStaffMember = "ZZZ";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Any Open Task Assigned To"];
			filter.Property = "ZZZ";
			filter.IsActive = true;
			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var filterCollection = GetNewOrdersBaseFilterBusinessObject().ModuleFilters;

			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);

			PrepareWorkflowTemplateCustomFields();

			filterCollection = GetNewOrdersBaseFilterBusinessObject().ModuleFilters;
			var errorMessage = ShouldDisplayWorkflowFilters ? "Custom filters should be displayed" : "Custom filters should not be displayed";

			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(filterCollection["C11"]), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(filterCollection["C12"]), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(filterCollection["C21"]), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(filterCollection["C22"]), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(filterCollection["Workflow Flags"]), ShouldDisplayWorkflowFilters);
		}

		public void TestAddWorkflowRelatedFilters()
		{
			var milestonesFilter = FilterStripBizO["Milestones"];
			var tasksFilter = FilterStripBizO["Tasks"];
			var triggersFilter = FilterStripBizO["Triggers"];
			var exceptionsFilter = FilterStripBizO["Exceptions"];

			var errorMessage = ShouldDisplayWorkflowFilters ? "Workflow related filters should be displayed" : "Workflow related filters should not be displayed";

			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(milestonesFilter), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(tasksFilter), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(triggersFilter), ShouldDisplayWorkflowFilters);
			AssertEquals(errorMessage, !BusinessObject.IsNullOrDeleted(exceptionsFilter), ShouldDisplayWorkflowFilters);
		}

		protected abstract bool ShouldDisplayWorkflowFilters { get; }

		protected virtual ZString WorkflowDescriptorCode => WorkflowDescriptors.OrderWorkflowDescriptorCode;

		void PrepareWorkflowTemplateCustomFields()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptorCode;

			var def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			var def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptorCode;

			var def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			var def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region Number and References Filter Tests

		public void TestOrderLineNumberFilters()
		{
			AssertOrderLineTextOrNumberFilter("Product #", JobOrderLineSchema.JO_Partno);
		}

		public virtual void TestOrderNumberFilter()
		{
			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextFilter)filter["Order #"]).Property = "ABC";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrderCollection orders = new OrderCollection(Factory);

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "123";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Factory.Save();

			orders.AdditionalFilter = filter.Filter;
			AssertCollectionContains(Order1, orders);
			AssertCollectionNotContains(Order2, orders);

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(2);
			Factory.Save();

			orders.AdditionalFilter = filter.Filter;
			AssertCollectionContains(Order1, orders);
			AssertCollectionContains(Order2, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "C-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;

			orders.AdditionalFilter = filter.Filter;
			AssertCollectionContains(Order1, orders);
			AssertCollectionNotContains(Order2, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "C-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(12);
			Factory.Save();

			orders.AdditionalFilter = filter.Filter;
			((IActiveBusinessObjectCollection)orders).Refresh();
			AssertCollectionContains(Order1, orders);
			AssertCollectionContains(Order2, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "ABC-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			orders.AdditionalFilter = filter.Filter;
			((IActiveBusinessObjectCollection)orders).Refresh();
			AssertCollectionContains(Order1, orders);
			AssertCollectionNotContains(Order2, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "C-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC-1";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ZZC-123";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(12);
			Factory.Save();

			orders.AdditionalFilter = filter.Filter;
			((IActiveBusinessObjectCollection)orders).Refresh();
			AssertCollectionContains(Order1, orders);
			AssertCollectionContains(Order2, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "C-1'";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			orders.AdditionalFilter = filter.Filter;
			((IActiveBusinessObjectCollection)orders).Refresh();
			AssertCollectionNotContains(Order1, orders);
			AssertCollectionNotContains(Order2, orders);
		}

		public void TestOrderNumberFilter_MoreThan35Characters()
		{
			var orders = GetNewCollection();

			Order1.JD_OrderNumber = "ZABCDEFG";
			Order1.JD_OrderNumberSplit = new ZByte(7);

			Factory.Save();

			var filter = FilterStripBizO;

			((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG7HIJKLMN7OPQRSTUVWXYZ7ABCDEABC,";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "A-BCDEFG7HIJKLMN7OPQRSTUVWXYZ7ABCDEABC";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG7HIJKLMN7OPQRSTUVWXYZ7ABCDEABC-7";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG-7,HIJKLMN-7,OPQRSTU-,VWXYAZ-7";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "QWQWQWQWQWQW,ABCDEFG-7,YZABCD,TUVWXYZ";

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "ZABCDEFG-7-HIJKLMN-7-OPQRSTU-7-VWXYZ-7";

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			((ModuleTextFilter)filter["Order #"]).Property = "ABCDEF,EEEEEEE,AZAZAZ,TUVWXYZ,ABCDEFGH,LSJLRSE";

			AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
			AssertCollectionContains(ExpectSearchResultsToContain, orders);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

			using (RawDataRegistry.Instance.MultiSearchSeparator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "-"))
			{
				((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG7HIJKLMN7OPQRSTUVWXYZ7-";
				((ModuleTextFilter)filter["Order #"]).IsActive = true;
				((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

				AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
				AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
				AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

				((ModuleTextFilter)filter["Order #"]).Property = "9-ABCDEFG7HIJKLMN7OPQRSTUVWXYZ71234";
				((ModuleTextFilter)filter["Order #"]).IsActive = true;
				((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

				AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
				AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
				AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

				((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG7HIJKLMN7OPQRSTUVWXYZ71234-7";
				((ModuleTextFilter)filter["Order #"]).IsActive = true;
				((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

				AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
				AssertCollectionNotContains(ExpectSearchResultsToContain, orders);
				AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);

				((ModuleTextFilter)filter["Order #"]).Property = "ABCDEFG-7-HIJKLMN-7-OPQRSTU-7-VWXYZ-79";

				AssertNoExceptionThrown("Filter should not throw index out of bounds exception", () => orders.AdditionalFilter = filter.Filter);
				AssertCollectionContains(ExpectSearchResultsToContain, orders);
				AssertCollectionNotContains(ExpectSearchResultsToNotContain, orders);
			}
		}

		public void TestOrderHeaderNumberFilters()
		{
			AssertOrderTextOrNumberFilter("Booking Conf. Ref. #", JobOrderHeaderSchema.JD_BookingConfRef);
			AssertOrderTextOrNumberFilter("Invoice #", JobOrderHeaderSchema.JD_InvoiceNumber);
			AssertOrderTextOrNumberFilter("House Bill", JobOrderHeaderSchema.JD_Waybill);
			AssertOrderTextOrNumberFilter("Master Bill", JobOrderHeaderSchema.JD_MasterWaybill);
		}

		public void TestOrderHeaderNumberFiltersWithExceededValue_NoErrorReported()
		{
			AssertOrderTextOrNumberFilterWithExceededValue("Booking Conf. Ref. #", JobOrderHeaderSchema.JD_BookingConfRef);
			AssertOrderTextOrNumberFilterWithExceededValue("Invoice #", JobOrderHeaderSchema.JD_InvoiceNumber);
			AssertOrderTextOrNumberFilterWithExceededValue("House Bill", JobOrderHeaderSchema.JD_Waybill);
			AssertOrderTextOrNumberFilterWithExceededValue("Master Bill", JobOrderHeaderSchema.JD_MasterWaybill);
		}

		public void TestShipmentNumberFilters()
		{
			AssertShipmentTextOrNumberFilter("House Bill", JobShipmentSchema.JS_HouseBill);
			AssertShipmentNoFilter();
		}

		public void TestShipmentNumberFilter_BlankOrNotBlank()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "hello";

			Order1.JD_JS = ZGuid.Empty;
			Order2.JD_JS = shipment.PK;
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Shipment #"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertSearchResults();

			Order1.JD_JS = shipment.PK;
			Order2.JD_JS = ZGuid.Empty;
			Factory.Save();

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertSearchResults();
		}

		public void TestConsolNumberFilters()
		{
			AssertConsolTextOrNumberFilter("Master Bill", JobConsolSchema.JK_MasterBillNum);
		}

		public void TestConsolNumberFiltersWithDashAndSpace()
		{
			var filterDescription = "Master Bill";
			var filterColumn = JobConsolSchema.JK_MasterBillNum;

			SetupModuleTextFilter(filterDescription, "1 2-3");

			Consol1[filterColumn] = "1 2-3";
			Consol2[filterColumn] = "ABCDEF";
			Factory.Save();
			AssertSearchResults();

			Consol1[filterColumn] = "123";
			Consol2[filterColumn] = "ABCDEF";
			Factory.Save();
			AssertSearchResults();
		}

		public void TestOrderMasterBillNumberFiltersWithDashAndSpace()
		{
			var filterDescription = "Master Bill";
			var filterColumn = JobOrderHeaderSchema.JD_MasterWaybill;

			SetupModuleTextFilter(filterDescription, "1 2-3");

			Order1[filterColumn] = "1 2-3";
			Order2[filterColumn] = "ABCDEF";
			Factory.Save();
			AssertSearchResults  ();

			Order1[filterColumn] = "123";
			Order2[filterColumn] = "ABCDEF";
			Factory.Save();
			AssertSearchResults();
		}

		#region TestContainerNoFilter

		public void TestContainerNoFilterWithLineDeliveryContainers()
		{
			SetupModuleTextFilter("Container #");

			LineDeliveryContainer1.J5_ContainerNum = "123";
			LineDeliveryContainer2.J5_ContainerNum = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		public void TestContainerNoFilterWithOrderContainers()
		{
			SetupModuleTextFilter("Container #");

			OrderContainer1.J1_ContainerNumber = "123";
			OrderContainer2.J1_ContainerNumber = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		public void TestContainerNoFilterWithJobContainers()
		{
			SetupModuleTextFilter("Container #");

			JobContainer1.JC_ContainerNum = "123";
			JobContainer2.JC_ContainerNum = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		#endregion

		#region TestVesselFilter

		public void TestPlannedVesselFilter()
		{
			#region Setup

			Order1.JD_RV_NKArrivalVessel = "NKVessel";
			Order1.JD_RV_NKDepartureVessel = "";
			Order1.JD_RV_NKIntermediateVessel = "";
			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).Property = "";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).NkProperty = "NK";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_RV_NKArrivalVessel = "";
			Order1.JD_RV_NKDepartureVessel = "NKVessel";
			Order1.JD_RV_NKIntermediateVessel = "";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_RV_NKArrivalVessel = "";
			Order1.JD_RV_NKDepartureVessel = "";
			Order1.JD_RV_NKIntermediateVessel = "NKVessel";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVesselFilter_ConsolLinked()
		{
			#region Setup

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "NKVessel";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment1.PK;

			CommonConsol consol1 = shipment1.Consols.AddNew();
			consol1.Transports[0].JW_JX = sailing.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			// test with vessel on unlinked sailing's voyage

			consol1.Transports[0].JW_IsLinked = false;
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVesselFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_VesselName] = "NKVessel";
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_VesselName] = "AUVessel";
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region TestVoyageFlightFilter

		public void TestPlannedVoyageFlightFilter()
		{
			#region Setup

			Order1.JD_ArrivalVoyage = "NKVoyage";
			Order1.JD_DepartureVoyage = "";
			Order1.JD_IntermediateVoyage = "";
			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).Property = "NK";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).NkProperty = "";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Flight/Voyage (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Flight/Voyage (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_ArrivalVoyage = "";
			Order1.JD_DepartureVoyage = "NKVoyage";
			Order1.JD_IntermediateVoyage = "";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Flight/Voyage (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Flight/Voyage (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_ArrivalVoyage = "";
			Order1.JD_DepartureVoyage = "";
			Order1.JD_IntermediateVoyage = "NKVoyage";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Flight/Voyage (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Flight/Voyage (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestPlannedVoyageFlightFilter_MaxLength()
		{
			var voyageVesselFilter = (ModuleTextAndNkFilter)FilterStripBizO["Planned Flight/Voyage # and Vessel"];
			AssertEquals(voyageVesselFilter.MaxLength, JobOrderHeaderSchema.JD_DepartureVoyage.MaxLength);
		}

		public void TestActualVoyageFlightFilter_ConsolLinked()
		{
			#region Setup

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "NKFlight";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment1.PK;

			CommonConsol consol1 = shipment1.Consols.AddNew();
			consol1.Transports[0].JW_JX = sailing.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Flight/Voyage (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Flight/Voyage (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			// test with vessel on unlinked sailing's voyage
			consol1.Transports[0].JW_IsLinked = false;
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Flight/Voyage (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Flight/Voyage (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVoyageFlightFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_VoyageFlightNo] = "NKVoyage";
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_VoyageFlightNo] = "AUVoyage";
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Flight/Voyage (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Flight/Voyage (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVoyageFlightFilter_MaxLength()
		{
			var voyageVesselFilter = (VoyageVesselModuleFilter)FilterStripBizO["Actual Flight/Voyage # and Vessel"];
			AssertEquals(voyageVesselFilter.MaxLength, JobVoyageSchema.JV_VoyageFlight.MaxLength);
		}

		#endregion

		#region TestVoyageAndVesselFilter

		public void TestPlannedVoyageAndVesselFilter()
		{
			#region Setup

			Order1.JD_ArrivalVoyage = "NKVoyage";
			Order1.JD_DepartureVoyage = "NKVoyage2";
			Order1.JD_IntermediateVoyage = "";

			Order1.JD_RV_NKArrivalVessel = "NKVessel";
			Order1.JD_RV_NKDepartureVessel = "";
			Order1.JD_RV_NKIntermediateVessel = "";

			Order2.JD_ArrivalVoyage = "Voyage1";
			Order2.JD_DepartureVoyage = "Voyage2";
			Order2.JD_IntermediateVoyage = "Voyage3";

			Order2.JD_RV_NKArrivalVessel = "Vessel1";
			Order2.JD_RV_NKDepartureVessel = "Vessel2";
			Order2.JD_RV_NKIntermediateVessel = "Vessel13";
			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).Property = "NK";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).NkProperty = "NK";
			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertEquals("Operator is Starts With", ((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).SqlComparisonOperator, SQLComparisonOperator.StartsWith);
			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_RV_NKArrivalVessel = "";
			Order1.JD_RV_NKDepartureVessel = "NKVessel";
			Order1.JD_RV_NKIntermediateVessel = "";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_RV_NKDepartureVessel = "";
			Order1.JD_RV_NKIntermediateVessel = "NKVessel";
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionNotContains("Planned Vessel (as in Order) should not match either order.", ExpectSearchResultsToNotContain, filterResults);

			// Test isBlank and isNotBlank
			Order1.JD_RV_NKIntermediateVessel = "";
			Order1.JD_DepartureVoyage = "";
			Factory.Save();

			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Vessel (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_RV_NKArrivalVessel = "";
			Order1.JD_RV_NKDepartureVessel = "";
			Order1.JD_RV_NKIntermediateVessel = "";
			Factory.Save();

			((ModuleTextAndNkFilter)filter["Planned Flight/Voyage # and Vessel"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionNotContains("Planned Vessel (as in Order) should not match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionContains("Planned Vessel (as in Order) should match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVoyageAndVesselFilter_ConsolLinked()
		{
			#region Setup

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "NKVessel";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "NKFlight";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment1.PK;

			CommonConsol consol1 = shipment1.Consols.AddNew();
			consol1.Transports[0].JW_JX = sailing.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			// test with vessel on unlinked sailing's voyage

			consol1.Transports[0].JW_IsLinked = false;
			Factory.Save();

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualVoyageAndVesselFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_VoyageFlightNo] = "NKVoyNo";
			declaration1[JobDeclarationSchema.Constants.JE_VesselName] = "NKVessel";
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_VoyageFlightNo] = "";
			declaration2[JobDeclarationSchema.Constants.JE_VesselName] = "";
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			ResetModuleFilters();

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).VoyageFlightNo = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).Vessel = "NK";
			((VoyageVesselModuleFilter)filter["Actual Flight/Voyage # and Vessel"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Vessel (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Vessel (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#endregion

		#region Quantity Invoiced / Received

		public void TestQuantityInvoicedFilter()
		{
			ResetModuleFilters();

			Line1.JO_QtyInvoiced = 10;
			Line2.JO_QtyInvoiced = 20;
			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Order Line - Quantity Invoiced"];
			filter.Property1 = 0;
			filter.Property2 = 5;
			filter.IsActive = true;
			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(0, filterResults.Count);

			filter.Property1 = 10;
			filter.Property2 = 10;
			filter.IsActive = true;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, filterResults.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, filterResults);

			filter.Property1 = 0;
			filter.Property2 = 20;
			filter.IsActive = true;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(2, filterResults.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, filterResults);
		}

		public void TestQuantityReceivedFilter()
		{
			ResetModuleFilters();

			Line1.JO_QtyReceived = 10;
			Line2.JO_QtyReceived = 20;
			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Order Line - Quantity Received"];
			filter.Property1 = 0;
			filter.Property2 = 5;
			filter.IsActive = true;
			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(0, filterResults.Count);

			filter.Property1 = 10;
			filter.Property2 = 10;
			filter.IsActive = true;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, filterResults.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, filterResults);

			filter.Property1 = 0;
			filter.Property2 = 20;
			filter.IsActive = true;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertEquals(2, filterResults.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, filterResults);
		}

		#endregion

		#region Date Filter Tests

		public void TestOrderDateFilters()
		{
			// Detach the Shipments, otherwise dates will be proxied from them. This mimics the same behaviour in the
			// Order module, though perhaps we should changes the queries to also look at the attached Shipments' dates.
			Order1.JD_JS = ZGuid.Empty;
			Order2.JD_JS = ZGuid.Empty;

			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.OrderDate, JobOrderHeaderSchema.JD_OrderDate);
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.ConfirmedDate, JobOrderHeaderSchema.JD_BookingConfDate);
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.FollowUpDate, JobOrderHeaderSchema.JD_FollowUpDate);
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.ReqInStore, JobOrderHeaderSchema.JD_DeliveryRequiredBy);
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.ReqExWorks, JobOrderHeaderSchema.JD_ExWorksRequiredBy);
		}

		#endregion

		#region Product Filter

		public void TestProductFilter()
		{
			OrgSupplierPart product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "TestProduct";
			Line1.JO_Partno = product.OP_PartNum;
			Factory.Save();

			ResetModuleFilters();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Product"];
			filter.IsActive = true;
			filter.Property = product.PK;

			AssertSearchResults();
		}

		#endregion

		#region Organisation Filter Tests

		public void TestInactiveBuyerSupplierFilter()
		{
			var originalActiveStatus = Org1.OH_IsActive;
			Org1.OH_IsActive = false;
			Order1[JobOrderHeaderSchema.JD_OA_BuyerAddress.Name] = Org1.MainAddress.PK;
			Order1[JobOrderHeaderSchema.JD_OA_SupplierAddress.Name] = Org1.MainAddress.PK;

			Factory.Save();

			ResetModuleFilters();
			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO["Buyer / Supplier"];
			filter.IsActive = true;
			filter.Property1 = Org1.PK;
			filter.Property2 = ZGuid.Empty;
			AssertHasWarning(filter.Property1Info, "Organization is in-active.");
			AssertNoWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			ResetModuleFilters();

			filter.IsActive = true;
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = Org1.PK;
			AssertNoWarning(filter.Property1Info, "Organization is in-active.");
			AssertHasWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			Org1.OH_IsActive = originalActiveStatus;
		}

		public void TestInactivePlannedSendReceiveAgentsFilter()
		{
			var originalActiveStatus = Org1.OH_IsActive;
			Org1.OH_IsActive = false;
			Order1[JobOrderHeaderSchema.JD_OH_SendingAgent.Name] = Org1.PK;
			Order1[JobOrderHeaderSchema.JD_OH_ReceivingAgent.Name] = Org1.PK;

			Factory.Save();

			ResetModuleFilters();
			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO["Planned Send / Receive Agents"];
			filter.IsActive = true;
			filter.Property1 = Org1.PK;
			filter.Property2 = ZGuid.Empty;
			AssertHasWarning(filter.Property1Info, "Organization is in-active.");
			AssertNoWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			ResetModuleFilters();

			filter.IsActive = true;
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = Org1.PK;
			AssertNoWarning(filter.Property1Info, "Organization is in-active.");
			AssertHasWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			Org1.OH_IsActive = originalActiveStatus;
		}

		public void TestInactiveActualSendReceiveAgentsFilter()
		{
			var originalActiveStatus = Org1.OH_IsActive;
			Org1.OH_IsActive = false;
			var shipment = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_SendingForwarderAddress = Org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = Org1.MainAddress.PK;
			Order1[JobOrderHeaderSchema.JD_OH_SendingAgent.Name] = Org1.PK;
			Order1[JobOrderHeaderSchema.JD_OH_ReceivingAgent.Name] = Org1.PK;

			Factory.Save();

			ResetModuleFilters();
			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO["Actual Send / Receive Agents"];
			filter.IsActive = true;
			filter.Property1 = Org1.PK;
			filter.Property2 = ZGuid.Empty;
			AssertHasWarning(filter.Property1Info, "Organization is in-active.");
			AssertNoWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			ResetModuleFilters();

			filter.IsActive = true;
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = Org1.PK;
			AssertNoWarning(filter.Property1Info, "Organization is in-active.");
			AssertHasWarning(filter.Property2Info, "Organization is in-active.");
			AssertSearchResults();

			Org1.OH_IsActive = originalActiveStatus;
		}

		public void TestOrderOrganisationFilters()
		{
			AssertOrderAddressFilter("Buyer / Supplier", JobOrderHeaderSchema.JD_OA_BuyerAddress, JobOrderHeaderSchema.JD_OA_SupplierAddress);
			AssertOrderGuidsFilter("Planned Send / Receive Agents", JobOrderHeaderSchema.JD_OH_SendingAgent, JobOrderHeaderSchema.JD_OH_ReceivingAgent);
			AssertOrderTextOrNumberFilter("Registered Staff", JobOrderHeaderSchema.JD_SystemCreateUser);
		}

		public void TestPlannedCarrierFilter()
		{
			#region Setup

			Org1.OH_IsShippingProvider = true;
			Org2.OH_IsShippingProvider = true;
			Order1.JD_OH_Carrier = Org1.PK;
			Order2.JD_OH_Carrier = Org2.PK;

			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var preAdviceCarrier = NewCarrierOrgHeader("PREADVICE");
			preAdvice.EF_OH_Carrier = preAdviceCarrier.PK;
			preAdvice.BuyerPK = preAdviceCarrier.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Planned Carrier"];
			filter.IsActive = true;
			filter.Property = Org1.PK;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("First order should match order header carrier", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Second order should not match order header carrier", ExpectSearchResultsToNotContain, filterResults);

			Order1.JD_EF_ShipmentPrePlanning = preAdvice.PK;
			Factory.Save();

			filter.Property = preAdviceCarrier.PK;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("First order should match preAdvice carrier", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Second order should not match preAdvice carrier", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualCarrierFilter_DeclarationLinked()
		{
			#region Setup

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier = NewCarrierOrgHeader("DECLARATION");
			declaration["JE_OH_ShippingLine"] = declarationCarrier.PK;
			Order1.JD_JE = declaration.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Actual Carrier"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.Property = declarationCarrier.PK;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Declaration Carrier should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Declaration Carrier should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualCarrierFilter_ConsolLinked()
		{
			#region Setup

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			Order1.JD_JS = shipment.PK;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "SGSIN";
			departureConsol.JK_RL_NKDischargePort = "NZWLG";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "NZWLG";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var arrivalConsolCarrier = NewCarrierOrgHeader("ARVCONSOL");
			arrivalConsol.JK_OA_ShippingLineAddress = arrivalConsolCarrier.MainAddress.PK;

			var arrivalConsolLegCarrier = NewCarrierOrgHeader("ARVCONLEG");
			arrivalConsol.Transports[0].JW_IsLinked = false;
			arrivalConsol.Transports[0].CarrierPK = arrivalConsolLegCarrier.PK;

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var departureConsolLegCarrier = NewCarrierOrgHeader("DEPCONLEG");
			departureConsol.Transports[0].JW_IsLinked = true;
			departureConsol.Transports[0].JW_Vessel = vessel.RV_FK;
			departureConsol.Transports[0].JW_VoyageFlight = "123";
			departureConsol.Transports[0].CarrierPK = departureConsolLegCarrier.PK;

			var departureConsolCarrier = NewCarrierOrgHeader("DEPCONSOL");
			departureConsol.JK_OA_ShippingLineAddress = departureConsolCarrier.MainAddress.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Actual Carrier"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.Property = arrivalConsolCarrier.PK;
			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Arrival Consol Carrier should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Arrival Consol Carrier should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Property = departureConsolCarrier.PK;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Departure Consol Carrier should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Departure Consol Carrier should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Property = arrivalConsolLegCarrier.PK;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Arrival Consol Leg Carrier should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Arrival Consol Leg Carrier should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Property = departureConsolLegCarrier.PK;
			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Departure Consol Leg Carrier should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Departure Consol Leg Carrier should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public virtual void TestPlannedNotifyPartyFilter()
		{
			#region Setup

			Order1.NotifyPartyDocAddress.OrganisationPK = Org1.PK;
			Order2.NotifyPartyDocAddress.OrganisationPK = Org2.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Planned Notify Party"];
			filter.IsActive = true;
			filter.Property = Org1.PK;

			var filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public virtual void TestActualNotifyPartyFilter_ShipmentLinked()
		{
			#region Setup

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.NotifyPartyDocumentaryAddress.OrganisationPK = Org1.PK;
			Order1.NotifyPartyDocAddress.OrganisationPK = Org2.PK;
			Order1.JD_JS = shipment1.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.NotifyPartyDocumentaryAddress.OrganisationPK = Org2.PK;
			Order2.NotifyPartyDocAddress.OrganisationPK = Org1.PK;
			Order2.JD_JS = shipment2.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party"];
			filter.IsActive = true;
			filter.Property = Org1.PK;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualNotifyPartyFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration1[JobDeclarationSchema.Constants.JE_OH_NotifyParty] = Org1.PK;
			Order1.JD_JE = declaration1.PK;
			Order1.NotifyPartyDocAddress.OrganisationPK = Org2.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration2[JobDeclarationSchema.Constants.JE_OH_NotifyParty] = Org2.PK;
			Order2.JD_JE = declaration2.PK;
			Order2.NotifyPartyDocAddress.OrganisationPK = Org1.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party"];
			filter.IsActive = true;
			filter.Property = Org1.PK;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party (as Declaration) should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestPlannedSendReceiveAgentsFilter()
		{
			#region Setup

			Order1.JD_OH_SendingAgent = Org1.PK;
			Order1.JD_OH_ReceivingAgent = Org2.PK;
			Order2.JD_OH_SendingAgent = Org2.PK;
			Order2.JD_OH_ReceivingAgent = Org1.PK;
			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleGuidsFilter)filter["Planned Send / Receive Agents"]).Property1 = Org1.PK;
			((ModuleGuidsFilter)filter["Planned Send / Receive Agents"]).Property2 = Org2.PK;
			((ModuleGuidsFilter)filter["Planned Send / Receive Agents"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Send / Receive Agents (as in Order) should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Send / Receive Agents (as in Order) should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualSendReceiveAgentsFilter_ConsolLinked()
		{
			#region Setup

			var shipment1 = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment1.PK;

			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_OA_SendingForwarderAddress = Org1.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = Org2.MainAddress.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			Order2.JD_JS = shipment2.PK;

			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_OA_SendingForwarderAddress = Org2.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = Org1.MainAddress.PK;

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property1 = Org1.PK;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property2 = Org2.PK;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Send / Receive Agents (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Send / Receive Agents (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualSendReceiveAgentsFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_OH_Forwarder] = Org1.PK;
			declaration1[JobDeclarationSchema.Constants.JE_MessageType] = (ZString)Core.Constants.Sales.Mode.Export;
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_OH_Forwarder] = Org2.PK;
			declaration2[JobDeclarationSchema.Constants.JE_MessageType] = (ZString)Core.Constants.Sales.Mode.Import;
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property1 = Org1.PK;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property2 = ZGuid.Empty;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Send / Receive Agents (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Send / Receive Agents (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property1 = ZGuid.Empty;
			((ModuleGuidsFilter)filter["Actual Send / Receive Agents"]).Property2 = Org2.PK;
			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionNotContains("Actual Send / Receive Agents (as in Declaration) should not match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionContains("Actual Send / Receive Agents (as in Declaration) should match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestBuyerCompanyNameFilter()
		{
			Order1.Buyer.OH_FullName = "CARGO WISE EDI";
			Order2.Buyer.OH_FullName = "COMPANY INTERNATIONAL";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Buyer Company Name"];
			filter.Property = "C";
			filter.IsActive = true;
			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(2, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = "CARGO";
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = "BOO";
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(0, collection.Count);
		}

		public void TestSupplierCompanyNameFilter()
		{
			OrgHeader supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "COMPANY INTERNATIONAL";
			supplier2.OH_FullName = "CARGO WISE EDI";
			Order1.SupplierPK = supplier1.PK;
			Order2.SupplierPK = supplier2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Supplier Company Name"];
			filter.Property = "C";
			filter.IsActive = true;
			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(2, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = "COMPANY";
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = "BOO";
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(0, collection.Count);
		}

		public void TestCommodityCodeFilter()
		{
			Line1.JO_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			Line2.JO_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;

			RefCommodityCode commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";

			RefCommodityCode commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_RH_NKCommodityCode = commodityCode1.RH_Code;
			OrgPartRelation relation = part1.RelatedOrganisations.AddNew();
			relation.OU_OH = Line1.Supplier.PK;
			relation = part1.RelatedOrganisations.AddNew();
			relation.OU_OH = Line2.Supplier.PK;

			OrgSupplierPart part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_RH_NKCommodityCode = commodityCode2.RH_Code;
			relation = part2.RelatedOrganisations.AddNew();
			relation.OU_OH = Line1.Supplier.PK;
			relation = part2.RelatedOrganisations.AddNew();
			relation.OU_OH = Line2.Supplier.PK;

			OrgSupplierPart part3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part3.OP_PartNum = "Part3";
			part3.OP_RH_NKCommodityCode = commodityCode1.RH_Code;
			relation = part3.RelatedOrganisations.AddNew();
			relation.OU_OH = Line1.Supplier.PK;
			relation = part3.RelatedOrganisations.AddNew();
			relation.OU_OH = Line2.Supplier.PK;

			Line1.JO_Partno = part1.OP_PartNum;
			Line2.JO_Partno = part3.OP_PartNum;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Commodity Code"];
			filter.Property = commodityCode1.RH_Code;
			filter.IsActive = true;
			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(2, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = commodityCode2.RH_Code;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(0, collection.Count);

			Line2.JO_Partno = part2.OP_PartNum;
			Factory.Save();

			filter.Property = commodityCode1.RH_Code;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);
		}

		#endregion

		#region Location Filter Tests

		public void TestPlannedOrderLocationFilters()
		{
			AssertOrderLocationFilter("Planned Load / Discharge", JobOrderHeaderSchema.JD_RL_NKPortOfLoading, JobOrderHeaderSchema.JD_RL_NKPortOfDischarge);
			AssertOrderLocationFilter("Planned Origin / Destination", JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo);
		}

		public void TestPlannedLoadDischargeFilter()
		{
			#region Setup

			Order1[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "AUSYD";
			Order1[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "NZAKL";
			Order2[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "USLAX";
			Order2[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "HKHKG";

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Planned Load / Discharge"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Planned Load / Discharge"]).Property2 = "NZAKL";
			((ModuleLocationFilter)filter["Planned Load / Discharge"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			CombineAssertions(() =>
			{
				AssertCollectionContains("Planned Load / Discharge (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
				AssertCollectionNotContains("Planned Load / Discharge (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
			});

			((ModuleLocationFilter)filter["Planned Load / Discharge"]).Property1 = "AU";
			((ModuleLocationFilter)filter["Planned Load / Discharge"]).Property2 = "NZ";

			filterResults = GetCollectionLoadedWithFilterApplied();
			CombineAssertions(() =>
			{
				AssertCollectionContains("Planned Load / Discharge (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
				AssertCollectionNotContains("Planned Load / Discharge (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
			});
		}

		public void TestActualLoadDischargeFilter_ConsolLinked()
		{
			#region Setup

			var shipment1 = Factory.New<ForwardingShipment>();
			Order1.JD_JS = shipment1.PK;

			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "NZWLG";

			var shipment2 = Factory.New<ForwardingShipment>();
			Order2.JD_JS = shipment2.PK;

			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).Property1 = "SGSIN";
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).Property2 = "NZWLG";
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Load / Discharge (as in Consol) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Load / Discharge (as in Consol) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualLoadDischargeFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "SGSIN";
			declaration1[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "NZWLG";
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AUSYD";
			declaration2[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "USLAX";
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).Property1 = "SGSIN";
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).Property2 = "NZWLG";
			((ModuleLocationFilter)filter["Actual Load / Discharge"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Load / Discharge (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Load / Discharge (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestPlannedOriginDestinationFilter()
		{
			#region Setup

			Order1[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "AUSYD";
			Order1[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "NZAKL";
			Order2[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "USLAX";
			Order2[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "HKHKG";

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Planned Origin / Destination"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Planned Origin / Destination"]).Property2 = "NZAKL";
			((ModuleLocationFilter)filter["Planned Origin / Destination"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Planned Origin / Destination (as in Order) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Planned Origin / Destination (as in Order) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualOriginDestinationFilter_ShipmentLinked()
		{
			#region Setup

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "SGSIN";
			shipment1.JS_RL_NKDestination = "AUSYD";
			Order1.JD_JS = shipment1.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			Order2.JD_JS = shipment2.PK;

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).Property1 = "SGSIN";
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).Property2 = "AUSYD";
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Origin / Destination (as in Shipment) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Origin / Destination (as in Shipment) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestActualOriginDestinationFilter_DeclarationLinked()
		{
			#region Setup

			var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier1 = NewCarrierOrgHeader("DECLARATION1");
			declaration1[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "SGSIN";
			declaration1[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "NZWLG";
			Order1.JD_JE = declaration1.PK;

			var declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationCarrier2 = NewCarrierOrgHeader("DECLARATION2");
			declaration2[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AUSYD";
			declaration2[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "USLAX";
			Order2.JD_JE = declaration2.PK;

			Factory.Save();

			#endregion

			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).Property1 = "SGSIN";
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).Property2 = "NZWLG";
			((ModuleLocationFilter)filter["Actual Origin / Destination"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Actual Origin / Destination (as in Declaration) should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Actual Origin / Destination (as in Declaration) should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region Mode Filter Tests

		public void TestOrderModeFilters()
		{
			AssertOrderTextOrNumberFilter("Container Mode", JobOrderHeaderSchema.JD_ContainerMode);
			AssertOrderTextOrNumberFilter("Transport Mode", JobOrderHeaderSchema.JD_TransportMode);
			AssertOrderTextOrNumberFilter("Service Level", JobOrderHeaderSchema.JD_RS_NKServiceLevel_NI);
		}

		#endregion

		#region Status and Flags Tests

		public void TestStatusAndFlagsFilters()
		{
			AssertOrderLineTextOrNumberFilter("Line Status", JobOrderLineSchema.JO_LineStatus);
			AssertOrderTextOrNumberFilter("Order Status", JobOrderHeaderSchema.JD_OrderStatus);
		}

		public void TestStatusUndeliveredFilter()
		{
			ResetModuleFilters();
			ModuleTextBaseFilter filter = (ModuleTextBaseFilter)FilterStripBizO["Order Status"];
			filter.IsActive = true;
			filter.Property = OrdersBaseFilterBusinessObject.UndeliveredOrderStatus;

			Order1.JD_OrderStatus = Constants.OrderStatus.Shipped;
			Order2.JD_OrderStatus = Constants.OrderStatus.Delivered;
			Factory.Save();

			AssertSearchResults();
		}

		public void TestStatusAllFilter()
		{
			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextFilter)filter["Order Status"]).Property = "ALL";
			((ModuleTextFilter)filter["Order Status"]).IsActive = true;

			OrderCollection orders = new OrderCollection(Factory);

			Order1.JD_OrderStatus = Constants.OrderStatus.Shipped;
			Order2.JD_OrderStatus = Constants.OrderStatus.Delivered;
			Factory.Save();

			orders.AdditionalFilter = filter.Filter;

			AssertCollectionContains(Order1, orders);
			AssertCollectionContains(Order2, orders);
		}

		public void TestOrderAttachedUnattachedFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Attached / Unattached Orders"];
			filter.IsActive = true;

			// test with Shipment

			filter.Property = OrdersConstants.OrdersAttachedState.Both;
			Order1.JD_JS = Shipment1.PK;
			Order2.JD_JS = ZGuid.Empty;
			Order1.JD_JE = ZGuid.Empty;
			Order2.JD_JE = ZGuid.Empty;
			Factory.Save();

			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = OrdersConstants.OrdersAttachedState.AttachedOnly;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = OrdersConstants.OrdersAttachedState.UnattachedOnly;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionNotContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			// test with declaration

			filter.Property = OrdersConstants.OrdersAttachedState.Both;
			Order1.JD_JS = ZGuid.Empty;
			Order2.JD_JS = ZGuid.Empty;
			Order1.JD_JE = Declaration.PK;
			Order2.JD_JE = ZGuid.Empty;
			Factory.Save();

			collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = OrdersConstants.OrdersAttachedState.AttachedOnly;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);

			filter.Property = OrdersConstants.OrdersAttachedState.UnattachedOnly;
			collection = GetCollectionLoadedWithFilterApplied();
			AssertCollectionNotContains(ExpectSearchResultsToContain, collection);
			AssertCollectionContains(ExpectSearchResultsToNotContain, collection);
		}

		#endregion

		#region Manufacturer Tests

		public void TestManufacturerFilter()
		{
			#region Setup

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_FullName = "Valid Company 1";
			var orgAddress1 = orgHeader1.Addresses[0];
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress1.E2_AddressType = "MAN";
			Line1.DocAddresses.Add(docAddress1);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "Invalid Company 2";
			var orgAddress2 = orgHeader2.Addresses[0];
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_OA_Address = orgAddress2.PK;
			docAddress2.E2_AddressType = "MAN";
			Line2.DocAddresses.Add(docAddress2);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;
			((ModuleTextFilter)filter["Manufacturer Company Name"]).Property = "Valid Company 1";
			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = true;

			var filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer Name should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer Name should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = false;
			((ModuleGuidFilter)filter["Manufacturer"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)filter["Manufacturer"]).IsActive = true;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestManufacturerFilterFallback()
		{
			#region Setup

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_FullName = "Valid Company 1";
			var orgAddress1 = orgHeader1.Addresses[0];
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress1.E2_AddressType = "MAN";
			docAddress1.E2_OA_Address = orgAddress1.PK;
			Order1.DocAddresses.Add(docAddress1);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "Invalid Company 2";
			var orgAddress2 = orgHeader2.Addresses[0];
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_OA_Address = orgAddress2.PK;
			docAddress2.E2_AddressType = "MAN";
			Order2.DocAddresses.Add(docAddress2);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;
			((ModuleTextFilter)filter["Manufacturer Company Name"]).Property = "Valid Company 1";
			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer Name should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer Name should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = false;
			((ModuleGuidFilter)filter["Manufacturer"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)filter["Manufacturer"]).IsActive = true;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestManufacturerFilterWorksForOverriddenManufacturer()
		{
			#region Setup

			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress1.E2_AddressOverride = true;
			docAddress1.E2_CompanyName = "Valid Company 1";
			docAddress1.E2_AddressType = "MAN";
			Line1.DocAddresses.Add(docAddress1);

			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_AddressOverride = true;
			docAddress2.E2_CompanyName = "Invalid Company 2";
			docAddress2.E2_AddressType = "MAN";
			Line2.DocAddresses.Add(docAddress2);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;
			((ModuleTextFilter)filter["Manufacturer Company Name"]).Property = "Valid Company 1";
			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = true;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer Name should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer Name should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region TestGoodsAvailableAtFilter

		public void TestGoodsAvailableAtFilter()
		{
			#region Setup

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "Invalid Company 1";
			var orgAddress2 = orgHeader2.Addresses[0];
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.GoodsAvailableAt;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			Line1.DocAddresses.Add(docAddress2);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;

			((ModuleTextFilter)filter["GoodsAvailableAt Company Name"]).IsActive = false;
			((ModuleGuidFilter)filter["GoodsAvailableAt"]).Property = orgHeader2.PK;
			((ModuleGuidFilter)filter["GoodsAvailableAt"]).IsActive = true;

			var filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Goods Available At should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Goods Available At should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			filter = FilterStripBizO;
			((ModuleTextFilter)filter["GoodsAvailableAt Company Name"]).IsActive = true;
			((ModuleTextFilter)filter["GoodsAvailableAt Company Name"]).Property = "Invalid";
			((ModuleGuidFilter)filter["GoodsAvailableAt"]).IsActive = false;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Goods Available At should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Goods Available At should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region TestGoodsDeliveredToFilter

		public void TestGoodsDeliveredToFilter()
		{
			#region Setup

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "Invalid Company 1";
			var orgAddress2 = orgHeader2.Addresses[0];
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.GoodsDeliveredTo;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			Line1.DocAddresses.Add(docAddress2);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;

			((ModuleTextFilter)filter["GoodsDeliveredTo Company Name"]).IsActive = false;
			((ModuleGuidFilter)filter["GoodsDeliveredTo"]).Property = orgHeader2.PK;
			((ModuleGuidFilter)filter["GoodsDeliveredTo"]).IsActive = true;

			var filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Goods Delivered To should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Goods Delivered To should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			filter = FilterStripBizO;

			((ModuleTextFilter)filter["GoodsDeliveredTo Company Name"]).IsActive = true;
			((ModuleTextFilter)filter["GoodsDeliveredTo Company Name"]).Property = "Invalid";
			((ModuleGuidFilter)filter["GoodsDeliveredTo"]).IsActive = false;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Goods Delivered To should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Goods Delivered To should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region ModuleFilter Setup

		// text filter setup

		void SetupModuleTextFilter(ZString filterDescription, ZString? filterProperty = null)
		{
			ResetModuleFilters();

			ModuleTextBaseFilter filter = (ModuleTextBaseFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.Property = filterProperty ?? "123";
		}

		// date filter setup

		void SetupModuleDateFilter(ZString filterDescription)
		{
			ResetModuleFilters();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = DateTimeMatchingQuery;
			filter.Property2 = DateTimeMatchingQuery;
		}

		ZDateTime DateTimeMatchingQuery
		{
			get { return new ZDateTime(1979, 6, 5); }
		}

		ZDateTime DateTimeNotMatchingQuery
		{
			get { return new ZDateTime(1978, 2, 10); }
		}

		// NumberRange filter setup

		void SetupModuleNumberRangeFilter(ZString filterDescription)
		{
			ResetModuleFilters();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.Property1 = DecimalMatchingQuery;
			filter.Property2 = DecimalMatchingQuery;
		}

		ZDecimal DecimalMatchingQuery
		{
			get { return new ZDecimal(8888.88); }
		}

		ZDecimal DecimalNotMatchingQuery
		{
			get { return new ZDecimal(6666.66); }
		}

		// flags filter setup

		void SetupModuleFlagsFilter(ZString filterDescription)
		{
			ResetModuleFilters();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.Property0 = true;
		}

		// guid filter setup

		void SetupModuleGuidsFilter(ZString filterDescription, bool filterBy2ndOrg)
		{
			ResetModuleFilters();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.Property1 = (filterBy2ndOrg) ? ZGuid.Empty : Org1.PK;
			filter.Property2 = (filterBy2ndOrg) ? Org1.PK : ZGuid.Empty;
		}

		// location filter setup

		void SetupModuleLocationFilter(ZString filterDescription, bool filterBy2ndOrg)
		{
			ResetModuleFilters();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStripBizO[filterDescription];
			filter.IsActive = true;
			filter.Property1 = (filterBy2ndOrg) ? ZString.Empty : Unloco1.Code;
			filter.Property2 = (filterBy2ndOrg) ? Unloco1.Code : ZString.Empty;
		}

		// reset

		void ResetModuleFilters()
		{
			foreach (ModuleFilter moduleFilter in FilterStripBizO)
			{
				moduleFilter.IsActive = false;
			}
		}

		#endregion

		#region ModuleFilter Assertions

		// filter by orderline		

		void AssertOrderLineTextOrNumberFilter(ZString filterDescription, SchemaStringColumn orderLineFilterColumn)
		{
			SetupModuleTextFilter(filterDescription);

			Line1[orderLineFilterColumn.Name] = "123";
			Line2[orderLineFilterColumn.Name] = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		protected void AssertOrderLineDateFilter(ZString filterDescription, SchemaDateTimeColumn orderLineFilterColumn)
		{
			SetupModuleDateFilter(filterDescription);

			Line1[orderLineFilterColumn.Name] = DateTimeMatchingQuery;
			Line2[orderLineFilterColumn.Name] = DateTimeNotMatchingQuery;

			Factory.Save();
			AssertSearchResults();
		}

		void AssertOrderLineDecimalFilter(ZString filterDescription, SchemaDecimalColumn orderLineFilterColumn)
		{
			SetupModuleNumberRangeFilter(filterDescription);

			Line1[orderLineFilterColumn.Name] = DecimalMatchingQuery;
			Line2[orderLineFilterColumn.Name] = DecimalNotMatchingQuery;
			Factory.Save();

			AssertSearchResults();
		}

		void AssertOrderLineFlagsFilter(ZString filterDescription, SchemaBoolColumn orderLineFilterColumn)
		{
			SetupModuleFlagsFilter(filterDescription);

			Line1[orderLineFilterColumn.Name] = true;
			Line2[orderLineFilterColumn.Name] = false;
			Factory.Save();

			AssertSearchResults();
		}

		// filter by order header

		void AssertOrderTextOrNumberFilter(ZString filterDescription, SchemaStringColumn orderFilterColumn)
		{
			SetupModuleTextFilter(filterDescription);

			Order1[orderFilterColumn.Name] = "123";
			Order2[orderFilterColumn.Name] = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		void AssertOrderTextOrNumberFilterWithExceededValue(ZString filterDescription, SchemaStringColumn orderFilterColumn)
		{
			if (orderFilterColumn.HasMaxLength)
			{
				ResetModuleFilters();
				ErrorReporter.Clear();

				ModuleTextBaseFilter filter = (ModuleTextBaseFilter)FilterStripBizO[filterDescription];
				filter.IsActive = true;
				filter.Property = new ZString('a', orderFilterColumn.MaxLength + 1);
				GetCollectionLoadedWithFilterApplied();

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
			else
			{
				Assert(true);
			}
		}

		protected void AssertOrderDateFilter(ZString filterDescription, SchemaDateTimeColumn orderFilterColumn)
		{
			SetupModuleDateFilter(filterDescription);

			Order1[orderFilterColumn.Name] = DateTimeMatchingQuery;
			Order2[orderFilterColumn.Name] = DateTimeNotMatchingQuery;
			Factory.Save();

			AssertSearchResults();
		}

		void AssertOrderDecimalFilter(ZString filterDescription, SchemaDecimalColumn orderFilterColumn)
		{
			SetupModuleNumberRangeFilter(filterDescription);

			Order1[orderFilterColumn.Name] = DecimalMatchingQuery;
			Order2[orderFilterColumn.Name] = DecimalNotMatchingQuery;

			Factory.Save();
			AssertSearchResults();
		}

		void AssertOrderFlagsFilter(ZString filterDescription, SchemaBoolColumn orderFilterColumn)
		{
			SetupModuleFlagsFilter(filterDescription);

			Order1[orderFilterColumn.Name] = true;
			Order2[orderFilterColumn.Name] = false;

			Factory.Save();
			AssertSearchResults();
		}

		void AssertOrderGuidsFilter(ZString filterDescription, SchemaGuidColumn orderFilterColumn1, SchemaGuidColumn orderFilterColumn2)
		{
			Order1[orderFilterColumn1.Name] = Org1.PK;
			Order1[orderFilterColumn2.Name] = Org1.PK;

			Order2[orderFilterColumn1.Name] = Org2.PK;
			Order2[orderFilterColumn2.Name] = Org2.PK;

			Factory.Save();

			SetupModuleGuidsFilter(filterDescription, false);
			AssertSearchResults();

			SetupModuleGuidsFilter(filterDescription, true);
			AssertSearchResults();
		}

		void AssertOrderAddressFilter(ZString filterDescription, SchemaGuidColumn orderFilterColumn1, SchemaGuidColumn orderFilterColumn2)
		{
			Order1[orderFilterColumn1.Name] = Org1.MainAddress.PK;
			Order1[orderFilterColumn2.Name] = Org1.MainAddress.PK;

			Order2[orderFilterColumn1.Name] = Org2.MainAddress.PK;
			Order2[orderFilterColumn2.Name] = Org2.MainAddress.PK;

			Factory.Save();

			SetupModuleGuidsFilter(filterDescription, false);
			AssertSearchResults();

			SetupModuleGuidsFilter(filterDescription, true);
			AssertSearchResults();
		}

		void AssertOrderLocationFilter(ZString filterDescription, SchemaStringColumn orderNk1FilterColumn, SchemaStringColumn orderNk2FilterColumn)
		{
			Order1[orderNk1FilterColumn.Name] = Unloco1.Code;
			Order1[orderNk2FilterColumn.Name] = Unloco1.Code;

			Order2[orderNk1FilterColumn.Name] = Unloco2.Code;
			Order2[orderNk2FilterColumn.Name] = Unloco2.Code;

			Factory.Save();

			SetupModuleLocationFilter(filterDescription, false);
			AssertSearchResults();

			SetupModuleLocationFilter(filterDescription, true);
			AssertSearchResults();
		}

		// filter by shipment

		void AssertShipmentTextOrNumberFilter(ZString filterDescription, SchemaStringColumn shipmentFilterColumn)
		{
			SetupModuleTextFilter(filterDescription);

			Shipment1[shipmentFilterColumn] = "123";
			Shipment2[shipmentFilterColumn] = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		void AssertShipmentNoFilter()
		{
			ResetModuleFilters();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Shipment #"];
			filter.IsActive = true;
			filter.Property = "123456789"; // special case - JS_UniqueConsignRef is padded when saving so 123 won't work

			Shipment1.JS_UniqueConsignRef = "123456789";
			Shipment2.JS_UniqueConsignRef = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		// filter by consol

		void AssertConsolTextOrNumberFilter(ZString filterDescription, SchemaStringColumn consolFilterColumn)
		{
			SetupModuleTextFilter(filterDescription);

			Consol1[consolFilterColumn] = "123";
			Consol2[consolFilterColumn] = "ABC";
			Factory.Save();

			AssertSearchResults();
		}

		// actual assertions

		protected void AssertSearchResults()
		{
			IBusinessObjectCollection collection = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains(ExpectSearchResultsToContain, collection);
			AssertCollectionNotContains(ExpectSearchResultsToNotContain, collection);
		}

		protected IBusinessObjectCollection GetCollectionLoadedWithFilterApplied()
		{
			IBusinessObjectCollection result = GetNewCollection();
			BusinessObjectCollection legacyCollection = result as BusinessObjectCollection;
			IActiveBusinessObjectCollection activeCollection = result as IActiveBusinessObjectCollection;
			if (legacyCollection != null)
			{
				legacyCollection.Load(FilterStripBizO.Filter);
			}
			if (activeCollection != null)
			{
				activeCollection.AdditionalFilter = FilterStripBizO.Filter;
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter("JobContainer", "Container #"));
			result.Add(TableFilter("JobContainerPackPivot", "Container #"));
			result.Add(TableFilter("JobOrderContainer", "Container #"));
			result.Add(TableFilter("JobOrderLineDeliverContainer", "Container #"));
			result.Add(TableFilter("JobOrderLineDelivery", "Container #"));
			result.Add(TableFilter("JobPackLines", "Container #"));

			result.Add(TableFilter("JobConShipLink", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobConsol", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobConsolTransport", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobDeclaration", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobSailing", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobShipment", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobVoyage", "Actual Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobVoyDestination", "Actual Flight/Voyage # and Vessel"));

			result.Add(TableFilter("JobOrderHeader", "House Bill"));
			result.Add(TableFilter("JobShipment", "House Bill"));
			result.Add(TableFilter("JobConShipLink", "Master Bill"));
			result.Add(TableFilter("JobConsol", "Master Bill"));
			result.Add(TableFilter("JobShipment", "Master Bill"));

			result.Add(TableFilter("JobShipment", "Shipment #"));

			result.Add(TableFilter("OrgHeader", "Supplier Company Name"));

			result.Add(TableFilter("JobConShipLink", "Actual Send / Receive Agents"));
			result.Add(TableFilter("JobConsol", "Actual Send / Receive Agents"));
			result.Add(TableFilter("JobDeclaration", "Actual Send / Receive Agents"));
			result.Add(TableFilter("JobShipment", "Actual Send / Receive Agents"));
			result.Add(TableFilter("OrgAddress", "Actual Send / Receive Agents"));

			result.Add(TableFilter("JobConShipLink", "Actual Carrier"));
			result.Add(TableFilter("JobConsol", "Actual Carrier"));
			result.Add(TableFilter("JobConsolTransport", "Actual Carrier"));
			result.Add(TableFilter("JobDeclaration", "Actual Carrier"));
			result.Add(TableFilter("JobSailing", "Actual Carrier"));
			result.Add(TableFilter("JobShipment", "Actual Carrier"));
			result.Add(TableFilter("JobVoyage", "Actual Carrier"));
			result.Add(TableFilter("JobVoyOrigin", "Actual Carrier"));
			result.Add(TableFilter("OrgAddress", "Actual Carrier"));

			result.Add(TableFilter("JobOrderHeader", "Client Assigned Staff"));
			result.Add(TableFilter("OrgStaffAssignments", "Client Assigned Staff"));

			result.Add(TableFilter("JobConShipLink", "Actual Load / Discharge"));
			result.Add(TableFilter("JobConsol", "Actual Load / Discharge"));
			result.Add(TableFilter("JobConsolTransport", "Actual Load / Discharge"));
			result.Add(TableFilter("JobDeclaration", "Actual Load / Discharge"));
			result.Add(TableFilter("JobSailing", "Actual Load / Discharge"));
			result.Add(TableFilter("JobShipment", "Actual Load / Discharge"));
			result.Add(TableFilter("JobVoyOrigin", "Actual Load / Discharge"));
			result.Add(TableFilter("JobDeclaration", "Actual Origin / Destination"));
			result.Add(TableFilter("JobShipment", "Actual Origin / Destination"));

			result.Add(TableFilter("ProcessTasks", "Tasks"));
			result.Add(TableFilter("ProcessTasks", "Exceptions"));
			result.Add(TableFilter("ProcessTasks", "Milestones"));
			result.Add(TableFilter("ProcessTasks", "Triggers"));

			return result;
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			Consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			Shipment1 = Consol1.Shipments.AddNew();
			Shipment2 = Consol2.Shipments.AddNew();

			Order1 = Factory.NewWithValidTestData<Order>();
			Order2 = Factory.NewWithValidTestData<Order>();
			Order1.JD_JS = Shipment1.PK;
			Order2.JD_JS = Shipment2.PK;

			Line1 = Order1.OrderLines.AddNew();
			Line2 = Order2.OrderLines.AddNew();

			LineDelivery1 = Factory.New<OrderLineDelivery>();
			LineDelivery2 = Factory.New<OrderLineDelivery>();
			LineDelivery1.J4_JO = Line1.PK;
			LineDelivery2.J4_JO = Line2.PK;

			LineDeliveryContainer1 = Factory.New<OrderLineDeliverContainer>();
			LineDeliveryContainer2 = Factory.New<OrderLineDeliverContainer>();
			LineDeliveryContainer1.J5_J4 = LineDelivery1.PK;
			LineDeliveryContainer2.J5_J4 = LineDelivery2.PK;

			OrderContainer1 = Order1.PlannedContainers.AddNew();
			OrderContainer2 = Order2.PlannedContainers.AddNew();

			JobContainerPackPivot1 = Factory.New<JobContainerPackPivot>();
			JobContainerPackPivot2 = Factory.New<JobContainerPackPivot>();
			JobContainer1 = Factory.NewWithValidTestData<CommonContainer>();
			JobContainer2 = Factory.NewWithValidTestData<CommonContainer>();
			JobContainerPackPivot1.J6_JL = Shipment1.OuterPackLines.AddNew().PK;
			JobContainerPackPivot2.J6_JL = Shipment2.OuterPackLines.AddNew().PK;
			JobContainerPackPivot1.J6_JC = JobContainer1.PK;
			JobContainerPackPivot2.J6_JC = JobContainer2.PK;

			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org3.OH_IsActive = false;
			Org4 = Factory.NewWithValidTestData<OrgHeader>();
			Org4.OH_IsActive = false;
			Unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			Unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			Declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

			Factory.Save();
		}

		protected OrdersBaseFilterBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = (OrdersBaseFilterBusinessObject)GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		protected Order Order1;
		protected Order Order2;
		protected OrderLine Line1;
		protected OrderLine Line2;
		ForwardingConsol Consol1;
		ForwardingConsol Consol2;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		OrderLineDelivery LineDelivery1;
		OrderLineDelivery LineDelivery2;
		OrderLineDeliverContainer LineDeliveryContainer1;
		OrderLineDeliverContainer LineDeliveryContainer2;
		OrdersBaseFilterBusinessObject fFilterStripBizO;
		OrderContainer OrderContainer1;
		OrderContainer OrderContainer2;
		JobContainerPackPivot JobContainerPackPivot1;
		JobContainerPackPivot JobContainerPackPivot2;
		CommonContainer JobContainer1;
		CommonContainer JobContainer2;

		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;
		RefUNLOCO Unloco1;
		RefUNLOCO Unloco2;

		BusinessObject Declaration;

		#endregion

		protected sealed override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return GetNewOrdersBaseFilterBusinessObject();
		}

		protected OrgHeader NewCarrierOrgHeader(string carrierCode)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_Code = carrierCode;
			return carrier;
		}

		protected abstract OrdersBaseFilterBusinessObject GetNewOrdersBaseFilterBusinessObject();
		protected abstract IActiveBusinessObjectCollection GetNewCollection();
		protected abstract BusinessObject ExpectSearchResultsToContain { get; }
		protected abstract BusinessObject ExpectSearchResultsToNotContain { get; }

		#endregion
	}
}
