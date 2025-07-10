using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsReceive), "References")]
	public sealed class WhsDocketReference : AutoWhsDocketReference
	{
		public WhsDocketReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override void OnElementChanged()
		{
			base.OnElementChanged();
			if (Docket != null)
			{
				Docket.RefreshProxyProperties();
			}
		}

		#endregion

		#region Related Business Objects

		public WhsDocket Docket
		{
			get { return Factory.Load<WhsDocket>(WX_WD); }
		}

		#endregion

		#region Properties

		[List("Lookups.ReferenceTypes")]
		public override ZString WX_RefType
		{
			get { return base.WX_RefType; }
			set { base.WX_RefType = value; }
		}

		#endregion
	}
}
