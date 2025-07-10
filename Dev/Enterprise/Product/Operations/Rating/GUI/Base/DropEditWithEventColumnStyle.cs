using System;
using System.Windows.Forms;

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class DropEditWithEventColumnStyle : ZDropEditColumnStyle
	{
		public DropEditWithEventColumnStyle(DropEditColumnWithEventStyleInfo columnInfo) : base(columnInfo)
		{
		}

		public DropEditWithEventColumnStyle(DropEditColumnWithEventStyleInfo columnInfo, Func<Control> editControl) : base(columnInfo, editControl)
		{
		}

		#region Implementation

		string OldText;
		protected override void ColumnTextBoxChanged(object sender, EventArgs e)
		{
			if (OldText != DropEdit.Text)
			{
				OldText = DropEdit.Text;
				((DropEditColumnWithEventStyleInfo)ColumnInfo).OnColumnTextBoxChanged(sender, e);
			}

			base.ColumnTextBoxChanged(sender, e);
		}

		#endregion
	}

	public class DropEditColumnWithEventStyleInfo : ZDropEditColumnStyleInfo
	{
		public DropEditColumnWithEventStyleInfo() : base()
		{
		}

		public override Type ColumnStyleType
		{
			get { return typeof(DropEditWithEventColumnStyle); }
		}

		public event EventHandler ColumnTextBoxChanged;
		internal void OnColumnTextBoxChanged(object sender, EventArgs e)
		{
			if (ColumnTextBoxChanged != null)
			{
				ColumnTextBoxChanged(sender, e);
			}
		}
	}
}
