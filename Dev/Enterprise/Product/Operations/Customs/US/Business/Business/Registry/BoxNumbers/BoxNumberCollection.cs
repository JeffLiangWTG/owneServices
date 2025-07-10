using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	[XmlRoot("BoxNumbers")]
	public class BoxNumberCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new BoxNumber this[int index]
		{
			get { return (BoxNumber)Elements[index]; }
		}

		public new BoxNumber AddNew()
		{
			return (BoxNumber)base.AddNew();
		}

		public bool IsEmpty
		{
			get { return Count == 0; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			BoxNumberCollection result = new BoxNumberCollection();
			result.CompanyPK = CompanyPK;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BoxNumber();
		}

		public ZGuid CompanyPK;
	}
}
