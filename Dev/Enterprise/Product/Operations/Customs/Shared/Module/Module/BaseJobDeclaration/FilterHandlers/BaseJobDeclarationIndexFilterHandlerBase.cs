using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public abstract class BaseJobDeclarationIndexFilterHandlerBase : IndexFilterHandlerBase
	{
		protected BaseJobDeclarationIndexFilterHandlerBase(FilterStripBusinessObject parent) : base(parent)
		{
		}

		public override IReadOnlyCollection<Type> ApplicableTypes =>
			new[]
			{
				typeof(JobDeclarationFilterBusinessObject),
			};
	}
}
