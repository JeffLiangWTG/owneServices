using System;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class MessageCodeColumnStyle : ZTextBoxColumnStyle, IDynamicToolTipWithRowInfo
	{
		public MessageCodeColumnStyle(MessageCodeColumnStyleInfo info)
			: base(info)
		{
		}

		#region RowNumber

		public int RowNumber
		{
			get { return fRowNumber; }
			set { fRowNumber = value; }
		}

		protected int fRowNumber;

		#endregion

		#region IToolTipRequest Members

		public event QueryToolTipEventHandler QueryToolTip;

		public virtual void GetToolTip(ToolTipInfo info)
		{
			var newToolTip = info.ToolTipText;

			if (RowNumber != -1)
			{
				var objectValue = GetColumnValueAtRow(parentZGrid.ListManager, RowNumber);
				ZString description = (objectValue is string || objectValue is ZString) ? new ZString(objectValue) : ZString.Empty;

				if (!description.IsEmpty)
				{
					newToolTip = description + System.Environment.NewLine + newToolTip;
				}

				info.ToolTipText = newToolTip;
			}

			if (QueryToolTip != null)
			{
				QueryToolTip(this, info);
			}
		}

		#endregion
	}

	public class MessageCodeColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(MessageCodeColumnStyle); }
		}
	}
}
