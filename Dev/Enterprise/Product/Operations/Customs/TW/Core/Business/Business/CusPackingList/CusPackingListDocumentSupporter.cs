using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackingListDocumentSupporter(CusPackingList cusPackingList) : Customs.Business.CusPackingListDocumentSupporter(cusPackingList)
	{
		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappers();

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun) => GetDocumentWrappers();

		DocumentWrapper[] GetDocumentWrappers()
		{
			if (PackingList.Declaration != null)
			{
				return [new DocCusPackingList((CusPackingList)PackingList)];
			}
			else
			{
				return [InvoiceDocCusPackingList.New((CusPackingList)PackingList, PackingList.Factory)];
			}
		}
	}
}
