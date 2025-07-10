using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationDuplicateLookups : Customs.Business.CusCodeDataLookups
	{
		public DeclarationDuplicateLookups(DeclarationDuplicate parent)
			: base(parent)
		{
		}

		public new DeclarationDuplicate Parent => (DeclarationDuplicate)base.Parent;

		public override CodeDescriptionPairList CY_CodeList => DuplicateTypeList;

		public CodeDescriptionPairList DuplicateTypeList
		{
			get
			{
				var isImport = Parent.Parent?.JobDeclaration?.IsImport ?? false;
				if (isImport)
				{
					return Factory.GetCachedValue<IMPDuplicateTypeList>();
				}
				else
				{
					return Factory.GetCachedValue<EXPDuplicateTypeList>();
				}
			}
		}
	}
}
