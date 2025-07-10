using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsWarehouseValidation : AutoWhsWarehouseValidation
	{
		#region Constructor

		public WhsWarehouseValidation(AutoWhsWarehouse parent)
			: base(parent)
		{
			MaxColumnInWarehouse = 0;
			MaxLevelInWarehouse = 0;
			MaxTrayInWarehouse = 0;
		}

		#endregion

		// persistent

		#region CheckWW_WarehouseCode

		protected override void CheckWW_WarehouseCode()
		{
			base.CheckWW_WarehouseCode();
			if (Parent.WW_WarehouseCode.IsEmpty)
			{
				Parent.WW_WarehouseCodeInfo.AddError(Res.GetString("987b8327-ba4f-49f6-a7dd-5c8eb20228c3", "Please enter a Warehouse Code"));
			}
			else
			{
				CheckWW_WarehouseCodeForDuplicates();
			}
		}

		#endregion

		#region CheckWW_UsesVehicleBookingIntegration

		protected override void CheckWW_UsesVehicleBookingIntegration()
		{
			base.CheckWW_UsesVehicleBookingIntegration();
			CheckMappedOrgCodeWhenEnableVehicleBookingIntegration();
		}

		void CheckMappedOrgCodeWhenEnableVehicleBookingIntegration()
		{
			if (Parent.WW_UsesVehicleBookingIntegration && Parent.WW_WarehouseType.EqualsIgnoringCase(WarehouseTypes.Codes.ContainerYard))
			{
				var unMappedOrgCodes = new List<string>();
				unMappedOrgCodes.AddRange(GetUnMappedOrgCodes<ICYDReceiveAdvice>(CYDReceiveAdviceSchema.YRA_ToDate));
				unMappedOrgCodes.AddRange(GetUnMappedOrgCodes<ICYDReleaseAdvice>(CYDReleaseAdviceSchema.YRE_ToDate));
				if (unMappedOrgCodes.Count > 0)
				{
					Parent.WW_UsesVehicleBookingIntegrationInfo.AddError(Res.GetString("b148b7a1-1282-40ae-8b9c-d6ddc9c04a1a", "Active organizations in pre-arrival instruction and release order must have VBS community code set before VBS integration can be enabled."));
				}
			}
		}

		List<string> GetUnMappedOrgCodes<T>(SchemaColumn toDateProperty) where T : class
		{
			var unMappedOrgCodes = new List<string>();
			var query = new ZDBOnlyQuery(typeof(T));
			query.AddToFilter(toDateProperty, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			var availableAdvices = Parent.Factory.Load<T>(query);

			var communityCodePair = new KeyValuePair<string, string>();

			foreach (var advice in availableAdvices)
			{
				if (toDateProperty == CYDReceiveAdviceSchema.YRA_ToDate)
				{
					communityCodePair = ((ICYDReceiveAdvice)advice).MappedCommunityCode;
				}
				else
				{
					communityCodePair = ((ICYDReleaseAdvice)advice).MappedCommunityCode;
				}

				if (!string.IsNullOrEmpty(communityCodePair.Key) && string.IsNullOrEmpty(communityCodePair.Value))
				{
					unMappedOrgCodes.Add(communityCodePair.Key);
				}
			}

			return unMappedOrgCodes;
		}

		#endregion

		#region CheckWW_WarehouseCodeForDuplicates

		protected void CheckWW_WarehouseCodeForDuplicates()
		{
			var dupKeyCheck = new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, Parent.WW_WarehouseCode);
			dupKeyCheck.AddToFilter(WhsWarehouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.LoadTop1<WhsWarehouse>(dupKeyCheck) != null)
			{
				Parent.WW_WarehouseCodeInfo.AddError(Res.GetString("f65d57c5-6d93-462c-9557-00223574b9b4", "This Warehouse Code is already used by another warehouse. Please select a unique code for this warehouse"));
			}
		}

		#endregion

		#region CheckWW_WarehouseName

		protected override void CheckWW_WarehouseName()
		{
			base.CheckWW_WarehouseName();
			if (Parent.WW_WarehouseName.IsEmpty)
			{
				Parent.WW_WarehouseNameInfo.AddError(Res.GetString("9eec3ec1-9900-4774-8d30-86b4b70ad41d", "Please enter a Warehouse Name"));
			}
			else
			{
				CheckWW_WarehouseNameForDuplicates();
			}
			TranslatableDataFieldAttribute.Validate(Parent.WW_WarehouseNameInfo);
		}

		#endregion

		#region CheckWW_WarehouseNameForDuplicates

		protected void CheckWW_WarehouseNameForDuplicates()
		{
			var dupKeyCheck = new ZQuery(WhsWarehouseSchema.WW_WarehouseName, Parent.WW_WarehouseName);
			dupKeyCheck.AddToFilter(WhsWarehouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.LoadTop1<WhsWarehouse>(dupKeyCheck) != null)
			{
				Parent.WW_WarehouseNameInfo.AddError(Res.GetString("ff946f38-b755-4bb0-822b-29f3de1354e9", "This Warehouse Name is already used by another warehouse. Please select a unique name for this warehouse"));
			}
		}

		#endregion

		#region CheckWW_WarehouseType

		protected override void CheckWW_WarehouseType()
		{
			base.CheckWW_WarehouseType();

			MandatoryValidation.CheckEntered(Parent.WW_WarehouseTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WW_WarehouseTypeInfo);
			CheckWW_WarehouseType_CanBeVirtualWarehouse();
			CheckBranchIsUniquePerWarehousePerWarehouseTypeForActiveWarehouses(Parent.WW_WarehouseTypeInfo);
			CheckWW_WarehouseType_CannotBeChangedToOrFromTransitIfReferencedAlready();
			CheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder();
			CheckWW_WarehouseType_CannotBeChangedToOrFromContainerYardIfReferencedAlready();
			CheckWW_WarehouseType_WarehouseTypeModifiedWithInvalidUNDGState();
		}

		void CheckWW_WarehouseType_CanBeVirtualWarehouse()
		{
			if (!Parent.WW_WarehouseTypeInfo.HasErrors() && Parent.WW_IsVirtualWarehouse)
			{
				if (Parent.WW_WarehouseType != WarehouseTypes.Codes.Product && Parent.WW_WarehouseType != WarehouseTypes.Codes.FreeTradeZone)
				{
					Parent.WW_WarehouseTypeInfo.AddError(Res.GetString("f0e36c4a-ac66-48da-97b1-e5ffd42dc101", "This type of warehouse cannot be virtual warehouse."));
				}
			}
		}

		#region CheckWW_WarehouseType_CannotBeChangedToOrFromTransitIfReferencedAlready

		void CheckWW_WarehouseType_CannotBeChangedToOrFromTransitIfReferencedAlready()
		{
			if (!Parent.WW_WarehouseTypeInfo.HasErrors()
					 && Parent.IsInDatabase
					 && Parent.WW_WarehouseType == WarehouseTypes.Codes.Transit ^ Parent.WW_WarehouseTypeInfo.OriginalValue.Equals(WarehouseTypes.Codes.Transit))
			{
				bool changingToTransit = Parent.WW_WarehouseType == WarehouseTypes.Codes.Transit;
				AddErrorIfWarehouseIsInUse(checkNonTransitTables: changingToTransit);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddErrorIfWarehouseIsInUse(bool checkNonTransitTables)
		{
			var sql = new ZStringBuilder();
			var whereClause = string.Join("\r\nUNION ", ColumnsReferencingWarehouse // May be a part of SQL expression.
				.Where(c => checkNonTransitTables ^ c.TableName.ToUpper(CultureInfo.InvariantCulture).StartsWith("WHSITEM", System.StringComparison.OrdinalIgnoreCase))
				.Select(c => string.Format(CultureInfo.InvariantCulture, "SELECT NULL as Value FROM {0} WHERE {1} = @WarehousePK", c.TableName, c.Name)));

			sql.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"SELECT 1 WHERE EXISTS ({0})", whereClause)); // May be a part of SQL expression.

			using (var command = Db.Connection.Command(sql.ToString())) // No other way to easily Query multiple tables without access to all their types
			{
				command.AddParameterBasedOnDbColumn("@WarehousePK", Parent.PK.ToGuid(), WhsWarehouseSchema.PK);
				if (command.ExecuteScalar() != null)
				{
					Parent.WW_WarehouseTypeInfo.AddError(Res.GetString("d56debe6-5337-4e84-bc2d-d9bd3bf6dc52",
						"Cannot change Warehouse Type as this Warehouse is already in use."));
				}
			}
		}

		// Cached because obviously the DB Schema will never change while app is running
		SchemaColumn[] ColumnsReferencingWarehouse
		{
			get { return Parent.Factory.GetCachedValue("WhsWarehouseValidation|ColumnsReferencingWarehouse", GetColumnsReferencingWarehouse); }
		}

		SchemaColumn[] GetColumnsReferencingWarehouse()
		{
			var sql = string.Format(Culture.Invariant, @" 
SELECT
	OBJECT_NAME(ForeignKeys.parent_object_id) as TableName,
	COL_NAME(ForeignKeyColumns.parent_object_id, ForeignKeyColumns.parent_column_id) as ColumnName
FROM
	sys.foreign_keys as ForeignKeys
	JOIN sys.foreign_key_columns as ForeignKeyColumns ON ForeignKeys.OBJECT_ID = ForeignKeyColumns.constraint_object_id
	JOIN sys.tables Tables ON Tables.OBJECT_ID = ForeignKeyColumns.referenced_object_id
WHERE
	 OBJECT_NAME (ForeignKeys.referenced_object_id) = '{0}'
", WhsWarehouseSchema.Constants.TableName);  // Part of SQL expression.

			var collection = new DynamicBusinessObjectCollection(Parent.Factory);
			collection.Load(sql);

			var result = new List<SchemaColumn>(collection.Count);

			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			foreach (DynamicBusinessObject bizO in collection)
			{
				var tableName = (ZString)bizO["TableName"];
				if (!IsTableExcludedFromWarehouseTypeCheck(tableName))
				{
					result.Add(schemaResolver.GetSchemaColumn((ZString)bizO["ColumnName"], tableName));
				}
			}

			return result.ToArray();
		}

		bool IsTableExcludedFromWarehouseTypeCheck(string tableName)
		{
			return
				tableName == WhsRowSchema.Constants.TableName ||
				tableName == WhsAreaSchema.Constants.TableName ||
				tableName == "WhsPutawayLocationCache" ||
				tableName == "GateTransportCFSDetail";
		}

		#endregion

		#region CheckWW_WarehouseType_CannotBeChangedToOrFromContainerYardIfReferencedAlready

		public void CheckWW_WarehouseType_CannotBeChangedToOrFromContainerYardIfReferencedAlready()
		{
			if (!Parent.WW_WarehouseTypeInfo.HasErrors() && Parent.WW_WarehouseTypeInfo.HasChanges && Parent.IsInDatabase && Parent.WW_IsActive && Parent.WW_WarehouseType == WarehouseTypes.Codes.ContainerYard)
			{
				var changeFromTransit = Parent.WW_WarehouseTypeInfo.OriginalValue.Equals(WarehouseTypes.Codes.Transit);
				AddErrorIfWarehouseIsInUse(checkNonTransitTables: !changeFromTransit);
			}
		}

		#endregion

		#region CheckWarehouseTypeModifiedWithInvalidUNDGState

		void CheckWW_WarehouseType_WarehouseTypeModifiedWithInvalidUNDGState()
		{
			if (!Parent.WW_WarehouseTypeInfo.HasErrors() && Parent.WarehouseTypeModifiedWithInvalidUNDGState)
			{
				Parent.WW_WarehouseTypeInfo.AddWarning(Res.GetString("5f62fa14-ae5a-4ec2-8ba1-2c03f789bd8b", "Attempted to change to a Warehouse Type which does not support UNDG Thresholds. Disable Threshold Limits and clear Warning Percentage to change Warehouse Type."));
			}
		}

		#endregion

		#region CheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder

		void CheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder()
		{
			if (!Parent.WW_WarehouseTypeInfo.HasErrors() && Parent.WW_WarehouseTypeInfo.HasChanges && IsFTZWarehouseThatUsesPermitsAndIsReferencedByUnfinalisedPickForFTZCustomsOrder())
			{
				var countryCode = Parent.CountryCode;
				countryCode = BondedHelper.IsCountrySupportedForFTZPermits(countryCode) ? countryCode : GetOriginalCountryCode();

				Parent.WW_WarehouseTypeInfo.AddError(Res.GetString("9CAECA1C-7CA2-443E-8133-7B15D452F7B4",
					"Cannot change the Warehouse Type to/from FTZ in {0} because there are un-finalized Picks or Orders with no pick in the Warehouse.", countryCode));
			}
		}

		bool IsFTZWarehouseThatUsesPermitsAndIsReferencedByUnfinalisedPickForFTZCustomsOrder()
		{
			return Parent.IsInDatabase
				&& Parent.WW_IsActive
				&& (BondedHelper.IsCountrySupportedForFTZPermits(Parent.CountryCode) || BondedHelper.IsCountrySupportedForFTZPermits(GetOriginalCountryCode()))
				&& (Parent.WW_WarehouseType.Equals(WarehouseTypes.Codes.FreeTradeZone) || Parent.WW_WarehouseTypeInfo.OriginalValue.Equals(WarehouseTypes.Codes.FreeTradeZone))
				&& IsReferencedByCustomsOrderNotPickedOrUnfinalisedPick();
		}

		ZString GetOriginalCountryCode()
		{
			var originalAddress = (ZGuid)Parent.WW_OA_WarehouseAddressInfo.OriginalValue;
			var originalOrgAddress = Parent.Factory.Load<OrgAddress>(originalAddress);
			return originalOrgAddress.GetCountryCode();
		}

		// TODO: Share constants between Transactions and Environment somehow
		bool IsReferencedByCustomsOrderNotPickedOrUnfinalisedPick()
		{
			var orderQuery = new ZDBOnlyQuery(typeof(IWhsOrder));
			orderQuery.AddToFilter(WhsDocketSchema.WD_WP, null);

			var pickSubQuery = new ZDBOnlySubQuery(typeof(IWhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, "FIN");
			orderQuery.AddSubQuery(pickSubQuery, JoinCondition.Or);

			orderQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, Parent.PK);
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, "ORD");
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, new[] { "CUS", "CPS" });
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, "CAN");

			return Parent.Factory.ExistsInDatabase(WhsDocketSchema.Constants.TableName, orderQuery);
		}

		#endregion

		#endregion

		#region CheckWW_IsActive

		protected override void CheckWW_IsActive()
		{
			base.CheckWW_IsActive();
			CheckWarehouseHasSOH();
		}

		void CheckWarehouseHasSOH()
		{
			if (!Parent.WW_IsActive)
			{
				var query = new ZDBOnlyQuery(typeof(IWhsInventoryView));
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, Parent.PK);
				query.AddSubQuery(locationSubQuery, JoinCondition.And);

				if (Parent.Factory.ExistsInDatabase(WhsInventoryViewSchema.Constants.TableName, query))
				{
					Parent.WW_IsActiveInfo.AddError(CannotDeactivateAWarehouseWithSOHError);
				}
			}
		}

		#endregion

		#region CheckWW_IsVirtualWarehouse

		protected override void CheckWW_IsVirtualWarehouse()
		{
			base.CheckWW_IsVirtualWarehouse();
			CheckThereAreNoInwardProcessingAreasWhenMakingAWarehouseNonVirtual();

			void CheckThereAreNoInwardProcessingAreasWhenMakingAWarehouseNonVirtual()
			{
				if (!Parent.WW_IsVirtualWarehouse &&
					(!Parent.IsInDatabase || Parent.WW_IsVirtualWarehouseInfo.HasChanges) &&
					Parent.Areas.Any(a => a.WA_AreaType == AreaTypes.Codes.InwardProcessing))
				{
					Parent.WW_IsVirtualWarehouseInfo.AddError(Res.GetString("b9318ff5-66cf-49bd-b537-cc57e5b5eca6", "Warehouses with Inward Processing Areas must be marked as Virtual."));
				}
			}
		}

		#endregion

		#region CannotDeactivateAWarehouseWithSOHError

		public static string CannotDeactivateAWarehouseWithSOHError => Res.GetString("aa36fa14-3ded-4f83-a60b-4fec4af90fec", "This warehouse cannot be deactivated because it has current stock on hand quantities. You must remove these quantities first, either using a Warehouse Release or Warehouse Adjustment. Print a Warehouse Stock on Hand report or use the Warehouse Inventory module to find these stock quantities");

		#endregion

		#region CheckWW_LocationComponentDelimiter

		protected override void CheckWW_LocationComponentDelimiter()
		{
			base.CheckWW_LocationComponentDelimiter();

			if (Parent.WW_LocationComponentDelimiter.Trim().IsEmpty)
			{
				Parent.WW_LocationComponentDelimiterInfo.AddError(Res.GetString("711a04bb-82eb-4034-bfe3-28cde902f73d", "A space or empty delimiter is not allowed"));
			}
		}

		#endregion

		#region CheckWW_LocationColumnsAlpha

		protected override void CheckWW_LocationColumnsAlpha()
		{
			base.CheckWW_LocationColumnsAlpha();

			FindMaxRowSize();
			if (Parent.WW_LocationColumnsAlpha && MaxColumnInWarehouse > 26)
			{
				Parent.WW_LocationColumnsAlphaInfo.AddError(Res.GetString("b05e8a31-2795-47eb-9c58-c496093db983", "This option cannot be used as there is currently one or more rows in this warehouse which contain more than 26 columns"));
			}
		}

		#endregion

		#region CheckWW_LocationLevelsAlpha

		protected override void CheckWW_LocationLevelsAlpha()
		{
			base.CheckWW_LocationLevelsAlpha();

			FindMaxRowSize();
			if (Parent.WW_LocationLevelsAlpha && MaxLevelInWarehouse > 26)
			{
				Parent.WW_LocationLevelsAlphaInfo.AddError(Res.GetString("77b07876-3344-4f69-8130-bbf45f5c9470", "This option cannot be used as there is currently one or more rows in this warehouse which contain more than 26 levels"));
			}
		}

		#endregion

		#region CheckWW_LocationTraysAlpha

		protected override void CheckWW_LocationTraysAlpha()
		{
			base.CheckWW_LocationTraysAlpha();

			FindMaxRowSize();
			if (Parent.WW_LocationTraysAlpha && MaxTrayInWarehouse > 26)
			{
				Parent.WW_LocationTraysAlphaInfo.AddError(Res.GetString("1f06de58-e88f-482c-9592-7db424da6c9c", "This option cannot be used as there is currently one or more rows in this warehouse which contain more than 26 trays"));
			}
		}

		#endregion

		#region CheckWW_OA_WarehouseAddress

		protected override void CheckWW_OA_WarehouseAddress()
		{
			base.CheckWW_OA_WarehouseAddress();

			if (Parent.IsWarehouseBondEnabled)
			{
				if (!DoesAddressHaveControlledPremisesID(Parent.WarehouseAddress))
				{
					Parent.WW_OA_WarehouseAddressInfo.AddWarning(NoControlledPremisesIDWarningMessage);
				}
			}

			CheckWW_OA_WarehouseAddress_CannotBeChangedIfWarehouseTypeIsFTZAndReferencedByUnFinalisedPickWithCustomsOrder();
			CheckWW_OA_WarehouseAddress_OnlyOneCYDPerOrgAddress(Parent.WarehouseAddress);
		}

		void CheckWW_OA_WarehouseAddress_CannotBeChangedIfWarehouseTypeIsFTZAndReferencedByUnFinalisedPickWithCustomsOrder()
		{
			if (!Parent.WW_OA_WarehouseAddressInfo.HasErrors() && Parent.WW_OA_WarehouseAddressInfo.HasChanges && IsFTZWarehouseThatUsesPermitsAndIsReferencedByUnfinalisedPickForFTZCustomsOrder())
			{
				var ftzCountries = string.Join(", ", BondedHelper.SupportedCountriesForFTZPermits);
				Parent.WW_OA_WarehouseAddressInfo.AddError(Res.GetString("81411d1d-5bb1-4350-ad24-d8d82b81647b",
					"Cannot change the Warehouse Address to/from a country that requires Permits ({0}) because there are un-finalized Picks or Orders with no pick in the Warehouse.", ftzCountries));
			}
		}
		void CheckWW_OA_WarehouseAddress_OnlyOneCYDPerOrgAddress(OrgAddress warehouseAddress)
		{
			if (Parent.WW_WarehouseType == WarehouseTypes.Codes.ContainerYard && warehouseAddress != null)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard);
				query.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, Parent.WW_OA_WarehouseAddress);
				query.AddToFilter(WhsWarehouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsWarehouse>(query) != null)
				{
					Parent.WW_OA_WarehouseAddressInfo.AddError(Res.GetString("C09A25C0-3E7B-445A-BFF4-9B07E78ABD50",
						"Address must be Unique per Container Yard Branch."));
				}
			}
		}

		#endregion

		#region CheckWW_DG_

		protected override void CheckWW_IsDangerousGoodsManagementEnabled()
		{
			base.CheckWW_IsDangerousGoodsManagementEnabled();

			if (!Parent.WW_IsDangerousGoodsManagementEnabled && (Parent.WW_DGThresholdPercentage > 0 || Parent.UNDGLimits.Count > 0))
			{
				var warningMessage = Res.GetString("c945434f-c739-42c2-9842-a9f6b6073b4f", "If DG Limits are disabled then entered Dangerous Goods in the grid will have no limit applied.");
				Parent.WW_IsDangerousGoodsManagementEnabledInfo.AddWarning(warningMessage);
			}
		}

		#endregion

		#region CheckWW_OC_DGContact

		protected override void CheckWW_OC_DGContact()
		{
			base.CheckWW_OC_DGContact();

			if (Parent.WW_OC_DGContact.IsValid)
			{
				var dgContact = Parent.DGContact;
				var warehouseOrgPK = Parent.WarehouseAddress?.OA_OH ?? ZGuid.Empty;
				if (dgContact == null || dgContact.OC_OH != warehouseOrgPK)
				{
					Parent.WW_OC_DGContactInfo.AddError(Res.GetString("f131ca52-d22c-4b60-a6aa-f016e1f58885", "Enter a valid DG Contact."));
				}
			}
		}

		#endregion

		#region CheckWW_DGContactPhoneType

		protected override void CheckWW_DGContactPhoneType()
		{
			base.CheckWW_DGContactPhoneType();
			ListValidation.ErrorIfInvalidCode(Parent.WW_DGContactPhoneTypeInfo, Parent.Lookups.PhoneTypes);

			if (Parent.WW_OC_DGContact.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.WW_DGContactPhoneTypeInfo);
			}
			else if (!Parent.WW_DGContactPhoneType.IsEmpty)
			{
				Parent.WW_DGContactPhoneTypeInfo.AddError(Res.GetString("f131ca52-d22c-4b60-a6aa-f016e1f58885", "Enter a valid DG Contact."));
			}
		}

		#endregion

		#region CheckWW_GB_RelatedCompanyBranch

		protected override void CheckWW_GB_RelatedCompanyBranch()
		{
			base.CheckWW_GB_RelatedCompanyBranch();

			ListValidation.ErrorIfInvalidPK(Parent.WW_GB_RelatedCompanyBranchInfo);
			CheckBranchIsUniquePerWarehousePerWarehouseTypeForActiveWarehouses(Parent.WW_GB_RelatedCompanyBranchInfo);
		}

		void CheckBranchIsUniquePerWarehousePerWarehouseTypeForActiveWarehouses(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.WW_IsActive && !Parent.WW_IsVirtualWarehouse && Parent.WW_GB_RelatedCompanyBranch.IsValid)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, Parent.WW_GB_RelatedCompanyBranch);
				query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
				query.AddToFilter(WhsWarehouseSchema.WW_IsVirtualWarehouse, false);
				query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, Parent.WW_WarehouseType);
				query.AddToFilter(WhsWarehouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsWarehouse>(query) != null)
				{
					info.AddError(Res.GetString("53dad68c-a5f5-4b6a-985b-ee0b35c7f5bc", "Warehouse Branch must be Unique per Warehouse and Warehouse Type."));
				}
			}
		}

		#endregion

		#region CheckWW_GG_ReleaseGroup

		protected override void CheckWW_GG_ReleaseGroup()
		{
			base.CheckWW_GG_ReleaseGroup();

			if (!Parent.WW_GG_ReleaseGroup.IsEmpty)
			{
				if (!Parent.WarehouseTypeSupportsTaskManagement)
				{
					Parent.WW_GG_ReleaseGroupInfo.AddError(Res.GetString("44fcee6b-a71f-4493-978b-93d408632b62", "This type of Warehouse does not support Task Management."));
				}
				else if (Parent.WW_IsVirtualWarehouse)
				{
					Parent.WW_GG_ReleaseGroupInfo.AddError(Res.GetString("23f7e0ef-b43e-4d03-9691-587274efc92e", "Virtual Warehouses do not support Task Management."));
				}
			}

			if (Parent.IsInDatabase && Parent.WW_GG_ReleaseGroupInfo.HasChanges && HasWarehouseJobsCurrentlyUsingTaskManagement())
			{
				Parent.WW_GG_ReleaseGroupInfo.AddError(Res.GetString("73165683-bb9c-4050-8d08-e748434845fc", "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management."));
			}
		}

		bool HasWarehouseJobsCurrentlyUsingTaskManagement()
		{
			var getWarehouseJobsCountWithStatusReadyForPlanningOrPlanned = $@"
SELECT
	TOP 1 1 AS JobExists
FROM
	dbo.WhsDocket
WHERE
	WD_DocketType NOT IN ('ADJ', 'DWO', 'WOR', 'ORD')
	AND WD_TaskPlanningStatus IN ('RFP', 'PLA')
	AND	WD_WW_Whs = @WarehousePK

UNION ALL

SELECT
	TOP 1 1 AS JobExists
FROM
	dbo.WhsPick
WHERE
	WP_TaskPlanningStatus IN ('RFP', 'PLA')
	AND	WP_WW_Whs = @WarehousePK

UNION ALL

SELECT
	TOP 1 1 AS JobExists
FROM
	dbo.WhsCycleCountLocation
	JOIN dbo.WhsLocationView ON WCL_WL_Location = WLV_PK
WHERE
	WCL_TaskPlanningStatus IN ('RFP', 'PLA')
	AND	WLV_WW_Whs = @WarehousePK

UNION ALL

SELECT
	TOP 1 1 AS JobExists
FROM
	dbo.WhsLoad
	JOIN dbo.WhsLocationView ON WLO_WL_PlannedDockDoor = WLV_PK
WHERE
	WLO_TaskPlanningStatus IN ('RFP', 'PLA')
	AND	WLV_WW_Whs = @WarehousePK
";

			var sqlParameters = new ZSqlParameterCollection
			{
				{ "@WarehousePK", Parent.PK, WhsLocationViewSchema.WLV_WW_Whs }
			};

			var warehouseJobs = new DynamicBusinessObjectCollection(Parent.Factory);
			warehouseJobs.Load(getWarehouseJobsCountWithStatusReadyForPlanningOrPlanned, sqlParameters);

			return warehouseJobs.Count > 0;
		}

		#endregion

		#region CheckWW_UseGS1PrefixFallback

		protected override void CheckWW_UseGS1PrefixFallback()
		{
			base.CheckWW_UseGS1PrefixFallback();

			if (Parent.WW_UseGS1PrefixFallback && Parent.GetSSCCPrefix().IsEmpty)
			{
				Parent.WW_UseGS1PrefixFallbackInfo.AddError(Res.GetString("c5dbcba3-1d33-483a-b494-d78c73e7527d", "Cannot use a GS1 prefix as it does not exist for this address."));
			}
		}

		#endregion

		#region CheckWW_DefaultDockDoor

		protected override void CheckWW_DefaultInboundDockDoor()
		{
			base.CheckWW_DefaultInboundDockDoor();
			DefaultDockDoorValidation(Parent.WW_DefaultInboundDockDoor, Parent.DefaultInboundDockDoorLocation, WhsLocationViewValidation.Inbound, Parent.WW_DefaultInboundDockDoorInfo);
		}

		protected override void CheckWW_DefaultOutboundDockDoor()
		{
			base.CheckWW_DefaultOutboundDockDoor();
			DefaultDockDoorValidation(Parent.WW_DefaultOutboundDockDoor, Parent.DefaultOutboundDockDoorLocation, WhsLocationViewValidation.Outbound, Parent.WW_DefaultOutboundDockDoorInfo);
		}

		void DefaultDockDoorValidation(ZGuid defaultDockDoor, WhsLocation defaultDockDoorLocation, string dockDoorType, ZPropertyInfo dockDoorInfo)
		{
			if (!dockDoorInfo.HasErrors()
				&& Parent.IsInDatabase
				&& Parent.IsUsingDockDoorLocation)
			{
				if (defaultDockDoor.IsEmpty)
				{
					dockDoorInfo.AddError(DefaultDockDoorLocationMissingError(dockDoorType));
				}
				else
				{
					CheckIfDockDoorLocationIsCorrect(defaultDockDoorLocation, dockDoorType, dockDoorInfo);
				}
			}
		}

		void CheckIfDockDoorLocationIsCorrect(WhsLocation defaultDockDoorLocation, string dockDoorType, ZPropertyInfo dockDoorInfo)
		{
			if (defaultDockDoorLocation == null)
			{
				dockDoorInfo.AddError(DefaultDockDoorLocationMissingError(dockDoorType));
			}
			else if (!defaultDockDoorLocation.IsDockDoorLocation)
			{
				dockDoorInfo.AddError(DefaultDockDoorLocationWrongTypeError(dockDoorType));
			}
			else if (Parent.PK != defaultDockDoorLocation.WLV_WW_Whs)
			{
				dockDoorInfo.AddError(DefaultDockDoorLocationWrongWarehouse(dockDoorType));
			}
			else if (defaultDockDoorLocation.WLV_LocationStatus != LocationStatus.Codes.Normal)
			{
				dockDoorInfo.AddError(DefaultDockDoorLocationWrongStatus(dockDoorType));
			}
		}

		public static string DefaultDockDoorLocationMissingError(string dockDoorType)
		{
			return Res.GetString("B7B57E8E-DBC5-4863-93D1-2EAB0D673278", "Default {0} Dock Door Location must be specified for an Active Product Warehouse.", dockDoorType);
		}

		public static string DefaultDockDoorLocationWrongTypeError(string dockDoorType)
		{
			return Res.GetString("F4778E56-65F5-4FB4-B8AF-522746E25C85", "Default {0} Dock Door Location should have a Dock Door Location Type.", dockDoorType);
		}

		public static string DefaultDockDoorLocationWrongWarehouse(string dockDoorType)
		{
			return Res.GetString("CF66579D-B8AA-4F44-B19C-2B6852FDDAD5", "Default {0} Dock Door Location should be a location within this Warehouse.", dockDoorType);
		}

		public static string DefaultDockDoorLocationWrongStatus(string dockDoorType)
		{
			return Res.GetString("b083e393-a6f9-4a48-b9aa-5f8172df6ec4", "Default {0} Dock Door Location should have a Location Status of {1}.", dockDoorType, LocationStatus.Codes.Normal);
		}

		#endregion

		#region CheckWW_WLT_DefaultLocationType

		protected override void CheckWW_WLT_DefaultLocationType()
		{
			base.CheckWW_WLT_DefaultLocationType();

			var locationType = Parent.LocationType;
			if (locationType != null && !locationType.IsLocationTypeSupportedByThisWarehouseType(Parent.WW_WarehouseType))
			{
				Parent.WW_WLT_DefaultLocationTypeInfo.AddError(Res.GetString("39babf1e-4640-493f-99df-7b026d7845c2", "This Location Type is not supported for this type of Warehouse."));
			}
		}

		#endregion

		#region CheckWW_FTZIsDetailedTrackingEnabled

		protected override void CheckWW_FTZIsDetailedTrackingEnabled()
		{
			base.CheckWW_FTZIsDetailedTrackingEnabled();

			if (Parent.WW_FTZIsDetailedTrackingEnabled)
			{
				var countryCode = Parent.CountryCode;
				if (!Parent.IsFTZWarehouse || !(countryCode == Enterprise.Core.Constants.CountryCodes.UnitedStates || countryCode == Enterprise.Core.Constants.CountryCodes.PuertoRico))
				{
					Parent.WW_FTZIsDetailedTrackingEnabledInfo.AddError(Res.GetString("c4082213-3ef0-40e1-b51f-e3473c604a24", "Detailed Tracking only available for Warehouse in Country/Region US or PR and Type 'FTZ'."));
				}
			}
		}

		#endregion

		#region CheckWW_FTZDetailedTrackingMethod

		protected override void CheckWW_FTZDetailedTrackingMethod()
		{
			base.CheckWW_FTZDetailedTrackingMethod();

			MandatoryValidation.CheckEntered(Parent.WW_FTZDetailedTrackingMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WW_FTZDetailedTrackingMethodInfo, Parent.Lookups.DetailedTrackingMethods);
		}

		#endregion

		#region CheckWW_NumberOfCycleCountLocationsToAutoAssign

		protected override void CheckWW_NumberOfCycleCountLocationsToAutoAssign()
		{
			base.CheckWW_NumberOfCycleCountLocationsToAutoAssign();

			MandatoryValidation.CheckNotZero(Parent.WW_NumberOfCycleCountLocationsToAutoAssignInfo);
		}

		#endregion

		#region CheckWW_LocationsHaveLeadingZeros

		protected override void CheckWW_LocationsHaveLeadingZeros()
		{
			base.CheckWW_LocationsHaveLeadingZeros();
			if (Parent.IsFixedWidthLocation && !Parent.WW_LocationsHaveLeadingZeros)
			{
				Parent.WW_LocationsHaveLeadingZerosInfo.AddError(Res.GetString("3ff13163-31e5-4320-b94f-83ce6134523b", "Locations must have leading zeros if Location Fixed Width is enabled."));
			}
		}

		#endregion

		#region DoesAddressHaveControlledPremisesID

		bool DoesAddressHaveControlledPremisesID(OrgAddress warehouseAddress)
		{
			var result = false;
			if (warehouseAddress != null)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
				query.AddToFilter(OrgCusCodeSchema.OK_OH, warehouseAddress.OA_OH);
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, Parent.WW_OA_WarehouseAddress);
				result = Parent.Factory.LoadTop1<OrgCusCode>(query) != null;
			}
			return result;
		}

		#endregion

		#region NoControlledPremisesIDWarningMessage

		public static string NoControlledPremisesIDWarningMessage
		{
			get { return Res.GetString("249308dc-8045-4647-8177-3a6fd2cb6c03", "This Warehouse is enabled for Bond Transactions but this Address does not have a Customs Controlled Premises ID"); }
		}

		#endregion

		#region LocationFixedWidthParameters

		protected override void CheckWW_LocationColumnsFixedWidth()
		{
			base.CheckWW_LocationColumnsFixedWidth();
			CheckLocationComponentFixedWidth(
				Parent.WW_LocationColumnsFixedWidth,
				Parent.WW_LocationColumnsFixedWidthInfo,
				3,
				Parent.WW_LocationColumnsAlpha,
				Res.GetString("6beaee4c-5166-41c6-a711-87ba47d51c45", "Columns"),
				() => MaxColumnInWarehouse);
		}

		protected override void CheckWW_LocationLevelsFixedWidth()
		{
			base.CheckWW_LocationLevelsFixedWidth();
			CheckLocationComponentFixedWidth(
				Parent.WW_LocationLevelsFixedWidth,
				Parent.WW_LocationLevelsFixedWidthInfo,
				3,
				Parent.WW_LocationLevelsAlpha,
				Res.GetString("f360dce1-da51-4084-a78b-1412fd65141e", "Levels"),
				() => MaxLevelInWarehouse);
		}

		protected override void CheckWW_LocationTraysFixedWidth()
		{
			base.CheckWW_LocationTraysFixedWidth();
			CheckLocationComponentFixedWidth(
				Parent.WW_LocationTraysFixedWidth,
				Parent.WW_LocationTraysFixedWidthInfo,
				2,
				Parent.WW_LocationTraysAlpha,
				Res.GetString("2c5b5598-d4f9-45a1-91e3-dbaf51f973b8", "Trays"),
				() => MaxTrayInWarehouse);
		}

		void CheckLocationComponentFixedWidth(
			ZByte locationComponentFixedWidth,
			ZPropertyInfo locationComponentFixedWidthPropertyInfo,
			int maxFixedWidthValue,
			bool isLocationComponentAlpha,
			string locationComponent,
			Func<int> getMaxRowLocationComponent)
		{
			if (Parent.IsFixedWidthLocation)
			{
				if (locationComponentFixedWidth == 0)
				{
					locationComponentFixedWidthPropertyInfo.AddError(Res.GetString("9eff3f8d-d46b-4022-b622-9bd7898b07e6", "{0} Fixed Width cannot be 0 when Location Fixed Width is enabled.", locationComponent));
				}
				else if (locationComponentFixedWidth > maxFixedWidthValue)
				{
					locationComponentFixedWidthPropertyInfo.AddError(Res.GetString("80a407c1-29dd-47a7-b394-a1dd89cb5903", "Location {0} Fixed Width cannot be greater than {1}.", locationComponent, maxFixedWidthValue));
				}
				else if (isLocationComponentAlpha && locationComponentFixedWidth > 1)
				{
					locationComponentFixedWidthPropertyInfo.AddError(Res.GetString("bf2308cc-306c-4bd0-9875-9bbbcb0931b6", "Location {0} Fixed Width cannot be greater than 1 for Alpha {0}.", locationComponent));
				}
				else
				{
					CheckMaxLocationComponentSize(locationComponentFixedWidth, locationComponentFixedWidthPropertyInfo, maxFixedWidthValue, locationComponent, getMaxRowLocationComponent);
				}
			}
			else if (locationComponentFixedWidth > 0)
			{
				locationComponentFixedWidthPropertyInfo.AddError(Res.GetString("c847cef2-d63b-4dfb-b49d-e7d79a1efb75", "You cannot set the fixed width if Location Fixed Width is not enabled."));
			}
		}

		void CheckMaxLocationComponentSize(ZByte locationComponentFixedWidth, ZPropertyInfo locationComponentFixedWidthPropertyInfo, int maxFixedWidthValue, string locationComponent, Func<int> getMaxRowLocationComponent)
		{
			if (locationComponentFixedWidth != maxFixedWidthValue)
			{
				FindMaxRowSize();
				var maxLocationComponentSize = WhsLocationHelper.GetMaxLocationComponentValueWithFixedWidth(locationComponentFixedWidth);
				if (getMaxRowLocationComponent() > maxLocationComponentSize)
				{
					locationComponentFixedWidthPropertyInfo.AddError(
						Res.GetString("1196f4d5-13f6-4fd9-a8ce-4f1704a50cca",
							"You cannot set the fixed width to less than {0} as there is currently one or more rows in this warehouse which contain more than {1} {2}.",
							locationComponentFixedWidth + 1,
							maxLocationComponentSize,
							locationComponent));
				}
			}
		}

		#endregion

		// non persistent

		#region ValidateRFPickPackPrinterPK

		public void ValidateRFPickPackPrinterPK()
		{
			ValidateCalculatedProperty(Parent.RFPickPackPrinterPKInfo);
		}

		protected void CheckRFPickPackPrinterPK()
		{
			TypeValidation.CheckValidGuid(Parent.RFPickPackPrinterPKInfo);
		}

		#endregion

		#region ValidateIsFixedWidthLocation

		public void ValidateIsFixedWidthLocation()
		{
			ValidateCalculatedProperty(Parent.IsFixedWidthLocationInfo);
		}

		protected void CheckIsFixedWidthLocation()
		{
			if (Parent.IsFixedWidthLocation
				&& WarehouseDataRegistry.Instance.EnableRowNamePrefixValidationForFixedWidthWarehouses.Value
				&& Parent.IsInDatabase
				&& FixedWidthHasChanges())
			{
				const string sql = @"
SELECT
	WR_PK
FROM
	WhsRow CurrentRow
WHERE
	WR_WW_Whs = @WhsPK
	AND EXISTS
	(
		SELECT NULL
		FROM
			WhsRow RowToCheck
		WHERE
			RowToCheck.WR_WW_Whs = @WhsPK
			AND RowToCheck.WR_PK != CurrentRow.WR_PK
			AND RowToCheck.WR_Name LIKE CurrentRow.WR_Name + '%'
	)";

				var parameters = new ZSqlParameterCollection
				{
					{ "@WhsPK", Parent.PK, WhsRowSchema.WR_WW_Whs }
				};

				var query = new ZDBOnlyQuery(typeof(WhsRow));
				query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsRowSchema.PK.Name, sql), parameters);

				if (Parent.Factory.LoadTop1<WhsRow>(query) != null)
				{
					Parent.IsFixedWidthLocationInfo.AddError(Res.GetString("fd3e5d4e-8bcb-4480-97b7-16a07a09917d", "Warehouse contains Row Names that are prefixes of other Row Names. Rows must not contain another Row's Name within their name to ensure that Fixed Width Locations can be uniquely addressed."));
				}
			}
		}

		bool FixedWidthHasChanges()
			=> Parent.WW_LocationColumnsFixedWidthInfo.HasChanges
			|| Parent.WW_LocationLevelsFixedWidthInfo.HasChanges
			|| Parent.WW_LocationTraysFixedWidthInfo.HasChanges;

		#endregion

		#region Parent

		protected new WhsWarehouse Parent
		{
			get { return (WhsWarehouse)base.Parent; }
		}

		#endregion

		#region FindMaxRowSize

		void FindMaxRowSize()
		{
			if (MaxColumnInWarehouse == 0)
			{
				foreach (var row in Parent.Rows)
				{
					MaxColumnInWarehouse = Math.Max(MaxColumnInWarehouse, row.WR_Columns);
					MaxLevelInWarehouse = Math.Max(MaxLevelInWarehouse, row.WR_Levels);
					MaxTrayInWarehouse = Math.Max(MaxTrayInWarehouse, row.WR_Trays);
				}
			}
		}
		int MaxColumnInWarehouse;
		int MaxLevelInWarehouse;
		int MaxTrayInWarehouse;

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRFPickPackPrinterPK();
			ValidateIsFixedWidthLocation();
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsWarehouseSchema.Constants.WW_DefaultInboundDockDoor, WhsWarehouseSchema.Constants.WW_DefaultOutboundDockDoor);

		#endregion
	}
}
// ensure error messages match (if the generic error is not good, look at how multilingual string is used so that we can use it)
// the lookups class should have a test that tests that the list does not load branches belonging to other companies
