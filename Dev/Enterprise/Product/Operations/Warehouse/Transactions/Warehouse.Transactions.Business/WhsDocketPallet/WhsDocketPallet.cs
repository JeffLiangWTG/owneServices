using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsReceive), "Pallets")]
	public sealed class WhsDocketPallet : AutoWhsDocketPallet
	{
		public WhsDocketPallet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			W2_PalletType = CodeLists.PalletType.Codes.Chep;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Related Business Objects

		public WhsDocket Docket
		{
			get { return Factory.Load<WhsDocket>(W2_WD); }
		}

		#endregion

		#region Properties

		[List("Lookups.PalletTypes")]
		public override ZString W2_PalletType
		{
			get { return base.W2_PalletType; }
			set { base.W2_PalletType = value; }
		}

		#endregion
	}
}
