//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsStocktakeProductFilterValidation
//
//    This class should be used for overriding validation in AutoWhsStocktakeProductFilterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeProductFilterValidation : AutoWhsStocktakeProductFilterValidation
	{
		public WhsStocktakeProductFilterValidation(AutoWhsStocktakeProductFilter parent) : base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> WhsStocktakeProductFilterSchema.Constants.WSP_WS_Stocktake != info.Name && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
