using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ImportStatQtyValidator
	{
		public void Validate(IDutyData invoiceLine, ZPropertyInfo qtyInfo, ZString uQ, Func<bool> checkIfQtyIsRequired, char qtyCode)
		{
			Validate(invoiceLine, qtyInfo, uQ, checkIfQtyIsRequired, qtyCode, NotificationType.Warning);
		}

		public void Validate(IDutyData invoiceLine, ZPropertyInfo qtyInfo, ZString uQ, Func<bool> checkIfQtyIsRequired, char qtyCode, CargoWise.ComponentModel.INotificationType notificationType)
		{
			if (checkIfQtyIsRequired())
			{
				new ValueQuantityBoundValidator().ValidateQuantityLowerAndUpperBound(qtyInfo, qtyCode, invoiceLine);
			}

			if (IsQuantityRequired(uQ))
			{
				if (qtyInfo.Value.IsEmpty)
				{
					qtyInfo.AddNotification(notificationType, notificationType == NotificationType.Warning ? ValidationConstants.WarnIfStatQTYisZero : ValidationConstants.StatQTYRequired);
				}
			}
		}

		bool IsQuantityRequired(ZString uq)
		{
			return !uq.IsEmpty && uq != ABIUnitOfMeasureList.Codes.NoUnitRequired;
		}
	}
}
