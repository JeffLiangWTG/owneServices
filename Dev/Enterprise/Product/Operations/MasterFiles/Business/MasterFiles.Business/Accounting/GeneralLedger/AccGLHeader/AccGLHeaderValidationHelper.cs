using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderValidationHelper : GLAccountCommonValidationHelper
	{
		public AccGLHeaderValidationHelper(IGLAccount gLAccount, BusinessObjectFactory factory) : base(gLAccount, factory)
		{
		}

		protected override void ValidateUniqueAccountNumber()
		{
			if (!AccountNumInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AccountNum, AccountNumber);
				filter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
				AccGLHeader gLAccount = (AccGLHeader)Factory.LoadTop1(typeof(AccGLHeader), filter);

				if (gLAccount != null)
				{
					AccountNumInfo.AddError(Res.GetString("8b6d45ff-1a43-4f4a-9d1d-ea8162e0cc92", "Account Number must be unique"));
				}
			}
		}
	}
}
