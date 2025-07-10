using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusPermitHeader : BaseCusPermitHeader, Integration.Customs.US.ICusPermitHeader
	{
		public CusPermitHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Override Properties

		protected override ZBool AllowNewLineTransactions => IsCUM;

		#endregion

		#region Business Object Overrides

		protected override Customs.Business.CusPermitHeaderValidation GetNewValidation()
		{
			return new CusPermitHeaderValidation(this);
		}

		#endregion
	}
}
