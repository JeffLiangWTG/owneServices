using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusReconSnapshotCollection : ActiveBusinessObjectCollection<CusReconSnapshot>
	{
		public CusReconSnapshotCollection(CusReconEntry master)
			: base(master.Factory, master, new ZQuery(), CusReconSnapshotSchema.CRS_CRE_Entry)
		{
		}

		public CusReconSnapshotCollection(CusReconEntryLine master)
			: base(master.Factory, master, new ZQuery(), CusReconSnapshotSchema.CRS_CRL_Line)
		{
		}

		protected override bool AllowNew => false;
	}
}
