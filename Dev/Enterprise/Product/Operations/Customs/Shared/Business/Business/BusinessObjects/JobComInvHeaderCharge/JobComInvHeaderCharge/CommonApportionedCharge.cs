using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class CommonApportionedCharge : BaseJobComInvHeaderCharge
	{
		protected CommonApportionedCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ReadOnly = true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J7_IsApportionedCharge = true;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.AllChargeTypeList))]
		public override ZString J7_ChargeType { get => base.J7_ChargeType; set => base.J7_ChargeType = value; }
	}
}
