using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionDeepCloneStrategy : Customs.Business.CusEntryInstructionDeepCloneStrategy
	{
		public CusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction entryInstructionToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(entryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		CusEntryInstruction CusEntryInstructionToClone => (CusEntryInstruction)base.bizObjToClone;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedResult = (CusEntryInstruction)base.CloneInternal(args);

			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				if (IsTemplateCopy)
				{
					CusEntryInstructionToClone.Notes.FindByDescription(PredefinedNoteTypes.Instance.TWTradersRemarks.Description).Where(x => !x.IsDeleted).DeepCloneNotesTo(clonedResult.Notes);
				}
			}
			return clonedResult;
		}

		public override BusinessObject Clone()
		{
			var clone = (CusEntryInstruction)base.Clone();
			clone.CEI_DaysOfDelayedDeclaration = 0;
			return clone;
		}
	}
}
