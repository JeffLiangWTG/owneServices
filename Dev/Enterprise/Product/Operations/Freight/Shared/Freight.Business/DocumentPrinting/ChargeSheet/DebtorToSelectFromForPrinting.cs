using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class DebtorToSelectFromForPrinting : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DebtorToSelectFromForPrinting(OrgHeader debtor)
			: base(debtor.Factory)
		{
			fDebtor = debtor;
			fOH_Calc_PrintDebtor = ZBool.True;

			fDebtor.ReadOnly = true;
		}

		#region Debtor

		public OrgHeader Debtor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fDebtor; }
		}
		protected OrgHeader fDebtor;

		#endregion

		#region OH_Calc_PrintDebtor

		public ZBool OH_Calc_PrintDebtor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fOH_Calc_PrintDebtor; }
			set { SetNonPersistentPropertyValue(OH_Calc_PrintDebtorInfo, ref fOH_Calc_PrintDebtor, value); }
		}
		protected ZBool fOH_Calc_PrintDebtor;

		public ZPropertyInfo OH_Calc_PrintDebtorInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Calc_PrintDebtor)); }
		}

		#endregion
	}
}
