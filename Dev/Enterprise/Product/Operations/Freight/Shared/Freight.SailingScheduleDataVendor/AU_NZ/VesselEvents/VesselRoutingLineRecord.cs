using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	internal class VesselRoutingLineRecord : OneStopLineRecord
	{
		public VesselRoutingLineRecord(string[] vesselRoutingLine)
			: base(vesselRoutingLine)
		{
		}

		#region Public Properties

		public string TerminalCode => LineRecord[ColumnIndices.TerminalCode];
		public string TerminalName => LineRecord[ColumnIndices.TerminalName];
		public string ShipName => LineRecord[ColumnIndices.ShipName];
		public string LloydsID => LineRecord[ColumnIndices.LloydsID];
		public string VoyageNumber => LineRecord[ColumnIndices.VoyageNumber];
		public string DischargeCountry => LineRecord[ColumnIndices.DischargeCountry];
		public string DischargePortName => LineRecord[ColumnIndices.DischargePortName];
		public string DischargePortCode => LineRecord[ColumnIndices.DischargePortCode];
		public string DischargePortState => LineRecord[ColumnIndices.DischargePortState];

		#endregion

		#region Validity

		public bool IsValid()
		{
			if (LineRecord.Length > ColumnIndices.DischargePortState)
			{
				if (!AnyIdentifyingFieldExceedsMaxLength())
				{
					return true;
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
			return TerminalCode.Length > JobVesselRoutingSchema.E1_TerminalCode.MaxLength
				|| LloydsID.Length > JobVesselRoutingSchema.E1_LloydsID.MaxLength
				|| VoyageNumber.Length > JobVesselRoutingSchema.E1_VoyageNumber.MaxLength
				|| DischargePortCode.Length > JobVesselRoutingSchema.E1_RL_NKDischargePortCode.MaxLength;
		}

		#endregion

		static class ColumnIndices
		{
			public const int TerminalCode = 0;
			public const int TerminalName = 1;
			public const int ShipName = 2;
			public const int LloydsID = 3;
			public const int VoyageNumber = 4;
			public const int DischargeCountry = 5;
			public const int DischargePortName = 6;
			public const int DischargePortCode = 7;
			public const int DischargePortState = 8;
		}

		protected override string ErrorKey => "Invalid1StopVesselRoutingFileRecordLine";
	}
}
