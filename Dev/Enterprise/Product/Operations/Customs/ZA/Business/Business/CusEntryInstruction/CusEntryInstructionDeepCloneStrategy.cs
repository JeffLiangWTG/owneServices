using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryInstructionDeepCloneStrategy : Customs.Business.CusEntryInstructionDeepCloneStrategy
	{
		public CusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction entryInstructionToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(entryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		public override BusinessObject Clone()
		{
			var newEntryInstruction = (CusEntryInstruction)base.Clone();
			var entryInstructionToClone = (CusEntryInstruction)bizObjToClone;
			newEntryInstruction.CEI_CustomsOfficeOverride = entryInstructionToClone?.CEI_CustomsOfficeOverride ?? ZString.Empty;
			return newEntryInstruction;
		}
	}
}
