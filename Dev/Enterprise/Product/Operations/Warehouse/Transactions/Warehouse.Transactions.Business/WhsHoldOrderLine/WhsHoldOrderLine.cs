using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsHoldOrderLine : NonPersistentBusinessObject
	{
		#region Constructor

		public WhsHoldOrderLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Related Entities

		public OrgSupplierPart Product
		{
			get { return Factory.Load<OrgSupplierPart>(ProductPK); }
		}

		#endregion

		#region Properties

		#region ProductPK

		[RelatedBusinessObject("Product")]
		public ZGuid ProductPK
		{
			get { return productPK; }
			set { SetNonPersistentPropertyValue(ProductPKInfo, ref productPK, value); }
		}

		public ZPropertyInfo ProductPKInfo
		{
			get { return GetZPropertyInfo(nameof(ProductPK)); }
		}

		ZGuid productPK;

		#endregion

		#region FromHoldCode

		public ZString FromHoldCode
		{
			get { return fromHoldCode; }
			set { SetNonPersistentPropertyValue(FromHoldCodeInfo, ref fromHoldCode, value); }
		}

		public ZPropertyInfo FromHoldCodeInfo
		{
			get { return GetZPropertyInfo(nameof(FromHoldCode)); }
		}

		ZString fromHoldCode;

		#endregion

		#region ToHoldCode

		public ZString ToHoldCode
		{
			get { return toHoldCode; }
			set { SetNonPersistentPropertyValue(ToHoldCodeInfo, ref toHoldCode, value); }
		}

		public ZPropertyInfo ToHoldCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ToHoldCode)); }
		}

		ZString toHoldCode;

		#endregion

		#region HoldReason

		public ZString HoldReason
		{
			get => holdReason;
			set => SetNonPersistentPropertyValue(HoldReasonInfo, ref holdReason, value);
		}

		public ZPropertyInfo HoldReasonInfo => GetZPropertyInfo(nameof(HoldReason));

		ZString holdReason;

		#endregion

		#region Quantity

		public ZDecimal Quantity
		{
			get { return quantity; }
			set { SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value); }
		}

		public ZPropertyInfo QuantityInfo
		{
			get { return GetZPropertyInfo(nameof(Quantity)); }
		}

		ZDecimal quantity;

		#endregion

		#region Attributes

		#region ExpiryDate

		public ZDate ExpiryDate
		{
			get { return expiryDate; }
			set { SetNonPersistentPropertyValue(ExpiryDateInfo, ref expiryDate, value); }
		}

		public ZPropertyInfo ExpiryDateInfo
		{
			get { return GetZPropertyInfo(nameof(ExpiryDate)); }
		}

		ZDate expiryDate;

		#endregion

		#region PackingDate

		public ZDate PackingDate
		{
			get { return packingDate; }
			set { SetNonPersistentPropertyValue(PackingDateInfo, ref packingDate, value); }
		}

		public ZPropertyInfo PackingDateInfo
		{
			get { return GetZPropertyInfo(nameof(PackingDate)); }
		}

		ZDate packingDate;

		#endregion

		#region PartAttrib1

		public ZString PartAttrib1
		{
			get { return partAttrib1; }
			set { SetNonPersistentPropertyValue(PartAttrib1Info, ref partAttrib1, value); }
		}

		public ZPropertyInfo PartAttrib1Info
		{
			get { return GetZPropertyInfo(nameof(PartAttrib1)); }
		}

		ZString partAttrib1;

		#endregion

		#region PartAttrib2

		public ZString PartAttrib2
		{
			get { return partAttrib2; }
			set { SetNonPersistentPropertyValue(PartAttrib2Info, ref partAttrib2, value); }
		}

		public ZPropertyInfo PartAttrib2Info
		{
			get { return GetZPropertyInfo(nameof(PartAttrib2)); }
		}

		ZString partAttrib2;

		#endregion

		#region PartAttrib3

		public ZString PartAttrib3
		{
			get { return partAttrib3; }
			set { SetNonPersistentPropertyValue(PartAttrib3Info, ref partAttrib3, value); }
		}

		public ZPropertyInfo PartAttrib3Info
		{
			get { return GetZPropertyInfo(nameof(PartAttrib3)); }
		}

		ZString partAttrib3;

		#endregion

		#region SerialNumber

		public ZString SerialNumber
		{
			get { return serialNumber; }
			set { SetNonPersistentPropertyValue(SerialNumberInfo, ref serialNumber, value); }
		}

		public ZPropertyInfo SerialNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SerialNumber)); }
		}

		ZString serialNumber;

		#endregion

		#endregion

		#endregion

		#region Finalise

		public bool Finalise(WhsWarehouse warehouse, OrgHeader client)
		{
			var result = true;
			var inventoryLineAndUnitsToChangeDictionary = new Dictionary<WhsDocketLine, decimal>();

			if (TryMatchInventoryLines(warehouse, client, inventoryLineAndUnitsToChangeDictionary))
			{
				foreach (var key in inventoryLineAndUnitsToChangeDictionary.Keys)
				{
					result &= ChangeHoldCode(key, inventoryLineAndUnitsToChangeDictionary[key]);
				}
			}
			else
			{
				result = false;
				AddErrorMessage();
			}

			return result;
		}

		#region TryMatchInventoryLines

		bool TryMatchInventoryLines(WhsWarehouse warehouse, OrgHeader client, Dictionary<WhsDocketLine, decimal> inventoryLineAndUnitsToChangeDictionary)
		{
			var quantityMatched = new Decimal(0);
			var matchingDocketLines = GetMatchingInventoryLineResults(warehouse.PK, client.PK);

			var enumerator = ((IEnumerable<InventoryLineResult>)matchingDocketLines).GetEnumerator();
			while (quantityMatched < Quantity && enumerator.MoveNext())
			{
				// Must check hold code and AvailableToTransferQuantity in memory as they may be changed in the factory
				var query = new ZQuery(WhsDocketLineSchema.PK, enumerator.Current.DocketLinePK);
				query.AddToFilter(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, FromHoldCode);

				var docketLine = Factory.LoadTop1<WhsDocketLine>(query);
				var inventory = docketLine != null && docketLine.Inventory.Count > 0 ? docketLine.Inventory[0] : null;

				if (inventory != null && inventory.WI_AvailableToTransferQuantity > 0)
				{
					var quantityToChange = Math.Min(inventory.WI_AvailableToTransferQuantity, Quantity - quantityMatched);
					inventoryLineAndUnitsToChangeDictionary.Add(docketLine, quantityToChange);
					quantityMatched += quantityToChange;
				}
			}

			return quantityMatched == Quantity;
		}

		#endregion

		#region ChangeHoldCode

		bool ChangeHoldCode(WhsDocketLine inventoryLine, decimal unitsToChange)
		{
			inventoryLine.HeldCodeChangeQuantity = unitsToChange;
			inventoryLine.HeldCodeToChangeTo = ToHoldCode;
			inventoryLine.HoldReasonToChangeTo = HoldReason;
			return inventoryLine.ChangeInventoryHeldCode(true);
		}

		#endregion

		#region GetMatchingDocketLineResults

		DynamicBusinessObjectCollection<InventoryLineResult> GetMatchingInventoryLineResults(ZGuid warehousePK, ZGuid clientPK)
		{
			var query = new ZStringBuilder(@"
SELECT
	WE_PK
FROM
	dbo.WhsDocketLine InventoryLine
	CROSS APPLY
	(
		SELECT
			SUM(WZ_Units) AS CommittedUnits
		FROM
			dbo.WhsPickLine
			-- only consider non-finalised picklines
			JOIN dbo.WhsDocketLine TransactionLine ON TransactionLine.WE_PK = WZ_WE_TransactionLine
			-- picklines should not exist on Cancelled Dockets, we can safely ignore them if they do (as they should be deleted anyway)
			JOIN dbo.WhsDocket ON WD_PK = TransactionLine.WE_WD AND WD_WW_Whs = @WarehousePK and WD_OH_Client = @ClientPK AND WD_DocketStatus <> 'CAN'
		WHERE
			-- If a PickLine is not Picked it is Committed
			WZ_PickedDateTime IS NULL
			AND WZ_WE_InventoryLine = InventoryLine.WE_PK
	) PickLines
WHERE
	InventoryLine.WE_StockOnHand > 0
	AND (InventoryLine.WE_StockOnHand - COALESCE(PickLines.CommittedUnits, 0)) > 0
	AND InventoryLine.WE_DocketLineStatus = 'FIN'
	AND @ProductPK = InventoryLine.WE_OP
	AND @FromHoldCode = InventoryLine.WE_WHC_NKCurrentInventoryHeldCode
	AND EXISTS 
	(
		SELECT 
			1
		FROM 
			dbo.WhsDocket 
		WHERE 
			InventoryLine.WE_WD = WD_PK
			AND WD_WW_Whs = @WarehousePK 
			AND WD_OH_Client = @ClientPK
	)
");

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@WarehousePK", warehousePK, WhsDocketSchema.WD_WW_Whs);
			parameters.Add("@ClientPK", clientPK, WhsDocketSchema.WD_OH_Client);
			parameters.Add("@ProductPK", ProductPK, WhsDocketLineSchema.WE_OP);
			parameters.Add("@FromHoldCode", FromHoldCode, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode);

			if (!PartAttrib1.IsEmpty)
			{
				query.Append(" AND WE_PartAttrib1 <> '' AND WE_PartAttrib1 = @PartAttrib1"); // This is an SQL Statement.....
				parameters.Add("@PartAttrib1", PartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
			}

			if (!PartAttrib2.IsEmpty)
			{
				query.Append(" AND WE_PartAttrib2 <> '' AND WE_PartAttrib2 = @PartAttrib2"); // This is an SQL Statement.....
				parameters.Add("@PartAttrib2", PartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
			}

			if (!PartAttrib3.IsEmpty)
			{
				query.Append(" AND WE_PartAttrib3 <> '' AND WE_PartAttrib3 = @PartAttrib3"); // This is an SQL Statement.....
				parameters.Add("@PartAttrib3", PartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
			}

			if (!SerialNumber.IsEmpty)
			{
				query.Append(" AND WE_SerialNumber <> '' AND WE_SerialNumber = @SerialNumber"); // This is an SQL Statement.....
				parameters.Add("@SerialNumber", SerialNumber, WhsDocketLineSchema.WE_SerialNumber);
			}

			if (PackingDate.IsValid)
			{
				query.Append(" AND WE_PackingDate IS NOT NULL AND WE_PackingDate = @PackingDate"); // This is an SQL Statement.....
				parameters.Add("@PackingDate", PackingDate, WhsDocketLineSchema.WE_PackingDate);
			}

			if (ExpiryDate.IsValid)
			{
				query.Append(" AND WE_ExpiryDate IS NOT NULL AND WE_ExpiryDate = @ExpiryDate"); // This is an SQL Statement.....
				parameters.Add("@ExpiryDate", ExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
			}

			query.Append(" ORDER BY InventoryLine.WE_StockOnHand - COALESCE(PickLines.CommittedUnits, 0) DESC"); // This is an SQL Statement.....

			var result = new DynamicBusinessObjectCollection<InventoryLineResult>(Factory);
			result.Load(query.ToStringWithNewLineBetweenAppends(), parameters);

			return result;
		}

		#endregion

		#region InventoryLineResult

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class InventoryLineResult : DynamicBusinessObject
		{
			#region Constructor

			public InventoryLineResult(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#endregion

			#region DocketLinePK

			public ZGuid DocketLinePK
			{
				get { return new ZGuid(this[WhsDocketLineSchema.Constants.PK]); }
			}

			#endregion
		}

		#endregion

		#region AddErrorMessage

		void AddErrorMessage()
		{
			var errorMessageBuilder = new ZStringBuilder(Res.GetString("9bbc1433-38cc-485c-9b12-55c114cd4799", "Could not match inventory for"));
			errorMessageBuilder.Append(Res.GetString("e3ae3630-1f2c-42da-8f4e-8847497ebc0d", "Product:{0}", Product.OP_PartNum));
			errorMessageBuilder.Append(Res.GetString("f488e4e6-3894-4122-9562-5c98cdd8f8aa", "Quantity:{0}", Quantity.ToStringTrimZeros()));
			errorMessageBuilder.Append(Res.GetString("710cca8d-22dd-4447-8df7-1ebaa0f24e9d", "Current Hold Code:{0}", FromHoldCode));
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("7f9c1da9-1b78-4010-a62a-5df3f61b0bb2", "Part Attribute 1:"), PartAttrib1);
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("f687f8b7-46a2-4032-8b1c-2335ee5e79bb", "Part Attribute 2:"), PartAttrib2);
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("1965dd39-3454-4718-a3be-8f6258197dcf", "Part Attribute 3:"), PartAttrib3);
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("ce154c56-8110-47c6-80d3-2008166a06ef", "Serial Number:"), SerialNumber);
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("637fd8cb-5dd7-45c6-b230-754282392d0b", "Packing Date:"), PackingDate.ToString());
			errorMessageBuilder.AppendIfNotEmpty(Res.GetString("af42efbe-6f04-4311-94f8-e4b788e35211", "Expiry Date:"), ExpiryDate.ToString());
			AddRowError(errorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		#endregion

		#endregion
	}
}
