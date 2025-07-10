using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class DefaultCusSCAOceanBill : BaseCusSCAOceanBill
	{
		public DefaultCusSCAOceanBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString[] ApplicationCodesForBase => new ZString[] { ZString.Empty };

		protected override DocManagerInfo GetDocManagerInfo()
		{
			return new DocManagerInfo(this, Core.Constants.DocManagerCodes.SCAOceanBill);
		}
	}
}
