using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EPaymentReasonCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EPaymentReasonCollection() : base()
		{
		}

		public new EPaymentReason this[int x]
		{
			get { return (EPaymentReason)base[x]; }
		}

		public new EPaymentReason AddNew()
		{
			return (EPaymentReason)base.AddNew();
		}

		internal virtual void PopulatePaymentReasonsForAllProviders()
		{
			SuspendValidation();
			AddRange(EPaymentReasonCodes.GetPaymentReasonsForAllProviders());
			ResumeValidation();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EPaymentReason();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EPaymentReasonCollection();
		}
	}
}
