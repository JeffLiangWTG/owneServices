using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Project; }
		}

		public override Type RootType
		{
			get { return typeof(Project); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Project;

		public override string SingularElementNoun
		{
			get { return Project.SingularName; }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("B0FAA112-8AF4-4491-950D-B9976F43380C", "Projects"); }
		}
	}
}
