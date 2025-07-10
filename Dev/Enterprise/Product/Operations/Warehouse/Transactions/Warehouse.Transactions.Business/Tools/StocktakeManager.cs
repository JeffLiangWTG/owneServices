using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Tools
{
	public class StocktakeManager
	{
		#region Constructors

		public StocktakeManager(BusinessObjectFactory factory, ZString area, ZString pickMethod, WhsWarehouse warehouse, GlbStaff staff)
		{
			Argument.NotNull(factory, "Factory");
			Argument.NotNull(warehouse, "Warehouse");
			Argument.NotNull(staff, "Staff");

			Factory = factory;
			Area = area.Trim().ToUpper();
			PickMethod = pickMethod.Trim().ToUpper();
			Warehouse = warehouse;
			Staff = staff;
		}

		public StocktakeManager(BusinessObjectFactory factory, WhsWarehouse warehouse, GlbStaff staff)
				: this(factory, "", "", warehouse, staff)
		{
		}

		readonly BusinessObjectFactory Factory;

		#endregion

		#region Properties

		#region Stocktake

		public WhsStocktake Stocktake { get; private set; }

		#endregion

		#region Area

		public ZString Area { get; private set; }

		#endregion

		#region PickMethod

		public ZString PickMethod { get; private set; }

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse { get; private set; }

		#endregion

		#region Staff

		public GlbStaff Staff { get; private set; }

		#endregion

		#region LinesToCount

		public WhsStocktakeLineCollectionND LinesToCount => linesToCount ?? (linesToCount = new WhsStocktakeLineCollectionND(Factory));
		WhsStocktakeLineCollectionND linesToCount;

		#endregion

		#endregion

		#region FindAndAssignNextStocktake

		public bool FindAndAssignNextStocktake(ZString referenceOrEmpty)
		{
			Stocktake = null;
			LinesToCount.RemoveAll();
			var result = false;

			Stocktake = FindUnfinalizedStocktake(referenceOrEmpty);

			if (Stocktake != null)
			{
				AddFetchHintsAndSortStocktakeLines(Stocktake.Lines);
				using (((IActiveBusinessObjectCollection)Stocktake.Lines).SuspendListChanged())
				{
					var lines = Stocktake.Lines;
					foreach (var line in lines)
					{
						AddAndAssignLine(line);
					}
				}

				if (LinesToCount.Count > 0)
				{
					Factory.Save();
					result = true;
				}
				else
				{
					Stocktake = null;
				}
			}

			return result;
		}

		#region FindUnfinalizedStocktake

		WhsStocktake FindUnfinalizedStocktake(ZString referenceOrEmpty)
		{
			WhsStocktake result = null;

			if (!referenceOrEmpty.IsEmpty)
			{
				result = FindUnfinalizedStocktake_ByNumber(referenceOrEmpty);
			}
			else
			{
				result = FindNextUnfinalizedStocktake();
			}

			return result;
		}

		#region FindUnfinalizedStocktake_ByNumber

		WhsStocktake FindUnfinalizedStocktake_ByNumber(ZString stocktakeNumber)
		{
			var query = GetBaseStocktakeQuery();
			query.AddToFilter(JoinCondition.And, WhsStocktakeSchema.WS_StocktakeNumber, SQLComparisonOperator.Equal, stocktakeNumber);

			return Factory.LoadTop1<WhsStocktake>(query);
		}

		#endregion

		#region FindNextUnfinalizedStocktake

		WhsStocktake FindNextUnfinalizedStocktake()
		{
			WhsStocktake result = null;

			var query = GetBaseStocktakeQuery();
			var stocktakes = Factory.Load<WhsStocktake>(query);

			if (stocktakes.Length > 0)
			{
				// Already sorted by stocktake date, now we take the first stocktake having an assigned line
				// if none then we can take the first one.
				result = stocktakes.FirstOrDefault(s => s.Lines.Any(l => IsValidLineForStocktake(l) && l.WU_GS_NKVerifiedBy == Staff.GS_Code));
				if (result == null)
				{
					result = stocktakes[0];
				}
			}

			return result;
		}

		#endregion

		#region GetBaseStocktakeQuery

		ZDBOnlyQuery GetBaseStocktakeQuery()
		{
			var result = new ZDBOnlyQuery(typeof(WhsStocktake));

			// Stocktake Status
			result.AddToFilter(JoinCondition.And, WhsStocktakeSchema.WS_StocktakeStatus, SQLComparisonOperator.Equal, CodeLists.StocktakeStatus.Codes.Loaded);

			// Warehouse
			result.AddToFilter(JoinCondition.And, WhsStocktakeSchema.WS_WW_Whs, SQLComparisonOperator.Equal, Warehouse.PK);

			// Stocktake Lines
			var stocktakeLineSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			//// Line Status
			stocktakeLineSubQuery.AddToFilter(JoinCondition.And, WhsStocktakeLineSchema.WU_Status, SQLComparisonOperator.Equal, CodeLists.StocktakeLineStatus.Codes.Open);
			//// Area / Pick Method
			var areaCode = Area == "ANY" ? ZString.Empty : Area;
			var pickMethodCode = PickMethod == "ANY" ? ZString.Empty : PickMethod;
			if (!areaCode.IsEmpty || !pickMethodCode.IsEmpty)
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);

				if (!areaCode.IsEmpty)
				{
					var areaSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea);
					areaSubQuery.AddToFilter(WhsAreaSchema.WA_Name, SQLComparisonOperator.Equal, areaCode);
					locationSubQuery.AddSubQuery(areaSubQuery, JoinCondition.And);
				}

				if (!pickMethodCode.IsEmpty)
				{
					locationSubQuery.AddToFilter(JoinCondition.And, WhsLocationViewSchema.WLV_PickMethod, SQLComparisonOperator.Equal, pickMethodCode);
				}

				stocktakeLineSubQuery.AddSubQuery(locationSubQuery, JoinCondition.And);
			}
			//// Current Count
			var countIterationQuery = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			countIterationQuery.AddToFilter(GetFiltersForCountIteration(1), JoinCondition.Or);
			countIterationQuery.AddToFilter(GetFiltersForCountIteration(2), JoinCondition.Or);
			countIterationQuery.AddToFilter(GetFiltersForCountIteration(3), JoinCondition.Or);
			stocktakeLineSubQuery.AddToFilter(countIterationQuery, JoinCondition.And);
			result.AddSubQuery(stocktakeLineSubQuery, JoinCondition.And);

			// Order by
			result.OrderBy = WhsStocktakeSchema.WS_StocktakeDate.Name + OrderByClause.Ascending;

			return result;
		}

		ZDBOnlyQuery GetFiltersForCountIteration(byte countIteration)
		{
			if (countIteration < 1 || countIteration > 3)
			{
				throw new ArgumentException("Count Iteration must be greater than 0 or less than 3.", nameof(countIteration));
			}

			SchemaStringColumn verifiedByColumn = null;
			SchemaDateTimeColumn verifiedDateColumn = null;

			if (countIteration == 1)
			{
				verifiedByColumn = WhsStocktakeLineSchema.WU_GS_NKVerifiedBy;
				verifiedDateColumn = WhsStocktakeLineSchema.WU_DateVerified;
			}
			else if (countIteration == 2)
			{
				verifiedByColumn = WhsStocktakeLineSchema.WU_Count2VerifiedBy;
				verifiedDateColumn = WhsStocktakeLineSchema.WU_Count2DateVerified;
			}
			else if (countIteration == 3)
			{
				verifiedByColumn = WhsStocktakeLineSchema.WU_Count3VerifiedBy;
				verifiedDateColumn = WhsStocktakeLineSchema.WU_Count3DateVerified;
			}

			var stocktakeLineIterationSubQuery = new ZDBOnlyQuery(typeof(WhsStocktakeLine));

			// Total Counts
			stocktakeLineIterationSubQuery.AddToFilter(WhsStocktakeLineSchema.WU_TotalCounts, SQLComparisonOperator.Equal, countIteration);

			// Verified date
			stocktakeLineIterationSubQuery.AddToFilter(JoinCondition.And, verifiedDateColumn, SQLComparisonOperator.Equal, ZDateTime.Empty);

			// Verified by
			var verifiedByQuery = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			verifiedByQuery.AddToFilter(verifiedByColumn, SQLComparisonOperator.Equal, ZString.Empty);
			verifiedByQuery.AddToFilter(JoinCondition.Or, verifiedByColumn, SQLComparisonOperator.Equal, Staff.GS_Code);
			stocktakeLineIterationSubQuery.AddToFilter(verifiedByQuery, JoinCondition.And);

			return stocktakeLineIterationSubQuery;
		}

		#endregion

		#endregion

		#region AddFetchHintsAndSortStocktakeLines

		void AddFetchHintsAndSortStocktakeLines(WhsStocktakeLineCollection stocktakeLines)
		{
			stocktakeLines.ForEach(l => AddFetchHintsForSort(l));
			Factory.AddFetchHint(WhsRowSchema.Instance, GetRowQuery(stocktakeLines.Select(l => l.WU_WL).Distinct().ToArray()));
			stocktakeLines.ApplySort(new SortStocktakeLines());
		}

		void AddFetchHintsForSort(WhsStocktakeLine stocktakeLine)
		{
			Factory.AddFetchHint(WhsLocationViewSchema.PK, stocktakeLine.WU_WL);
			Factory.AddFetchHint(OrgSupplierPartSchema.PK, stocktakeLine.WU_OP);
			Factory.AddFetchHint(OrgHeaderSchema.PK, stocktakeLine.WU_OH_Client);
		}

		ZQuery GetRowQuery(ZGuid[] locationPKs)
		{
			var subQueryLoc = new ZDBOnlySubQuery(typeof(AutoWhsLocationView), WhsLocationViewSchema.WLV_WR);
			subQueryLoc.AddToFilter(WhsLocationViewSchema.PK, locationPKs);
			var queryRow = new ZDBOnlyQuery(typeof(WhsRow));
			queryRow.AddSubQuery(subQueryLoc, JoinCondition.And);

			return queryRow;
		}

		#endregion

		#region AddAndAssignLine

		void AddAndAssignLine(WhsStocktakeLine stocktakeLine)
		{
			if (IsValidLineForStocktake(stocktakeLine))
			{
				stocktakeLine.CurrentCountVerifiedBy = Staff.GS_Code;
				LinesToCount.Add(stocktakeLine);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		bool IsValidLineForStocktake(WhsStocktakeLine stocktakeLine)
		{
			var location = stocktakeLine.Location;
			return (Area.IsEmpty || Area == "ANY" || Area.EqualsIgnoringCase(location.PickingArea.WA_Name)) // stocktake line is located in correct area.
					 && (PickMethod.IsEmpty || PickMethod == "ANY" || PickMethod.EqualsIgnoringCase(location.WLV_PickMethod)) // stocktake line has a correct pick method.
					 && (stocktakeLine.CurrentCountVerifiedBy.IsEmpty || stocktakeLine.CurrentCountVerifiedBy == Staff.GS_Code) // stocktake line wasn't assigned for someone else.
					 && (location.Row.Warehouse.PK.Equals(Warehouse.PK)) // stocktake line is located in correct warehouse.
					 && (stocktakeLine.CurrentCountVerifiedDate.IsEmpty) // stocktake line wasn't verified yet.
					 && (stocktakeLine.WU_Status == CodeLists.StocktakeLineStatus.Codes.Open); // stocktake line wasn't closed.
		}

		#endregion

		#endregion

		#region SetStocktakeLineEmptyLocationConfirm

		public string SetStocktakeLineEmptyLocationConfirm(WhsStocktakeLine stocktakeLine, bool isEmpty)
		{
			return !isEmpty
				? DeleteStockLine(stocktakeLine)
				: ValidateAndSaveStockLine(stocktakeLine, Staff.GS_Code, ZDateTime.Now);
		}

		string DeleteStockLine(WhsStocktakeLine stocktakeLine)
		{
			var result = ValidateStocktakeLineUpdatePermission(stocktakeLine);
			if (result.IsNullOrEmpty())
			{
				stocktakeLine.Delete();
				result = SaveStocktakeLine(stocktakeLine);
			}

			return result;
		}

		#endregion

		#region SetStocktakeLineCountQuantity

		public string SetStocktakeLineCountQuantity(WhsStocktakeLine stocktakeLine, ZString packType, ZDecimal count)
		{
			return SetLastCount(stocktakeLine, packType, count, Staff.GS_Code, ZDateTime.Now, false);
		}

		#endregion

		#region AddStocktakeLineCountQuantity

		public string AddStocktakeLineCountQuantity(WhsStocktakeLine stocktakeLine, ZString packType, ZDecimal count)
		{
			return SetLastCount(stocktakeLine, packType, count, Staff.GS_Code, ZDateTime.Now, true);
		}

		#endregion

		#region AddNewStocktakeLine

		public AddWhsStocktakeLineResult AddNewStocktakeLine(ZGuid stocktakePk, ZGuid supplierPartPk, ZGuid locationPk, ZGuid clientPK, ZString packUQ, ZString attr1, ZString attr2, ZString attr3, ZString serial,
				ZDate expiryDate, ZDate packingDate, ZString palletID, ZDecimal countQuantity, ZString inventoryStatus)
		{
			var validationResult = string.Empty;

			Stocktake = Factory.Load<WhsStocktake>(stocktakePk);
			if (Stocktake == null)
			{
				validationResult = ResString.GetMultilingualString("b27a25c2-39f4-42ef-ace2-c1007dc558fb", "Stocktake not found");
			}

			var clientOrgPK = ZGuid.Empty;
			if (validationResult.IsNullOrEmpty())
			{
				clientOrgPK = clientPK.IsValid ? clientPK : Stocktake.WS_OH_Client;
				var client = Factory.Load<OrgHeader>(clientOrgPK);
				if (client == null)
				{
					validationResult = ResString.GetMultilingualString("2bfc02f4-875a-4d20-be34-942efc3d6f26", "Client not found");
				}
			}

			if (validationResult.IsNullOrEmpty())
			{
				var supplierPart = Factory.Load<OrgSupplierPart>(supplierPartPk);
				if (supplierPart == null)
				{
					validationResult = ResString.GetMultilingualString("3d1afc7c-416b-4f3c-80ec-875a3d8ea102", "Part not found");
				}
			}

			return validationResult.IsNullOrEmpty()
				? AddNewStocktakeLineCore(stocktakePk, supplierPartPk, locationPk, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, palletID, countQuantity, inventoryStatus, clientOrgPK)
				: new AddWhsStocktakeLineResult(null, validationResult);
		}

		AddWhsStocktakeLineResult AddNewStocktakeLineCore(ZGuid stocktakePk, ZGuid supplierPartPk, ZGuid locationPk, ZString packUQ, ZString attr1, ZString attr2, ZString attr3, ZString serial, ZDate expiryDate, ZDate packingDate, ZString palletID, ZDecimal countQuantity, ZString inventoryStatus, ZGuid clientOrgPK)
		{
			var line = FindExistingManualLine(stocktakePk, supplierPartPk, locationPk, clientOrgPK, palletID, attr1, attr2, attr3, packingDate, expiryDate, inventoryStatus);
			if (line == null)
			{
				line = Stocktake.Lines.AddNew();
				line.WU_OH_Client = clientOrgPK;
				line.WU_OP = supplierPartPk;
				line.WU_WL = locationPk;
				line.WU_ExpiryDate = expiryDate;
				line.WU_PackingDate = packingDate;
				line.WU_PartAttrib1 = attr1;
				line.WU_PartAttrib2 = attr2;
				line.WU_PartAttrib3 = attr3;
				line.WU_SerialNumber = serial;
				line.WU_PalletID = palletID;
				line.WU_InventoryStatus = inventoryStatus;
			}

			var error = SetLastCount(line, packUQ, countQuantity, Staff.GS_Code, ZDateTime.Now, true);
			var stocktakeLineToReturn = error.IsNullOrEmpty() ? line : null;
			return new AddWhsStocktakeLineResult(stocktakeLineToReturn, error);
		}

		WhsStocktakeLine FindExistingManualLine(ZGuid stocktakePk, ZGuid supplierPartPk, ZGuid locationPk, ZGuid clientPK, ZString palletId, ZString partAttribute1, ZString partAttribute2, ZString partAttribute3, ZDate packingDate, ZDate expiryDate, ZString inventoryStatus)
		{
			var stocktakeLineQuery = new ZDBOnlyQuery(typeof(WhsStocktakeLine));
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_WS, stocktakePk);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_IsManuallyAdded, true);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_OP, supplierPartPk);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_WL, locationPk);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_OH_Client, clientPK);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_PalletID, palletId);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_PartAttrib1, partAttribute1);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_PartAttrib2, partAttribute2);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_PartAttrib3, partAttribute3);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_PackingDate, packingDate);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_ExpiryDate, expiryDate);
			stocktakeLineQuery.AddToFilter(WhsStocktakeLineSchema.WU_InventoryStatus, inventoryStatus);

			return Factory.LoadTop1<WhsStocktakeLine>(stocktakeLineQuery);
		}

		#endregion

		#region SetLastCount

		string SetLastCount(WhsStocktakeLine stocktakeLine, ZString packType, ZDecimal count, ZString verifiedBy, ZDateTime verifiedDate, bool addQuantity = false)
		{
			var result = ValidateStocktakeLineCountQuantity(stocktakeLine, count, addQuantity);
			if (result.IsNullOrEmpty())
			{
				stocktakeLine.WU_F3_NKPackType = packType;// setting pack type has to be done before setting pack quantity for conversion
				if (addQuantity)
				{
					stocktakeLine.WU_PackQty += count;
				}
				else
				{
					stocktakeLine.WU_PackQty = count;
				}

				result = ValidateAndSaveStockLine(stocktakeLine, verifiedBy, verifiedDate);
			}

			return result;
		}

		string ValidateStocktakeLineCountQuantity(WhsStocktakeLine stocktakeLine, ZDecimal countQuantity, bool addQuantity)
		{
			var result = string.Empty;
			if (!addQuantity)
			{
				var verifiedDate = stocktakeLine.CurrentCountVerifiedDate;
				if (!verifiedDate.IsEmpty)
				{
					result = ResString.GetMultilingualString("57b0eeb7-67fc-4a03-b7d6-f3ae63503f89", "Unable to update stocktake line count quantity. This stocktake line has already been counted.");
				}
			}

			if (result.IsNullOrEmpty() && countQuantity < 0)
			{
				result = ResString.GetMultilingualString("f8a9446c-3ba6-452c-a9d4-c0bb375f65fb", "The count quantity cannot be negative.");
			}

			return result;
		}

		string ValidateAndSaveStockLine(WhsStocktakeLine stocktakeLine, ZString verifiedBy, ZDateTime verifiedDate)
		{
			var result = ValidateStocktakeLineUpdatePermission(stocktakeLine);
			if (result.IsNullOrEmpty())
			{
				stocktakeLine.CurrentCountVerifiedBy = verifiedBy;
				stocktakeLine.CurrentCountVerifiedDate = verifiedDate;

				result = SaveStocktakeLine(stocktakeLine);
			}

			return result;
		}

		string ValidateStocktakeLineUpdatePermission(WhsStocktakeLine stocktakeLine)
		{
			var verifiedBy = stocktakeLine.CurrentCountVerifiedBy;
			return !verifiedBy.IsEmpty && !verifiedBy.Equals(Staff.GS_Code)
				? ResString.GetMultilingualString("fe58870d-d6ee-4a97-9ff6-2f2114769d86", "Unable to update stocktake line count quantity. This stocktake line is not assigned or assigned to another operator.")
				: string.Empty;
		}

		#endregion

		#region SaveStocktakeLine

		string SaveStocktakeLine(WhsStocktakeLine stocktakeLine)
		{
			var result = string.Empty;

			stocktakeLine.RunPreSaveValidation();
			if (stocktakeLine.HasErrors)
			{
				var errorMessage = new ZStringBuilder(stocktakeLine.Notifications.Select(n => n.Message));
				result = ResString.GetMultilingualString("16e3b89f-ef76-4e17-84ad-3b8ee4758e97", "Invalid stocktake line:\r\n{0}", errorMessage.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				stocktakeLine.Factory.Save();
			}

			return result;
		}

		#endregion
	}
}
