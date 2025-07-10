using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	internal class VesselScheduleLineRecord : OneStopLineRecord
	{
		public VesselScheduleLineRecord(string[] vesselScheduleLine)
			: base(vesselScheduleLine)
		{
			var stImportAvailabilityDate = ColumnIndices.FirstFreeImportDate < vesselScheduleLine.Length && !vesselScheduleLine[ColumnIndices.FirstFreeImportDate].IsNullOrEmpty()
				? vesselScheduleLine[ColumnIndices.FirstFreeImportDate]
				: vesselScheduleLine[ColumnIndices.ImportAvailability];

			DateTimesParsedOk = TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ETA], out eta, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ETD], out etd, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.CargoCutoff], out cargoCutoff, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ReeferCutoff], out reeferCutoff, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ExportRecivalCommencement], out exportReceivalCommencement, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(stImportAvailabilityDate, out importAvailability, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ImportStorage], out importStorage, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ActualArrival], out actualArrival, FileDateTimeFormat)
				&& TryParseAsSmallDateTime(vesselScheduleLine[ColumnIndices.ActualDepart], out actualDepart, FileDateTimeFormat);
		}

		#region Public Properties

		public string UNLOCO => LineRecord[ColumnIndices.UNLOCO];
		public string TerminalID => LineRecord[ColumnIndices.TerminalID];
		public string ShipName => LineRecord[ColumnIndices.ShipName];
		public string ShipOperatorVoyageOut => LineRecord[ColumnIndices.ShipOperatorVoyageOut];
		public string LloydsID => LineRecord[ColumnIndices.LloydsID];
		public string LineOperator => LineRecord[ColumnIndices.LineOperator];
		public string OperatorDescription => LineRecord[ColumnIndices.OperatorDescription];
		public string ShipOperatorCode => LineRecord[ColumnIndices.ShipOperatorCode];
		public string ShipOperatorVoyageIn => LineRecord[ColumnIndices.ShipOperatorVoyageIn];
		public string ContainerVessel => LineRecord[ColumnIndices.ContainerVessel];
		public string VesselCode => LineRecord[ColumnIndices.VesselCode];
		public bool IsNewRecord { get; set; }

		public ZDateTime ETA => eta;
		readonly ZDateTime eta;

		public ZDateTime ETD => etd;
		readonly ZDateTime etd;

		public ZDateTime CargoCutoff => cargoCutoff;
		readonly ZDateTime cargoCutoff;

		public ZDateTime ReeferCutoff => reeferCutoff;
		readonly ZDateTime reeferCutoff;

		public ZDateTime ExportReceivalCommencement => exportReceivalCommencement;
		readonly ZDateTime exportReceivalCommencement;

		public ZDateTime ImportAvailability => importAvailability;
		readonly ZDateTime importAvailability;

		public ZDateTime ImportStorage => importStorage;
		readonly ZDateTime importStorage;

		public ZDateTime ActualArrival => actualArrival;
		readonly ZDateTime actualArrival;

		public ZDateTime ActualDepart => actualDepart;
		readonly ZDateTime actualDepart;

		#endregion

		#region Validity

		public bool IsValid()
		{
			if (LineRecord.Length >= ColumnIndices.FirstFreeImportDate)
			{
				if (!AnyIdentifyingFieldExceedsMaxLength())
				{
					if (DateTimesParsedOk)
					{
						return true;
					}
					else
					{
						ReportErrorIfUAT(InvalidDateTimeRecord);
					}
				}
				else
				{
					ReportErrorIfUAT(MaxLengthExceededErrorMessage);
				}
			}
			else
			{
				ReportErrorIfUAT(InvalidNumberOfFieldsErrorMessage);
			}

			return false;
		}

		bool AnyIdentifyingFieldExceedsMaxLength()
		{
			return TerminalID.Length > JobVesselScheduleSchema.EV_TerminalID.MaxLength
				|| LloydsID.Length > JobVesselScheduleSchema.EV_IMOLloydsNumber.MaxLength
				|| ShipOperatorVoyageIn.Length > JobVesselScheduleSchema.EV_ShipOperatorVoyageIn.MaxLength
				|| ShipOperatorVoyageOut.Length > JobVesselScheduleSchema.EV_ShipOperatorVoyageOut.MaxLength
				|| LineOperator.Length > JobVesselScheduleSchema.EV_LineOperator.MaxLength;
		}

		bool TryParseAsSmallDateTime(string value, out ZDateTime result, string format)
		{
			bool parsed = ZDateTime.TryParseExact(value, out result, format)
				&& (result.IsEmpty || result.IsValidSmallDateTime);

			if (parsed && !result.IsEmpty)
			{
				result = result.ToSmallDateTime();
			}

			return parsed;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date time format constant")]
		const string FileDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		bool DateTimesParsedOk { get; }

		#endregion

		static class ColumnIndices
		{
			public const int UNLOCO = 0;
			public const int TerminalID = 1;
			public const int ETA = 2;
			public const int ETD = 3;
			public const int ShipName = 4;
			public const int ShipOperatorVoyageOut = 5;
			public const int LloydsID = 6;
			public const int CargoCutoff = 7;
			public const int ReeferCutoff = 8;
			public const int LineOperator = 9;
			public const int OperatorDescription = 10;
			public const int ShipOperatorCode = 11;
			public const int ShipOperatorVoyageIn = 12;
			public const int ExportRecivalCommencement = 13;
			public const int ImportAvailability = 14;
			public const int ImportStorage = 15;
			public const int ContainerVessel = 16;
			public const int ActualArrival = 17;
			public const int ActualDepart = 18;
			public const int VesselCode = 19;
			public const int FirstFreeImportDate = 20;
		}

		protected override string ErrorKey => "Invalid1StopVesselScheduleFileRecordLine";
	}
}
