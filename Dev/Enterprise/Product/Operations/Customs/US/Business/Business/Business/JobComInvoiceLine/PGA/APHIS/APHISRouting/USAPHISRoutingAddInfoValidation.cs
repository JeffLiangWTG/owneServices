//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISRoutingAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISRoutingAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USAPHISRoutingAddInfoValidation : AutoUSAPHISRoutingAddInfoValidation
	{
		public USAPHISRoutingAddInfoValidation(AutoUSAPHISRoutingAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Type()
		{
			base.CheckUS_Type();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TypeInfo, Parent.Lookups.RoutingTypeList);
			}
		}

		protected override void CheckUS_Country()
		{
			base.CheckUS_Country();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CountryInfo, Parent.Lookups.CountryList);
			}
		}

		protected new USAPHISRoutingAddInfo Parent
		{
			get { return (USAPHISRoutingAddInfo)base.Parent; }
		}

		protected APHISRouting Routing
		{
			get { return Parent.Parent; }
		}

		protected APHISHeader Header
		{
			get
			{
				var routing = Routing;
				return routing == null ? null : routing.Header;
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null)
				{
					var invoiceLine = aphisHeader.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
