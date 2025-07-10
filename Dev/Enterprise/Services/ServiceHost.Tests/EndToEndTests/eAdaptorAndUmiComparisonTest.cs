using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Services.ServiceHost.Tests.EndToEndTests
{
	/// <summary>
	/// In the majority of cases incoming Universal XML should produce the same result when processed via UMI service task or via HTTP+XML
	/// Here you can write a test with incoming xml and assert the end results
	/// The xml will then be processed by UMI and HTTP+XML in seperate tests with the same assertions
	/// </summary>
	[UseSnapshotProtection]
	public abstract class eAdaptorAndUmiComparisonTest : eAdaptorEndToEndNonTransactionedTest
	{
		public void TestImportChargeCodeToConsol_NoRevenueRecognition()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000001";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "AIR";
			consol.AddShipment(shipment);

			var chargeDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			chargeCodes.ForEach(c => c.AC_ChargeGroup = "");

			Factory.Save();

			var xml = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>{consol.JK_UniqueConsignRef}</Key>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
          <EnterpriseID>EDI</EnterpriseID>
          <ServerID>DAT</ServerID>
        </DataContext>
        <ConsolCosts>
          <ConsolCostLineCollection>
            <ConsolCostLine>
              <ImportMetaData>
                <Instruction>Insert</Instruction>
              </ImportMetaData>
              <ChargeCode>
                <Code>BAF</Code>
              </ChargeCode>
              <CostOSAmount>19999</CostOSAmount>
              <CostOSCurrency>
                <Code>CNY</Code>
              </CostOSCurrency>
              <Creditor>
                <Type>Organization</Type>
                <Key>AALSHI</Key>
              </Creditor>
            </ConsolCostLine>
          </ConsolCostLineCollection>
        </ConsolCosts>
      </Shipment>
    </UniversalShipment>
</UniversalShipment>";

			var valuesForTest = new RevenueRecognitionCollection();
			using (AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, valuesForTest))
			using (SetContextFromMessageProcessing(Env.CurrentBranchPK, chargeDepartment.PK.ToGuid()))
			{
				var message = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);
				var messageNote = GetMessageNoteText(message.PK);
				AssertContains("Error - Local Cost Amount: You have not setup Revenue Recognition", messageNote);
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			}
		}

		public void TestImportChargeCodeToShipment_NoRevenueRecognition()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000001";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OH_ImportBroker = ZGuid.Empty;
			consol.AddShipment(shipment);

			var chargeDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			chargeCodes.ForEach(c => c.AC_ChargeGroup = "");

			Factory.Save();

			var xml = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>{consol.JK_UniqueConsignRef}</Key>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
          <EnterpriseID>EDI</EnterpriseID>
          <ServerID>DAT</ServerID>
        </DataContext>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>ForwardingShipment</Type>
                  <Key>{shipment.JS_UniqueConsignRef}</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <JobCosting>
              <Branch>
                <Code>BRA</Code>
              </Branch>
              <ChargeLineCollection>
                <ChargeLine>
                  <ImportMetaData>
                <Instruction>Insert</Instruction>
              	  </ImportMetaData>
              	  <ChargeCode>
                	<Code>BAF</Code>
              	  </ChargeCode>
              	  <CostOSAmount>19999</CostOSAmount>
              	  <CostOSCurrency>
                	<Code>CNY</Code>
              	  </CostOSCurrency>
              	  <Creditor>
                	<Type>Organization</Type>
                	<Key>AALSHI</Key>
              	  </Creditor>
				  <Department>
					<Code>FEA</Code>
                  </Department>
                </ChargeLine>
              </ChargeLineCollection>
            </JobCosting>
          </SubShipment>
        </SubShipmentCollection>
      </Shipment>
    </UniversalShipment>
