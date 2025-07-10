using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryInstructionCollection : Customs.Business.CusEntryInstructionCollection<CusEntryInstruction>
	{
		public CusEntryInstructionCollection(JobDeclaration parentBO)
			: base(parentBO)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.SuspendSettingHasChanges())
			{
				if (child is CusEntryInstruction cei)
				{
					cei.CEI_EntityType = EntityTypeList.Codes.CustomsCode;
					cei.CEI_IsUCROverridden = cei.JobDeclaration?.IsImport ?? false;
				}
			}
		}
	}
}
