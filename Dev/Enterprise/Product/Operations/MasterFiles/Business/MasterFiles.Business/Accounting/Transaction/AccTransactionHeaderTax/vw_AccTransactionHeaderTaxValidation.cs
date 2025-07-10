//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_AccTransactionHeaderTaxValidation
//
//    This class should be used for overriding validation in Autovw_AccTransactionHeaderTaxValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class vw_AccTransactionHeaderTaxValidation : Autovw_AccTransactionHeaderTaxValidation
	{
		public vw_AccTransactionHeaderTaxValidation(Autovw_AccTransactionHeaderTax parent) : base(parent)
		{
		}
	}
}
