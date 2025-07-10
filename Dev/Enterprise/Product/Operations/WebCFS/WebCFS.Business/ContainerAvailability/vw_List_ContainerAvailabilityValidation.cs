//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_List_ContainerAvailabilityValidation
//
//    This class should be used for overriding validation in Autovw_List_ContainerAvailabilityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.WebCFS.Business
{
	public class vw_List_ContainerAvailabilityValidation : Autovw_List_ContainerAvailabilityValidation
	{
		public vw_List_ContainerAvailabilityValidation(Autovw_List_ContainerAvailability parent) : base(parent)
		{
		}
	}
}
