using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WorkItem; }
		}

		public override Type RootType
		{
			get { return typeof(WorkItem); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WorkItem;

		public override string SingularElementNoun
		{
			get { return WorkItem.SingularName; }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("99F4FD8C-E252-41D6-92E6-3C380FBC7F9E", "Work Items"); }
		}
	}
}
