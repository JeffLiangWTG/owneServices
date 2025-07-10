using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseLiqSuspensionCollection : ActiveBusinessObjectCollection<USCACCaseLiqSuspension>
	{
		public USCACCaseLiqSuspensionCollection(USCACCase parentCase)
			: base(parentCase.Factory, new ZQuery(USCACCaseLiqSuspensionSchema.UN_CaseNumber, parentCase.U5_CaseNumber))
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

		protected override void SetRelationshipDefaultsForElementCore(USCACCaseLiqSuspension newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.UN_CaseNumber = parentCase.U5_CaseNumber;
		}
	}
}
