using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.Testing
{
	public abstract class PGADataCorrectionlTest<T> : PGADataCorrectionlCoreTest<T>
		where T : BusinessObject, IPGADataCorrection
	{
		protected override T GetPGAInDiffFactory(BusinessObjectFactory factory, T pga)
		{
			return (T)factory.Load(pga.GetType(), pga.PK);
		}
	}
}
