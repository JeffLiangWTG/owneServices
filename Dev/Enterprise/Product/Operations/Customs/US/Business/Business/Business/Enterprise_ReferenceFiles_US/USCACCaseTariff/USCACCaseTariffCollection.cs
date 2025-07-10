using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseTariffCollection : ActiveBusinessObjectCollection<USCACCaseTariff>
	{
		public USCACCaseTariffCollection(USCACCase parentCase)
			: base(parentCase.Factory, new ZQuery(USCACCaseTariffSchema.U9_CaseNumber, parentCase.U5_CaseNumber))
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

		protected override void SetRelationshipDefaultsForElementCore(USCACCaseTariff newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.U9_CaseNumber = parentCase.U5_CaseNumber;
		}
	}
}