</UniversalShipment>";

			var valuesForTest = new RevenueRecognitionCollection();
			using (AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, valuesForTest))
			using (SetContextFromMessageProcessing(Env.CurrentBranchPK, chargeDepartment.PK.ToGuid()))
			{
				var message = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);
				var messageNote = GetMessageNoteText(message.PK);
				AssertContains("Error - Local Cost Amount: You have not setup Revenue Recognition", messageNote);
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			}
		}

		public void TestImportChargeCodeToShipment_NoDepartment()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000001";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_OH_ImportBroker = ZGuid.Empty;
			consol.AddShipment(shipment);

			var chargeDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			chargeCodes.ForEach(c => c.AC_ChargeGroup = "");

			Factory.Save();

			var xml = $@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>ForwardingConsol</Type>
              <Key>{consol.JK_UniqueConsignRef}</Key>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
          <EnterpriseID>EDI</EnterpriseID>
          <ServerID>DAT</ServerID>
        </DataContext>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>ForwardingShipment</Type>
                  <Key>{shipment.JS_UniqueConsignRef}</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <JobCosting>
              <Branch>
                <Code>BRA</Code>
              </Branch>
              <ChargeLineCollection>
                <ChargeLine>
                  <ImportMetaData>
                <Instruction>Insert</Instruction>
              	  </ImportMetaData>
              	  <ChargeCode>
                	<Code>BAF</Code>
              	  </ChargeCode>
              	  <CostOSAmount>19999</CostOSAmount>
              	  <CostOSCurrency>
                	<Code>CNY</Code>
              	  </CostOSCurrency>
              	  <Creditor>
                	<Type>Organization</Type>
                	<Key>AALSHI</Key>
              	  </Creditor>
                </ChargeLine>
              </ChargeLineCollection>
            </JobCosting>
          </SubShipment>
        </SubShipmentCollection>
      </Shipment>
    </UniversalShipment>
