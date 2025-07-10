using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class DummyBusinessObjectWithUploadSupport : EnterpriseBusinessObject, IWebDocumentsWithUploadSupport
	{
		public DummyBusinessObjectWithUploadSupport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentUploadSupport DocumentUploadHelper => documentUploadHelper ?? (documentUploadHelper = new DummyDocumentUploadSupport(Factory));

		DummyDocumentUploadSupport documentUploadHelper;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, "DOC"));

		DocManagerInfo docManagerInfo;

		public OrgContact LoggedInContact => throw new NotImplementedException();

		public DocumentSupport DocumentHelper => new DocumentSupport(this);

		public ZGuid DocParentPK => throw new NotImplementedException();

		public List<ZGuid> DocRelatedPKs => throw new NotImplementedException();

		public override SchemaGuidColumn PKSchemaColumn => DummyBizoSchema.PK;

		public void ResetDocumentHelper()
		{
		}

		public abstract class Schema
		{
			public const string TableName = "DummyBizo";
		}
	}
}
