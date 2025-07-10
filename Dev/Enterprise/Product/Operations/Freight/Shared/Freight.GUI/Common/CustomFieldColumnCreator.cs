using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class CustomFieldColumnCreator
	{
		public void Set(ZGrid grid, ICustomFieldsDescriptor customFieldsDescriptor, string bindingPath = "", bool showActiveOnly = false, bool isColumnVisible = false)
		{
			Argument.NotNull(grid, "grid");
			Argument.NotNull(customFieldsDescriptor, "customFieldsDescriptor");

			foreach (var customFieldInfo in customFieldsDescriptor.CustomFieldsInfos)
			{
				if (!showActiveOnly || customFieldInfo.IsActive)
				{
					Set(grid, customFieldInfo, bindingPath, isColumnVisible);
				}
			}
		}

		void Set(ZGrid grid, CustomFieldInfo customFieldInfo, ZString bindingPath, bool isColumnVisible)
		{
			var column = GetNewGridColumnForType(customFieldInfo.ZDataType);

			if (column != null)
			{
				column.ColumnName = string.IsNullOrWhiteSpace(bindingPath)
					? customFieldInfo.SchemaColumnName
					: string.Concat(bindingPath, "+", customFieldInfo.SchemaColumnName);
				column.Caption = customFieldInfo.Caption;
				column.ToolTip = customFieldInfo.Hint;
				column.IsVisible = isColumnVisible;
				grid.ColumnStyles.Add(column);

#if DEBUG
				TypeDescriptor.AddAttributes(column, new SuppressFormsLocalizedTestAttribute());
#endif
			}
		}

		ZGridColumnInfo GetNewGridColumnForType(Type zType)
		{
			ZGridColumnInfo result = null;

			if (typeof(ZString).IsAssignableFrom(zType))
			{
				result = new ZTextBoxColumnStyleInfo();
			}
			else if (typeof(ZDecimal).IsAssignableFrom(zType))
			{
				result = new ZCalcEditColumnStyleInfo();
			}
			else if (typeof(ZDateTime).IsAssignableFrom(zType))
			{
				result = new ZDateEditColumnStyleInfo();
			}
			else if (typeof(ZBool).IsAssignableFrom(zType))
			{
				result = new ZCheckBoxColumnStyleInfo();
			}

			return result;
		}
	}
}
