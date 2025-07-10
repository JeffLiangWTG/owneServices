//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPackageAuditValidation
//
//    This class should be used for overriding validation in AutoWhsPackageAuditValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageAuditValidation : AutoWhsPackageAuditValidation
	{
		public WhsPackageAuditValidation(AutoWhsPackageAudit parent) : base(parent)
		{
		}

		#region CheckWPA_PackageID

		protected override void CheckWPA_PackageID()
		{
			base.CheckWPA_PackageID();
			MandatoryValidation.CheckEntered(Parent.WPA_PackageIDInfo);
		}

		#endregion

		#region CheckWPA_GS_NKAuditor

		protected override void CheckWPA_GS_NKAuditor()
		{
			base.CheckWPA_GS_NKAuditor();
			MandatoryValidation.CheckEntered(Parent.WPA_GS_NKAuditorInfo);
		}

		#endregion
	}
}
