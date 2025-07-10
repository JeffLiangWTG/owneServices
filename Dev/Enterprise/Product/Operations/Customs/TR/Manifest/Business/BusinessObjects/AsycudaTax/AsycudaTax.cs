using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaTax : ASYCUDA.Business.AsycudaTax
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.AET_MethodOfCalculation = "%";
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted && AET_ChargeAmount == 0)
			{
				this.Delete();
			}
		}
	}
}
