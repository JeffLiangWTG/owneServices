using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RequiredDocToBulkUpdateCollection : NonPersistentBusinessObjectCollection<RequiredDocToBulkUpdate>
	{
		public RequiredDocToBulkUpdateCollection(BusinessObjectFactory factory, DocumentTrackingBulkUpdateBusinessObject parent)
			: base(factory)
		{
			this.Parent = parent;
		}

		public readonly DocumentTrackingBulkUpdateBusinessObject Parent;

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RequiredDocToBulkUpdate(Factory, Parent);
		}

		public RequiredDocToBulkUpdate AddDocToBulkUpdate(ZGuid documentPK, Type parentType)
		{
			JobRequiredDocument selectedDocument = (JobRequiredDocument)Factory.Load(typeof(JobRequiredDocument), documentPK);
			if (parentType != null)
			{
				selectedDocument.ParentType = parentType;
			}
			RequiredDocToBulkUpdate result = AddNew();
			result.SetDocument(selectedDocument);
			return result;
		}
	}
}
