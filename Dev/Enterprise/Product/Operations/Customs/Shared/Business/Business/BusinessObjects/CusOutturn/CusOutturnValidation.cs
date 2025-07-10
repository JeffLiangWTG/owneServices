//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusOutturnValidation
//
//    This class should be used for overriding validation in AutoCusOutturnValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusOutturnValidation : AutoCusOutturnValidation
	{
		public CusOutturnValidation(AutoCusOutturn parent) : base(parent)
		{
			outturn = (CusOutturn)parent;
		}

		readonly CusOutturn outturn;

		#region ParentStringRepresentation Validation

		public void ValidateParentStringRepresentation()
		{
			ValidateCalculatedProperty(outturn.ParentStringRepresentationInfo);
		}

		protected virtual void CheckParentStringRepresentation()
		{
		}

		#endregion

	}
}
