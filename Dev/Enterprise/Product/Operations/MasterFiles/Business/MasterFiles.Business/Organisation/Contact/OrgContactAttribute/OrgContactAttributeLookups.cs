using Enterprise.Environment;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAttributeLookups : AutoOrgContactAttributeLookups
	{
		public OrgContactAttributeLookups(AutoOrgContactAttribute parent) : base(parent)
		{
		}

		#region Attribute Types

		public ReadOnlyCodeDescriptionPairList AttributeTypes
		{
			get { return Env.Registry.OrgListOfInterests; }
		}

		#endregion

		#region Allocation Types

		public CodeDescriptionPairList AllocationTypes
		{
			get
			{
				return Factory.GetCachedValue("AllocationTypes", delegate
				{
					return new CodeDescriptionPairList(Env.Registry.OrgListOfContactAllocations);
				});
			}
		}

		#endregion
	}
}
