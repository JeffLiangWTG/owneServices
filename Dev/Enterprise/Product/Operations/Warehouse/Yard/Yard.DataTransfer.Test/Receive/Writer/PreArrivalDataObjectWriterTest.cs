using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(PreArrivalDataObjectWriter))]
	public class PreArrivalDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();
		}

		public void TestWriteToDataObjectForContainerCollection()
		{
			#region Setup Test Data
			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";

			var receiveAdviceLine1 = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine1.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_YRL_ReceiveLine = receiveAdviceLine1.PK;
			yardUnitState1.YUS_UnitID = "CON0001";
			var unitLine1 = Factory.NewWithValidTestData<CYDUnitLineItem>();
			receiveAdviceLine1.YRL_YLI_UnitLineItem = unitLine1.PK;
			unitLine1.YLI_Quantity = 1;
			unitLine1.YLI_IsEmpty = true;
			unitLine1.YLI_RC_ContainerType = containerType.PK;
			unitLine1.YLI_SealNumber = "seal number1";
			unitLine1.YLI_Type = "CNT";

			var receiveAdviceLine2 = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine2.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			var unitLine2 = Factory.NewWithValidTestData<CYDUnitLineItem>();
			receiveAdviceLine2.YRL_YLI_UnitLineItem = unitLine2.PK;

			Factory.SaveForTesting();

			#endregion Setup Test Data

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);

			AssertEquals(nameof(DataContextType.CYDReceiveAdvice), dataObject.DataContext.DataSourceCollection.FirstOrDefault().Type);
			AssertEquals("PAI00000001", dataObject.DataContext.DataSourceCollection.FirstOrDefault().Key);
			AssertEquals("1 container should be created under Container Collection.",1 , dataObject.SubShipmentCollection.FirstOrDefault().RelatedShipmentCollection.FirstOrDefault().ContainerCollection.Count);
			var containerInfo1 = dataObject.SubShipmentCollection.FirstOrDefault().RelatedShipmentCollection.FirstOrDefault().ContainerCollection.FirstOrDefault();
			AssertEquals("Container for CON0001 should be in Container Collection.","CON0001" , containerInfo1.ContainerNumber);
			AssertEquals(1, containerInfo1.ContainerCount);
			AssertEquals("20G1", containerInfo1.ContainerType.Code);
			AssertEquals(true, containerInfo1.IsEmptyContainer);
			AssertNotNull("Container for receiveAdviceLine1 should have a seal number 1", containerInfo1.AdditionalSealNumberCollection.FirstOrDefault(a => a.Number.Equals("seal number1")));
		}

		public void TestWriteToDataObjectForOrganizationAddressCollection()
		{
			#region Setup Test Data

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "YRA00001000";
			receiveAdvice.YRA_WW_Yard = Data.Yard.PK;
			Factory.SaveForTesting();

			#endregion

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);
			AssertEquals("OrganizationAddressCollection should contain the org address for yard warehouse", 1, dataObject.OrganizationAddressCollection.Count);
			var organizationAddress = dataObject.OrganizationAddressCollection.FirstOrDefault();
			AssertEquals(nameof(DocAddressType.LocalCartageYard), organizationAddress.AddressType);
			AssertEquals("WUFSHIJNB", organizationAddress.OrganizationCode);
			AssertEquals("WUFU SHIPPING LINE", organizationAddress.CompanyName);
			AssertEquals("Level 2, Building G", organizationAddress.Address1);
			AssertEquals("34 Dock Lane", organizationAddress.Address2);
			AssertEquals("Johannesburg", organizationAddress.City);
			AssertEquals("SC1", organizationAddress.AddressShortCode);
			AssertEquals("ZA", organizationAddress.Country.Code);
			AssertEquals("South Africa", organizationAddress.Country.Name);
		}

		public void TestNonCNTTypeContainerShouldNotBeExported()
		{
			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";

			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;
			yardUnitState.YUS_UnitID = "CON0001";
			var unitLine = Factory.NewWithValidTestData<CYDUnitLineItem>();
			receiveAdviceLine.YRL_YLI_UnitLineItem = unitLine.PK;
			unitLine.YLI_Quantity = 1;
			unitLine.YLI_IsEmpty = true;
			unitLine.YLI_RC_ContainerType = containerType.PK;
			unitLine.YLI_SealNumber = "seal number1";
			unitLine.YLI_Type = "GEN";

			Factory.SaveForTesting();

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);

			AssertEquals(nameof(DataContextType.CYDReceiveAdvice), dataObject.DataContext.DataSourceCollection.FirstOrDefault().Type);
			AssertEquals("PAI00000001", dataObject.DataContext.DataSourceCollection.FirstOrDefault().Key);
			AssertEquals("no container in container collection", 0, dataObject.SubShipmentCollection.FirstOrDefault().RelatedShipmentCollection.FirstOrDefault().ContainerCollection.Count);
		}

		public void TestWriteToDataObjectWhenLinkedToDeliveries()
		{
			#region Setup Test Data

			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";

			var receiveAdviceLine1 = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine1.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_YRL_ReceiveLine = receiveAdviceLine1.PK;
			yardUnitState1.YUS_UnitID = "CON0001";
			var unitLine1 = Factory.NewWithValidTestData<CYDUnitLineItem>();
			receiveAdviceLine1.YRL_YLI_UnitLineItem = unitLine1.PK;
			unitLine1.YLI_Quantity = 1;
			unitLine1.YLI_IsEmpty = true;
			unitLine1.YLI_RC_ContainerType = containerType.PK;
			unitLine1.YLI_SealNumber = "seal number1";
			unitLine1.YLI_Type = "CNT";

			var receiveAdviceLine2 = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine2.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			var unitLine2 = Factory.NewWithValidTestData<CYDUnitLineItem>();
			receiveAdviceLine2.YRL_YLI_UnitLineItem = unitLine2.PK;

			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YRL_ReceiveAdviceLine = receiveAdviceLine2.PK;
			Factory.SaveForTesting();

			#endregion Setup Test Data

			var writer = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice)));
			AssertExceptionThrown<DataObjectValidationException>("The PRA is associated with one or more deliveries.", () => writer.GetDataObject(receiveAdvice));
		}

		public void TestWriteToDataObjectAcceptanceNumber()
		{
			#region Setup Test Data

			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";
			receiveAdvice.YRA_AcceptanceNumber = "ACC1";
			Factory.SaveForTesting();

			#endregion Setup Test Data

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);
			AssertEquals("ACC1", dataObject.BookingConfirmationReference);
		}

		public void TestWriteToDataObjectPopulateFromDate()
		{
			#region Setup Test Data

			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";
			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			receiveAdvice.YRA_FromDate = fromDate;
			Factory.SaveForTesting();

			#endregion Setup Test Data

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);
			AssertEquals(fromDate, dataObject.DateCollection.First(a => a.Type == DateType.Start).Value);
		}

		public void TestWriteToDataObjectPopulateToDate()
		{
			#region Setup Test Data

			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20G1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);
			receiveAdvice.YRA_ToDate = toDate;
			Factory.SaveForTesting();

			#endregion Setup Test Data

			var dataObject = new PreArrivalDataObjectWriter(new DataWritingManager(new ActionInfo(null, receiveAdvice))).GetDataObject(receiveAdvice);
			AssertEquals(toDate, dataObject.DateCollection.First(a => a.Type == DateType.End).Value);
		}
	}
}
