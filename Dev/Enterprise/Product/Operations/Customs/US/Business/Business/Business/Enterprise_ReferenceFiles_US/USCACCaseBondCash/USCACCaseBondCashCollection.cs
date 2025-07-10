using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseBondCashCollection : ActiveBusinessObjectCollection<USCACCaseBondCash>
	{
		public USCACCaseBondCashCollection(USCACCase parentCase)
			: base(parentCase.Factory, new ZQuery(USCACCaseBondCashSchema.U8_CaseNumber, parentCase.U5_CaseNumber))
		{
			this.parentCase = parentCase;
		}

		readonly USCACCase parentCase;

		internal void MarkForDelete()
		{
			foreach (var element in this)
			{
				element.RemoveOnFactorySaving = true;
			}
		}

		protected override void SetRelationshipDefaultsForElementCore(USCACCaseBondCash newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.U8_CaseNumber = parentCase.U5_CaseNumber;
		}

		public bool IsCashRequired(ZDate effectiveDate)
		{
			USCACCaseBondCash bondCashWithMaxDate = null;
			foreach (USCACCaseBondCash bondCash in this)
			{
				if (bondCash.U8_InactivatedDate.IsEmpty && bondCash.U8_EffectiveDate <= effectiveDate)
				{
					if (bondCashWithMaxDate == null || bondCashWithMaxDate.U8_EffectiveDate < bondCash.U8_EffectiveDate)
					{
						bondCashWithMaxDate = bondCash;
					}
				}
			}
			return bondCashWithMaxDate != null && bondCashWithMaxDate.U8_Indicator == BondCashIndicatorList.Codes.Cash;
		}
	}
}
