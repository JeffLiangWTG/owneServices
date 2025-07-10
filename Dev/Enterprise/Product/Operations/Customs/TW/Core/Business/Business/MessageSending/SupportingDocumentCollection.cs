using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class SupportingDocumentCollection : NonPersistentBusinessObjectCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObjectFactory factory)
			: this(factory, null, null)
		{
		}

		public SupportingDocumentCollection(BusinessObjectFactory factory, CodeDescriptionPairList storageDocs, IStorageDocsBaseCollection[] eDocs)
			: this(factory, storageDocs, eDocs, null)
		{
		}

		public SupportingDocumentCollection(BusinessObjectFactory factory, CodeDescriptionPairList storageDocs, IStorageDocsBaseCollection[] eDocs, CodeDescriptionPairList typeList, CodeDescriptionPairList billNumberList = null) : base(factory)
		{
			this.storageDocs = storageDocs;
			this.TypeList = typeList;
			this.BillNumberList = billNumberList;
			docs = eDocs;
			MaxCountValidationEnable(10);
		}

		protected CodeDescriptionPairList TypeList { get; }
		protected CodeDescriptionPairList BillNumberList { get; }
		readonly IStorageDocsBaseCollection[] docs;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupportingDocument(Factory, TypeList, BillNumberList);
		}

		#region StorageDocs

		public CodeDescriptionPairList StorageDocs => storageDocs ?? (storageDocs = new CodeDescriptionPairList());
		CodeDescriptionPairList storageDocs;

		public IStorageDocsBaseCollection[] GetDocs() => docs;

		#endregion
	}
}
