using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAirlineBranchAccountValidation : AutoOrgAirlineBranchAccountValidation
	{
		public OrgAirlineBranchAccountValidation(AutoOrgAirlineBranchAccount parent) : base(parent)
		{
		}

		#region OAA_APAirlineAccountNumber

		protected override void CheckOAA_APAirlineAccountNumber()
		{
			base.CheckOAA_APAirlineAccountNumber();

			if (Parent.OAA_APAirlineAccountNumber.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.OAA_APAirlineAccountNumberInfo);
			}
		}

		#endregion

		#region OAA_GB_Branch

		protected override void CheckOAA_GB_Branch()
		{
			base.CheckOAA_GB_Branch();

			if (Parent.Carrier != null && Parent.Carrier.OrgAirlineBranchAccounts.Any(o => o.PK != Parent.PK && o.OAA_GB_Branch == Parent.OAA_GB_Branch))
			{
				Parent.OAA_GB_BranchInfo.AddError(Res.GetString("f5d701ab-fefa-4c0f-b54f-9941a16b528a", "The Branch has been duplicated and must be unique."));
			}
		}

		#endregion
	}
}
