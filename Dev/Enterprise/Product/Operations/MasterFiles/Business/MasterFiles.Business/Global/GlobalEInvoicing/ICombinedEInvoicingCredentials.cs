namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Enables form binding on either Company or Branch for EInvoicing Credentials
	/// </summary>
	public interface ICombinedEInvoicingCredentials
	{
		CombinedEInvoicingCertificateCollection EInvoicingCertificateCredentials { get; }
		CombinedEInvoicingPasswordCollection EInvoicingPasswordCredentials { get; }
	}
}
