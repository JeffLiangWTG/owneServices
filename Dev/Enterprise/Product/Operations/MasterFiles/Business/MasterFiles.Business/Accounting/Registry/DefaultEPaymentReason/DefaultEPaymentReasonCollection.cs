using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultEPaymentReasonCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultEPaymentReasonCollection() : base()
		{
		}

		public DefaultEPaymentReasonCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new DefaultEPaymentReason this[int x]
		{
			get { return (DefaultEPaymentReason)base[x]; }
		}

		public new DefaultEPaymentReason AddNew()
		{
			return (DefaultEPaymentReason)base.AddNew();
		}

		internal void PopulateDefaultPaymentReasonsForAllProviders()
		{
			SuspendValidation();
			var defaultReasonForOFX = AddNew();
			defaultReasonForOFX.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			defaultReasonForOFX.ReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			ResumeValidation();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DefaultEPaymentReason(CurrentFallbackLevel);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DefaultEPaymentReasonCollection(fallbackLevel);
	}
}
