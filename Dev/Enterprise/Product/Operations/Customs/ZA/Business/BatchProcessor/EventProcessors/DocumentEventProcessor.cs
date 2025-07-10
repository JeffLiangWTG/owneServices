using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.EventProcessors
{
	public class DocumentEventProcessor : IEventProcessor
	{
		public DocumentEventProcessor(IXmlImportLogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}

		public void Process(JobDeclaration declaration, IXmlEventValueObject xmlEvent)
		{
			if (declaration != null && xmlEvent != null)
			{
				var parameters = StmALog.GetParametersFromReference(xmlEvent.EventReference);
				var lrn = parameters.GetValueSafe(ParameterCodes.ReferenceNumber);
				if (!string.IsNullOrEmpty(lrn))
				{
					var entryHeader = declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault(x => x.CH_BGMReference == lrn);
					var entryInstruction = entryHeader?.EntryInstruction;
					var requestNumber = parameters.GetValueSafe(ParameterCodes.RequestNumber);
					if (entryInstruction != null && !string.IsNullOrEmpty(requestNumber))
					{
						var caseNumber = entryInstruction.CaseNumbers.OfType<CaseNumber>().FirstOrDefault(x => x.CY_Data == requestNumber);
						if (caseNumber != null)
						{
							SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber, (UniversalEvent)xmlEvent);
							logger.LogBoth(LogType.Information, Res.GetString("DocumentEventProcessor|3DAE385F-06AD-4B93-A255-34FC5AFE2F12", "Case Number status has been updated to {0}.", caseNumber.Document_Status));
						}
						else
						{
							logger.LogBoth(LogType.Error, Res.GetString("DocumentEventProcessor|3017E9FC-1251-424A-B9A7-F2878AB81CF0", "Unable to find the linked Case Number for {0} Event: {1}", xmlEvent.EventType, xmlEvent.EventReference));
						}
					}
					else
					{
						logger.LogBoth(LogType.Error, Res.GetString("DocumentEventProcessor|63202E17-9FCB-49F2-843D-1511CC8EABB5", "Unable to find the linked Entry Header for {0} Event: {1}", xmlEvent.EventType, xmlEvent.EventReference));
					}
				}

				PostProcessing(declaration, xmlEvent);
			}
		}

		protected virtual void PostProcessing(JobDeclaration declaration, IXmlEventValueObject xmlEvent)
		{
		}

		protected readonly IXmlImportLogger logger;
	}
}
