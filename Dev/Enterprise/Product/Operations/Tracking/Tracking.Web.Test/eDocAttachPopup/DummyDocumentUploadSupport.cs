using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class DummyDocumentUploadSupport : DocumentUploadSupport
	{
		public DummyDocumentUploadSupport(BusinessObjectFactory factory)
		: base(factory)
		{
		}

		protected override RefDocTypeCollection GetDocTypes() => docTypes ?? (docTypes = new RefDocTypeCollection(Factory));

		RefDocTypeCollection docTypes;
	}
}
