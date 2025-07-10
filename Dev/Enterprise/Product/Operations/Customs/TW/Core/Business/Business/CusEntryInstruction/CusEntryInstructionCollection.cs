using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
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

			using (child.GetValidationSuspender())
			using (child.SuspendSettingHasChanges())
			{
				var entryInstruction = (CusEntryInstruction)child;
				var jobDeclaration = entryInstruction?.JobDeclaration;
				var locationOfGoods = jobDeclaration?.JE_LocationOfGoods ?? ZString.Empty;
				if (!locationOfGoods.IsEmpty)
				{
					entryInstruction.CEI_GoodsLocation = locationOfGoods;
				}
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			var businessObject = (CusEntryInstruction)e.BizObject;
			businessObject?.JobDeclaration?.Validation.ValidateJE_CustomsOffice();
		}
	}
}
