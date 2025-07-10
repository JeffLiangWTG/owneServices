using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class CustomLabelControlFactory
	{
		public CustomLabelControlFactory(CustomLabelInfoList customFields)
		{
			this.fCustomFields = customFields;
		}

		public void AddOrRefreshCustomFieldGridColumns(ZGrid grid, string mappingPrefix)
		{
			ClearCustomFieldGridColumns(grid);
			foreach (CustomLabelInfoBase field in fCustomFields)
			{
				var gridColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>()
					.FirstOrDefault(x => x.ColumnName.Equals(mappingPrefix + field.PropertyName));

				var column = NewCustomFieldGridColumn(field, mappingPrefix, gridColumn);

				if (gridColumn != null)
				{
					grid.ColumnStyles.Remove(gridColumn);
				}

				grid.ColumnStyles.Add(column);

				if (grid.Columns.Contains(column.ColumnName))
				{
					grid.RemoveAndDisposeColumn(column.ColumnName);
				}
				// must do this a second time because JO_QtyInvoiced wouldnt be removed for some reason in rare circumstances
				if (grid.Columns.Contains(column.ColumnName))
				{
					grid.RemoveAndDisposeColumn(column.ColumnName);
				}
				grid.AddColumn(column);
			}
			grid.RefreshTableStyles();
		}

		public void ClearCustomFieldGridColumns(ZGrid grid)
		{
			for (int index = grid.Columns.Count - 1; index >= 0; index--)
			{
				ZGridColumn column = grid.Columns[index];
				string columnName = column.ColumnStyle.MappingName;
				CustomLabelInfoBase label = fCustomFields.GetFieldByPropertyName(columnName);
				if (label != null)
				{
					bool isShowByDefault = (label.Styles & CustomLabelStyles.ShowByDefault) != 0;
					if (fCustomFields.Contains(columnName) && !isShowByDefault)
					{
						grid.RemoveAndDisposeColumn(columnName);
					}
				}
			}
		}

		public void ResetToOriginalCustomColumns(ZGrid grid, string mappingPrefix, ArrayList originalGridLayout)
		{
			ClearCustomFieldGridColumns(grid);
			var originalColumnsLayout = originalGridLayout.Cast<ZGridColumnInfo>().ToList();

			foreach (CustomLabelInfoBase field in fCustomFields)
			{
				var gridColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>()
					.FirstOrDefault(x => x.ColumnName.Equals(mappingPrefix + field.PropertyName));

				var column = originalColumnsLayout.FirstOrDefault(o => o.ColumnName.Equals(mappingPrefix + field.PropertyName));

				if (gridColumn != null)
				{
					grid.ColumnStyles.Remove(gridColumn);
				}

				if (column != null)
				{
					grid.ColumnStyles.Add(column);

					if (grid.Columns.Contains(column.ColumnName))
					{
						grid.RemoveAndDisposeColumn(column.ColumnName);
					}
					// keep same as Line 49
					if (grid.Columns.Contains(column.ColumnName))
					{
						grid.RemoveAndDisposeColumn(column.ColumnName);
					}

					grid.AddColumn(column);
				}
			}

			grid.RefreshTableStyles();
		}

		public Control NewCustomFieldControl(CustomLabelInfoBase field)
		{
			Type propertyType = field.PropertyType;
			Control result;
			if (propertyType == typeof(ZString))
			{
				result = new ZTextBox();
				if ((field.Styles & CustomLabelStyles.UpperCase) > 0)
				{
					((ZTextBox)result).CharacterCasing = CharacterCasing.Upper;
				}
				else
				{
					((ZTextBox)result).CharacterCasing = CharacterCasing.Normal;
				}
				ControlDpiScalingHelper.SetWidth(ref result, 100, true);
			}
			else if (propertyType == typeof(ZBool))
			{
				result = new ZCheckBox();
				ControlDpiScalingHelper.SetWidth(ref result, 30, true);
			}
			else if (propertyType == typeof(ZDateTime))
			{
				result = new ZDateEdit();
			}
			else if (propertyType == typeof(ZDecimal))
			{
				result = new ZCalcEdit();
				ControlDpiScalingHelper.SetWidth(ref result, 100, true);
			}
			else if (propertyType == typeof(OrgContact))
			{
				ZDropEdit contactsDropEdit = new ZDropEdit();
				contactsDropEdit.ShowDescriptionBox = false;
				contactsDropEdit.BindToList = "Contacts";
				result = contactsDropEdit;
			}
			else
			{
				throw new Exception("Unsupported custom field property type " + propertyType.FullName);
			}
			if (result is IDynamicToolTip)
			{
				((IDynamicToolTip)result).QueryToolTip += new QueryToolTipEventHandler(field.OnQueryToolTip);
			}
			return result;
		}

		internal string[] GetCustomisedMappingNames(string mappingPrefix, bool getNotCustomised)
		{
			List<string> result = new List<string>();

			foreach (CustomLabelInfoBase field in fCustomFields)
			{
				bool available = (field.Styles & CustomLabelStyles.AvailableByDefault) != 0 || field.IsEnabled;

				if (getNotCustomised != available)
				{
					result.Add(mappingPrefix + field.PropertyName);
				}
			}

			return result.ToArray();
		}

		#region Implementation

		readonly CustomLabelInfoList fCustomFields;

		protected ZGridColumnInfo NewCustomFieldGridColumn(CustomLabelInfoBase field, string mappingPrefix, ZGridColumnInfo column)
		{
			ZGridColumnInfo result;
			if (field is PartCustomLabelInfo && column != null && column.ColumnStyleType == typeof(ZDropEditColumnStyle))
			{
				result = new ZDropEditColumnStyleInfo();
			}
			else if (field.PropertyType == typeof(ZString))
			{
				if ((field.Styles & CustomLabelStyles.MultiLineTextBox) > 0)
				{
					result = new ZMultiLineTextBoxColumnInfo();
				}
				else
				{
					result = new ZTextBoxColumnStyleInfo();
				}
				if ((field.Styles & CustomLabelStyles.UpperCase) > 0)
				{
					result.CharacterCasing = CharacterCasing.Upper;
				}
			}
			else if (field.PropertyType == typeof(ZBool))
			{
				result = new ZCheckBoxColumnStyleInfo();
			}
			else if (field.PropertyType == typeof(ZDateTime))
			{
				result = new ZDateEditColumnStyleInfo();
			}
			else if (field.PropertyType == typeof(ZDecimal))
			{
				result = new ZCalcEditColumnStyleInfo();
			}
			else if (field.PropertyType == typeof(OrgContact))
			{
				result = new ZDropEditColumnStyleInfo();
				((ZDropEditColumnStyleInfo)result).BindToList = "Contacts";
			}
			else
			{
				throw new NotSupportedException("Unsupported custom field property type " + field.PropertyType.FullName);
			}

			result.ColumnName = mappingPrefix + field.PropertyName;
			result.Caption = field.Caption;
			bool available = (field.Styles & CustomLabelStyles.AvailableByDefault) != 0 || field.IsEnabled;
			result.IsUnavailable = !available;
			result.ErrorMessageWhenUnavailable = Res.GetString("11e09fed-9dc1-4ce1-9d57-cb4899c67c9f", @"You have not defined any meaning for this column.
This is customizable in the organization settings for {0}.", fCustomFields.ConfigOrgLocatedAt);
			result.IsVisible = field.IsEnabled;

#if DEBUG
			TypeDescriptor.AddAttributes(result, new SuppressFormsLocalizedTestAttribute());
#endif

			return result;
		}

		#endregion
	}
}
