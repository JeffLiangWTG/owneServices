using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	[TestClass]
	sealed class OrgFlattenedDataTransferProcessorForTest : OrgFlattenedDataTransferProcessor
	{
		public OrgFlattenedDataTransferProcessorForTest(ImportCollectionInfoImplForOrgFlattened importCollectionInfo, Dictionary<OrgFlattened, OrgHeader> orgsToLinkDict)
			: base(importCollectionInfo, orgsToLinkDict) { }

		public OrgHeader CreateHeaderForTest(OrgFlattened orgFlattened)
		{
			return base.CreateHeader(headerCollection, orgFlattened);
		}
	}
}
