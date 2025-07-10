using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedCalculator
	{
		#region Constructor

		public WhsBondedCalculator(BusinessObjectFactory factory, string entryKey, short entryLineNo)
		{
			this.factory = factory;
			bondedEntryKey = WhsBondedWarehouseAttribute.BuildKey(entryKey, entryLineNo);
		}
		readonly BusinessObjectFactory factory;
		readonly string bondedEntryKey;

		#region BondedWhsQtyClosingBalance

		ZDecimal BondedWhsQtyClosingBalance
		{
			get
			{
				if (!bondedWhsQtyClosingBalance.HasValue)
				{
					bondedWhsQtyClosingBalance = CalculateWhsBondedQtyClosingBalance(bondedEntryKey);
				}
				return bondedWhsQtyClosingBalance.Value;
			}
		}
		ZDecimal? bondedWhsQtyClosingBalance;

		#endregion

		#region CalculateWhsBondedQtyClosingBalance

		ZDecimal CalculateWhsBondedQtyClosingBalance(string entryKey)
		{
			var resultSet = new DynamicBusinessObjectCollection(factory);

			var @params = new ZSqlParameterCollection();
			@params.Add("@EntryKey", entryKey, WhsDocketLineSchema.WE_BondedEntryKey);

			string rawQuery = $@"
			select
					sum(
						IIF(d.WD_DocketType = '{DocketType.Codes.Order}',
							ISNULL(pl.WZ_Units, 0) * -1, dl.WE_TransactionQuantity)
					) AS BondedWhsQty
			from
				dbo.WhsDocketLine dl
				left join dbo.WhsPickLine pl on WZ_WE_TransactionLine = dl.WE_PK
				join dbo.WhsDocket d on d.WD_PK = dl.WE_WD
				join dbo.WhsBondedWarehouseAttribute a on a.WB_ParentID = dl.WE_PK
			where 1=1
				and WD_FinalisedDate IS NOT NULL
				and	WE_BondedEntryKey <> ''
				and	WE_BondedEntryKey = @EntryKey";

			ZDecimal result = 0m;
			resultSet.Load(rawQuery, @params);

			if (resultSet.Count > 0)
			{
				result = new ZDecimal(resultSet[0]["BondedWhsQty"]);
			}
			return result;
		}

		#endregion

		#region GetAvailableBondedWhsQty

		public ZDecimal GetAvailableBondedWhsQty()
		{
			return BondedWhsQtyClosingBalance;
		}

		#endregion

		#endregion

		#region SetCustomsData

		public static void SetCustomsData(WhsDocketLine line)
		{
			var bondedInventoryLine = line.PickLines.Count > 0 ? line.PickLines[0].Inventory : null;
			if (bondedInventoryLine == null && line.WE_TransactionQuantity < 0)
			{
				var customsData = line.CustomsData;
				customsData.WB_EntryDate = ZDateTime.Empty;
				customsData.WB_CustomsQty = 0m;
				customsData.WB_CustomsUnitOfQty = "";
				customsData.WB_CustomsSecondQuantity = 0m;
				customsData.WB_CustomsSecondUnitQty = "";
				customsData.WB_CustomsThirdQuantity = 0m;
				customsData.WB_CustomsThirdUnitQty = "";
				customsData.WB_ValueForDuty = 0m;

				customsData.WB_TILV = 0m;
				customsData.WB_RX_NKTILVCurrency = "";
				customsData.WB_RN_NKCountryOfOrigin = "";
				customsData.WB_AddInfo = "";
				customsData.WB_BondedWhsQty = 0m;
				customsData.WB_BondedWhsUnitOfQty = "";
				customsData.WB_WB_InwardsEntry = ZGuid.Empty;
			}
			else if (bondedInventoryLine != null)
			{
				var lineCustomsData = line.CustomsData;
				var bondedLineCustomsData = bondedInventoryLine.CustomsData;

				lineCustomsData.WB_EntryDate = bondedLineCustomsData.WB_EntryDate;
				lineCustomsData.WB_CustomsQty = bondedLineCustomsData.WB_CustomsQty;
				lineCustomsData.WB_CustomsUnitOfQty = bondedLineCustomsData.WB_CustomsUnitOfQty;
				lineCustomsData.WB_CustomsSecondQuantity = bondedLineCustomsData.WB_CustomsSecondQuantity;
				lineCustomsData.WB_CustomsThirdQuantity = bondedLineCustomsData.WB_CustomsThirdQuantity;
				lineCustomsData.WB_CustomsSecondUnitQty = bondedLineCustomsData.WB_CustomsSecondUnitQty;
				lineCustomsData.WB_CustomsThirdUnitQty = bondedLineCustomsData.WB_CustomsThirdUnitQty;
				lineCustomsData.WB_ValueForDuty = bondedLineCustomsData.WB_ValueForDuty;

				var tilv = new Money(bondedLineCustomsData.WB_TILV, bondedLineCustomsData.TILVCurrency);
				lineCustomsData.WB_TILV = tilv.Amount;
				lineCustomsData.WB_RX_NKTILVCurrency = tilv.Currency?.Code ?? "";
				lineCustomsData.WB_RN_NKCountryOfOrigin = bondedLineCustomsData.WB_RN_NKCountryOfOrigin;
				lineCustomsData.WB_AddInfo = bondedLineCustomsData.WB_AddInfo;
				lineCustomsData.WB_BondedWhsQty = bondedLineCustomsData.WB_BondedWhsQty;
				lineCustomsData.WB_BondedWhsUnitOfQty = bondedLineCustomsData.WB_BondedWhsUnitOfQty;
				lineCustomsData.WB_WB_InwardsEntry = bondedLineCustomsData.PK;
			}
		}

		#endregion
	}
}
