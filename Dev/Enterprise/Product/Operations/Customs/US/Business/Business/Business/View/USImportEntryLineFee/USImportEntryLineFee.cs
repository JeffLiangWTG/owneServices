using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[SingleObjectAroundARow]
	public class USImportEntryLineFee : AutoUSImportEntryLineFee, IFee
	{
		public USImportEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SuspendValidation();
		}

		ZString IFee.Code { get => USF_ChargeType; set => throw new NotSupportedException(); }
		ZDecimal IFee.Amount { get => USF_ChargeAmount; set => throw new NotSupportedException(); }
		ZString IFee.SelectedRateType { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		ZBool IFee.IsOverridden => throw new NotSupportedException();

		public override void Delete()
		{
			throw new NotSupportedException("Cannot delete a view data");
		}
	}
}
