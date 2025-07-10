//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusTWProductLabelRangeValidation
//
//    This class should be used for overriding validation in AutoCusTWProductLabelRangeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWProductLabelRangeValidation : AutoCusTWProductLabelRangeValidation
	{
		public CusTWProductLabelRangeValidation(AutoCusTWProductLabelRange parent) : base(parent)
		{
		}

		protected override void CheckTW0_Status()
		{
			base.CheckTW0_Status();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TW0_StatusInfo, Parent.Lookups.ProductLabelRangeStatusList);
		}

		protected override void CheckTW0_EndNumber()
		{
			base.CheckTW0_EndNumber();

			var parent = Parent;
			var endNumber = parent.TW0_EndNumber;
			if (!endNumber.IsEmpty)
			{
				var targetInfo = parent.TW0_EndNumberInfo;
				CheckNumberMustBeEightCharacters(endNumber, targetInfo);
				CheckAlphanumericCharactersOnly(endNumber, targetInfo);
			}
		}

		protected override void CheckTW0_StartNumber()
		{
			base.CheckTW0_StartNumber();

			var parent = Parent;
			var startNumber = parent.TW0_StartNumber;
			if (!startNumber.IsEmpty)
			{
				var targetInfo = parent.TW0_StartNumberInfo;
				CheckNumberMustBeEightCharacters(startNumber, targetInfo);
				CheckAlphanumericCharactersOnly(startNumber, targetInfo);
			}
		}

		protected override void CheckTW0_RunNumber()
		{
			base.CheckTW0_RunNumber();

			var parent = Parent;
			var runNumber = parent.TW0_RunNumber;
			if (!runNumber.IsEmpty)
			{
				var targetInfo = parent.TW0_RunNumberInfo;
				CheckAlphanumericCharactersOnly(runNumber, targetInfo);
			}
		}

		protected override void CheckTW0_Year()
		{
			base.CheckTW0_Year();

			var parent = Parent;
			var year = parent.TW0_Year;
			if (!year.IsEmpty)
			{
				var targetInfo = parent.TW0_YearInfo;
				CheckAlphanumericCharactersOnly(year, targetInfo);
			}
		}

		void CheckNumberMustBeEightCharacters(ZString value, ZPropertyInfo targetInfo)
		{
			if (value.Length != 8)
			{
				targetInfo.AddMessageError(ValidationConstants.ProductLabelRange.NumberMustBeEightCharacters(targetInfo.HumanReadableName));
			}
		}

		void CheckAlphanumericCharactersOnly(ZString value, ZPropertyInfo targetInfo)
		{
			if (!value.IsLettersAndNumbersOnlyOrEmpty)
			{
				targetInfo.AddMessageError(ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			}
		}

		protected new CusTWProductLabelRange Parent => (CusTWProductLabelRange)base.Parent;
	}
}
