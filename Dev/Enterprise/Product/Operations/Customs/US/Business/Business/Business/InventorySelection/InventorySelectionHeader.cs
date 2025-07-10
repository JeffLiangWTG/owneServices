using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class InventorySelectionHeader : DeclarationInventorySelectionHeader
	{
		public InventorySelectionHeader(JobDeclaration declaration)
			: base(declaration)
		{
			isConsumptionFTZ = declaration.IsENSFormalImportAndConsumptionFTZ;
			AllowWithdrawalOfMultipleEntryDetailsCore = !declaration.IsExWarehouseEntryType;
		}
		readonly bool isConsumptionFTZ;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override ZString GetInwardEntryNumber(BaseJobComInvoiceLine invoiceLine) => isConsumptionFTZ ? ((JobComInvoiceLine)invoiceLine).US_WHSEntryNumber : CustomsEntryKey;

		protected override ZInt GetInwardEntryLineNumber(BaseJobComInvoiceLine invoiceLine) => ((JobComInvoiceLine)invoiceLine).US_WHSEntryLineNo;

		protected override ZString InwardEntryNumberHumanReadable => isConsumptionFTZ
			? Res.GetString("F15B1919-EFA9-4F95-AE50-E7A5E7686FE7", "WHS Ent./FTZ Adm No.")
			: Res.GetString("112FF480-B8E0-481B-BFB9-B5DB2BBAB9EB", "Entry No. On Declaration");

		protected override ZString InwardEntryLineNumberHumanReadable => Res.GetString("D31D7B4F-2BAD-4749-95C5-49ADCCFB0299", "WHS/FTZ Line No.");

		protected override FilterBusinessObjectDefaults GetFilterDefaultsCore()
		{
			var result = base.GetFilterDefaultsCore();
			var customsEntryKey = CustomsEntryKey;
			if (!isConsumptionFTZ && !customsEntryKey.IsEmpty)
			{
				result.Add(new FilterBusinessObjectDefault("Customs Entry Key", "Property", customsEntryKey, false));
			}
			return result;
		}

		ZString CustomsEntryKey => !Declaration.US_WHSEntryFilerCode.IsEmpty || !Declaration.US_WHSEntryNumber.IsEmpty
			? ZString.Format("{0}-{1}", Declaration.US_WHSEntryFilerCode, Declaration.US_WHSEntryNumber)
			: ZString.Empty;

		protected override void UpdateParentData()
		{
			base.UpdateParentData();
			if (!isConsumptionFTZ)
			{
				if (Declaration.US_WHSEntryNumber.IsEmpty || Declaration.US_WHSEntryFilerCode.IsEmpty)
				{
					var line = SelectedLines.OfType<WhsInventoryWrapper>().FirstOrDefault();

					if (line != null)
					{
						if (IsGroupByCarton)
						{
							UpdateWHSEntryNumberAndFilerCode(line.CustomsEntryKey);
						}
						else
						{
							UpdateWHSEntryNumberAndFilerCode(new Common.EntryLineCodeParser(line.CustomsEntryKey).EntryNumber);
						}
					}
				}

				UpdateQtyInWarehouse();
			}
		}

		void UpdateQtyInWarehouse()
		{
			var decEntryNumber = Declaration.US_WHSEntryFilerCode + "-" + Declaration.US_WHSEntryNumber;

			var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.PK);
			subQuery.AddToFilter(WhsDocketLineSchema.WE_BondedEntryKey, SQLComparisonOperator.StartsWith, decEntryNumber);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(IWhsInventoryView));
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, subQuery, JoinCondition.And);

			var whsInventories = Factory.Load<IWhsInventoryView>(query);
			Declaration.US_QtyInWHBeforeWithdrawal = whsInventories.Sum(x => x.WI_TotalUnits);
		}

		void UpdateWHSEntryNumberAndFilerCode(ZString entryNumber)
		{
			if (entryNumber.IndexOf('-') == 3)
			{
				Declaration.US_WHSEntryFilerCode = entryNumber.Left(3);
				Declaration.US_WHSEntryNumber = entryNumber.SubstringSafe(4);
			}
		}

		protected override bool AllowWithdrawalOfMultipleEntryDetailsCore { get; }

		protected override string GetCannotWithdrawProductHavingDifferentEntryNumber()
		{
			return Invariant($"Products which have different entry numbers cannot be withdrawn on entry type '{Declaration.US_EntryType}'.");
		}

		void PopulateUSCustomsThirdQuantityAndUnit(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty == invoiceLine.JI_CustomsThirdUnitQty)
			{
				ZDecimal customs3ndQty = ratio * whsBondedWarehouseAttribute.WB_CustomsThirdQuantity;
				invoiceLine.JI_CustomsThirdQuantity = customs3ndQty.Round(JobComInvoiceLineSchema.JI_CustomsThirdQuantity.Scale);
			}
		}

		protected override void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			PopulateUSCustomsThirdQuantityAndUnit(invoiceLine, whsBondedWarehouseAttribute, ratio);

			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (whsBondedWarehouseAttribute != null)
			{
				addInfos = WarehouseCustomsAddInfoWrapper.GetAddInfosDetails(usInvoiceLine, addInfos);
				usInvoiceLine.GetAddInfo().LoadPropertiesFromString(AddInfoParser.Serialise(addInfos), false);
				usInvoiceLine.US_WHSEntryLineNo = whsBondedWarehouseAttribute.WB_EntryLineNo;
				if (isConsumptionFTZ)
				{
					usInvoiceLine.US_WHSEntryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
				}
			}

			ApplyRatioToData(ratio, usInvoiceLine);

			if (!usInvoiceLine.US_AMMVPerUnit.IsEmpty)
			{
				usInvoiceLine.JI_LinePrice = Round(usInvoiceLine.JI_LinePrice - (invoiceQuantity * usInvoiceLine.US_AMMVPerUnit), 5);
			}
			else if (!usInvoiceLine.US_AMMVPercentage.IsEmpty)
			{
				usInvoiceLine.JI_LinePrice = Round(usInvoiceLine.JI_LinePrice / (1 + usInvoiceLine.US_AMMVPercentage / 100), 5);
			}

			var receiveLine = inventoryWrapper.ReceiveLine;
			if (receiveLine != null)
			{
				var childLines = new List<JobComInvoiceLine>(usInvoiceLine.ChildLines.OrderBy(x => x.JI_LineNo));
				var productRelatedLines = new List<JobComInvoiceLine>(usInvoiceLine.ProductRelatedLines.OrderBy(x => x.JI_LineNo));
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, receiveLine.PK);
				query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.WarehouseCustomsAddInfo);
				foreach (var warehouseCustomsAddInfoWrapper in inventoryWrapper.Factory.Load<IWarehouseCustomsAddInfo>(query).Select(x => new WarehouseCustomsAddInfoWrapper(x)).OrderBy(o => o.LineNo))
				{
					var list = warehouseCustomsAddInfoWrapper.IsProductRelatedLine ? productRelatedLines : childLines;
					var relatedOrChildLine = list.FirstOrDefault(x => x.JI_Tariff == warehouseCustomsAddInfoWrapper.Tariff);
					if (relatedOrChildLine == null)
					{
						relatedOrChildLine = warehouseCustomsAddInfoWrapper.IsProductRelatedLine ? usInvoiceLine.AddProductRelatedInvoiceLine() : usInvoiceLine.AddSecondaryInvoiceLine();
					}
					else
					{
						list.Remove(relatedOrChildLine);
					}
					UpdateChildLine(relatedOrChildLine, ratio, warehouseCustomsAddInfoWrapper);
				}
			}
		}

		void UpdateChildLine(JobComInvoiceLine invoiceLine, ZDecimal ratio, WarehouseCustomsAddInfoWrapper warehouseCustomsAddInfoWrapper)
		{
			invoiceLine.JI_Tariff = warehouseCustomsAddInfoWrapper.Tariff;
			invoiceLine.JI_InvoiceQuantity = Round(ratio * warehouseCustomsAddInfoWrapper.InvoiceQuantity, 5);
			invoiceLine.JI_InvoiceUQ = warehouseCustomsAddInfoWrapper.InvoiceQuantityUnit;
			invoiceLine.JI_LinePrice = Round(ratio * warehouseCustomsAddInfoWrapper.LinePrice, 2);
			invoiceLine.JI_CustomsQuantity = warehouseCustomsAddInfoWrapper.CustomsQuantity;
			WarehouseCustomsAddInfoWrapper.SetupQty(ratio, invoiceLine.JI_CustomsQuantityInfo, invoiceLine.JI_CustomsUnitQty, warehouseCustomsAddInfoWrapper.CustomsQuantityUnit, (ZDecimal)invoiceLine.JI_CustomsQuantityInfo.Value);
			invoiceLine.GetAddInfo().LoadPropertiesFromString(warehouseCustomsAddInfoWrapper.AddInfo, false);
			WarehouseCustomsAddInfoWrapper.SetupQty(ratio, invoiceLine.JI_CustomsSecondQuantityInfo, invoiceLine.JI_CustomsSecondUnitQty, warehouseCustomsAddInfoWrapper.SecondCustomsQuantityUnit, warehouseCustomsAddInfoWrapper.SecondQty);
			WarehouseCustomsAddInfoWrapper.SetupQty(ratio, invoiceLine.JI_CustomsThirdQuantityInfo, invoiceLine.JI_CustomsThirdUnitQty, warehouseCustomsAddInfoWrapper.ThirdCustomsQuantityUnit, warehouseCustomsAddInfoWrapper.ThirdQty);
			ApplyRatioToData(ratio, invoiceLine);
		}

		void ApplyRatioToData(ZDecimal ratio, JobComInvoiceLine usInvoiceLine)
		{
			foreach (var fieldNeedRatio in FieldsNeedToApplyRatio)
			{
				var info = usInvoiceLine.ZPropertyInfoHash.GetPropertySafe(fieldNeedRatio);
				if (info != null)
				{
					var value = info.Value;
					if (!value.IsEmpty)
					{
						var valueTYpe = value.GetType();
						if (valueTYpe == DecimalType)
						{
							var decimalPlaces = usInvoiceLine.GetDecimalPlacesMetaData(info.Name);
							info.Value = Round(new ZDecimal(ratio * (ZDecimal)value), decimalPlaces > -1 ? decimalPlaces : 5);
						}
						else if (valueTYpe == IntType)
						{
							info.Value = ZInt.ParseSafe(new ZDecimal(ratio * (ZInt)value).ToString(0), 0);
						}
						else if (valueTYpe == ShortType)
						{
							info.Value = ZShort.ParseSafe((new ZDecimal(ratio * (ZShort)value)).ToString(0), 0);
						}
					}
				}
			}
		}

		Type DecimalType
		{
			get { return decimalType ?? (decimalType = typeof(ZDecimal)); }
		}
		Type decimalType;

		Type IntType
		{
			get { return intType ?? (intType = typeof(ZInt)); }
		}
		Type intType;

		Type ShortType
		{
			get { return shortType ?? (shortType = typeof(ZShort)); }
		}
		Type shortType;

		string[] FieldsNeedToApplyRatio
		{
			get
			{
				if (fieldsNeedToApplyRatio == null)
				{
					fieldsNeedToApplyRatio = new[]
					{
						JobComInvoiceLine.Schema.US_98GoodsValue,
						JobComInvoiceLine.Schema.US_GrossWeight,
						JobComInvoiceLine.Schema.US_WeightNET,
						JobComInvoiceLine.Schema.US_ADDDepositValue,
						JobComInvoiceLine.Schema.US_ADDuty,
						JobComInvoiceLine.Schema.US_ADDQty,
						JobComInvoiceLine.Schema.US_CVDDepositValue,
						JobComInvoiceLine.Schema.US_CVDQty,
						JobComInvoiceLine.Schema.US_CVDuty,
						JobComInvoiceLine.Schema.US_CustomsValue,
						JobComInvoiceLine.Schema.US_DDTCQuantity,
						JobComInvoiceLine.Schema.US_Duty,
						JobComInvoiceLine.Schema.US_SupDuty,
						JobComInvoiceLine.Schema.US_SupQty1,
						JobComInvoiceLine.Schema.US_SupQty2,
						JobComInvoiceLine.Schema.US_SupQty3,
						JobComInvoiceLine.Schema.US_VisaQty
					};
				}
				return fieldsNeedToApplyRatio;
			}
		}
		string[] fieldsNeedToApplyRatio;
	}
}
