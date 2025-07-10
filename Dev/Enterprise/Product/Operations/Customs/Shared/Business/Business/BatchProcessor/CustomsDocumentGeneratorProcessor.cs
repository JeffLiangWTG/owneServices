using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class CustomsDocumentGeneratorProcessor : BaseCustomsStmProcessQueueBatchProcessor
	{
		public CustomsDocumentGeneratorProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ProcessQueueApplicationCode => CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode;

		protected override void ProcessQueuedItemCore(StmProcessQueue queuedItem)
		{
			if (queuedItem != null)
			{
				var documentName = ZString.Empty;
				var jobReference = ZString.Empty;

				try
				{
					var bizObj = queuedItem.Factory.Load(queuedItem.SW_ReferenceTableCode, queuedItem.SW_ReferenceID);
					if (bizObj is ICustomsDocumentGeneratorSupporter supporter)
					{
						using (DisposableEnvironment.ForBranch(supporter.BranchPK))
						{
							var actionCode = queuedItem.SW_ActionCode;
							jobReference = supporter.JobReference;
							documentName = supporter.GetDocumentName(actionCode);
							if (!documentName.IsEmpty && !jobReference.IsEmpty)
							{
								Logger.Log($"System is generating {documentName} document for Job {jobReference}.");
								if (supporter.GenerateCustomsDocument(actionCode))
								{
									Logger.Log($"{documentName} document has been generated successfully for Job {jobReference}.");
								}
								else
								{
									var stringBuilder = new ZStringBuilder();
									stringBuilder.Append($"System is unable to generate {documentName} document for Job {jobReference}");
									var reasonForUnableToGenerate = supporter.GetReasonForUnableToGenerateCustomsDocument();
									if (!reasonForUnableToGenerate.IsEmpty)
									{
										stringBuilder.Append(", becasue " + reasonForUnableToGenerate);
									}
									stringBuilder.Append(".");
									Logger.LogError(stringBuilder.ToString());
								}
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException() || ex.IsExceptionPresentIncludingInner<OutOfMemoryException>())
				{
					Logger.LogError($"System is unable to generate {documentName} document for Job {jobReference} due to following error:\r\n" + ex.Message);

					if (ex.IsExceptionPresentIncludingInner<OutOfMemoryException>())
					{
						ErrorReporter.ReportOnce("OutOfMemoryExceptionCaught", $"OutOfMemoryException caught when generate {documentName} document for Job {jobReference}.", ex); // Possible that the ErrorReporter won't be able to do anything due to out of merory
					}
				}
			}
		}
	}
}
