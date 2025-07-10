using System.Drawing;
using System.Text;

// Extract from DataGrid.cs

namespace System.Windows.Forms
{
	public partial class DataGrid
	{
		// <summary>
		//      This simple data structure holds all of the layout information
		//      for the DataGrid.
		// </summary>
		internal class LayoutData
		{
			internal bool dirty = true;
			// region inside the Control's borders.
			public Rectangle Inside = Rectangle.Empty;

			public Rectangle RowHeaders = Rectangle.Empty;

			public Rectangle TopLeftHeader = Rectangle.Empty;
			public Rectangle ColumnHeaders = Rectangle.Empty;
			public Rectangle Data = Rectangle.Empty;

			public Rectangle Caption = Rectangle.Empty;
			public Rectangle ParentRows = Rectangle.Empty;

			public Rectangle ResizeBoxRect = Rectangle.Empty;

			public bool ColumnHeadersVisible;
			public bool RowHeadersVisible;
			public bool CaptionVisible;
			public bool ParentRowsVisible;

			// used for resizing.
			public Rectangle ClientRectangle = Rectangle.Empty;

			public LayoutData()
			{
			}

			public LayoutData(LayoutData src)
			{
				GrabLayout(src);
			}

			private void GrabLayout(LayoutData src)
			{
				Inside = src.Inside;
				TopLeftHeader = src.TopLeftHeader;
				ColumnHeaders = src.ColumnHeaders;
				RowHeaders = src.RowHeaders;
				Data = src.Data;
				Caption = src.Caption;
				ParentRows = src.ParentRows;
				ResizeBoxRect = src.ResizeBoxRect;
				ColumnHeadersVisible = src.ColumnHeadersVisible;
				RowHeadersVisible = src.RowHeadersVisible;
				CaptionVisible = src.CaptionVisible;
				ParentRowsVisible = src.ParentRowsVisible;
				ClientRectangle = src.ClientRectangle;
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder(200);
				sb.Append(base.ToString());
				sb.Append(" { \n");
				sb.Append("Inside = ");
				sb.Append(Inside.ToString());
				sb.Append('\n');
				sb.Append("TopLeftHeader = ");
				sb.Append(TopLeftHeader.ToString());
				sb.Append('\n');
				sb.Append("ColumnHeaders = ");
				sb.Append(ColumnHeaders.ToString());
				sb.Append('\n');
				sb.Append("RowHeaders = ");
				sb.Append(RowHeaders.ToString());
				sb.Append('\n');
				sb.Append("Data = ");
				sb.Append(Data.ToString());
				sb.Append('\n');
				sb.Append("Caption = ");
				sb.Append(Caption.ToString());
				sb.Append('\n');
				sb.Append("ParentRows = ");
				sb.Append(ParentRows.ToString());
				sb.Append('\n');
				sb.Append("ResizeBoxRect = ");
				sb.Append(ResizeBoxRect.ToString());
				sb.Append('\n');
				sb.Append("ColumnHeadersVisible = ");
				sb.Append(ColumnHeadersVisible.ToString());
				sb.Append('\n');
				sb.Append("RowHeadersVisible = ");
				sb.Append(RowHeadersVisible.ToString());
				sb.Append('\n');
				sb.Append("CaptionVisible = ");
				sb.Append(CaptionVisible.ToString());
				sb.Append('\n');
				sb.Append("ParentRowsVisible = ");
				sb.Append(ParentRowsVisible.ToString());
				sb.Append('\n');
				sb.Append("ClientRectangle = ");
				sb.Append(ClientRectangle.ToString());
				sb.Append(" } ");
				return sb.ToString();
			}
		}
	}
}
