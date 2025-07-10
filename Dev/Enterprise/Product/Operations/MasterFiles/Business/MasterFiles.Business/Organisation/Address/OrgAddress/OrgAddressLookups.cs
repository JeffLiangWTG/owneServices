namespace Enterprise.MasterFiles.Business
{
	using Enterprise.ZArchitecture.Core;

	public class OrgAddressLookups : AutoOrgAddressLookups
	{
		public OrgAddressLookups(AutoOrgAddress parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CartageEquipmentNeededFCL
		{
			get { return Factory.GetCachedValue<FCLEquipmentNeededList>(); }
		}

		public CodeDescriptionPairList CartageEquipmentNeededLCL
		{
			get { return Factory.GetCachedValue<LCLAIREquipmentNeededList>(); }
		}

		public CodeDescriptionPairList CartageEquipmentNeededAir
		{
			get { return Factory.GetCachedValue<LCLAIREquipmentNeededList>(); }
		}

		public CodeDescriptionPairList AuthorityToLeaveOptions
		{
			get { return Factory.GetCachedValue<AuthorityToLeaveOptions>(); }
		}
	}
}
