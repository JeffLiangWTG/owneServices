
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class GlbExternalPasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
	{
		protected GlbExternalPasswordLookups(GlbExternalPassword parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList InsuranceAgents => GlbExternalPasswordHelper.GetInsuranceAgents(Factory);

		protected new GlbExternalPassword Parent => (GlbExternalPassword)base.Parent;
	}
}
