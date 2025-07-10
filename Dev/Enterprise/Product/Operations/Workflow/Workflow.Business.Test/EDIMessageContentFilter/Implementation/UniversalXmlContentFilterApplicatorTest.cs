using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Workflow.Business.Test
{
	class UniversalXmlContentFilterApplicatorTest : TestCaseWithFactory
	{
		public void TestGetShouldUniversalShipmentExcludeCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var eDIMessageContentFilter = Factory.NewWithValidTestData<EDIMessageContentFilter>();
			var filterLine = eDIMessageContentFilter.UniversalShipment.Lines.AddNew();
			filterLine.SchemaElement = "SubShipmentCollection";
			filterLine.DataContext = "HVLVConsignment";

			var eDIMessagePurpose = Factory.NewWithValidTestData<EDIMessagePurpose>();
			eDIMessagePurpose.EMP_ECF_Filter = eDIMessageContentFilter.PK;

			Factory.Save();

			var action = new ActionInfo(null, shipment);
			action.PurposeCode = eDIMessagePurpose.EMP_Code;
			var universalShipment = new UniversalShipment();

			var shouldSkipCollection = new UniversalXmlContentFilterApplicator().GetShouldUniversalShipmentExcludeCollection(universalShipment, action, "HVLVConsignment");

			Assert(shouldSkipCollection);
		}

		public void TestGetShouldExcludeEmptyElements()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var eDIMessageContentFilter = Factory.NewWithValidTestData<EDIMessageContentFilter>();
			eDIMessageContentFilter.Config.ExcludeEmptyElements = true;

			var eDIMessagePurpose = Factory.NewWithValidTestData<EDIMessagePurpose>();
			eDIMessagePurpose.EMP_ECF_Filter = eDIMessageContentFilter.PK;

			Factory.Save();

			var action = new ActionInfo(null, shipment);
			action.PurposeCode = eDIMessagePurpose.EMP_Code;

			var shouldExcludeEmptyElements = new UniversalXmlContentFilterApplicator().ShouldExcludeEmptyElements(action);

			Assert(shouldExcludeEmptyElements);
		}
	}
}
