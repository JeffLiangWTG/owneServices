using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class CertificationCodeMappingCollection : RegistryBusinessObjectCollection
	{
		public CertificationCodeMappingCollection()
		{
		}

		public CertificationCodeMappingCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new CertificationCodeMapping this[int i]
		{
			get { return (CertificationCodeMapping)base[i]; }
		}

		public new CertificationCodeMapping AddNew() => (CertificationCodeMapping)base.AddNew();

		public CertificationCodeMapping AddPair(string mainCode, string specialisationCode)
		{
			var mapping = AddNew();
			mapping.MainCode = mainCode;
			mapping.SpecialisationCode = specialisationCode;

			return mapping;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CertificationCodeMapping();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CertificationCodeMappingCollection(fallbackLevel);
		}
	}
}
