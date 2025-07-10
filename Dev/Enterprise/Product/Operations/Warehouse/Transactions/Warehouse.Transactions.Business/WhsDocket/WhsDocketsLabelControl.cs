using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketsLabelControl : NonPersistentBusinessObject
	{
		#region Constructors

		public WhsDocketsLabelControl(WhsDocketLabelLineCollection lines)
		{
			this.lines = lines;
		}

		#endregion

		#region Related Business Objects

		public WhsDocketLabelLineCollection Lines
		{
			get { return lines; }
		}
		readonly WhsDocketLabelLineCollection lines;

		#endregion
	}
}
