using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitLogColumn<ColumnEnum> where ColumnEnum : struct
	{
		public TransitLogColumn(ZString header, ZString[] values, ColumnEnum columnID)
		{
			Header = header;
			Values = values ?? Array.Empty<ZString>();
			ColumnID = columnID;
		}

		public int Width { get => width; set => width = value; }
		public ZString Header { get => header; set => header = value; }
		public ZString[] Values { get => values; set => values = value; }
		public ColumnEnum ColumnID { get; set; }

		int width;
		ZString header;
		ZString[] values;
	}
}
