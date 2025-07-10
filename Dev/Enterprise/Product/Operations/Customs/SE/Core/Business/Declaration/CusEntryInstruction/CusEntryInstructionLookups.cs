using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SE.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(EU.Business.Declaration.CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override CodeDescriptionPairList DeclarationTypeListCore => GetEntryStyleList();

		CodeDescriptionPairList GetEntryStyleList()
		{
			var entryTypes = new CodeDescriptionPairList();
			if (Parent?.JobDeclaration?.IsImport ?? false)
			{
				entryTypes = new ImportDeclarationTypeList();
			}
			return entryTypes;
		}
	}
}
