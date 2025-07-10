using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodeCollectionForTest : OrgCusCodeCollection
	{
		public OrgCusCodeCollectionForTest(OrgHeader parentOrganisation, BusinessObjectFactory factory)
			: base(parentOrganisation, factory)
		{
		}

		public ISecurityCheckpoint FailingCheckpointExposed
		{
			get
			{
				return base.FailingCheckpoint;
			}
		}

		public bool ExposedCheckpointDeniesDelete(OrgCusCode elementToDelete)
		{
			return base.CheckpointDeniesDelete(elementToDelete);
		}
	}
}
