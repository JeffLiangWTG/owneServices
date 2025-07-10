using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	[XmlRoot(ElementName = "TransportReferenceNumberTypes")]
	public class TransportReferenceNumberTypeCollection : RegistryBusinessObjectCollection
	{
		#region Overrides

		public new TransportReferenceNumberType this[int index]
		{
			get { return (TransportReferenceNumberType)Elements[index]; }
		}

		public new TransportReferenceNumberType AddNew()
		{
			return (TransportReferenceNumberType)base.AddNew();
		}

		public TransportReferenceNumberType Add(ZString code, MultilingualString description, bool isUnique = true)
		{
			var result = AddNew();
			result.Code = code;
			result.Description = description;
			result.IsUnique = isUnique;
			return result;
		}

		public TransportReferenceNumberType AddSystemDefined(ZString code, MultilingualString description, bool isUnique = true)
		{
			var result = Add(code, description, isUnique);
			result.SystemDefined = true;
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportReferenceNumberTypeCollection();
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			TransportReferenceNumberType transportReferenceNumberType = businessObject as TransportReferenceNumberType;

			if (transportReferenceNumberType != null)
			{
				transportReferenceNumberType.Parent = this;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransportReferenceNumberType(this);
		}

		protected override BusinessObject AddNewCore()
		{
			TransportReferenceNumberType transportReferenceNumberType = (TransportReferenceNumberType)base.AddNewCore();
			transportReferenceNumberType.Parent = this;
			return transportReferenceNumberType;
		}

		#endregion
	}
}
