using System.Linq;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGSubstanceMultiColumnsManager
	{
		public UNDGSubstanceMultiColumnsManager(ZGrid grid)
		{
			this.Grid = grid;
			packMaxQtyFieldType = nameof(Business.UNDGSubstance.PackMaxQtyFieldType);
			packUQFieldType = nameof(Business.UNDGSubstance.PackUQFieldType);
			packingInstructionsFieldType = nameof(Business.UNDGSubstance.PackingInstructionsFieldType);
		}

		readonly ZGrid Grid;
		readonly string packMaxQtyFieldType;
		readonly string packUQFieldType;
		readonly string packingInstructionsFieldType;

		public void AddColumns(string groupName)
		{
			switch (groupName)
			{
				case GroupColumnConstant.LimitedQuantities:
					{
						var groupNameRes = Res.GetData("UNDGSubstanceMultiColumnsManager|D3939BE9-E08E-41C3-81EA-CD5C415161FA", GroupColumnConstant.LimitedQuantities);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|85957868-C584-4C78-A901-4C62F41B2471", "Ltd Qty - Pack Max. Qty", "Limited Quantity - Pack Max. Qty", "The maximum quantity per pack that this substance may be transported in limited quantities."), "DG_LQMaxAmt", packMaxQtyFieldType, 180);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|AC4423AF-7107-45BA-BFB5-D7A70E2692BF", "Ltd Qty - Pack UQ", "Limited Quantity - Pack UQ", "The unit of quantity for the maximum quantity per pack that this substance may be transported in limited quantities."), "DG_LQMaxAmtUQ", packUQFieldType, 160);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|4A080EBF-4A0B-4A0E-AFF7-1E768D277C4A", "Ltd Qty - PI", "Limited Quantity - Packing Instruction", "The packing instruction code that relates to the transportation of this substance in limited quantities."), "DG_PackIns", packingInstructionsFieldType, 200);
						break;
					}
				case GroupColumnConstant.CargoAircraftOnly:
					{
						var groupNameRes = Res.GetData("UNDGSubstanceMultiColumnsManager|315DB219-39FA-4CB5-A2D2-DF7DC00AC742", GroupColumnConstant.CargoAircraftOnly);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|FAA9DBC4-A517-4B13-AF02-0863EF37F011", "CAO - Pack Max.Qty", "Cargo Aircraft Only - Pack Max. Qty", "The maximum quantity per pack that this substance may be transported on cargo aircraft only."), "DG_CargoMaxAmt", packMaxQtyFieldType, 180);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|1C39295D-85A6-40CB-BC5F-4660D4A404B1", "CAO - Pack UQ", "Cargo Aircraft Only - Pack UQ", "The unit of quantity for the maximum quantity per pack that this substance may be transported on cargo aircraft only."), "DG_CargoMaxAmtUQ", packUQFieldType, 160);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|8B6CF016-9EAB-4613-8D35-0C824C02DBE4", "CAO - PI", "Cargo Aircraft Only - Packing Instruction", "The packing instruction code that relates to the transportation of this substance on cargo aircraft only."), "DG_CargoPackIns", packingInstructionsFieldType, 210);
						break;
					}
				case GroupColumnConstant.PassengerCargoAircraft:
					{
						var groupNameRes = Res.GetData("UNDGSubstanceMultiColumnsManager|7A2ECEE3-253B-43A5-BF9B-429141B7F28D", GroupColumnConstant.PassengerCargoAircraft);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|F749E160-F76C-4B4F-A23C-4263E5F12102", "PAX/CA - Pack Max.Qty", "Passenger and Cargo Aircraft - Pack Max. Qty", "The type of limitation imposed on the quantity of this substance when transported on passenger and cargo aircraft."), "DG_LQ2OrPaxMaxAmt", packMaxQtyFieldType, 240);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|4831C1C5-01F8-4C36-8A47-DB853D55AD9F", "PAX/CA - Pack UQ", "Passenger and Cargo Aircraft - Pack UQ", "The unit of quantity for the maximum quantity per pack that this substance may be transported on passenger and cargo aircraft."), "DG_LQ2OrPaxMaxAmtUQ", packUQFieldType, 200);
						AddColumnCore(groupNameRes, Res.GetData("UNDGSubstanceMultiColumnsManager|13E3B02B-FD51-4B2C-8F42-AF84704120C7", "PAX/CA - PI", "Passenger and Cargo Aircraft - Packing Instruction", "The packing instruction code that relates to the transportation of this substance on passenger and cargo aircraft."), "DG_PaxPackIns", packingInstructionsFieldType, 260);
						break;
					}
				default:
					break;
			}
		}

		void AddColumnCore(ResourceStringData groupName, ResourceStringData caption, string columnName, string filedType, int? width = null)
		{
			if (Grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnName))
			{
				return;
			}

			var gridColumnInfo = new ZMultiControlColumnStyleInfo()
			{
				ColumnName = columnName,
				FieldTypeColumnName = filedType,
				GroupName = groupName,
				CaptionResourceString = caption,
				IsVisible = false,
				Width = width.HasValue ? ControlDpiScalingHelper.ScaleToCurrentDpiX(width.Value) : ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};

			Grid.ColumnStyles.Add(gridColumnInfo);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Limited Quantities Group Name, Cargo Aircraft Only Group Name, Passenger and Cargo Aircraft Group Name")]
	public static class GroupColumnConstant
	{
		public const string LimitedQuantities = "Limited Quantities";
		public const string CargoAircraftOnly = "Cargo Aircraft Only";
		public const string PassengerCargoAircraft = "Passenger and Cargo Aircraft";
	}
}
