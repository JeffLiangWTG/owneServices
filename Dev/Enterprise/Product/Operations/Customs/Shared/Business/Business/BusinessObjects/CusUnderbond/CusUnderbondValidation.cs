//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUnderbondValidation
//
//    This class should be used for overriding validation in AutoCusUnderbondValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusUnderbondValidation : AutoCusUnderbondValidation
	{
		public CusUnderbondValidation(AutoCusUnderbond parent)
			: base(parent)
		{
		}

		CusUnderbond Underbond
		{
			get { return (CusUnderbond)Parent; }
		}

		protected override void CheckC4_Outurned()
		{
			base.CheckC4_Outurned();

			foreach (CusOutturn outturn in Underbond.Outturns)
			{
				if (!outturn.C5_ReceiptOnlyIndicator && Underbond.C4_Outurned.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Underbond.C4_OuturnedInfo, Res.GetString("17928a61-e1f9-493f-9942-189fd9a1a182", "Date of Outturn"));
				}
			}
		}
	}
}
