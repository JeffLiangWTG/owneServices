//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoContainerLoadListHeaderValidation
//
//    This class should be used for overriding validation in AutoContainerLoadListHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListHeaderValidation : AutoContainerLoadListHeaderValidation
	{
		public ContainerLoadListHeaderValidation(AutoContainerLoadListHeader parent) : base(parent)
		{
		}
	}
}
