using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusReconEntryLineCollection : ActiveBusinessObjectCollection<CusReconEntryLine>
	{
		public CusReconEntryLineCollection(CusReconEntry master)
			: base(master.Factory, master, new ZQuery(), CusReconEntryLineSchema.CRL_CRE)
		{
		}

		protected override bool AllowNew => false;
	}
}
