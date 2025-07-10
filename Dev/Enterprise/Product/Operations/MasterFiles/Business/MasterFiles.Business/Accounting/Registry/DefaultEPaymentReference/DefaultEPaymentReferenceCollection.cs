using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultEPaymentReferenceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultEPaymentReferenceCollection() : base()
		{
		}

		public DefaultEPaymentReferenceCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new DefaultEPaymentReference this[int x]
		{
			get { return (DefaultEPaymentReference)base[x]; }
		}

		public new DefaultEPaymentReference AddNew()
		{
			return (DefaultEPaymentReference)base.AddNew();
		}

		/*
		 * This function suspends the validation and then adds the default provider code(OFX) and the default reference types(INV: Invoice number)
		 * In the end the validatin is done.
		 * */
		internal void PopulateDefaultPaymentReferenceForAllProviders()
		{
			SuspendValidation();
			var defaultReferenceForOFX = AddNew();
			defaultReferenceForOFX.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			defaultReferenceForOFX.ReferenceType = EPaymentReferenceTypes.InvoiceNumbers;
			ResumeValidation();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DefaultEPaymentReference(CurrentFallbackLevel);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DefaultEPaymentReferenceCollection(fallbackLevel);
	}
}
