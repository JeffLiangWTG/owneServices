using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchExtraPortsValidation : AutoGlbBranchExtraPortsValidation
	{
		public GlbBranchExtraPortsValidation(AutoGlbBranchExtraPorts parent) : base(parent)
		{
		}

		#region GY_RL_NKAdditionalBranchRelatedPort

		protected override void CheckGY_RL_NKAdditionalBranchRelatedPort()
		{
			base.CheckGY_RL_NKAdditionalBranchRelatedPort();
			ListValidation.ErrorIfInvalidCode(Parent.GY_RL_NKAdditionalBranchRelatedPortInfo);

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.GY_RL_NKAdditionalBranchRelatedPortInfo, Parent.Factory.Load<GlbBranchExtraPorts>(new ZQuery(GlbBranchExtraPortsSchema.GY_GB, Parent.GY_GB)));

			if (Parent.GY_RL_NKAdditionalBranchRelatedPort == Parent.Branch.GB_RL_NKHomePort)
			{
				Parent.GY_RL_NKAdditionalBranchRelatedPortInfo.AddError(Res.GetString("fac04b77-373a-4bf1-8202-92b869429ba0", "Additional related ports should be different to the Home port."));
			}

			if (Parent.Branch.Company != null && Parent.AdditionalBranchRelatedPort != null && !Parent.GY_RL_NKAdditionalBranchRelatedPort.IsEmpty && !Parent.Branch.Company.GC_RN_NKCountryCode.IsEmpty && (Parent.AdditionalBranchRelatedPort.Country.Code != Parent.Branch.Company.GC_RN_NKCountryCode))
			{
				string portString = Parent.AdditionalBranchRelatedPort.Country.Code;
				Parent.GY_RL_NKAdditionalBranchRelatedPortInfo.AddWarning(Res.GetString("C4E29768-13DC-4BF7-967C-964396B95836", "The Home Port entered belongs to a different country/region to the country/region code entered on the company specified. Please ensure this is correct. If the home port is correct It is strongly recommend that you create a company for {0} as accounting information will be entered against this company", portString));
			}
		}

		#endregion
	}
}
