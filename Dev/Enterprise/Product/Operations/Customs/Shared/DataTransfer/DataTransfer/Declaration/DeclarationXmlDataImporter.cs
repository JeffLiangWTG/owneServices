using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationXmlDataImporter : XmlDataImporter, Integration.Customs.DataTransfer.IDeclarationXmlDataImporter
	{
		public DeclarationXmlDataImporter(DeclarationValueObjectDataAdapter adapter) : base(adapter)
		{
		}

		public DeclarationXmlDataImporter(BusinessObjectFactoryProvider factoryProvider, DeclarationValueObjectDataAdapter adapter)
			: base(factoryProvider, adapter)
		{
		}

		protected new DeclarationValueObjectDataAdapter Adapter
		{
			get { return (DeclarationValueObjectDataAdapter)base.Adapter; }
		}

		protected override XmlValueObjectSerializer GetSerializer()
		{
			return Serializer;
		}

		public bool IsImportedDeclarationRejected
		{
			get { return Serializer.IsDeclarationRejected; }
		}

		public ZString GetDeclarationCreatedUpdatedRejectedMessage()
		{
			return Serializer.GetDeclarationImportStatuses();
		}

		DeclarationXmlValueObjectSerializer Serializer
		{
			get { return (serializer) ?? (serializer = new DeclarationXmlValueObjectSerializer()); }
		}
		DeclarationXmlValueObjectSerializer serializer;
	}
}
