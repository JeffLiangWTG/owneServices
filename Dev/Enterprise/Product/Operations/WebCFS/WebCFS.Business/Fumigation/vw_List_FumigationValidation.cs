//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_List_FumigationValidation
//
//    This class should be used for overriding validation in Autovw_List_FumigationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.WebCFS.Business
{
	public class vw_List_FumigationValidation : Autovw_List_FumigationValidation
	{
		public vw_List_FumigationValidation(Autovw_List_Fumigation parent) : base(parent)
		{
		}
	}
}
