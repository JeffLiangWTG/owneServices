using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business.Messaging;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public partial class ExportAWBHeader : AutoExportAWBHeader
	{
		public ExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly ExportAWBHeaderTypeDecider TypeDecider = new ExportAWBHeaderTypeDecider();

		#region Constants, Schema and TypeOfAWB Enumeration

		public new class Schema : AutoExportAWBHeader.Schema
		{
			public const string EH_AWBSerialNo = "EH_AWBSerialNo";
			public const string EH_AirlinePrefix = "EH_AirlinePrefix";
			public const string EH_AgentIATACodeFormatted = "EH_AgentIATACodeFormatted";
			public const string EH_AgentApprovedExporterNumber = "EH_AgentApprovedExporterNumber";

			public const string EH_By1stAirlineName = "EH_By1stAirlineName";

			public const string EH_ConsolNumber = "EH_ConsolNumber";
			public const string EH_ECNCRNNumber = "EH_ECNCRNNumber";

			public const string EH_WeightPPD = "EH_WeightPPD";
			public const string EH_WeightCOL = "EH_WeightCOL";
			public const string EH_WeightPrepaidCollect = "EH_WeightPrepaidCollect";

			public const string EH_OtherPPD = "EH_OtherPPD";
			public const string EH_OtherCOL = "EH_OtherCOL";
			public const string EH_OtherPrepaidCollect = "EH_OtherPrepaidCollect";

			public const string EH_SecurityStatus = "EH_SecurityStatus";
			public const string EH_SecurityStatusForNonBorrowedMAWBs = "EH_SecurityStatusForNonBorrowedMAWBs";
			public const string EH_KnownConsignorCode = "EH_KnownConsignorCode";

			public const string EH_TotalLineTotals = "EH_TotalLineTotals";
			public const string EH_TotalNoOfPieces = "EH_TotalNoOfPieces";

			public const string EH_TotalGrossWeight = "EH_TotalGrossWeight";

			public const string EH_TotalPPD = "EH_TotalPPD";
			public const string EH_TotalCOL = "EH_TotalCOL";
			public const string EH_OtherChargesDueAgentCOL = "EH_OtherChargesDueAgentCOL";
			public const string EH_OtherChargesDueCarrierCOL = "EH_OtherChargesDueCarrierCOL";
			public const string EH_OtherChargesDueAgentPPD = "EH_OtherChargesDueAgentPPD";
			public const string EH_OtherChargesDueCarrierPPD = "EH_OtherChargesDueCarrierPPD";
			public const string EH_TotalWeightCOL = "EH_TotalWeightCOL";
			public const string EH_TotalWeightPPD = "EH_TotalWeightPPD";
		}

		public enum TypeOfAWB
		{
			UndefinedMaster,
			AgentMaster,
			DirectMaster,
			MasterHouse,
			House
		}

		public static class Constants
		{
			public const int NumberOfRateLines = 12;
			public const int BreakNatureAndQtyOfGoodsonPosition = 29;
			public const int NumberNatureAndDescriptionLines = 12;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IATA Text")]
			public const string NoDimensionsAvailable = "No Dimensions Available";
			public const string VOL = "VOL";
			public const string SLAC = "SLAC"; // IATA Text
			public const int AgentNameMaxLength = 50;
			public const int AgentPlaceMaxLength = 50;
			public const int AgentIATACodeMaxLength = 14;
			public const int AgentAccountNoMaxLength = 14;

			public static class PrepaidCollect1CharCodes
			{
				public const string Prepaid = Core.Constants.AWB.PPDCollect.Prepaid;
				public const string Collect = Core.Constants.AWB.PPDCollect.Collect;
			}

			public static class PrepaidCollect3CharCodes
			{
				public const string Prepaid = "PPD";
				public const string Collect = "COL";
			}

			public static class ChargeCodes
			{
				public const string AllChargesCollect = "CC";
				public const string AllChargesCollectByCreditCard = "CZ";
				public const string AllChargesCollectByGBL = "CG";
				public const string AllChargesPrepaidCash = "PP";
				public const string AllChargesPrepaidCredit = "PX";
				public const string AllChargesPrepaidByCreditCard = "PZ";
				public const string AllChargesPrepaidByGBL = "PG";
				public const string DestinationCollectCash = "CP";
				public const string DestinationCollectCredit = "CX";
				public const string DestinationCollectByMCO = "CM";
				public const string NoCharge = "NC";
				public const string NoWeightCharge_OtherChargesCollect = "NT";
				public const string NoWeightCharge_OtherChargesPrepaidByCreditCard = "NZ";
				public const string NoWeightCharge_OtherChargesPrepaidByGBL = "NG";
				public const string NoWeightCharge_OtherChargesPrepaidCash = "NP";
				public const string NoWeightCharge_OtherChargesPrepaidCredit = "NX";
				public const string PartialCollectCredit_PartialPrepaidCash = "CA";
				public const string PartialCollectCredit_PartialPrepaidCredit = "CB";
				public const string PartialCollectCreditCard_PartialPrepaidCash = "CE";
				public const string PartialCollectCreditCard_PartialPrepaidCredit = "CH";
				public const string PartialPrepaidCash_PartialCollectCash = "PC";
				public const string PartialPrepaidCredit_PartialCollectCash = "PD";
				public const string PartialPrepaidCreditCard_PartialCollectCash = "PE";
				public const string PartialPrepaidCreditCard_PartialCollectCredit = "PH";
				public const string PartialPrepaidCreditCard_PartialCollectCreditCard = "PF";
			}

			public static class AirlineMessagesAddInfoKeys
			{
				public const string SendFWBOrFHLToAirlineBasedOnMAWBPrefix = "SendFWBOrFHLToAirlineBasedOnMAWBPrefix";
				public const string SendFWBNatureAndQuantityOfGoodsType = "SendFWBNatureAndQuantityOfGoodsType";
				public const string SendFWB = "SendFWB";
				public const string SendFHL = "SendFHL";
				public const string IncludeECSD = "IncludeECSD";
				public const string CargoIMPVersion = "CargoIMPVersion";
				public const string DefaultIdentifierForCneNfyName = "DefaultIdentifierForCneNfyName";
				public const string DefaultIdentifierForCneNfyPhone = "DefaultIdentifierForCneNfyPhone";
			}
		}

		#endregion

		#region Child Collections

		#region AWBAccountingInformations

		[ChildEditable(true)]
		public ExportAWBAccountingInformationCollection AWBAccountingInformations
		{
			get
			{
				if (fAWBAccountingInformations == null)
				{
					fAWBAccountingInformations = GetNewAWBAccountingInformations();
					fAWBAccountingInformations.Load();
					RegisterEditableChildObject(fAWBAccountingInformations);
				}

				return fAWBAccountingInformations;
			}
		}
		ExportAWBAccountingInformationCollection fAWBAccountingInformations;

		protected virtual ExportAWBAccountingInformationCollection GetNewAWBAccountingInformations()
		{
			return new ExportAWBAccountingInformationCollection(this, Factory);
		}

		#endregion

		#region AWBOtherCharges

		[ChildEditable(true)]
		public ExportAWBOtherChargesCollection AWBOtherCharges
		{
			get
			{
				if (awbOtherCharges == null)
				{
					awbOtherCharges = GetNewAWBOtherCharges();
					awbOtherCharges.Load();
					RegisterEditableChildObject(awbOtherCharges);

					awbOtherCharges.CountChanged += (s, e) =>
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateEH_OtherPrepaidCollect();
							Validation.ValidateEH_WeightPrepaidCollect();
						}
					};
				}

				return awbOtherCharges;
			}
		}
		ExportAWBOtherChargesCollection awbOtherCharges;

		protected virtual ExportAWBOtherChargesCollection GetNewAWBOtherCharges()
		{
			return new ExportAWBOtherChargesCollection(this);
		}

		#endregion

		#region AWBRateLines

		[ChildEditable(true)]
		public ExportAWBRateLineCollection AWBRateLines
		{
			get
			{
				if (fAWBRateLines == null)
				{
					fAWBRateLines = GetNewAWBRateLines();
					PrepareRateLinesCollection(fAWBRateLines);
					RegisterEditableChildObject(fAWBRateLines);
				}

				return fAWBRateLines;
			}
		}
		protected ExportAWBRateLineCollection fAWBRateLines;

		protected virtual ExportAWBRateLineCollection GetNewAWBRateLines()
		{
			return new ExportAWBRateLineCollection(this);
		}

		void PrepareRateLinesCollection(ExportAWBRateLineCollection collection)
		{
			collection.Load();

			using (collection.SuspendListChanged())
			{
				for (ZByte i = 1; i <= Constants.NumberOfRateLines; i++)
				{
					if (!collection.Contains(ExportAWBRateLine.Schema.ER_LineCount, i))
					{
						var rateLine = collection.AddNew();
						using (rateLine.SuspendSettingHasChanges())
						{
							rateLine.ER_LineCount = i;
						}
					}
				}

				var sortInfo = new SortInfo(ExportAWBRateLine.Schema.ER_LineCount, System.ComponentModel.ListSortDirection.Ascending);
				collection.Sort(sortInfo);
			}
		}

		#endregion

		#region AWBSpecialHandlingItems

		ExportAWBSpecialHandlingCollection _awbSpecialHandlingItems;

		[ChildEditable]
		public ExportAWBSpecialHandlingCollection AWBSpecialHandlingItems
		{
			get { return _awbSpecialHandlingItems ?? (_awbSpecialHandlingItems = GetAndLoadNewAWBSpecialHandlingItems()); }
		}

		ExportAWBSpecialHandlingCollection GetAndLoadNewAWBSpecialHandlingItems()
		{
			var result = GetNewAWBSpecialHandlingItems();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual ExportAWBSpecialHandlingCollection GetNewAWBSpecialHandlingItems()
		{
			return new ExportAWBSpecialHandlingCollection(this);
		}

		public virtual ZString EH_SecurityStatus
		{
			get
			{
				if (CargoSecurityScreeningMethods
					.Cast<ExportAWBSecurityStatusLine>()
					.Any(m => m.EAS_ScreeningMethod == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					return AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
				}

				return AWBSpecialHandlingItems
					.Cast<ExportAWBSpecialHandling>()
					.Where(x => x.IsSecurityStatus)
					.Select(x => x.EP_SpecialHandling)
					.FirstOrDefault();
			}
		}

		public ZPropertyInfo EH_SecurityStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EH_SecurityStatus); }
		}

		public virtual ZString EH_SecurityStatusForNonBorrowedMAWBs
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo EH_SecurityStatusForNonBorrowedMAWBsInfo
		{
			get { return GetZPropertyInfo(Schema.EH_SecurityStatusForNonBorrowedMAWBs); }
		}

		public ExportAWBSpecialHandling SecurityStatus
		{
			get { return AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().FirstOrDefault(s => s.IsSecurityStatus); }
		}

		#endregion

		#region ExportAWBSecurityStatusLines

		[ChildEditable(true)]
		public ExportAWBSecurityStatusLineCollection ExportAWBSecurityStatusLines
		{
			get
			{
				if (exportAWBSecurityStatusLines == null)
				{
					exportAWBSecurityStatusLines = GetNewExportAWBSecurityStatusLines();
					exportAWBSecurityStatusLines.Load();

					RegisterEditableChildObject(exportAWBSecurityStatusLines);
				}

				return exportAWBSecurityStatusLines;
			}
		}

		ExportAWBSecurityStatusLineCollection exportAWBSecurityStatusLines;

		protected virtual ExportAWBSecurityStatusLineCollection GetNewExportAWBSecurityStatusLines()
		{
			return new ExportAWBSecurityStatusLineCollection(this);
		}

		#endregion

		public ExportAWBSecurityStatusLineView CargoSecurityKnownShippers
		{
			get
			{
				if (cargoSecurityKnowShippers == null)
				{
					cargoSecurityKnowShippers = new ExportAWBSecurityStatusLineView(ExportAWBSecurityStatusLines,
						SecurityStatusLineType.KnownConsignor,
						true, true);

					cargoSecurityKnowShippers.Rebuild();
				}

				return cargoSecurityKnowShippers;
			}
		}

		ExportAWBSecurityStatusLineView cargoSecurityKnowShippers;

		public ExportAWBSecurityStatusLineView CargoSecurityScreeningMethods
		{
			get
			{
				if (cargoSecurityScreeningMethods == null)
				{
					cargoSecurityScreeningMethods = new ExportAWBSecurityStatusLineView(ExportAWBSecurityStatusLines,
						SecurityStatusLineType.ScreeningMethod,
						false, false);

					cargoSecurityScreeningMethods.Rebuild();
				}

				return cargoSecurityScreeningMethods;
			}
		}

		ExportAWBSecurityStatusLineView cargoSecurityScreeningMethods;

		public ExportAWBSecurityStatusLineView CargoSecurityExemptionGrounds
		{
			get
			{
				if (cargoSecurityExemptionGrounds == null)
				{
					cargoSecurityExemptionGrounds = new ExportAWBSecurityStatusLineView(ExportAWBSecurityStatusLines,
						SecurityStatusLineType.ExceptionCode,
						false, false);

					cargoSecurityExemptionGrounds.Rebuild();
				}

				return cargoSecurityExemptionGrounds;
			}
		}

		ExportAWBSecurityStatusLineView cargoSecurityExemptionGrounds;

		#endregion

		protected override IDisposable SuspendSettingHasChangesCore()
		{
			List<IDisposable> disposables = new List<IDisposable>
			{
				base.SuspendSettingHasChangesCore()
			};

			disposables.AddRange(AWBRateLines.Cast<ExportAWBRateLine>().Select((rateLine) => rateLine.SuspendSettingHasChanges()));

			return new DisposableAction(() => disposables.ForEach((disposable) => disposable.Dispose()));
		}

		#region NatureAndQtyOfGoods non persistent properties

		public ZString NatureAndQtyOfGoods
		{
			get
			{
				return AWBRateLine1.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine2.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine3.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine4.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine5.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine6.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine7.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine8.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine9.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine10.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine11.NatureAndQtyOfGoodsDescription + "\n"
					+ AWBRateLine12.NatureAndQtyOfGoodsDescription;
			}
			set
			{
				ZString[] sourceLines = value.Split(new[] { '\n' }, Constants.NumberNatureAndDescriptionLines);

				SetNatureNatureAndQtyOfGoods(AWBRateLine1, 0, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine2, 1, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine3, 2, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine4, 3, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine5, 4, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine6, 5, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine7, 6, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine8, 7, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine9, 8, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine10, 9, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine11, 10, sourceLines);
				SetNatureNatureAndQtyOfGoods(AWBRateLine12, 11, sourceLines);
			}
		}

		void SetNatureNatureAndQtyOfGoods(IExportAWBRateLine line, int index, ZString[] values)
		{
			if (line != null)
			{
				if (index < values.Length - 1)
				{
					line.NatureAndQtyOfGoodsDescription = values[index];
				}
				else if (index == values.Length - 1)
				{
					line.NatureAndQtyOfGoodsDescription = values[index].Replace(new string('\n', 1), "").Trim().Left(Constants.BreakNatureAndQtyOfGoodsonPosition);
				}
				else
				{
					line.NatureAndQtyOfGoodsDescription = ZString.Empty;
				}
			}
		}

		public IExportAWBRateLine GetRateLineAt(int i)
		{
			switch (i)
			{
				case 1:
					return AWBRateLine1;
				case 2:
					return AWBRateLine2;
				case 3:
					return AWBRateLine3;
				case 4:
					return AWBRateLine4;
				case 5:
					return AWBRateLine5;
				case 6:
					return AWBRateLine6;
				case 7:
					return AWBRateLine7;
				case 8:
					return AWBRateLine8;
				case 9:
					return AWBRateLine9;
				case 10:
					return AWBRateLine10;
				case 11:
					return AWBRateLine11;
				case 12:
					return AWBRateLine12;
				default:
					return null;
			}
		}

		public void PopulateNatureAndQtyOfGoodsLine(int i, string value, bool isText = false)
		{
			var linesToPopulate = GetNatureAndQtyOfGoodsLinesToPopulate(i, value, isText);
			if (linesToPopulate != null)
			{
				foreach (var line in linesToPopulate)
				{
					PopulateLine(line.Key, line.Value);
				}
			}
		}

		protected Dictionary<int, string> GetNatureAndQtyOfGoodsLinesToPopulate(int i, string value, bool isText, bool allowPartialPopulation = true)
		{
			if (i <= 0 || i > Constants.NumberNatureAndDescriptionLines)
			{
				return null;
			}

			if (!isText && AllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText)
			{
				return new Dictionary<int, string> { { i, value } };
			}

			var lineBreaker = new LineBreaker(ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength);
			var lines = lineBreaker.GetBrokenLinesList(value, Constants.NumberNatureAndDescriptionLines, i);
			if (lines.Count == 0)
			{
				return new Dictionary<int, string>();
			}

			if (!allowPartialPopulation && i + lines.Count - 1 > Constants.NumberNatureAndDescriptionLines)
			{
				return null;
			}

			var linesToPopulate = new Dictionary<int, string>();
			for (int lineNumber = i, currentLine = 0; lineNumber <= Constants.NumberNatureAndDescriptionLines && currentLine < lines.Count; lineNumber++, currentLine++)
			{
				linesToPopulate.Add(lineNumber, lines[currentLine]);
			}
			return linesToPopulate;
		}

		protected void PopulateLine(int lineNumber, string value, bool suspendSettingHasChanges = false)
		{
			var rateLine = GetRateLineAt(lineNumber) as ExportAWBRateLine;
			if (rateLine != null)
			{
				using (rateLine.GetSettingHasChangesSuspender(suspendSettingHasChanges))
				{
					rateLine.NatureAndQtyOfGoodsDescription = value;
				}
			}
		}

		public void PopulateNatureAndQtyOfGoodsType(int i, string type, bool suspendSettingHasChanges = false)
		{
			var rateLine = GetRateLineAt(i) as ExportAWBRateLine;
			if (rateLine != null)
			{
				using (rateLine.GetSettingHasChangesSuspender(suspendSettingHasChanges))
				{
					rateLine.NatureAndQtyOfGoodsType = type;
				}
			}
		}

		public ZString TextToNatureAndQtyOfGoodsLines(ZString sourceLine)
		{
			var lineBreaker = new LineBreaker(15, ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength, 56, Business.NatureAndQtyOfGoods.Font);
			return lineBreaker.GetBrokenLines(sourceLine, 1);
		}

		protected internal virtual bool AllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText
		{
			get { return true; }
		}

		protected IExportAWBRateLine SLACLine
		{
			get
			{
				if (slacLine == null)
				{
					slacLine = GetRateLineAt(Constants.NumberNatureAndDescriptionLines);
				}
				return slacLine;
			}
		}
		IExportAWBRateLine slacLine;

		#endregion

		#region AWBRateLines

		public ExportAWBRateLine AWBRateLine1
		{
			get { return AWBRateLines[0]; }
		}

		public ExportAWBRateLine AWBRateLine2
		{
			get { return AWBRateLines[1]; }
		}

		public ExportAWBRateLine AWBRateLine3
		{
			get { return AWBRateLines[2]; }
		}

		public ExportAWBRateLine AWBRateLine4
		{
			get { return AWBRateLines[3]; }
		}

		public ExportAWBRateLine AWBRateLine5
		{
			get { return AWBRateLines[4]; }
		}

		public ExportAWBRateLine AWBRateLine6
		{
			get { return AWBRateLines[5]; }
		}

		public ExportAWBRateLine AWBRateLine7
		{
			get { return AWBRateLines[6]; }
		}

		public ExportAWBRateLine AWBRateLine8
		{
			get { return AWBRateLines[7]; }
		}

		public ExportAWBRateLine AWBRateLine9
		{
			get { return AWBRateLines[8]; }
		}

		public ExportAWBRateLine AWBRateLine10
		{
			get { return AWBRateLines[9]; }
		}

		public ExportAWBRateLine AWBRateLine11
		{
			get { return AWBRateLines[10]; }
		}

		public ExportAWBRateLine AWBRateLine12
		{
			get { return AWBRateLines[11]; }
		}

		#endregion

		#region Calculated Properties

		#region EH_AWBSerialNo

		public ZPropertyInfo EH_AWBSerialNoInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_AWBSerialNo); }
		}

		public ZString EH_AWBSerialNo
		{
			get { return SerialNumber; }
		}

		#endregion

		#region EH_AirlinePrefix

		public ZPropertyInfo EH_AirlinePrefixInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_AirlinePrefix); }
		}

		public ZString EH_AirlinePrefix
		{
			get { return AirlinePrefix; }
		}

		#endregion

		#region EH_TotalGrossWeight

		[DecimalPlaces("TotalGrossWeightDecimalPlaces")]
		public virtual ZDecimal EH_TotalGrossWeight
		{
			get { return AWBRateLines.SumOfDecimal(ExportAWBRateLine.Schema.ER_GrossWeight); }
		}

		public ZPropertyInfo EH_TotalGrossWeightInfo
		{
			get { return GetZPropertyInfo(Schema.EH_TotalGrossWeight); }
		}

		public int TotalGrossWeightDecimalPlaces
		{
			get
			{
				return AWBRateLines.Cast<ExportAWBRateLine>().Max(rateLine => rateLine.GetNumberOfDecimals(rateLine.ER_GrossWeightInfo));
			}
		}

		#endregion

		#region EH_TotalLineTotals

		public virtual ZDecimal EH_TotalLineTotals
		{
			get { return AWBRateLines.SumOfDecimal(ExportAWBRateLine.Schema.ER_Total); }
		}

		public ZPropertyInfo EH_TotalLineTotalsInfo
		{
			get { return GetZPropertyInfo(Schema.EH_TotalLineTotals); }
		}

		#endregion

		#region EH_TotalNoOfPieces

		public virtual ZInt EH_TotalNoOfPieces
		{
			get { return AWBRateLines.SumOfInt(ExportAWBRateLine.Schema.ER_NoOfPiecesOrRCP); }
		}

		public ZPropertyInfo EH_TotalNoOfPiecesInfo
		{
			get { return GetZPropertyInfo(Schema.EH_TotalNoOfPieces); }
		}

		#endregion

		#region EH_KnownConsignorCode

		public virtual ZString EH_KnownConsignorCode
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo EH_KnownConsignorCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EH_KnownConsignorCode); }
		}

		#endregion

		#endregion

		#region EH_ConsolNumber

		public ZString EH_ConsolNumber
		{
			get { return ConsolNumber; }
		}

		public ZPropertyInfo EH_ConsolNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EH_ConsolNumber); }
		}

		#endregion

		#region EH_ECNCRNNumber

		public ZString EH_ECNCRNNumber
		{
			get { return CustomsEntryNumber; }
		}

		public ZPropertyInfo EH_ECNCRNNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EH_ECNCRNNumber); }
		}

		#endregion

		#region Total Properties from Bottom Left Hand Corner of AWB

		#region EH_TotalWeightPPD

		public virtual ZDecimal EH_TotalWeightPPD
		{
			get { return EH_WeightPPD ? EH_TotalLineTotals : (ZDecimal)0M; }
		}

		public ZPropertyInfo EH_TotalWeightPPDInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_TotalWeightPPD); }
		}

		#endregion

		#region EH_TotalWeightCOL

		public virtual ZDecimal EH_TotalWeightCOL
		{
			get { return EH_WeightCOL ? EH_TotalLineTotals : (ZDecimal)0M; }
		}

		public ZPropertyInfo EH_TotalWeightCOLInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_TotalWeightCOL); }
		}

		#endregion

		#region EH_WeightPPD

		public virtual ZBool EH_WeightPPD
		{
			get { return EH_WeightPrepaidCollect == Constants.PrepaidCollect1CharCodes.Prepaid; }
		}

		#endregion

		#region EH_WeightCOL

		public virtual ZBool EH_WeightCOL
		{
			get { return EH_WeightPrepaidCollect == Constants.PrepaidCollect1CharCodes.Collect; }
		}

		#endregion

		#region EH_OtherPPD

		public virtual ZBool EH_OtherPPD
		{
			get { return EH_OtherPrepaidCollect == Constants.PrepaidCollect1CharCodes.Prepaid; }
		}

		#endregion

		#region EH_OtherCOL

		public virtual ZBool EH_OtherCOL
		{
			get { return EH_OtherPrepaidCollect == Constants.PrepaidCollect1CharCodes.Collect; }
		}

		#endregion

		#region EH_OtherChargesDueAgentPPD

		public virtual ZDecimal EH_OtherChargesDueAgentPPD { get { return CalculateOtherCharges(PaymentTerm.Prepaid, ChargesDue.Agent); } }

		public ZPropertyInfo EH_OtherChargesDueAgentPPDInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_OtherChargesDueAgentPPD); }
		}

		#endregion

		#region EH_OtherChargesDueAgentCOL

		public virtual ZDecimal EH_OtherChargesDueAgentCOL { get { return CalculateOtherCharges(PaymentTerm.Collect, ChargesDue.Agent); } }

		public ZPropertyInfo EH_OtherChargesDueAgentCOLInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_OtherChargesDueAgentCOL); }
		}

		#endregion

		#region EH_OtherChargesDueCarrierPPD

		public virtual ZDecimal EH_OtherChargesDueCarrierPPD { get { return CalculateOtherCharges(PaymentTerm.Prepaid, ChargesDue.Carrier); } }
		public ZPropertyInfo EH_OtherChargesDueCarrierPPDInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_OtherChargesDueCarrierPPD); }
		}

		#endregion

		#region EH_OtherChargesDueCarrierCOL

		public virtual ZDecimal EH_OtherChargesDueCarrierCOL { get { return CalculateOtherCharges(PaymentTerm.Collect, ChargesDue.Carrier); } }

		public ZPropertyInfo EH_OtherChargesDueCarrierCOLInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_OtherChargesDueCarrierCOL); }
		}

		#endregion

		ZDecimal CalculateOtherCharges(PaymentTerm paymentTerm, ChargesDue chargesDue)
		{
			if (IsHouseAirWayBill)
			{
				return AWBOtherCharges.SumOf(paymentTerm, chargesDue);
			}
			else
			{
				return (EH_OtherCOL && paymentTerm == PaymentTerm.Collect) || (EH_OtherPPD && paymentTerm == PaymentTerm.Prepaid)
					? AWBOtherCharges.SumOf(chargesDue)
					: ZDecimal.Zero;
			}
		}

		#region EH_TotalPPD

		public virtual ZDecimal EH_TotalPPD
		{
			get { return EH_TotalWeightPPD + EH_ValuationPPD + EH_TaxesPPD + EH_OtherChargesDueAgentPPD + EH_OtherChargesDueCarrierPPD; }
		}

		public ZPropertyInfo EH_TotalPPDInfo
		{
			get { return GetZPropertyInfo(Schema.EH_TotalPPD); }
		}

		#endregion

		#region EH_TotalCOL

		public virtual ZDecimal EH_TotalCOL
		{
			get { return EH_TotalWeightCOL + EH_ValuationCOL + EH_TaxesCOL + EH_OtherChargesDueAgentCOL + EH_OtherChargesDueCarrierCOL; }
		}

		public ZPropertyInfo EH_TotalCOLInfo
		{
			get { return GetZPropertyInfo(Schema.EH_TotalCOL); }
		}

		#endregion

		#endregion

		#region New Properties

		#region EH_WeightPrepaidCollect

		[List("PrepaidCollectList")]
		[MaxLength(1)]
		public virtual ZString EH_WeightPrepaidCollect
		{
			get { return EH_WeightVPPDCOL.Left(1); }
			set
			{
				CheckMaximumLength(EH_WeightPrepaidCollectInfo, value);
				ZString newValue = Get3CharPrepaidCollectCode(value);
				if (newValue != EH_WeightVPPDCOL)
				{
					EH_WeightVPPDCOL = newValue;
					if (!IsValidationSuspended)
					{
						Validation.ValidateEH_WeightPrepaidCollect();
						Validation.ValidateEH_OtherPrepaidCollect();
					}
					EH_WeightPrepaidCollectInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EH_WeightPrepaidCollectInfo
		{
			get { return GetZPropertyInfo(Schema.EH_WeightPrepaidCollect); }
		}

		#endregion

		#region EH_OtherPrepaidCollect

		[List("PrepaidCollectList")]
		[MaxLength(1)]
		public virtual ZString EH_OtherPrepaidCollect
		{
			get { return EH_OtherPPDCOL.Left(1); }
			set
			{
				CheckMaximumLength(EH_OtherPrepaidCollectInfo, value);
				ZString newValue = Get3CharPrepaidCollectCode(value);
				if (newValue != EH_OtherPPDCOL)
				{
					EH_OtherPPDCOL = newValue;
					if (!IsValidationSuspended)
					{
						Validation.ValidateEH_OtherPrepaidCollect();
						Validation.ValidateEH_WeightPrepaidCollect();
					}
					EH_OtherPrepaidCollectInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EH_OtherPrepaidCollectInfo
		{
			get { return GetZPropertyInfo(Schema.EH_OtherPrepaidCollect); }
		}

		#endregion

		protected virtual ZString Get3CharPrepaidCollectCode(ZString shortCode)
		{
			ZString result = shortCode;
			switch (shortCode)
			{
				case Constants.PrepaidCollect1CharCodes.Prepaid:
					result = Constants.PrepaidCollect3CharCodes.Prepaid;
					break;

				case Constants.PrepaidCollect1CharCodes.Collect:
					result = Constants.PrepaidCollect3CharCodes.Collect;
					break;
			}

			return result;
		}

		public ZString EH_By1stAirlineName
		{
			get
			{
				RefAirline[] airlines = Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, EH_By1st));
				return airlines.Length == 1 ? airlines[0].RM_AirlineName1 : ZString.Empty;
			}
		}

		public ZPropertyInfo EH_By1stAirlineNameInfo
		{
			get { return GetZPropertyInfo(Schema.EH_By1stAirlineName); }
		}

		public bool IsShipperTraderNoExceedingMaxLength { get; protected set; }
		public bool IsConsigneeTraderNoExceedingMaxLength { get; protected set; }
		public bool IsAlsoNotifyTraderNoExceedingMaxLength { get; protected set; }
		public virtual bool IsAWBOverridden => false;
		public virtual bool IsCSDOverridden => false;

		#endregion

		#region Overidden Properties

		[List("Lookups.Airlines")]
		public override ZString EH_By1st
		{
			get { return base.EH_By1st; }
			set
			{
				base.EH_By1st = value;
				EH_By1stAirlineNameInfo.RefreshBinding();
			}
		}

		public override ZString EH_By2nd
		{
			get { return base.EH_By2nd; }
			set
			{
				base.EH_By2nd = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_To2nd();
				}
			}
		}

		public override ZString EH_By3rd
		{
			get { return base.EH_By3rd; }
			set
			{
				base.EH_By3rd = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_To3rd();
				}
			}
		}

		[List("SpecialEUInstructionsList")]
		public override ZString EH_SpecialHandlingCode
		{
			get { return base.EH_SpecialHandlingCode; }
			set { base.EH_SpecialHandlingCode = value; }
		}

		[List("ChargeCodesList")]
		public override ZString EH_ChargesCode
		{
			get { return base.EH_ChargesCode; }
			set { base.EH_ChargesCode = value; }
		}

		#region Bookings

		public override ZString EH_Booking1stCarrier
		{
			get { return base.EH_Booking1stCarrier; }
			set
			{
				base.EH_Booking1stCarrier = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		public override ZString EH_Booking1stFlight
		{
			get { return base.EH_Booking1stFlight; }
			set
			{
				ZString newValue = (!value.IsEmpty && value.Length < 3) ? value.PadLeft(3, '0') : value;
				base.EH_Booking1stFlight = newValue;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		public override ZString EH_Booking1stFlightDate
		{
			get { return base.EH_Booking1stFlightDate; }
			set
			{
				ZString newValue = (!value.IsEmpty && value.Length < 2) ? value.PadLeft(2, '0') : value;
				base.EH_Booking1stFlightDate = newValue;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		public override ZString EH_Booking2ndCarrier
		{
			get { return base.EH_Booking2ndCarrier; }
			set
			{
				base.EH_Booking2ndCarrier = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		public override ZString EH_Booking2ndFlight
		{
			get { return base.EH_Booking2ndFlight; }
			set
			{
				ZString newValue = (!value.IsEmpty && value.Length < 3) ? value.PadLeft(3, '0') : value;
				base.EH_Booking2ndFlight = newValue;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		public override ZString EH_Booking2ndFlightDate
		{
			get { return base.EH_Booking2ndFlightDate; }
			set
			{
				ZString newValue = (!value.IsEmpty && value.Length < 2) ? value.PadLeft(2, '0') : value;
				base.EH_Booking2ndFlightDate = newValue;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBookings();
				}
			}
		}

		#endregion

		public override ZString EH_ConsigneeContactDetail
		{
			get { return base.EH_ConsigneeContactDetail; }
			set
			{
				base.EH_ConsigneeContactDetail = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeContactCode();
				}
			}
		}

		public virtual ZString EH_ConsigneeContactEmail { get; set; }

		public override ZString EH_ConsigneeTraderNo
		{
			get => base.EH_ConsigneeTraderNo;
			set
			{
				base.EH_ConsigneeTraderNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeTraderNoType();
				}
			}
		}

		public override ZString EH_ConsigneeTraderNoType
		{
			get => base.EH_ConsigneeTraderNoType;
			set
			{
				base.EH_ConsigneeTraderNoType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeTraderNo();
				}
			}
		}

		public override ZString EH_ShipperContactDetail
		{
			get { return base.EH_ShipperContactDetail; }
			set
			{
				base.EH_ShipperContactDetail = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperContactCode();
				}
			}
		}

		public virtual ZString EH_ShipperContactEmail { get; set; }

		public override ZString EH_ShipperTraderNo
		{
			get => base.EH_ShipperTraderNo;
			set
			{
				base.EH_ShipperTraderNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperTraderNoType();
				}
			}
		}

		public override ZString EH_ShipperTraderNoType
		{
			get => base.EH_ShipperTraderNoType;
			set
			{
				base.EH_ShipperTraderNoType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperTraderNo();
				}
			}
		}

		public virtual ZString DestinationShipperComment => ZString.Empty;

		public override ZString EH_AlsoNotifyContactDetail
		{
			get { return base.EH_AlsoNotifyContactDetail; }
			set
			{
				base.EH_AlsoNotifyContactDetail = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_AlsoNotifyContactCode();
				}
			}
		}

		public override ZString EH_AlsoNotifyTraderNo
		{
			get => base.EH_AlsoNotifyTraderNo;
			set
			{
				base.EH_AlsoNotifyTraderNo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_AlsoNotifyTraderNoType();
				}
			}
		}

		public override ZString EH_AlsoNotifyTraderNoType
		{
			get => base.EH_AlsoNotifyTraderNoType;
			set
			{
				base.EH_AlsoNotifyTraderNoType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_AlsoNotifyTraderNo();
				}
			}
		}

		[List("ContactCodesList")]
		public override ZString EH_ConsigneeContactCode
		{
			get { return base.EH_ConsigneeContactCode; }
			set
			{
				base.EH_ConsigneeContactCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeContactDetail();
				}
			}
		}

		[List("ContactCodesList")]
		public override ZString EH_ShipperContactCode
		{
			get { return base.EH_ShipperContactCode; }
			set
			{
				base.EH_ShipperContactCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperContactDetail();
				}
			}
		}

		public override ZString EH_AgentApprovalNumber
		{
			get { return base.EH_AgentApprovalNumber; }
			set
			{
				base.EH_AgentApprovalNumber = value;
				ExportAWBSecurityStatusLines.MarkAsNeedingValidation();
			}
		}

		[List("ContactCodesList")]
		public override ZString EH_AlsoNotifyContactCode
		{
			get { return base.EH_AlsoNotifyContactCode; }
			set
			{
				base.EH_AlsoNotifyContactCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_AlsoNotifyContactDetail();
				}
			}
		}

		[List("Lookups.AWBTypeList")]
		public override ZString EH_AWBType
		{
			get { return base.EH_AWBType; }
			set
			{
				base.EH_AWBType = value;
				AWBOtherCharges.MarkAsNeedingValidation();
			}
		}

		[List("AsAgreed1stList")]
		public override ZString EH_AsAgreed1st
		{
			get { return base.EH_AsAgreed1st; }
			set { base.EH_AsAgreed1st = value; }
		}

		[List("AsAgreed2ndList")]
		public override ZString EH_AsAgreed2nd
		{
			get { return base.EH_AsAgreed2nd; }
			set { base.EH_AsAgreed2nd = value; }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ContactCodesList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.AWBContactCodes); }
		}

		public CodeDescriptionPairList PrepaidCollectList
		{
			get { return PrepaidCollectCoreList; }
		}

		protected virtual CodeDescriptionPairList PrepaidCollectCoreList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.AWBPrepayCollect); }
		}

		public CodeDescriptionPairList ChargeCodesList
		{
			get
			{
				CodeDescriptionPairList result = new UntranslatableCodeDescriptionPairList((NoResString)"AWB is always in English"); // Untranslatable reason

				result.AddPair(Constants.ChargeCodes.AllChargesCollect, (NoResString)"All Charges Collect"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesCollectByCreditCard, (NoResString)"All Charges Collect By Credit Card"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesCollectByGBL, (NoResString)"All Charges Collect By GBL"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesPrepaidCash, (NoResString)"All Charges Prepaid Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesPrepaidCredit, (NoResString)"All Charges Prepaid Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesPrepaidByCreditCard, (NoResString)"All Charges Prepaid By Credit Card"); // IATA Text
				result.AddPair(Constants.ChargeCodes.AllChargesPrepaidByGBL, (NoResString)"All Charges Prepaid By GBL"); // IATA Text
				result.AddPair(Constants.ChargeCodes.DestinationCollectCash, (NoResString)"Destination Collect Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.DestinationCollectCredit, (NoResString)"Destination Collect Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.DestinationCollectByMCO, (NoResString)"Destination Collect By MCO"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoCharge, (NoResString)"No Charge"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoWeightCharge_OtherChargesCollect, (NoResString)"No Weight Charge - Other Charges Collect"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByCreditCard, (NoResString)"No Weight Charge - Other Charges Prepaid By Credit Card"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByGBL, (NoResString)"No Weight Charge - Other Charges Prepaid By GBL"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCash, (NoResString)"No Weight Charge - Other Charges Prepaid Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCredit, (NoResString)"No Weight Charge - Other Charges Prepaid Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCash, (NoResString)"Partial Collect Credit - Partial Prepaid Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCredit, (NoResString)"Partial Collect Credit - Partial Prepaid Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCash, (NoResString)"Partial Collect Credit Card - Partial Prepaid Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCredit, (NoResString)"Partial Collect Credit Card - Partial Prepaid Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialPrepaidCash_PartialCollectCash, (NoResString)"Partial Prepaid Cash - Partial Collect Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialPrepaidCredit_PartialCollectCash, (NoResString)"Partial Prepaid Credit - Partial Collect Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCash, (NoResString)"Partial Prepaid Credit Card - Partial Collect Cash"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCredit, (NoResString)"Partial Prepaid Credit Card - Partial Collect Credit"); // IATA Text
				result.AddPair(Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCreditCard, (NoResString)"Partial Prepaid Credit Card - Partial Collect Credit Card"); // IATA Text

				return result;
			}
		}

		public CodeDescriptionPairList SpecialEUInstructionsList
		{
			get { return Enterprise.Customs.Common.CusEntryNumberTypes.EU.EUCustomsEntryTypeList; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public CodeDescriptionPairList AsAgreed1stList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.AWBAsAgreedFirstSetType); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public CodeDescriptionPairList AsAgreed2ndList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.AWBAsAgreedSecondSetType); }
		}

		#endregion

		#region Registry Acccess

		protected virtual ZString AgentIATACode
		{
			get { return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode).KeepNumericCharacters().Left(Schema.EH_AgentIATACodeMaxLength); }
		}

		protected virtual ZString AgentAccountNo
		{
			get { return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber).Trim().Left(Schema.EH_AgentAccountNoMaxLength); }
		}

		protected virtual ZString AgentName
		{
			get { return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName).Left(Constants.AgentNameMaxLength); }
		}

		protected virtual ZString AgentPlace
		{
			get { return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity).Left(Constants.AgentPlaceMaxLength); }
		}

		#endregion

		#region Reference Numbers

		protected ZString AirlinePrefix
		{
			get
			{
				ZString masterBill = MasterBill;
				return masterBill.Length > 2 ? masterBill.Left(3) : ZString.Empty;
			}
		}

		protected ZString SerialNumber
		{
			get
			{
				ZString masterBill = MasterBill;
				return masterBill.Length > 3 ? masterBill.SubstringSafe(3, 8) : ZString.Empty;
			}
		}

		#endregion

		public virtual ZString TSASecurityStatement
		{
			get { return ZString.Empty; }
		}

		public virtual RefCountry OriginCountry
		{
			get { return null; }
		}

		public virtual RefCountry DestinationCountry
		{
			get { return null; }
		}

		public ZString ConsigneeCategory { set; get; }

		public ZString ShipperCategory { set; get; }

		public ZString NotifyPartyCategory { set; get; }

		public void SetReadOnly(bool readOnly)
		{
			this.ReadOnly = readOnly;
			AWBOtherCharges.SetReadOnlyIncludingChildren(readOnly);
			AWBAccountingInformations.SetReadOnlyIncludingChildren(readOnly);
			AWBRateLines.SetReadOnly(readOnly);
			SetSLACLineToReadOnly(readOnly);
			AWBSpecialHandlingItems.SetReadOnlyIncludingChildren(readOnly);
		}

		protected virtual void SetSLACLineToReadOnly(bool readOnly)
		{
			((ExportAWBRateLine)SLACLine).SetReadOnlyIncludingChildren(true);
		}

		public override void Delete()
		{
			AWBAccountingInformations.RemoveAndDeleteAll();
			AWBOtherCharges.RemoveAndDeleteAll();
			AWBRateLines.RemoveAndDeleteAll();
			AWBSpecialHandlingItems.RemoveAndDeleteAll();
			ChildBills.RemoveAndDeleteAll();
			ExportAWBSecurityStatusLines.RemoveAndDeleteAll();
			base.Delete();

			DeleteStackTrace = System.Environment.StackTrace;
		}

		public string DeleteStackTrace { get; private set; }

		protected bool suppressOtherChargesRefreshings;

		public void RefreshOtherChargesData()
		{
			if (!suppressOtherChargesRefreshings)
			{
				EH_OtherChargesDueAgentCOLInfo.RefreshBinding();
				EH_OtherChargesDueCarrierCOLInfo.RefreshBinding();
				EH_OtherChargesDueAgentPPDInfo.RefreshBinding();
				EH_OtherChargesDueCarrierPPDInfo.RefreshBinding();
			}
		}

		public AWBDisplayOption GetDisplayOption(ZString iata_ChargeCode, ZString ppdclt)
		{
			AWBDisplayOptionCollection collection;
			AWBDisplayOptionRegistryItem displayOptionRegistryItem;
			if (ppdclt.IsEmpty || ((displayOptionRegistryItem = GetDisplayOptionRegistryItem(ppdclt)) == null))
			{
				collection = AWBDisplayOptionCollection.GetDefault(DisplayOptionType);
			}
			else
			{
				collection = displayOptionRegistryItem.Value;
			}

			return collection[iata_ChargeCode] ?? collection[AWBDisplayOption.MissingIATACode];
		}

		protected AWBDisplayOptionRegistryItem GetDisplayOptionRegistryItem(string prepaidCollect)
		{
			if (IsHouseAirWayBill)
			{
				return prepaidCollect == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid
					? ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption
					: ExportAWBRegistry.Instance.HAWBCollectDisplayOption;
			}
			else
			{
				return prepaidCollect == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid
					? ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption
					: ExportAWBRegistry.Instance.MAWBCollectDisplayOption;
			}
		}

		protected AWBDisplayOptionType DisplayOptionType
		{
			get { return IsHouseAirWayBill ? AWBDisplayOptionType.HAWB : AWBDisplayOptionType.MAWB; }
		}

		public virtual TypeOfAWB AWBType
		{
			get
			{
				switch (EH_AWBType)
				{
					case AWBTypeList.Codes.House:
						return TypeOfAWB.House;
					case AWBTypeList.Codes.AgentMaster:
						return TypeOfAWB.AgentMaster;
					case AWBTypeList.Codes.DirectMaster:
						return TypeOfAWB.DirectMaster;
					case AWBTypeList.Codes.MasterHouse:
						return TypeOfAWB.MasterHouse;
				}

				return TypeOfAWB.UndefinedMaster;
			}
		}

		internal bool IsHouseAirWayBill
		{
			get { return AWBType == TypeOfAWB.House; }
		}

		protected internal virtual ZString MasterBill
		{
			get
			{
				var parentBill = ParentBill;
				if (parentBill == null || parentBill == this)
				{
					return EH_WayBillNumber.Replace("-", "");
				}
				else
				{
					return parentBill.MasterBill;
				}
			}
		}

		public virtual ZString UniqueReference => ZString.Empty;

		public virtual ZString HouseBill
		{
			get { return IsHouseAirWayBill ? EH_WayBillNumber : ZString.Empty; }
		}

		protected virtual ZString ConsolNumber
		{
			get
			{
				var parentBill = ParentBill;
				if (parentBill == null || parentBill == this)
				{
					return EH_AWBType + EH_WayBillNumber.Replace("-", "");
				}
				else
				{
					return parentBill.ConsolNumber;
				}
			}
		}

		protected virtual ZString CustomsEntryNumber { get { return ""; } }

		public IEnumerable<EntryNumber> CustomsEntryNumbers
		{
			get { return GetCustomsEntryNumbers() ?? Enumerable.Empty<EntryNumber>(); }
		}

		protected virtual IEnumerable<EntryNumber> GetCustomsEntryNumbers()
		{
			return null;
		}

		public IEnumerable<MovementReferenceNumber> MovementReferenceNumbers
		{
			get { return GetMovementReferenceNumbers() ?? Enumerable.Empty<MovementReferenceNumber>(); }
		}

		protected virtual IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
		{
			return null;
		}

		#region Goods Declaration Reference Number

		public IEnumerable<GoodsDeclarationReferenceNumber> GoodsDeclarationReferenceNumbers
		{
			get { return GetGoodsDeclarationReferenceNumbers() ?? Enumerable.Empty<GoodsDeclarationReferenceNumber>(); }
		}

		protected virtual IEnumerable<GoodsDeclarationReferenceNumber> GetGoodsDeclarationReferenceNumbers()
		{
			return null;
		}

		#endregion

		public virtual ZString FreightForwarderOrCarrierCode => ZString.Empty;

		public virtual ZString DetailedGoodsDescription
		{
			get { return NatureAndQtyOfGoods; }
		}

		public IEnumerable<ZString> DGCodes
		{
			get { return GetDGCodes() ?? Enumerable.Empty<ZString>(); }
		}

		protected virtual IEnumerable<ZString> GetDGCodes()
		{
			return null;
		}

		public IEnumerable<ZString> DGUNNOValues
		{
			get => GetDGUNNOValues() ?? Enumerable.Empty<ZString>();
		}

		protected virtual IEnumerable<ZString> GetDGUNNOValues()
		{
			return null;
		}

		internal void SetNatureAndQtyOfGoodsBreakingIntoLines(string goodsDescription)
		{
			NatureAndQtyOfGoods = new LineBreaker(ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength)
				.GetBrokenLines(goodsDescription)
				.Left(ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength * AWBRateLines.Count);
		}

		public virtual List<IVATCountryHandler> GetVATCountryHandlers(IFBaseMessageDetailsProvider provider) => new List<IVATCountryHandler> { new DefaultVATCountryHandler(provider) };

		public virtual List<IContactNumberCountryHandler> GetContactNumberCountryHandlers(IFBaseMessageDetailsProvider provider) => new List<IContactNumberCountryHandler>() { new DefaultContactNumberCountryHandler(provider) };

		public virtual IACASCountryHandler GetACASCountryHandler() => new ACASCountryHandler();

		public const string AirCargoImpVersionV16 = "V16";
		public const string AirCargoImpVersionV17 = "V17";

		public ZString AirCargoImpVersion
		{
			get
			{
				var airImpVersionConfig = FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.Value;
				var matchedAirlineImpVersion = airImpVersionConfig.AirlineImpVersionMappings?
					.Cast<AirlineImpVersion>().FirstOrDefault(x => string.Equals(x.AirlinePrefix, EH_AirlinePrefix, StringComparison.OrdinalIgnoreCase));
				return matchedAirlineImpVersion?.ImpVersion ?? airImpVersionConfig.DefaultImpVersion;
			}
		}

		public const int V16NameOrAddressLength = 35;
		public const int V17NameOrAddressLength = 70;

		public ZInt AirMessageMaxNameAddressLength
		{
			get
			{
				return AirCargoImpVersion == AirCargoImpVersionV16 ? V16NameOrAddressLength : V17NameOrAddressLength;
			}
		}

		#region EORIValidation

		public virtual bool IsImportToICS2Zone => false;

		public virtual bool HasDestinationInIcs2Zone => false;

		#endregion

		#region Agent Details now opening up

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(AgentDetailsAreNotModifiable))]
		[MaxLength(Constants.AgentIATACodeMaxLength)]
		public ZString EH_AgentIATACodeFormatted
		{
			get
			{
				return GetFormattedAgentIATACode(AgentDetailsAreNotModifiable
													? AgentIATACode.KeepNumericCharacters().Left(Schema.EH_AgentIATACodeMaxLength)
													: EH_AgentIATACode);
			}
			set
			{
				EH_AgentIATACode = value.KeepNumericCharacters().Left(Schema.EH_AgentIATACodeMaxLength);
				Validation.ValidateEH_AgentIATACodeFormatted();
				EH_AgentIATACodeFormattedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EH_AgentIATACodeFormattedInfo
		{
			get { return GetZPropertyInfo(Schema.EH_AgentIATACodeFormatted); }
		}

		/// <summary>
		/// Format the Agent IATA Code to match standard formatting of "12-3 4567/0000".
		/// </summary>
		ZString GetFormattedAgentIATACode(ZString source)
		{
			if (source.IsEmpty)
			{
				return ZString.Empty;
			}

			ZString cassAddress = source.SubstringSafe(7, 4);
			return source.Left(2) + "-" + source.SubstringSafe(2, 1) + " " + source.SubstringSafe(3, 4) + (cassAddress.IsEmpty ? "" : "/" + cassAddress);
		}

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(AgentDetailsAreNotModifiable))]
		public override ZString EH_AgentAccountNo
		{
			get { return AgentDetailsAreNotModifiable ? AgentAccountNo : base.EH_AgentAccountNo; }
			set { base.EH_AgentAccountNo = value; }
		}

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(AgentDetailsAreNotModifiable))]
		public override ZString EH_AgentName
		{
			get { return AgentDetailsAreNotModifiable ? AgentName : base.EH_AgentName; }
			set { base.EH_AgentName = value; }
		}

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(AgentDetailsAreNotModifiable))]
		public override ZString EH_AgentPlace
		{
			get { return AgentDetailsAreNotModifiable ? AgentPlace : base.EH_AgentPlace; }
			set { base.EH_AgentPlace = value; }
		}

		#endregion

		#region EH_AgentApprovedExportNumber

		public ZPropertyInfo EH_AgentApprovedExporterNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EH_AgentApprovedExporterNumber); }
		}

		public virtual ZString EH_AgentApprovedExporterNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IssuingAgentName

		[BusinessObjectMaxLengthTestExclude]
		public override ZString EH_IssuingAgentName
		{
			get { return base.EH_IssuingAgentName; }
			set
			{
				if (value.Length > EH_IssuingAgentNameInfo.MaxLength)
				{
					value = value.SubstringSafe(0, EH_IssuingAgentNameInfo.MaxLength);
					IsEH_IssuingAgentNameTruncated = true;
				}
				else
				{
					IsEH_IssuingAgentNameTruncated = false;
				}

				base.EH_IssuingAgentName = value;
			}
		}

		#endregion

		#region IssuingAgentAddress

		[BusinessObjectMaxLengthTestExclude]
		public override ZString EH_IssuingAgentAddress1
		{
			get { return base.EH_IssuingAgentAddress1; }
			set
			{
				IsEH_IssuingAgentAddress1Truncated = value.Length > EH_IssuingAgentAddress1Info.MaxLength;
				base.EH_IssuingAgentAddress1 = value.Left(EH_IssuingAgentAddress1Info.MaxLength);
			}
		}

		[BusinessObjectMaxLengthTestExclude]
		public override ZString EH_IssuingAgentAddress2
		{
			get { return base.EH_IssuingAgentAddress2; }
			set
			{
				IsEH_IssuingAgentAddress2Truncated = value.Length > EH_IssuingAgentAddress2Info.MaxLength;
				base.EH_IssuingAgentAddress2 = value.Left(EH_IssuingAgentAddress2Info.MaxLength);
			}
		}

		public bool IsEH_IssuingAgentNameTruncated { get; private set; }
		public bool IsEH_IssuingAgentAddress1Truncated { get; private set; }
		public bool IsEH_IssuingAgentAddress2Truncated { get; private set; }

		#endregion

		#region Harmonised Codes

		public virtual StringCollectionX GetAvailableHarmonisedCodes()
		{
			return new StringCollectionX();
		}

		public virtual List<ZString> GetShipmentReferencesWithoutHSCode() => new List<ZString>();

		public virtual ZByte LineCountOfLastHSCode => 0;

		public virtual bool HasExtraHSCodes => false;

		#endregion

		#region Additional Security Information

		public virtual List<ZString> AdditionalSecurityInformations => new List<ZString>();

		#endregion

		#region Scheduled Arrival Date

		public virtual bool ShouldSetScheduledArrivalDate => false;

		#endregion

		#region Security Statement

		public virtual bool ShouldSetSecurityStatement => false;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.Value)
			{
				EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
			}

			SetDefaultValuesForStandaloneAWB();

			if (!AgentDetailsAreNotModifiable)
			{
				SetupDefaultAgentDetails();
			}
		}

		protected void SetupDefaultAgentDetails()
		{
			EH_AgentIATACodeFormatted = AgentIATACode;
			EH_AgentAccountNo = AgentAccountNo;
			EH_AgentName = AgentName.Left(ExportAWBHeader.Schema.EH_AgentNameMaxLength);
			EH_AgentPlace = AgentPlace.Left(ExportAWBHeader.Schema.EH_AgentPlaceMaxLength);
		}

		protected bool AgentDetailsAreNotModifiable
		{
			get { return EH_GB_UserBranch.IsEmpty; }
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(EH_GB_UserBranch); }
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ExportAWBHeaderFetchStrategy(this);
		}

		#endregion

		public virtual List<ExportAWBExportStatement> ExportStatements_CargoIMP
		{
			get { return new List<ExportAWBExportStatement>(); }
		}
	}
}
