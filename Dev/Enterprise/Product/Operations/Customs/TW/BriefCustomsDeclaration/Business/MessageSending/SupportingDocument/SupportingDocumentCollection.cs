using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class SupportingDocumentCollection : TW.Business.SupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObjectFactory factory, CodeDescriptionPairList storageDocs, IStorageDocsBaseCollection[] eDocs, CodeDescriptionPairList typeList, CodeDescriptionPairList billNumberList = null)
			: base(factory, storageDocs, eDocs, typeList, billNumberList)
		{
		}

		public new SupportingDocument this[int index] => (SupportingDocument)Elements[index];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new SupportingDocument(base.Factory, TypeList, BillNumberList);
	}
}
