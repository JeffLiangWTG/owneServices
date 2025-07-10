using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.StabilityChecker;

namespace Enterprise.Customs.NZ.Business
{
	public class NZDiagnosticConsol : DiagnosticConsol
	{
		public NZDiagnosticConsol(BusinessObjectFactory factory)
			: base(factory)
		{
			companyNzBrokerageId = NZCustomsDataRegistry.Instance.NZBrokerageID.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			nzDiagnosticTimestamp = NzDiagnosticMark + ZDateTime.Now + "]";
		}

		public override ZString InitialMessage
		{
			get
			{
				return
					"This tool will send a test message to New Zealand Customs.\r\n" +
					"Each stage of sending and processing the reply will be tracked and shown below.\r\n" +
					"Press 'Start' to commence the test, or press 'Cancel' if you do not wish to run the test.\r\n";
			}
		}

		protected override ZGuid CreateTestMessage(BusinessObjectFactory testMessageFactory, ZString consolKey)
		{
			var testMessage = testMessageFactory.New<NZCMessage>();
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			testMessage.EM_Status = EDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <ID>00000000</ID>
  <TypeCode>CRE</TypeCode>
  <FunctionalReferenceID>" + nzDiagnosticTimestamp + @"</FunctionalReferenceID>
  <FunctionCode>1</FunctionCode>
  <Submitter>
    <ID>" + companyNzBrokerageId + @"</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementDescription>Diagnostic entry no value</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>
  <Declarant></Declarant>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";
			testMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			testMessage.EM_MessageSubType = MessageSubTypeList.Codes.Cancellation;
			testMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			testMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			testMessage.EM_IsActive = true;
			testMessage.EM_ApplicationReference = nzDiagnosticTimestamp;
#if DEBUG
			testMessage.EM_IsTestMessage = true;
#else
			testMessage.EM_IsTestMessage = false;
#endif

			return testMessage.PK;
		}

		public override List<RegistryAndCertificateCheckResult> RegistryAndCertificateChecks()
		{
			var result = new List<RegistryAndCertificateCheckResult>();

			if (string.IsNullOrEmpty(companyNzBrokerageId))
			{
				result.Add(new RegistryAndCertificateCheckResult("There is no Customs Registration Number set for the current company.Please set registry 'Customs > Country or Region Specific > New Zealand > Brokerage ID'.", StabilityResultLevel.Critical));
			}

			return result;
		}

		public override ZString ProcessTimerTick(out ZString diagnosticConsolAction)
		{
			var result = ZString.Empty;
			diagnosticConsolAction = ZString.Empty;

			if (cycleController == null)
			{
				var messagePK = diagnosticNote.MessagePK;
				var messageCreateTime = Factory.Load<EDIMessage>(messagePK)?.EM_SystemCreateTimeUtc ?? ZDateTime.Now.AddDays(-3);

				cycleController = new DiagnosticCycleController(messagePK, messageCreateTime, companyNzBrokerageId);
				result = diagnosticStatusList.GetDescriptionFromCode(InitialStatus);
			}
			else
			{
				var previousStatus = diagnosticNote.Status;

				try
				{
					diagnosticNote.Status = cycleController.CheckNextDiagnosticCycleStep(previousStatus);
				}
				catch (DiagnosticMessageException diagEx)
				{
					diagnosticNote.Status = diagEx.FailStatus;
					diagnosticConsolAction = DiagnosticConsolActions.Codes.Failed;
				}
				catch (ApplicationException ex)
				{
					diagnosticConsolAction = DiagnosticConsolActions.Codes.ErrorOccurredInProcessing;
					result = ex.Message;
				}

				if (diagnosticNote.Status != previousStatus)
				{
					if (diagnosticNote.Status == DiagnosticStatusListNZ.Codes.OKSuccess)
					{
						diagnosticConsolAction = DiagnosticConsolActions.Codes.Success;
					}

					if (result.IsEmpty)
					{
						result = diagnosticStatusList.GetDescriptionFromCode(diagnosticNote.Status);
					}
				}
			}

			return result;
		}

		protected override ZString GetExtendedStatusDescription(ZString currentStatus)
		{
			var extendedStatusDescription = DiagnosticStatusListNZ.GetExtendedStatusDescription(currentStatus);

			if (extendedStatusDescription == "No additional information available")
			{
				extendedStatusDescription = DiagnosticStatusList.GetExtendedStatusDescription(currentStatus);
			}
			return extendedStatusDescription;
		}

		protected override ZString GetCurrentDiagnosticStatus()
		{
			return diagnosticNote.Status;
		}

		readonly string nzDiagnosticTimestamp;
		public const string NzDiagnosticMark = "[=NZ-DIAG ";
		DiagnosticCycleController cycleController;
		readonly string companyNzBrokerageId;
		readonly DiagnosticStatusListNZ diagnosticStatusList = new DiagnosticStatusListNZ();
	}
}
