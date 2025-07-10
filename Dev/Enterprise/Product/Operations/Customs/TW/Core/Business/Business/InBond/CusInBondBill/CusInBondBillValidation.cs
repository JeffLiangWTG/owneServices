using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondBillValidation : Customs.Business.CusInBondBillValidation
	{
		public CusInBondBillValidation(CusInBondBill parent)
			: base(parent)
		{
		}

		public new CusInBondBill Parent => (CusInBondBill)base.Parent;

		protected bool IsTransportModeAir => Parent.Header?.IsTransportModeAir ?? ZBool.False;

		protected bool IsTransportModeSea => Parent.Header?.IsTransportModeSea ?? ZBool.False;

		string GetWarningMessage(ZString billValue)
		{
			return new AirWayBillValidator().GetWarningMessage(billValue);
		}

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();

			var parent = Parent;
			if (parent.IsAirForMasterBill)
			{
				var masterBillNumber = parent.B0_MasterBillNumber;
				if (!masterBillNumber.IsEmpty)
				{
					var targetInfo = parent.B0_MasterBillNumberInfo;
					var message = GetWarningMessage(masterBillNumber);
					if (!string.IsNullOrEmpty(message))
					{
						targetInfo.AddMessageError(message);
					}

					CheckAirlinePrefix(targetInfo, masterBillNumber, parent.VoyageFlightNo);
				}
			}
		}

		void CheckAirlinePrefix(ZPropertyInfo targetInfo, ZString billValue, ZString flightNo)
		{
			if (flightNo.Length >= 2 && billValue.Length >= 3)
			{
				var airlinePrefix = flightNo.ToUpper().Left(2);
				var airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, airlinePrefix);
				if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != billValue.Left(3))
				{
					targetInfo.AddWarning(BillValidator.AirlinePrefixValidationMessage);
				}
			}
		}

		protected override void CheckB0_HouseBillNumber()
		{
			base.CheckB0_HouseBillNumber();

			var houseBillNumber = Parent.B0_HouseBillNumber;
			if (!houseBillNumber.IsEmpty && !houseBillNumber.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.B0_HouseBillNumberInfo.AddMessageError(ValidationConstants.MessageErrorIfNonAlphanumericCharacters);
			}
		}
	}
}
