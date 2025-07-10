namespace Enterprise.Customs.US.Business
{
	public interface IInvoiceHeader : Customs.Business.IBaseInvoiceHeader
	{
		IDeclaration Declaration { get; }
	}
}
