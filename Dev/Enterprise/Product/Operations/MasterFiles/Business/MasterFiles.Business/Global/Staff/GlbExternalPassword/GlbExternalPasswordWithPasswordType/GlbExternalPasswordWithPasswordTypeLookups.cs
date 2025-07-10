using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordWithPasswordTypeLookups : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordWithPasswordTypeLookups(GlbExternalPasswordWithPasswordType parent)
			: base(parent)
		{
		}

		new GlbExternalPasswordWithPasswordType Parent
		{
			get { return (GlbExternalPasswordWithPasswordType)base.Parent; }
		}

		public override CodeDescriptionPairList PasswordTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Parent.PasswordTypeCode, Parent.PasswordTypeDescription);
				return result;
			}
		}
	}
}
