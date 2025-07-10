using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseRateCollection : ActiveBusinessObjectCollection<USCACCaseRate>
	{
		public USCACCaseRateCollection(USCACCase parentCase)
			: base(parentCase.Factory, new ZQuery(USCACCaseRateSchema.U6_CaseNumber, parentCase.U5_CaseNumber))
		{
			this.parentCase = parentCase;
		}

		readonly USCACCase parentCase;

		public USCACCaseRate GetDepositRate(ZDate effectiveValuationDate)
		{
			var ratesOrdered = from USCACCaseRate rate in this
							   orderby rate.U6_EffectiveDate descending, rate.U6_AddedDate descending
							   where rate.U6_InactivatedDate.IsEmpty
							   select rate;

			return ratesOrdered.FirstOrDefault(x => x.U6_EffectiveDate <= effectiveValuationDate);
		}

		internal void MarkForDelete()
		{
			foreach (var element in this)
			{
				element.RemoveOnFactorySaving = true;
			}
		}

		protected override void SetRelationshipDefaultsForElementCore(USCACCaseRate newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.U6_CaseNumber = parentCase.U5_CaseNumber;
		}
	}
}
