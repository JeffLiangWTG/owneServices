using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSalesMainCompetitorModuleFilterValidation : ModuleFilterValidation
	{
		public OrgSalesMainCompetitorModuleFilterValidation(OrgSalesMainCompetitorModuleFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected OrgSalesMainCompetitorModuleFilter Parent;

		public override Type AutoValidationType => GetType();

		public override void ValidateAll()
		{
			ValidateCompetitorAndCompetitorType();
		}

		#region Validate Competitor and CompetitorType

		public void ValidateCompetitorAndCompetitorType()
		{
			ValidateCompetitor();
			ValidateCompetitorType();
		}

		public void ValidateCompetitor()
		{
			ValidateCalculatedProperty(Parent.CompetitorInfo);
		}

		protected virtual void CheckCompetitor()
		{
			TypeValidation.CheckValidGuid(Parent.CompetitorInfo);
			if (Parent.Competitor.IsEmpty && !Parent.CompetitorType.IsEmpty)
			{
				Parent.CompetitorInfo.AddWarning(CompetitorAndTypeBothNeedToBeSet);
			}
		}

		public void ValidateCompetitorType()
		{
			ValidateCalculatedProperty(Parent.CompetitorTypeInfo);
		}

		protected virtual void CheckCompetitorType()
		{
			if (Parent.CompetitorType.IsEmpty && !Parent.Competitor.IsEmpty)
			{
				Parent.CompetitorTypeInfo.AddWarning(CompetitorAndTypeBothNeedToBeSet);
			}
			ListValidation.WarnIfInvalidCode(Parent.CompetitorTypeInfo);
		}
		string CompetitorAndTypeBothNeedToBeSet => Enterprise.MasterFiles.Module.Res.GetString("237BB998-6B2E-4B6A-BE1A-47FE2F94E3A3", "Both fields need to be set to filter results");

		#endregion
	}
}
