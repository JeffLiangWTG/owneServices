using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondThatOverridesGetCanDoOutturn : CusUnderbond
	{
		public CusUnderbondThatOverridesGetCanDoOutturn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool CanDoOutturnResult;
		protected override bool GetCanDoOutturn()
		{
			return CanDoOutturnResult;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = "T3T";
		}
	}
}
