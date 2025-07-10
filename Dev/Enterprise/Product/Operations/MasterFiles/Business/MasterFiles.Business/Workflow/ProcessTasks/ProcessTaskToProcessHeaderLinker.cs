using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.MasterFiles.Business
{
	static class ProcessTaskToProcessHeaderLinker
	{
		internal static void SetBestMatchingProcessHeaderOnProcessTaskForTemplateApplication(BusinessObjectFactory factory, IWorkflowProvider parent, ProcessTaskTemplate template, ProcessTask templateTask, ProcessTask actualTask, TemplateApplicationParameters parameters)
		{
			var processJobHeader = ProcessJobHeaderProvider.GetForParent(parent, factory);
			if (processJobHeader == null)
			{
				return;
			}

			var templateWorkflow = templateTask.ProcessHeader;
			if (templateWorkflow == null)
			{
				if (processJobHeader.ProcessHeaders.Count == 1 && processJobHeader.ProcessHeaders[0].FH_CompletionStatement == BMGlobalConstants.DefaultWorkflowCompletionStatement)
				{
					actualTask.P9_FH_ProcessHeader = ZGuid.Invalid;
					actualTask.P9_FH_ProcessHeader = processJobHeader.ProcessHeaders[0].PK;
				}
				return;
			}
			IProcessHeader bestMatchingProcessHeader = null;

			foreach (var possibleMatch in processJobHeader.ProcessHeaders.Cast<IProcessHeader>())
			{
				var isMatch = false;
				if (possibleMatch.FH_CompletionStatement == templateWorkflow.FH_CompletionStatement || possibleMatch.FH_ParentTemplateId == templateWorkflow.PK)
				{
					isMatch = true;
				}
				else if (possibleMatch.FH_ParentTemplateId.IsValid)
				{
					var currentProcessHeaderTemplateWorkflow = factory.Load<IProcessHeader>(possibleMatch.FH_ParentTemplateId);
					if (currentProcessHeaderTemplateWorkflow != null && currentProcessHeaderTemplateWorkflow.FH_CompletionStatement == templateWorkflow.FH_CompletionStatement)
					{
						isMatch = true;
					}
				}

				if (!isMatch)
				{
					continue;
				}

				// Take the most recently created workflow by created time
				// Reapplying templates multiple times without saving is not allowed so there should always be a sufficient different in FH_SystemCreateTimeUtc
				if (bestMatchingProcessHeader == null)
				{
					bestMatchingProcessHeader = possibleMatch;
				}
				else if (!possibleMatch.IsInDatabase)
				{
					bestMatchingProcessHeader = possibleMatch;
				}
				else if (bestMatchingProcessHeader.FH_SystemCreateTimeUtc <= possibleMatch.FH_SystemCreateTimeUtc)
				{
					bestMatchingProcessHeader = possibleMatch;
				}
				else
				{
					// FH_SystemCreateTimeUtc shouldn't be empty if IsInDatabase
				}
			}

			if (bestMatchingProcessHeader == null)
			{
				bestMatchingProcessHeader = processJobHeader.ApplyTemplateWorkflow(template, templateWorkflow, parameters);
			}

			if (bestMatchingProcessHeader != null)
			{
				actualTask.P9_FH_ProcessHeader = bestMatchingProcessHeader.PK;
			}
		}
	}
}
