using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WorkflowSelectionOrgTypeCodes = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(Order))]
	public class OrderBOTest : BusinessObjectWithCustomLabelsTestCase
	{
		public void TestUniversalCopyAssociateElementAttribute()
		{
			var order = Factory.New<Order>();
			var type = order.GetType();

			var attributes = type
							.GetCustomAttributes<UniversalCopyAssociateElementAttribute>(true);

			var attribute1 = attributes.Cast<UniversalCopyAssociateElementAttribute>().First(x => x.DefinitionElement == "CustomsCharges");
			AssertNotNull(attribute1);

			AssertEquals("Charges", attribute1.ComponentElement);

			var attribute2 = attributes.Cast<UniversalCopyAssociateElementAttribute>().First(x => x.DefinitionElement == "ShipmentPrePlanning");
			AssertNotNull(attribute2);
			AssertEquals("PreAdvice", attribute2.ComponentElement);

			var chargePropertyInfo = type.GetProperty("Charges");
			AssertNotNull(chargePropertyInfo);
			AssertEquals(typeof(JobComInvChargeCollection<JobComInvCharge>), chargePropertyInfo.PropertyType);

			var preAdvicePropertyInfo = type.GetProperty("PreAdvice");
			AssertNotNull(preAdvicePropertyInfo);
			AssertEquals(typeof(JobShipmentPreplanning), preAdvicePropertyInfo.PropertyType);
		}

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.Order);
			}
		}

		#endregion

		public void TestCustomTrackingDates()
		{
			Order bizO = (Order)GetNewBusinessObject();
			bizO.FillWithValidTestData();

			OrgHeader testOrg = bizO.Buyer;

			OrgCustomLabels customLabels1 = testOrg.CustomLabels.AddNew();
			customLabels1.OT_FieldName = "OrderHeader.UserTrackDate1";
			customLabels1.OT_Caption = "Test date 1";

			OrgCustomLabels customLabels2 = testOrg.CustomLabels.AddNew();
			customLabels2.OT_FieldName = "OrderHeader.UserTrackDate2";
			customLabels2.OT_Caption = "Test date 2";

			OrgCustomLabels customLabels3 = testOrg.CustomLabels.AddNew();
			customLabels3.OT_FieldName = "OrderHeader.UserTrackDate3";
			customLabels3.OT_Caption = "Test date 3";

			OrgCustomLabels customLabels4 = testOrg.CustomLabels.AddNew();
			customLabels4.OT_FieldName = "OrderHeader.UserTrackDate4";
			customLabels4.OT_Caption = "Test date 4";

			AssertEquals("There must be 4 custom labels (created for testing)", 4, testOrg.CustomLabels.Count);

			ICustomLabelsProvider customLabelsProvider = GetNewCustomLabelsProvider(bizO);
			CustomLabelInfoList orderCustomLabels = customLabelsProvider.GetCustomFields(bizO.Buyer, Factory);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(Order.Schema.JD_EstimateUserDate1));
			CustomLabelInfo orderCustomLabel1 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(Order.Schema.JD_EstimateUserDate1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Estimated " + customLabels1.OT_Caption, orderCustomLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels1.OT_FieldName, orderCustomLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(Order.Schema.JD_ActualUserDate1));
			CustomLabelInfo orderCustomLabel2 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(Order.Schema.JD_ActualUserDate1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Actual " + customLabels1.OT_Caption, orderCustomLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels1.OT_FieldName, orderCustomLabel2.LabelName);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(Order.Schema.JD_EstimateUserDate3));
			CustomLabelInfo orderCustomLabel3 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(Order.Schema.JD_EstimateUserDate3);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Estimated " + customLabels3.OT_Caption, orderCustomLabel3.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels3.OT_FieldName, orderCustomLabel3.LabelName);

			Assert("All custom labels created must be retrieved.", orderCustomLabels.Contains(Order.Schema.JD_ActualUserDate3));
			CustomLabelInfo orderCustomLabel4 = (CustomLabelInfo)orderCustomLabels.GetFieldByPropertyName(Order.Schema.JD_ActualUserDate3);
			AssertEquals("Custom label properties retrieved must be the same as those set.", "Actual " + customLabels3.OT_Caption, orderCustomLabel4.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", customLabels3.OT_FieldName, orderCustomLabel4.LabelName);
		}

		public void TestUnsavedOrder_DisplaysCorrectCustomFields()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.SetRelatedParty(controllingCustomer.PK, "CCB", "PAD");

			var buyerCustomLabel = buyer.CustomLabels.AddNew();
			buyerCustomLabel.OT_FieldName = "OrderHeader.CustomFlag1";
			buyerCustomLabel.OT_Caption = "Buyer flag caption";

			var controllingCustomerCustomLabel = controllingCustomer.CustomLabels.AddNew();
			controllingCustomerCustomLabel.OT_FieldName = "OrderHeader.CustomFlag1";
			controllingCustomerCustomLabel.OT_Caption = "Controlling Customer flag caption";

			Factory.Save();

			Order order = Factory.New<Order>();

			var customLabelsProvider = order as ICustomLabelsConfigOrgProvider;
			int configOrgChangedTriggerCount = 0;

			customLabelsProvider.ConfigOrgChanged += (sender, e) =>
			{
				configOrgChangedTriggerCount++;
			};

			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			AssertEquals(controllingCustomer.PK, order.ControllingCustomerDocAddress.OrganisationPK);
			AssertEquals("ConfigOrgChanged should have been triggered once when the buyer was set, then once more when the controlling customer related to the buyer was set by default.", 2, configOrgChangedTriggerCount);
		}

		public void TestNoOverflowWhenCheckingHasOutstandingBalance()
		{
			var order = Factory.New<Order>();
			var line1 = order.OrderLines.AddNew();
			line1.JO_Quantity = 2000010000;
			line1.JO_QtyReceived = 2000000000;

			var line2 = order.OrderLines.AddNew();
			line2.JO_Quantity = 2000010000;
			line2.JO_QtyReceived = 2000000000;

			AssertNoExceptionThrown(() => { _ = order.HasOutstandingBalance; });
		}

		public void TestJD_OrderNumberCanBeSetWhenBlankOrderSplitExits()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Precondition, buyer has an address", buyer.MainAddress.PK.IsValid);

			var blankOrder = Factory.New<Order>();
			blankOrder.JD_OrderNumber = ZString.Empty;
			blankOrder.JD_OrderNumberSplit = new ZByte(1);
			blankOrder.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var newOrder = Factory.New<Order>();
			newOrder.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			AssertEquals("Precondition: newOrder.JD_OrderNumber", ZString.Empty, newOrder.JD_OrderNumber);
			AssertEquals("Precondition: newOrder.JD_OrderNumberSplit", new ZByte(0), newOrder.JD_OrderNumberSplit);

			newOrder.JD_OrderNumber = "P0001";
			AssertEquals("JD_OrderNumber was successfully set", "P0001", newOrder.JD_OrderNumber);
		}

		#region TestCustomLabelsMandatoryValidationForWeb

		public void TestDisableCustomLabelsMandatoryValidationForWeb()
		{
			Order bizO = (Order)GetNewBusinessObject();
			bizO.FillWithValidTestData();
			ICustomLabelsProvider customLabelsProvider = GetNewCustomLabelsProvider(bizO);

			AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider for this test to work", customLabelsProvider.ConfigOrgProvider);
			AssertNotNull("You must have a CustomLabelsProvider.ConfigOrgProvider.ConfigOrg for this test to work", customLabelsProvider.ConfigOrgProvider.ConfigOrg);
			OrgHeader configOrg = customLabelsProvider.ConfigOrgProvider.ConfigOrg;

			bool initialValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				string[] fieldsThatCanBeMandatory = new string[] { Order.Schema.JD_RL_NKGoodsAvailableAt, Order.Schema.JD_RL_NKGoodsDeliveredTo };

				CustomLabelInfoList customLabels = customLabelsProvider.GetCustomFields(configOrg, Factory);
				foreach (CustomLabelInfoBase customLabelBase in customLabels)
				{
					CustomLabelInfo customLabel = customLabelBase as CustomLabelInfo;

					if (customLabel != null && !((IList)fieldsThatCanBeMandatory).Contains(customLabel.PropertyName))
					{
						OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
						newLabel.OT_FieldName = customLabel.LabelName;
						newLabel.OT_IsMandatory = true;

						AssertPropertyValidation(bizO, customLabelBase.PropertyName);

						newLabel.Delete();
					}
				}
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		void AssertPropertyValidation(Order bizO, string propertyName)
		{
			bizO.Validation.GetType().GetMethod("Validate" + propertyName).Invoke(bizO.Validation, null);

			bool mandatoryValidationEnabled = false;
			foreach (INotification error in bizO.ZPropertyInfoHash[propertyName].GetErrors())
			{
				if (error.Message.ToLower().StartsWith("please enter"))
				{
					mandatoryValidationEnabled = true;
					break;
				}
			}

			AssertEquals(propertyName + " expected not to be mandatory for web applications", false, mandatoryValidationEnabled);
		}

		#endregion

		[TestedType(typeof(Order))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestILandedCostHeader_SupportsNoCostApportionmentItem()
		{
			var order = Factory.New<Order>();
			AssertEquals(false, ((ILandedCostHeader)order).SupportsNoCostApportionmentItem);
		}

		#region UniqueIndexToHandleTest

		public void TestUniqueIndexToHandle()
		{
			var order = Factory.New<OrderForTest>();
			AssertEquals(JobOrderHeaderSchema.Constants.Indexes.NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress, order.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single());
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			CreateOrder(orgAddress.PK);
			Factory.Save();

			CreateOrder(orgAddress.PK);

			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				var message = $"The value of Order No + Split Number + JobOrderHeader|JD_OA_BuyerAddress must be unique on Display Order. The duplicate value(s) are: (111, 1, {orgAddress.PK}).";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowSameOrderNumberAndOrderNumberSplitAndBuyerAddressWhenCancelled()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			CreateOrder(orgAddress.PK, true);
			Factory.Save();

			CreateOrder(orgAddress.PK, true);

			AssertNoExceptionThrown(() => Factory.Save());
		}

		void CreateOrder(ZGuid orgAddressPK, bool isCancelled = false)
		{
			var order = Factory.New<OrderForTest>();
			order.JD_OrderNumber = "111";
			order.JD_OrderNumberSplit = 1;
			order.JD_OA_BuyerAddress = orgAddressPK;
			order.JD_IsCancelled = isCancelled;
		}

		class OrderForTest : Order
		{
			public OrderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers => base.UniqueIndexFailureHandlers;
		}

		#endregion

		#region TemplateSelectionCriteria Client

		public void TestTemplateSelectionCriteria_Client()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;

			var ranker = (order as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			var valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(4, valueList.Length);
			AssertEquals(controllingCustomer.PK, valueList[0]);
			AssertEquals(buyer.PK, valueList[1]);
			AssertEquals(supplier.PK, valueList[2]);
			AssertEquals(ZGuid.Empty, valueList[3]);

			order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "NZAKL";
			ranker = (order as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(4, valueList.Length);
			AssertEquals(controllingCustomer.PK, valueList[0]);
			AssertEquals(supplier.PK, valueList[1]);
			AssertEquals(buyer.PK, valueList[2]);
			AssertEquals(ZGuid.Empty, valueList[3]);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertNotNull(clientInTemplateSelectionCriteria);
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode(WorkflowSelectionOrgTypeCodes.ControllingCustomer));
			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);
			Factory.Save();

			ranker = (order as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			valueList = ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertEquals(3, valueList.Length);
			AssertEquals(supplier.PK, valueList[0]);
			AssertEquals(buyer.PK, valueList[1]);
			AssertEquals(ZGuid.Empty, valueList[2]);
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public void TestICustomLabelsConfigOrgProvider_ConfigOrg()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.BuyerPK = buyer.PK;
			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;

			var org = (order as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Controlling customer will be matched.", controllingCustomer, org);

			order.ControllingCustomerDocAddress.E2_AddressOverride = true;
			order.ControllingCustomerDocAddress.E2_CompanyName = "XYZ IMPORT CO";
			order.ControllingCustomerDocAddress.E2_Address1 = "33 Pitt Street";

			org = (order as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Buyer will be matched if controlling customer becomes overridden.", buyer, org);

			var clientInTemplateSelectionCriteriaCollection = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertNotNull(clientInTemplateSelectionCriteria);
			clientInTemplateSelectionCriteria.SelectedItems.Remove(clientInTemplateSelectionCriteria.SelectedItems.GetValueByCode(WorkflowSelectionOrgTypeCodes.ControllingCustomer));
			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);
			Factory.Save();

			org = (order as ICustomLabelsConfigOrgProvider).ConfigOrg;
			AssertEquals("Buyer will be matched after controlling customer is removed.", buyer, org);
		}

		public void TestICustomLabelsConfigOrgProvider_ConfigOrgChanged()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerConfigOrgChangedCalled = false;

			var order1 = Factory.New<Order>();
			((ICustomLabelsConfigOrgProvider)order1).ConfigOrgChanged += (s, e) => buyerConfigOrgChangedCalled = true;
			order1.BuyerPK = buyer.PK;
			AssertEquals("Buyer ConfigOrgChanged called", true, buyerConfigOrgChangedCalled);
			AssertEquals(buyer, ((ICustomLabelsConfigOrgProvider)order1).ConfigOrg);

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomerConfigOrgChangedCalled = false;

			var order2 = Factory.New<Order>();
			((ICustomLabelsConfigOrgProvider)order2).ConfigOrgChanged += (s, e) => controllingCustomerConfigOrgChangedCalled = true;
			order2.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;
			AssertEquals("Controlling Customer ConfigOrgChanged called", true, controllingCustomerConfigOrgChangedCalled);
			AssertEquals(controllingCustomer, ((ICustomLabelsConfigOrgProvider)order2).ConfigOrg);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader buyer = CreateNewOrg(Factory, "TBY");
			Order result = Factory.New<Order>();
			result.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			return result;
		}

		protected OrgHeader CreateNewOrg(BusinessObjectFactory factory, ZString code)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_FullName = "some full name";
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "some place";
			return result;
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			return new Order.CustomLabelsProvider((Order)bO, false);
		}

		#endregion
	}
}
