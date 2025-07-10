using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusDV1DetailValidation : EU.Business.Declaration.CusDV1DetailValidation
	{
		public CusDV1DetailValidation(AutoCusDV1Detail parent) : base(parent)
		{
		}

		public new CusDV1Detail Parent => (CusDV1Detail)base.Parent;

		protected override void CheckDV1_ContractNumber()
		{
			var query = new ZQuery();
			query.AddToFilter(CusDV1DetailSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(CusDV1DetailSchema.DV1_JE, SQLComparisonOperator.Equal, Parent.DV1_JE);
			query.AddToFilter(CusDV1DetailSchema.DV1_ClusterKey, SQLComparisonOperator.Equal, Parent.DV1_ClusterKey);
			var dev1DetailList = Parent.Factory.LoadTop1<CusDV1Detail>(query);
			if (dev1DetailList != null)
			{
				Parent.DV1_ContractNumberInfo.AddMessageError(Res.GetString("0020EBE5-8F8F-4461-A149-222894A1427F", "Only one D.V.1 Details is allowed."));
			}
		}
	}
}
