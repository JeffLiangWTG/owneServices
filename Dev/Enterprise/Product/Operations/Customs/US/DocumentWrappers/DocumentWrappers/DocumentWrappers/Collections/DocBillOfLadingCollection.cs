using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocBillOfLadingCollection : Enterprise.DocumentWrappers.Customs.Base.DocBaseBillOfLadingCollection
	{
		public DocBillOfLadingCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void AddNewBill(Customs.Business.Bill bill)
		{
			Add(new DocBillOfLading((Bill)bill));
		}
	}
}
