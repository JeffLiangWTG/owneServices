using Enterprise.Billing.Integration;
namespace Enterprise.MasterFiles.Integration
{
	public interface IXmlDataTransferImporter
	{
		void PromptUserAndImport(BillingInterfaceName interfaceName);
	}
}
