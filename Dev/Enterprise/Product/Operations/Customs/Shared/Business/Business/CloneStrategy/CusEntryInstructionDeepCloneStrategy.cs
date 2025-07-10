using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public CusEntryInstructionDeepCloneStrategy(CusEntryInstruction entryInstructionToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(entryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}
	}
}
