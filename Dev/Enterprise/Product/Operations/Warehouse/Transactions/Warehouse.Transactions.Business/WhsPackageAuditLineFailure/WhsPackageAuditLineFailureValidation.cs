//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPackageAuditLineFailureValidation
//
//    This class should be used for overriding validation in AutoWhsPackageAuditLineFailureValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageAuditLineFailureValidation : AutoWhsPackageAuditLineFailureValidation
	{
		public WhsPackageAuditLineFailureValidation(AutoWhsPackageAuditLineFailure parent) : base(parent)
		{
		}

		protected override void CheckWPF_AuditedQty()
		{
			base.CheckWPF_AuditedQty();
			CheckExpectedAndAuditedQuantitiesAreDifferent(Parent.WPF_AuditedQtyInfo);
		}

		protected override void CheckWPF_ExpectedQty()
		{
			base.CheckWPF_ExpectedQty();
			CheckExpectedAndAuditedQuantitiesAreDifferent(Parent.WPF_ExpectedQtyInfo);
		}

		void CheckExpectedAndAuditedQuantitiesAreDifferent(ZPropertyInfo propertyInfo)
		{
			if (Parent.WPF_ExpectedQty == Parent.WPF_AuditedQty)
			{
				propertyInfo.AddError(Res.GetString("300E4A79-5515-4191-9CE9-6CFD73083C88", "Audited Quantity must be different than the Expected Quantity."));
			}
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> WhsPackageAuditLineFailureSchema.Constants.WPF_WPA_WhsPackageAudit != info.Name && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
