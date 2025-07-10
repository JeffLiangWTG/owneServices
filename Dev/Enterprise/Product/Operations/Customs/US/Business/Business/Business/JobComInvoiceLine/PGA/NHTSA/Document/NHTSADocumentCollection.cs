using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSADocumentCollection : DependentCusAddInfoCollection<NHTSADocument, NHTSAHeader>
	{
		public NHTSADocumentCollection(NHTSAHeader header)
			: base(header, CusAddInfoTypeAttribute.Codes.USNHTSADocument)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		public ZBool HasMultipleDocumentsWithSameType(ZString documentType)
		{
			return !documentType.IsEmpty && this.OfType<NHTSADocument>().Count(x => x.US_NHTDocumentType == documentType) > 1;
		}

		public ZBool HasDocument(ZString documentType)
		{
			return this.OfType<NHTSADocument>().Any(x => x.US_NHTDocumentType == documentType);
		}

		public ZBool HasOrganization(ZString documentOwner)
		{
			return this.OfType<NHTSADocument>().Any(x => x.US_NHTDocumentOwner == documentOwner);
		}
	}
}
