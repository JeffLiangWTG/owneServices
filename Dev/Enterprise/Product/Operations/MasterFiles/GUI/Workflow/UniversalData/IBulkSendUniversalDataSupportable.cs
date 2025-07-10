using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public interface IBulkSendUniversalDataSupportable
	{
		string NameOfSingleObject { get; }
		Type TypeOfSingleObject { get; }
		string WorkflowDescriptorCode { get; }
		IEnumerable<IWorkflowProvider> GetElementsToSend();
	}
}