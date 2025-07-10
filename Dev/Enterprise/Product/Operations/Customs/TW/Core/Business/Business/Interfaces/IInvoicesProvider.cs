namespace Enterprise.Customs.TW.Business
{
	public interface IInvoicesProvider : Customs.Business.IInvoicesProvider
	{
		bool IsImport { get; }
	}
}
