using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address
{
	public sealed class AddressBusinessObjectConfiguration : NonPersistentBusinessObjectCollection<AddressBusinessObject>
	{
		public string CertificateName { get; private set; }
		public bool EnableUnknown { get; private set; }

		public AddressBusinessObjectConfiguration(string certificateName, bool enableUnknown)
		{
			CertificateName = certificateName;
			EnableUnknown = enableUnknown;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AddressBusinessObject();
	}
}
