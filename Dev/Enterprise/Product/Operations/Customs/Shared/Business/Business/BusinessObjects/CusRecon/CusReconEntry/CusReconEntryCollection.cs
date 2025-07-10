using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusReconEntryCollection : ActiveBusinessObjectCollection<CusReconEntry>
	{
		public CusReconEntryCollection(CusReconDeclaration master)
			: base(master.Factory, master, new ZQuery(), CusReconEntrySchema.CRE_CRD)
		{
		}

		public CusReconEntryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override bool AllowNew => false;
	}
}
