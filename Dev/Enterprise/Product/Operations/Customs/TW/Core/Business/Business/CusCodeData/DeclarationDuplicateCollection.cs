using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationDuplicateCollection : CusCodeDataCollection<DeclarationDuplicate>
	{
		public DeclarationDuplicateCollection(CusEntryInstruction entryInstruction)
		: base(entryInstruction, CusCodeDataTypeList.Codes.DeclarationDuplicate)
		{
		}
	}
}
