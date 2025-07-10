using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgHasMainCompetitorModuleFilterValidation : ModuleFilterValidation
	{
		public OrgHasMainCompetitorModuleFilterValidation(OrgHasMainCompetitorModuleFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected OrgHasMainCompetitorModuleFilter Parent;

		public override Type AutoValidationType => GetType();

		public override void ValidateAll()
		{
			ValidateCompetitorType();
		}

		public void ValidateCompetitorType()
		{
			ValidateCalculatedProperty(Parent.CompetitorTypeInfo);
		}

		protected virtual void CheckCompetitorType()
		{
			ListValidation.WarnIfInvalidCode(Parent.CompetitorTypeInfo);
		}
	}
}
