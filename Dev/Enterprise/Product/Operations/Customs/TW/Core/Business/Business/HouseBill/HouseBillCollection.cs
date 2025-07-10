using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class BillCollection : Customs.Business.BillCollection<Bill, JobDeclaration>
	{
		public BillCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = (Bill)child;
			using (bill.GetValidationSuspender())
			using (bill.SuspendSettingHasChanges())
			{
				if (Count > 0)
				{
					var previousLine = this[Count - 1];
					SetDefaultFromPreviousLine(previousLine, bill);
				}
			}
		}

		void SetDefaultFromPreviousLine(Bill previousLine, Bill currentLine)
		{
			if (previousLine.CU_BillType == BillTypeList.Codes.ContainerNote)
			{
				currentLine.CU_BillType = BillTypeList.Codes.ContainerNote;
			}
		}
	}
}
