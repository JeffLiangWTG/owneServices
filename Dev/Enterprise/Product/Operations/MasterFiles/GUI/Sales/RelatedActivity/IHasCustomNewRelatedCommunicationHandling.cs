using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public interface IHasCustomNewRelatedCommunicationHandling
	{
		void OnShowingNewFormForRelated(NewRelatedCommunicationArgs newRelatedCommunicationArgs);
	}

	public class NewRelatedCommunicationArgs : EventArgs
	{
		public NewRelatedCommunicationArgs(IRelatableActivity newRelatedCommunication, IEnumerable<IRelatableActivity> activitiesToMakeParent)
		{
			NewRelatedCommunication = newRelatedCommunication;
			ActivitiesToMakeParent = new HashSet<IRelatableActivity>(activitiesToMakeParent);
		}

		public readonly IRelatableActivity NewRelatedCommunication;
		public HashSet<IRelatableActivity> ActivitiesToMakeParent;
	}
}
