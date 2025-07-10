using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	abstract class BaseJobDeclarationIndexFilterHandlerTestCase<TIndexFilterHandler> : IndexFilterHandlerBaseTestCase<TIndexFilterHandler> where TIndexFilterHandler : IndexFilterHandlerBase
	{
		protected override ZModule GetParentModule() => ZModule.GetZModule(ModuleIDs.Customs.JobDeclaration);

		protected override IReadOnlyCollection<Type> ExpectedApplicableTypes =>
			new[]
			{
				typeof(JobDeclarationFilterBusinessObject),
			};

		protected override FilterStripBusinessObject GetNewFilterStripBizO() => new JobDeclarationFilterBusinessObject();
	}
}
