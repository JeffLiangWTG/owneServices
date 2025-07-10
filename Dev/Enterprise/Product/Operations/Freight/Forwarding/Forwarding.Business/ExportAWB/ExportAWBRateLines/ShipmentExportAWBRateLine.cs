using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ShipmentExportAWBHeader), "AWBRateLines")]
	public class ShipmentExportAWBRateLine : ExportAWBRateLine
	{
		public ShipmentExportAWBRateLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected new ShipmentExportAWBHeader Master
		{
			get { return (ShipmentExportAWBHeader)base.Master; }
		}

		public override ZString ER_RateClass
		{
			get { return base.ER_RateClass; }
			set
			{
				if (ER_RateClass != value)
				{
					base.ER_RateClass = value;
					if (Master != null)
					{
						Master.EH_TotalWeightPPDInfo.RefreshBinding();
						Master.EH_TotalWeightCOLInfo.RefreshBinding();
						Master.EH_TotalPPDInfo.RefreshBinding();
						Master.EH_TotalCOLInfo.RefreshBinding();
					}
				}
			}
		}

		protected override CodeDescriptionPairList RateClassCoreList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Master != null && Master.EH_WeightBTH)
				{
					result = new CodeDescriptionPairList(OLookUpEditType.AWBPrepayCollect);
				}
				else
				{
					result = base.RateClassCoreList;
				}
				return result;
			}
		}

		public override ZString ER_NatureAndQtyOfGoodsType
		{
			get { return base.ER_NatureAndQtyOfGoodsType; }
			set
			{
				base.ER_NatureAndQtyOfGoodsType = value;
				if (Master != null)
				{
					Master.MarkAsNeedingValidation();
				}
			}
		}
		public override bool RequireHSCode
		{
			get
			{
				if (Master == null)
				{
					return false;
				}

				if (Master.HasInboundToICS2Zone || Master.IsImportToExportFromTransitingThroughUnitedArabEmirates)
				{
					return true;
				}

				return Master.DestinationCountryCode.ToString() == Core.Constants.CountryCodes.Argentina;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Prefix search string")]
		public override bool IsHSCodeLine
		{
			get
			{
				const string hsCodeLineRegex = @"^([\d][\d.]{0,17},? ?)+$";

				var rateLines = Master?.AWBRateLines.Cast<ExportAWBRateLine>().ToArray();
				var firstHSCodeLineIndex = rateLines.IndexOf(l => l.NatureAndQtyOfGoods.Text.StartsWith("HS Codes: "));
				var firstHSCodeLine = firstHSCodeLineIndex >= 0 ? rateLines[firstHSCodeLineIndex] : null;

				if (firstHSCodeLine == null || firstHSCodeLine.ER_LineCount > ER_LineCount)
				{
					return false;
				}

				var indexOffset = firstHSCodeLineIndex - firstHSCodeLine.ER_LineCount;

				if (firstHSCodeLine.ER_LineCount == ER_LineCount)
				{
					return true;
				}

				var result = false;

				for (var i = firstHSCodeLineIndex + 1; i < rateLines.Length && rateLines[i].ER_LineCount <= ER_LineCount; i++)
				{
					if (Regex.IsMatch(rateLines[i].NatureAndQtyOfGoods.Text, hsCodeLineRegex))
					{
						result = true;
					}
					else
					{
						return false;
					}
				}

				return result;
			}
		}
	}
}
