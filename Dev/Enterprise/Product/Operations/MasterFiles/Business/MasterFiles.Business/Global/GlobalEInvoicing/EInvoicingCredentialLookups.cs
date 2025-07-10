using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingCredentialLookups : GlbExternalPasswordLookups
	{
		public EInvoicingCredentialLookups(GlbExternalPassword parent)
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
