using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseEventCollection : ActiveBusinessObjectCollection<USCACCaseEvent>
	{
		public USCACCaseEventCollection(USCACCase parentCase)
			: base(parentCase.Factory, new ZQuery(USCACCaseEventSchema.U7_CaseNumber, parentCase.U5_CaseNumber))
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

		protected override void SetRelationshipDefaultsForElementCore(USCACCaseEvent newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.U7_CaseNumber = parentCase.U5_CaseNumber;
		}
	}
}
