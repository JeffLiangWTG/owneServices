// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Globalization;

// Excerpt from DataGrid.cs

namespace System.Windows.Forms
{
	public partial class DataGrid
	{
		/// <summary>
		///  Contains information
		///  about the part of the <see cref='DataGrid'/> control the user
		///  has clicked. This class cannot be inherited.
		/// </summary>
		public sealed class HitTestInfo
		{
			public HitTestType type = HitTestType.None;

			public int row;
			public int col;

			/// <summary>
			///  Allows the <see cref='HitTestInfo'/> object to inform you the
			///  extent of the grid.
			/// </summary>
			public static readonly HitTestInfo Nowhere = new HitTestInfo();

			public HitTestInfo()
			{
				type = (HitTestType)0;
				row = col = -1;
			}

			internal HitTestInfo(HitTestType type)
			{
				this.type = type;
				row = col = -1;
			}

			/// <summary>
			///  Gets the number of the clicked column.
			/// </summary>
			public int Column
			{
				get
				{
					return col;
				}
			}

			/// <summary>
			///  Gets the
			///  number of the clicked row.
			/// </summary>
			public int Row
			{
				get
				{
					return row;
				}
			}

			/// <summary>
			///  Gets the part of the <see cref='DataGrid'/> control, other than the row or column, that was
			///  clicked.
			/// </summary>
			public HitTestType Type
			{
				get
				{
					return type;
				}
			}

			/// <summary>
			///  Indicates whether two objects are identical.
			/// </summary>
			public override bool Equals(object value)
			{
				if (value is HitTestInfo ci)
				{
					return (type == ci.type &&
						   row == ci.row &&
						   col == ci.col);
				}
				return false;
			}

			/// <summary>
			///  Gets the hash code for the <see cref='HitTestInfo'/> instance.
			/// </summary>
			public override int GetHashCode() => HashCode.Combine(type, row, col);

			/// <summary>
			///  Gets the type, row number, and column number.
			/// </summary>
			public override string ToString()
			{
				return "{ " + ((type).ToString()) + "," + row.ToString(CultureInfo.InvariantCulture) + "," + col.ToString(CultureInfo.InvariantCulture) + "}";
			}
		}

		/// <summary>
		///  Specifies the part of the <see cref='DataGrid'/>
		///  control the user has clicked.<
		/// </summary>
		[Flags]
		public enum HitTestType
		{
			None = 0x00000000,
			Cell = 0x00000001,
			ColumnHeader = 0x00000002,
			RowHeader = 0x00000004,
			ColumnResize = 0x00000008,
			RowResize = 0x00000010,
			Caption = 0x00000020,
			ParentRows = 0x00000040
		}

	}
}
