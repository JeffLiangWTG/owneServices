using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingSupplierPart))]
	sealed class TrackingSupplierPartIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return new TrackingSupplierPart(Factory, Factory.New<OrgSupplierPart>());
		}

		new TrackingSupplierPart BizObj
		{
			get { return base.BizObj as TrackingSupplierPart; }
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return System.Array.Empty<ZGuid>(); }
		}
	}
}
