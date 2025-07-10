using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public static class ContainerCustomColumnAdder
	{
		public enum TargetGridType
		{
			Editable,
			Module,
		}

		public static void Set(ZGrid grid, TargetGridType targetGridType)
		{
			Set("", grid, targetGridType);
		}

		public static void Set(ZString objName, ZGrid grid, TargetGridType targetGridType)
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Constants.CountryCodes.Australia:
					SetupAustraliaCAN(objName, grid, targetGridType);
					break;
				default:
					break;
			}
		}

		static void SetupAustraliaCAN(ZString objName, ZGrid grid, TargetGridType targetGridType)
		{
			switch (targetGridType)
			{
				case TargetGridType.Editable:
					Setup(new ZDropEditColumnStyleInfo()
						, objName + AgencyShipmentContainer.Schema.CustomsEntryNumberType
						, Res.GetString("0ea7f6b7-c4e3-4f65-801f-8a0b41f1c453", "Entry Type"), Res.GetData("4d7dda1a-6bfc-4efd-a5bb-f1c17a727951", "CAN"), "", CharacterCasing.Upper, true, grid);

					Setup(new ZTextBoxColumnStyleInfo()
						, objName + AgencyShipmentContainer.Schema.CustomsEntryNumber
						, Res.GetString("edca8746-cf55-4aca-9e2c-33ed3a1ef888", "Entry No"), Res.GetData("4d7dda1a-6bfc-4efd-a5bb-f1c17a727951", "CAN"), "", CharacterCasing.Normal, true, grid);

					break;
				case TargetGridType.Module:
					Setup(new ZDropEditColumnStyleInfo()
						, objName + AgencyShipmentContainer.Schema.BillContainersEntryNumberType
						, Res.GetString("0ea7f6b7-c4e3-4f65-801f-8a0b41f1c453", "Entry Type"), Res.GetData("4d7dda1a-6bfc-4efd-a5bb-f1c17a727951", "CAN"), "", CharacterCasing.Upper, true, grid);

					Setup(new ZTextBoxColumnStyleInfo()
						, objName + AgencyShipmentContainer.Schema.BillContainersEntryNumber
						, Res.GetString("edca8746-cf55-4aca-9e2c-33ed3a1ef888", "Entry No"), Res.GetData("4d7dda1a-6bfc-4efd-a5bb-f1c17a727951", "CAN"), "", CharacterCasing.Normal, true, grid);

					break;
				default:
					break;
			}
		}

		static void Setup(ZGridColumnInfo column, ZString columnName, ZString caption, ResourceStringData groupName, ZString hint, CharacterCasing characterCasing, bool visible, ZGrid grid)
		{
			if (!columnName.IsEmpty)
			{
#if DEBUG
				TypeDescriptor.AddAttributes(column, new SuppressFormsLocalizedTestAttribute());
#endif

				column.ColumnName = columnName;
				column.Caption = caption;
				column.GroupName = groupName;
				column.ToolTip = hint;
				column.IsVisible = visible;
				column.CharacterCasing = characterCasing;
				grid.ColumnStyles.Add(column);
			}
			else
			{
				throw new ArgumentException("ColumnName should be NOT Empty");
			}
		}
	}
}
