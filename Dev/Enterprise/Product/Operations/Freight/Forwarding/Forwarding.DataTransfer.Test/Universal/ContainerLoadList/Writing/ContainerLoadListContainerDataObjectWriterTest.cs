using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ContainerLoadListContainerDataObjectWriterTest : OrganizationAddressTestHelper
	{
		ForwardingConsol consol;
		ForwardingContainer container;
		ContainerLoadListContainerLinkManager containerLinkManager;
		DataWritingManager writeManager;
		ContainerLoadListContainerDataObjectWriter writer;

		public void SetupTestData()
		{
			consol = ContainerLoadListDataObjectHelper.CreateConsol(Factory);
			container = ContainerLoadListDataObjectHelper.CreateContainer(Factory, consol, "INCZ3748651");
			containerLinkManager = new ContainerLoadListContainerLinkManager();
			writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol));
			writer = new ContainerLoadListContainerDataObjectWriter(writeManager, containerLinkManager);
		}

		public void TestWrite()
		{
			SetupTestData();

			var containerDataObject = writer.GetDataObject(container);
			AssertEquals(1, containerDataObject.Link);
			AssertEquals(1, containerDataObject.ContainerCount);
			AssertEquals("INCZ3748651", containerDataObject.ContainerNumber);
			AssertEquals("20GP", containerDataObject.ContainerType.Code);
			AssertEquals("CY/CY", containerDataObject.DeliveryMode);
			AssertEquals("LCL", containerDataObject.FCL_LCL_AIR.Code);
		}

		public void TestWrite_Successful_WithOneValidSeal()
		{
			SetupTestData();
			(container.JC_SealNum, container.JC_SealParty) = ("TESTSEAL1", ContainerSealParties.Codes.CarrierShippingLine);

			var containerDataObject = writer.GetDataObject(container);

			AssertEquals(container.JC_SealParty, containerDataObject.SealPartyType.Code);
			AssertEquals(container.JC_SealNum, containerDataObject.Seal);
			AssertNull(containerDataObject.SecondSeal);
			AssertNull(containerDataObject.SecondSealPartyType);
			AssertNull(containerDataObject.ThirdSeal);
			AssertNull(containerDataObject.ThirdSealPartyType);
		}

		public void TestWrite_Successful_WithTwoValidSeals()
		{
			SetupTestData();
			(container.JC_SealNum, container.JC_SealParty) = ("TESTSEAL1", ContainerSealParties.Codes.CarrierShippingLine);
			(container.JC_AdditionalSealNum, container.JC_AdditionalSealParty) = ("TESTSEAL2", ContainerSealParties.Codes.Customs);

			var containerDataObject = writer.GetDataObject(container);
			AssertEquals(container.JC_SealParty, containerDataObject.SealPartyType.Code);
			AssertEquals(container.JC_SealNum, containerDataObject.Seal);
			AssertEquals(container.JC_AdditionalSealParty, containerDataObject.SecondSealPartyType.Code);
			AssertEquals(container.JC_AdditionalSealNum, containerDataObject.SecondSeal);
			AssertNull(containerDataObject.ThirdSeal);
			AssertNull(containerDataObject.ThirdSealPartyType);
		}

		public void TestWrite_Successful_WithThreeValidSeals()
		{
			SetupTestData();
			(container.JC_SealNum, container.JC_SealParty) = ("TESTSEAL1", ContainerSealParties.Codes.CarrierShippingLine);
			(container.JC_AdditionalSealNum, container.JC_AdditionalSealParty) = ("TESTSEAL2", ContainerSealParties.Codes.Customs);
			(container.JC_Additional2SealNum, container.JC_Additional2SealParty) = ("TESTSEAL3", ContainerSealParties.Codes.Quarantine);

			var containerDataObject = writer.GetDataObject(container);
			AssertEquals(container.JC_SealParty, containerDataObject.SealPartyType.Code);
			AssertEquals(container.JC_SealNum, containerDataObject.Seal);
			AssertEquals(container.JC_AdditionalSealNum, containerDataObject.SecondSeal);
			AssertEquals(container.JC_AdditionalSealParty, containerDataObject.SecondSealPartyType.Code);
			AssertEquals(container.JC_Additional2SealNum, containerDataObject.ThirdSeal);
			AssertEquals(container.JC_Additional2SealParty, containerDataObject.ThirdSealPartyType.Code);
		}
	}
}