</UniversalShipment>";

			var valuesForTest = new RevenueRecognitionCollection();
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, valuesForTest))
			using (SetContextFromMessageProcessing(Env.CurrentBranchPK, chargeDepartment.PK.ToGuid()))
			{
				var message = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);
				var messageNote = GetMessageNoteText(message.PK);
				AssertContains("Error - Department: Please enter a Department.", messageNote);
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestImportXmlAndFireMilestoneWithFutureDate()
		{
			Globals.IsUserInteractive = false;
			var futureDate = ZDateTime.Now.AddDays(10);
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00002222";
			var workflowProvider = shipment as IWorkflowProvider;
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.InterimReceiptProducedCode;
			milestone.TriggerConditions.TriggerFiredCountdown = 1;

			factory.Save();

			var inboundXml =
		$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <DateCollection>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value>{futureDate}</Value>
      </Date>
    </DateCollection>
  </Shipment>
</UniversalShipment>";

			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var message = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, inboundXml);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			factory = new BusinessObjectFactory();
			milestone = factory.Load<ProcessTask>(milestone.PK);
			var wteLogs = milestone.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals(1, wteLogs.Count());
			AssertEquals(futureDate, milestone.P9_ActualDate);

			workflowProvider = factory.Load<Forwarding.IForwardingShipment>(shipment.PK) as IWorkflowProvider;
			var exception = workflowProvider.WorkflowItems.Exceptions.Where(e => e.P9_SE_NKExceptionEvent == ProcessWorkflowExceptionType.ExceptionFutureEvent);
			AssertEquals($"Expecting 1 {ProcessWorkflowExceptionType.ExceptionFutureEvent} milestone exception", 1, exception?.Count());
		}

		public void TestRollbackSaveExceptionCaughtInternally()
		{
			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>{0}</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>ANGRY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";
			bool skipNext = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => {
				// only want to cause the SaveAtEndOfImport in ImportShipment to rollback
				if (factory.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName || factory.NameForDebugging == "Universal Message Processing")
				{
					if (!skipNext)
					{
						//Action that corrupts the current transaction, no matter what this will be rolledback
						try
						{
							using (var manager = Db.Connection.BeginTransactionWithManager())
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
							using (var command = Db.Connection.Command("dbo.THIS_WILL_THROW"))
							{
								command.ExecuteNonQuery();
								manager.CommitTransaction();
							}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
						}
						catch (Exception)
						{
							skipNext = true;
						}
					}
					else
					{
						skipNext = false;
					}
				}
			}); 
			var result = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, inboundXml);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, result.EM_Status);
			var messageNote = GetMessageNoteText(result.PK);
			AssertContains("Transaction has been rolled back in the server.", messageNote);
			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, "ANGRY");
			var reloadedShipment = Factory.LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertNull("Should be null since the import was rolledback", reloadedShipment);
			var ex = ErrorReporter.LastMessageReported;
			AssertNotNullOrEmpty(ex);
			AssertContains("If you rollback a delayed transaction you must throw back to the layer where the delayed transaction in managed. Rollback happened here:", ex);
			ErrorReporter.Clear();
		}

		public void TestEmailNotificationOnDiscardedMessage()
		{
			var xml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
  </DataContext>

    <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataTargetCollection>
                <DataTarget>
                  <Type>ForwardingShipment</Type>
                  <Key>S00001492</Key>
                </DataTarget>
              </DataTargetCollection>
            </DataContext>
            <JobCosting>
              <Branch>
                <Code>RAH</Code>
              </Branch>
              <ChargeLineCollection>
                <ChargeLine>
                  <ImportMetaData>
                    <Instruction>Insert</Instruction>
                  </ImportMetaData>
                  <ChargeCode>
                    <Code>40552</Code>
                  </ChargeCode>
                  <CostOSAmount>20000</CostOSAmount>
                  <CostOSCurrency>
                    <Code>CNY</Code>
                  </CostOSCurrency>
                  <Creditor>
                    <Type>Organization</Type>
                    <Key>YUKRAH</Key>
                  </Creditor>
                </ChargeLine>
              </ChargeLineCollection>
            </JobCosting>
          </SubShipment>
        </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

			var group = Factory.New<IGlbGroup>();
			group.GG_Code = "Test";
			group.GG_Desc = "Test";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@test.com";

			var groupStaffLink = Factory.New<GlbGroupLink>();
			groupStaffLink.GK_GG = group.PK;
			groupStaffLink.GK_GS = staff.PK;
			Factory.Save();

			eServicesRegistry.Instance.ImportDiscardedNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			int CountEmailsInDB() => Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {MailDBItemsSchema.Constants.SqlSchemaName}.{MailDBItemsSchema.Constants.TableName}");
			var initialEmailCount = CountEmailsInDB();

			var result = ProcessMessage(EDIMessageSubTypeList.Codes.XmlUniversalShipment, xml);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, result.EM_Status);
			var emails = Env.OutgoingMailManager.EmailsCreated;

			AssertEquals("Email should be created", 1, emails.Count);
			AssertEquals("No really, Email should be created", 1, CountEmailsInDB() - initialEmailCount);
		}

		protected abstract IDisposable SetContextFromMessageProcessing(Guid branchPK, Guid departmentPK);

		protected abstract IEDIMessage ProcessMessage(string subType, string xml);

		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.EDIMessage");
		}

		ZString GetMessageNoteText(ZGuid messagePK)
		{
			var notes = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, messagePK));
			AssertEquals("Expecting 1 message note", 1, notes.Length);
			return notes[0].ST_NoteDataAsText;
		}
	}

	public class eAdaptorTest : eAdaptorAndUmiComparisonTest
	{
		protected override IEDIMessage ProcessMessage(string subType, string xml)
		{
			var response = PostRequest(xml);
			return Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
		}

		protected override IDisposable SetContextFromMessageProcessing(Guid branchPK, Guid departmentPK)
		{
			void SetWebContext(Guid branch, Guid department)
			{
				DataRegistry.Instance.WebDepartment = department;
				DataRegistry.Instance.WebBranch = branch;
			}

			var oldDepatmentPK = DataRegistry.Instance.WebDepartment;
			var oldBranchPK = DataRegistry.Instance.WebBranch;

			var branch2 = Factory.Load<GlbBranch>(branchPK);
			branch2[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";
			Factory.Save();

			SetWebContext(branchPK, departmentPK);

			return new DisposableAction(() => SetWebContext(oldBranchPK, oldDepatmentPK));
		}
	}

	public class UMITest : eAdaptorAndUmiComparisonTest
	{
		protected override IEDIMessage ProcessMessage(string subType, string xml)
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = subType;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = xml;
			Factory.Save();

			var serviceTask = new UMIServiceTask();
			serviceTask.RunTask();

			((BusinessObject)message).Reload();
			return message;
		}

		protected override IDisposable SetContextFromMessageProcessing(Guid branchPK, Guid departmentPK)
		{
			return Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, departmentPK);
		}
	}
}
