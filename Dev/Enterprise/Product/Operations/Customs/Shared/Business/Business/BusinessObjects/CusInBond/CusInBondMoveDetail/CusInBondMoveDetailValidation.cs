//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondMoveDetailValidation
//
//    This class should be used for overriding validation in AutoCusInBondMoveDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveDetailValidation : AutoCusInBondMoveDetailValidation
	{
		public CusInBondMoveDetailValidation(AutoCusInBondMoveDetail parent)
			: base(parent)
		{
		}

		protected virtual bool IsBillNumberMandatory { get; } = true;

		protected override void CheckB9_B0()
		{
			base.CheckB9_B0();
			if (IsBillNumberMandatory)
			{
				if (Parent.B9_B0.IsEmpty)
				{
					Parent.B9_B0Info.AddError(Res.GetString("5CDBB789-ED6D-446B-8F78-37B5B84DCB64", "Bill number cannot be empty."));
				}
			}
		}
	}
}
