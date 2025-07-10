//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDeliveryOrderHazmatAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDeliveryOrderHazmatAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USDeliveryOrderHazmatAddInfoValidation : AutoUSDeliveryOrderHazmatAddInfoValidation
	{
		public USDeliveryOrderHazmatAddInfoValidation(AutoUSDeliveryOrderHazmatAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_UNNumber()
		{
			base.CheckUS_UNNumber();
			MandatoryValidation.CheckEntered(Parent.US_UNNumberInfo);
			ListValidation.WarnIfInvalidCode(Parent.US_UNNumberInfo, Parent.Lookups.UNDGSubstances, (NoResString)UNNumberShouldBeInList);
		}
		internal const string UNNumberShouldBeInList = "Please enter a valid UN Number. The number you have selected is not in the UN Number List. ";
	}
}
