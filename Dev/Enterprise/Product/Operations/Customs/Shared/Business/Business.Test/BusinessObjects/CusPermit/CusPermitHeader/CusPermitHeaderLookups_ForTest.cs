using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitHeaderLookups_ForTest : BaseCusPermitHeaderLookups
	{
		public CusPermitHeaderLookups_ForTest(BaseCusPermitHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PermitTypes => new CodeDescriptionPairList() {
			new CodeDescriptionPair("IMP", "Basic Import Permit"),
			new CodeDescriptionPair("EXP", "Basic Export Permit")
		};

		public override CodeDescriptionPairList PermitSubTypes => new CodeDescriptionPairList() {
			new CodeDescriptionPair("LVE", "Light Motor Vehicles")
		};
	}
}
