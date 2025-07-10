namespace Enterprise.Customs.Business
{
	public interface ICustomsUnitDefaultingStrategy
	{
		void Initialise(BaseJobComInvoiceLine invoiceLine);
		void Deinitialise(BaseJobComInvoiceLine invoiceLine);
		void DefaultUOMs(BaseJobComInvoiceLine invoiceLine);
	}

	public interface ICustomsUnitDefaultingStrategy<TJobComInvoiceLine> : ICustomsUnitDefaultingStrategy
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		void Initialise(TJobComInvoiceLine invoiceLine);
		void Deinitialise(TJobComInvoiceLine invoiceLine);
		void DefaultUOMs(TJobComInvoiceLine invoiceLine);
	}
}
