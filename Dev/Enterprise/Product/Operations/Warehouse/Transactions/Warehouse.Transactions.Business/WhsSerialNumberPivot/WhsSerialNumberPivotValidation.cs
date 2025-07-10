using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivotValidation : AutoWhsSerialNumberPivotValidation
	{
		public WhsSerialNumberPivotValidation(AutoWhsSerialNumberPivot parent)
			: base(parent)
		{
		}

		protected override void CheckWSV_WSN_SerialNumber()
		{
			var pivot = (WhsSerialNumberPivot)Parent;
			var hasValidSerialNumber = !pivot.WSV_WSN_SerialNumber.IsEmpty && !pivot.WSV_ParentID.IsEmpty && !pivot.WSV_ParentTableCode.IsEmpty && pivot.SerialNumber != null;
			if (hasValidSerialNumber && !pivot.WSV_WSN_SerialNumberInfo.HasErrors())
			{
				if (pivot.IsSerialNumberAlreadyInUse())
				{
					pivot.WSV_WSN_SerialNumberInfo.AddError(Res.GetString("99dfd59b-5022-42bb-a0be-2b465c9e172e", "Serial # already used."));
				}
			}
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> WhsSerialNumberPivotSchema.Constants.WSV_WSN_SerialNumber != info.Name && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
