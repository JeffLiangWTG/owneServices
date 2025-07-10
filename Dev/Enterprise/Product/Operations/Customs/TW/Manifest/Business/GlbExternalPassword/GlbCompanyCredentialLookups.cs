using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class GlbCompanyCredentialLookups : GlbExternalPasswordLookups
	{
		public GlbCompanyCredentialLookups(GlbCompanyCredential parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PasswordTypeList
		{
			get
			{
				var codes = new PasswordTypesList();

				for (var index = codes.Count; index != 0; --index)
				{
					if (codes[index - 1].Code != Parent.GP_PasswordType)
					{
						codes.RemoveAt(index - 1);
					}
				}

				return codes;
			}
		}
	}
}
