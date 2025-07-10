using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingModuleOrgAgentRelationshipCollection))]
	sealed class ForwardingModuleOrgAgentRelationshipCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new ForwardingModuleOrgAgentRelationshipCollection(Factory);

		public void TestDefaultFilter()
		{
			var collection = GetCollectionToTest();
			AssertEquals((ZString)GlbCompany.CurrentCompany.GC_OH_OrgProxy.ToString(), collection.FilterBusinessObjectDefaults["Agency Office:Property"].Value);
			AssertEquals((ZString)OrgAgentRelationship.ProfitShareTypes.AgencyProfile, collection.FilterBusinessObjectDefaults["Profit Share Type:Property"].Value);
			AssertEquals((ZString)JobInvoicingConsumerTypes.GatewayConsolCode, collection.FilterBusinessObjectDefaults["Job Type:Property"].Value);
			AssertEquals(new ZString("is not blank"), collection.FilterBusinessObjectDefaults["Gateway Profit Apportionment Method:ComparisonOperator"].Value);
			AssertEquals(new ZString("Date Range"), collection.FilterBusinessObjectDefaults["Start Date:PropertySearch"].Value);
			AssertEquals(new ZString("Date Range"), collection.FilterBusinessObjectDefaults["End Date:PropertySearch"].Value);
			AssertEquals(ZString.Empty, collection.FilterBusinessObjectDefaults["Sending Location:Property"].Value);
			AssertEquals(ZString.Empty, collection.FilterBusinessObjectDefaults["Receiving Location:Property"].Value);
			AssertEquals(ZString.Empty, collection.FilterBusinessObjectDefaults["Freight Mode:Property"].Value);
		}

		public void TestSetDefaultForNewChild()
		{
			var newChild = (OrgAgentRelationship)Collection.AddNew();
			AssertEquals("AGY", newChild.O3_ProfitShareType);
		}

		public void TestGetExtraNotification()
		{
			var profitShareAgreement = (OrgAgentRelationship)Collection.AddNew();
			profitShareAgreement.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
			var expectedError = "Only Agency Profit Share Type can be selected.";
			AssertError(expectedError);

			profitShareAgreement.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			profitShareAgreement.O3_OH_SendingAgent = ZGuid.Empty;
			expectedError = "The Agency Office of Profit Share Agreement must be an Organization Proxy of the Current Company.";
			AssertError(expectedError);

			profitShareAgreement.O3_OH_SendingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertError(expectedError);

			profitShareAgreement.O3_OH_SendingAgent = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			expectedError = "Profit Share Setup (Rules) must have Job Type GCN and non-blank GW Profit Apportionment Method.";
			AssertError(expectedError);

			var details = profitShareAgreement.ProfitShareDetails.AddNew();
			details.O4_JobType = JobTypesList.Codes.Blank;
			AssertError(expectedError);

			details.O4_JobType = JobTypesList.Codes.SHP;
			AssertError(expectedError);

			details.O4_JobType = JobTypesList.Codes.GCN;
			AssertError(expectedError);

			details.O4_GatewayProfitApportionmentMethod = ZString.Empty;
			AssertError(expectedError);

			details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.SHP;
			AssertError(ZString.Empty);

			details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GWT;
			AssertError(ZString.Empty);

			details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.CHG;
			AssertError(ZString.Empty);

			details.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GVT;
			AssertError(ZString.Empty);

			details = profitShareAgreement.ProfitShareDetails.AddNew();
			details.O4_JobType = JobTypesList.Codes.SHP;
			AssertError(ZString.Empty);

			void AssertError(ZString errorMessage)
			{
				var notification = (Collection as IFilterModuleExtraNotificationProvider).GetExtraNotification(profitShareAgreement);
				if (!errorMessage.IsEmpty)
				{
					AssertEquals(true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
					AssertEquals("Expecting Error", errorMessage, notification.Message);
				}
				else
				{
					AssertNull(notification);
				}
			}
		}
	}
}
