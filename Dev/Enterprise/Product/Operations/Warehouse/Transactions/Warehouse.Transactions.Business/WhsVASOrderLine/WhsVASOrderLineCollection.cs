using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderLineCollection : ActiveBusinessObjectCollection<WhsVASOrderLine>
	{
		public WhsVASOrderLineCollection(WhsVASOrder vasOrder)
			: base(vasOrder.Factory, vasOrder, new ZQuery(), WhsVASOrderLineSchema.WVL_WVO_VASOrder)
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get
			{
				var vasOrder = (WhsVASOrder)Relationship.Master;
				return base.AllowNew && !vasOrder.ReadOnly && vasOrder.WVO_OH_Client.IsValid && vasOrder.WVO_WA_ServiceArea.IsValid;
			}
		}

		#endregion

		#region SetDefaultsForNewElementCore

		protected override void SetDefaultsForNewElementCore(WhsVASOrderLine newLine)
		{
			base.SetDefaultsForNewElementCore(newLine);

			var maxLineNo = (this.Count > 0) ? this.Max(l => l.WVL_LineNumber) : ZInt.Zero;
			newLine.WVL_LineNumber = maxLineNo + 1;
		}

		#endregion
	}
}
