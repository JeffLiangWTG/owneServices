using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(PreArrivalDataContextManager))]
	public class PreArrivalDataContextManagerTest : ShipmentDataContextManagerTestCase<PreArrivalDataContextManager, CYDReceiveAdvice>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.YIA };
		Lazy<EmbeddedResourceRetriever> ResourceRetriever;
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			ResourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();
		}

		public void TestMatchingByDataContextKey()
		{
			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_JobNumber = "PAI00000001";

			Factory.SaveForTesting();

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				clientAddress
			});

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var relatedShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			relatedShipment.SetContainerCollection(() => new DataObjectList<Container> { UniversalTestHelper.CreateContainer("CON1", "LD-3", true) });
			subShipment.SetRelatedShipmentCollection(() => new List<Shipment> { relatedShipment });
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			shipment.BookingConfirmationReference = "ACC1";

			var dateCollection = new List<Date>
			{
				{ DateType.Start, ZBool.True, ZDateTime.UtcNow.Date.AddDays(-2) },
				{ DateType.End, ZBool.True, ZDateTime.UtcNow.Date.AddDays(2) }
			};
			shipment.SetDateCollection(() => dateCollection);

			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice, "PAI00000001");
			shipment.DataContext.DataProviderForCodeMapping = GlbCompany.CurrentCompany.OrgProxy.OH_Code;

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress {
					AddressType = nameof(DocAddressType.LocalCartageYard),
					OrganizationCode = "WUFSHIJNB"
				},
				clientAddress
			});

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "LD-3";
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Updated Pre-Arrival Instruction PAI00000001 from UniversalShipment.
Successfully saved Pre-Arrival Instruction PAI00000001.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestPreArrivalDataObjectReader_TransitReceiveASN()
		{
			var transitReceiveASN = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.TransitReceiveASN.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveASN);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			var newFactory = new BusinessObjectFactory();
			var receiveAdvice = newFactory.LoadTop1<CYDReceiveAdvice>(new ZQuery());
			AssertNotNull("ReceiveAdvice should be populated.", receiveAdvice);

			var receiveAdviceLines = newFactory.Load<CYDReceiveAdviceLine>(new ZQuery());
			AssertNotNull("ReceiveAdviceLines should not be empty.", receiveAdviceLines);
			AssertEquals("2 ReceiveAdviceLine should be populated.", 2, receiveAdviceLines.Length);

			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			AssertNotNull("CYDYardUnitState should not be empty.", yardUnitStates);
			AssertEquals("2 CYDYardUnitState should be populated.", 2, yardUnitStates.Length);
			AssertEquals("CYDYardUnitState should not created with UnitID 'FBGT2341234'.", 1, yardUnitStates.Where(u => u.YUS_UnitID == "FBGT2341234").Count());
			AssertEquals("CYDYardUnitState should not created with UnitID 'FBGT34563456'.", 1, yardUnitStates.Where(u => u.YUS_UnitID == "FBGT34563456").Count());
		}

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.CPA } : base.SupportedRecipientServices(recipientRole);
		}

		protected override string ValidPopulatedUniversalShipmentXML => ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.ReceiveAdvice.xml");

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			var refContainerType = Factory.New<RefContainer>();
			refContainerType.RC_Code = "20G0";
			Factory.SaveForTesting();
		}

		protected override void ConfigureDataContext(IDataContextDataObject dataContext)
		{
			dataContext.DataProviderForCodeMapping = GlbCompany.CurrentCompany.OrgProxy.OH_Code;
		}
	}
}
