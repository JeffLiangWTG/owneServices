//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSealValidation
//
//    This class should be used for overriding validation in AutoCusSealValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using CargoWise.EntityFramework;

	public class CusSealValidation : AutoCusSealValidation
	{
		public CusSealValidation(AutoCusSeal parent) : base(parent)
		{
		}

		protected override void CheckBK_SequenceNumber()
		{
			base.CheckBK_SequenceNumber();
			MandatoryValidation.CheckNotNegative(Parent.BK_SequenceNumberInfo);
		}

		protected override void CheckBK_SealNumber()
		{
			base.CheckBK_SealNumber();
			MandatoryValidation.CheckEntered(Parent.BK_SealNumberInfo);
		}

		protected override void CheckBK_UnloadingState()
		{
			base.CheckBK_UnloadingState();
			ListValidation.MessageErrorIfInvalidCode(Parent.BK_UnloadingStateInfo);
		}
	}
}
