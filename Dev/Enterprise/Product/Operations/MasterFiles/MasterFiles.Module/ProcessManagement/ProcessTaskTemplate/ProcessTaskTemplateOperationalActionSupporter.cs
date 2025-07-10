using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	class ProcessTaskTemplateOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(ProcessTaskTemplate);

		public override BusinessContext BusinessContext => BusinessContext.WorkflowTemplates;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WorkflowTaskTemplates;

		public override string SingularElementNoun => Res.GetString("7616495d-24af-43b7-9d7a-3be54e413371", "Workflow Template");

		public override string PluralElementNoun => Res.GetString("341b242d-fb62-4207-8f87-43b6c0dc0bcc", "Workflow Templates");
	}
}
