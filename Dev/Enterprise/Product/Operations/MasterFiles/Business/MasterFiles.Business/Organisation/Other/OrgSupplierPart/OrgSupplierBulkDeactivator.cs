using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public partial class OrgSupplierBulkDeactivator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgSupplierBulkDeactivator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string EstimatedProductCount = "EstimatedProductCount";
			public const string ShouldDeactivate = "ShouldDeactivate";
			public const string ShouldActivate = "ShouldActivate";
		}

		public event DeactivatorProgressChangedEventHandler ProgressChanged;

		#region Properties

		[ReadOnly(true)]
		public ZInt EstimatedProductCount
		{
			get { return estimatedProductCount; }
			set { SetNonPersistentPropertyValue(EstimatedProductCountInfo, ref estimatedProductCount, value); }
		}
		ZInt estimatedProductCount;

		public ZPropertyInfo EstimatedProductCountInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedProductCount); }
		}

		public ZBool ShouldDeactivate
		{
			get { return shouldDeactivate; }
			set
			{
				SetNonPersistentPropertyValue(ShouldDeactivateInfo, ref shouldDeactivate, value);
				CalculateEstimatedProductCount();
			}
		}
		ZBool shouldDeactivate;

		public ZPropertyInfo ShouldDeactivateInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldDeactivate); }
		}

		public ZBool ShouldActivate
		{
			get { return shouldActivate; }
			set
			{
				SetNonPersistentPropertyValue(ShouldActivateInfo, ref shouldActivate, value);
				CalculateEstimatedProductCount();
			}
		}
		ZBool shouldActivate;

		public ZPropertyInfo ShouldActivateInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldActivate); }
		}

		public int ProductsChanged
		{
			get { return productsChanged; }
		}
		int productsChanged;

		public bool Cancelled { get; private set; }

		#endregion

		public void Cancel()
		{
			Cancelled = true;
		}

		public ZQuery ProductFilter
		{
			get { return filter ?? (new ZQuery()); }
			set { filter = value; }
		}
		ZQuery filter;

		public void CalculateEstimatedProductCount()
		{
			ZQuery filterForLoadCount = new ZQuery();
			filterForLoadCount.AddToFilter(ProductFilter);
			ApplyActiveFilter(filterForLoadCount);

			estimatedProductCount = Factory.GetDatabaseCount(typeof(OrgSupplierPart), filterForLoadCount);
			EstimatedProductCountInfo.RefreshBinding();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL not for translation")]
		public void Deactivate()
		{
			int step = 500;
			productsChanged = 0;
			ApplyActiveFilter(ProductFilter);
			var tempTableName = "";
			var whereClauses = "";
			if (ShouldActivate)
			{
				const string tempPrefix = "Temp";
				tempTableName = tempPrefix + ZGuid.NewZGuid().ToString().Replace("-", "");
				const string sqlCreateTempTable = @"
IF OBJECT_ID('tempdb..#{0}', 'u') IS NOT NULL
	DROP TABLE #{0}

CREATE TABLE #{0} ( PK UNIQUEIDENTIFIER NOT NULL);
CREATE INDEX PK_UX_{0} ON #{0} ( PK ASC );
";
				CargoWise.Data.Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sqlCreateTempTable, tempTableName));
				var productFilter = ProductFilter.DeepClone();
				productFilter.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{1} NOT IN (SELECT PK FROM #{0})", tempTableName, OrgSupplierPartSchema.Constants.PK), new ZSqlParameterCollection());
				whereClauses = productFilter.GetAsWhereAndOrderByClause(false);
			}
			else
			{
				whereClauses = ProductFilter.GetAsWhereAndOrderByClause(false);
			}
			var partPKsNotAbleToActivate = new List<ZGuid>();
			do
			{
				var factoryForSaving = new BusinessObjectFactory();
				SetBulkDeactivatorFactoryForTesting(factoryForSaving);
				factoryForSaving.SuspendValidation();
				factoryForSaving.RefreshEnabled = false;

				string sql;
				if (ShouldActivate)
				{
					sql = string.Format(CultureInfo.InvariantCulture,
					@"SELECT TOP {0}
						OP_PK,
						0 AS HasCurrentInventory
					FROM
						dbo.OrgSupplierPart
					{1}
					", step, whereClauses);
				}
				else
				{
					sql = string.Format(CultureInfo.InvariantCulture,
					@"SELECT DISTINCT TOP {0}
						OP_PK,
						CASE WHEN (CurrentInventory.WI_PK IS NULL AND CurrentAsnLine.WN_PK is NULL) THEN 0 ELSE 1 END as HasCurrentInventory
					FROM
						dbo.OrgSupplierPart
						OUTER APPLY
						(
							SELECT TOP 1 
								WI_PK
							FROM
								dbo.WhsInventoryView
							WHERE
								WI_OP = OP_PK AND
								WI_TotalUnits > 0
							ORDER BY
								WI_PK
						) as CurrentInventory
						OUTER APPLY
						(
							SELECT TOP 1
								WN_PK
							FROM
								dbo.WhsAsnLine
								JOIN dbo.WhsDocket ON WN_WD = WD_PK
							WHERE
								WN_OP = OP_PK AND
								WD_DocketStatus not in ('CAN', 'FIN') AND WD_DocketType = 'INW'
							ORDER BY
								WN_PK
						) as CurrentAsnLine
					{1}
					", step, whereClauses);
				}

				var collection = new DynamicBusinessObjectCollection(factoryForSaving);
				collection.Load(sql, ProductFilter.Params);

				var supplierPartPksAndHasCurrentInventory = collection.ToDictionary(k => (ZGuid)k[OrgSupplierPartSchema.PK], v => (ZInt)v["HasCurrentInventory"] == 1);
				AddFetchHints(factoryForSaving, supplierPartPksAndHasCurrentInventory.Keys);
				if (ShouldActivate)
				{
					var parts = factoryForSaving.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.PK, supplierPartPksAndHasCurrentInventory.Keys));
					var activePartQuery = new ZQuery(OrgSupplierPartSchema.OP_PartNum, parts.Select(x => x.OP_PartNum).Distinct());
					activePartQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
					var activeParts = factoryForSaving.Load<OrgSupplierPart>(activePartQuery);
					AddFetchHints(factoryForSaving, activeParts.Select(x => x.PK));
					AddSupplierPartBarcodeFetchHints(factoryForSaving, parts);
				}

				foreach (var supplierPartPkAndHasCurrentInventory in supplierPartPksAndHasCurrentInventory)
				{
					if (Cancelled)
					{
						break;
					}

					var supplierPartPK = supplierPartPkAndHasCurrentInventory.Key;
					var hasCurrentInventory = supplierPartPkAndHasCurrentInventory.Value;

					var orgSupplierPart = factoryForSaving.Load<OrgSupplierPart>(supplierPartPK);
					if (!ShouldActivate)
					{
						if (!hasCurrentInventory)
						{
							orgSupplierPart.OP_IsActive = false;
							productsChanged++;
						}
					}
					else if (!HasDuplicateActivePart(orgSupplierPart) && !HasDuplicateBarcode(orgSupplierPart))
					{
						orgSupplierPart.OP_IsActive = true;
						productsChanged++;
					}
					else
					{
						if (partPKsNotAbleToActivate.Count > 50)
						{
							InsertIntoTempTable(tempTableName, partPKsNotAbleToActivate);
						}
						partPKsNotAbleToActivate.Add(orgSupplierPart.PK);
					}
					orgSupplierPart.IsLightSaving = true;
				}
				OnProgressChanged(productsChanged);
				factoryForSaving.Save();
				if (collection.Count < step || Cancelled)
				{
					break;
				}
			}
			while (true);
		}

		void InsertIntoTempTable(string tempTableName, List<ZGuid> partPKsNotAbleToActivate)
		{
			var sqlBuilder = new ZStringBuilder();
			foreach (var partPK in partPKsNotAbleToActivate)
			{
				sqlBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"INSERT #{0} (PK) VALUES ('{1}');", tempTableName, partPK));
			}
			if (!sqlBuilder.IsEmpty)
			{
				CargoWise.Data.Db.Connection.ExecuteNonQuery(sqlBuilder.ToStringWithNewLineBetweenAppends());
			}
			partPKsNotAbleToActivate.Clear();
		}

		bool HasDuplicateActivePart(OrgSupplierPart part)
		{
			var result = false;
			GetRelationship(part, out var partOwnerPKs, out var partSupplierPKs);

			// Products with same code should not have same suppliers, if there is no owner that would allow to resolve ambiguity.
			var partStandaloneSupplierPKs = partSupplierPKs.Where(s => !partOwnerPKs.Any(o => o != s)).ToList();

			if (partOwnerPKs.Count > 0 || partStandaloneSupplierPKs.Count > 0)
			{
				var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, part.OP_PartNum);
				query.AddToFilter(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, part.PK);
				query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
				query.FetchOnlyFromLocalCache = true;

				foreach (var otherPart in part.Factory.Load<OrgSupplierPart>(query))
				{
					GetRelationship(otherPart, out var otherPartOwnerPKs, out var otherPartSupplierPKs);
					var otherStandaloneSupplierPKs = otherPartSupplierPKs.Where(s => !otherPartOwnerPKs.Any(o => o != s)).ToList();

					if (partOwnerPKs.Any(o => otherPartOwnerPKs.Contains(o)))
					{
						result = true;
						break;
					}
					if (partStandaloneSupplierPKs.Any(o => otherStandaloneSupplierPKs.Contains(o)))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		void GetRelationship(OrgSupplierPart part, out List<ZGuid> ownerPKs, out List<ZGuid> supplierPKs)
		{
			ownerPKs = new List<ZGuid>();
			supplierPKs = new List<ZGuid>();
			foreach (OrgPartRelation partRelation in part.RelatedOrganisations)
			{
				var orgPK = partRelation.OU_OH;
				if (partRelation.IsOwner)
				{
					if (!ownerPKs.Contains(orgPK))
					{
						ownerPKs.Add(orgPK);
					}
				}

				if (partRelation.IsSupplier)
				{
					if (!supplierPKs.Contains(orgPK))
					{
						supplierPKs.Add(orgPK);
					}
				}
			}
		}

		bool HasDuplicateBarcode(OrgSupplierPart part)
		{
			var owners = part.RelatedOrganisations.Cast<OrgPartRelation>().Where(o => o.IsOwner).Select(o => o.OU_OH);
			return OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(part.Factory, part.PK, owners, GetBarcodesToCheckForDuplicate(part)).Any();
		}

		void AddFetchHints(BusinessObjectFactory factoryForSaving, IEnumerable<ZGuid> supplierPartPks)
		{
			foreach (var supplierPartPk in supplierPartPks)
			{
				if (!ShouldActivate)
				{
					factoryForSaving.AddFetchHint(OrgSupplierPartSchema.PK, supplierPartPk);
				}
				factoryForSaving.AddFetchHint(ProcessTasksSchema.P9_ParentID, supplierPartPk);
				factoryForSaving.AddFetchHint(StmALogSchema.SL_Parent, supplierPartPk);
				factoryForSaving.AddFetchHint(OrgPartRelationSchema.OU_OP, supplierPartPk);
			}
		}

		void AddSupplierPartBarcodeFetchHints(BusinessObjectFactory factoryForSaving, IEnumerable<OrgSupplierPart> products)
		{
			AddPartBarcodeDuplicateCheckFetchHints(factoryForSaving, products);
			AddOrgSupplierPartBarcodeCheckFetchHints(factoryForSaving, products);
		}

		static void AddPartBarcodeDuplicateCheckFetchHints(BusinessObjectFactory factoryForSaving, IEnumerable<OrgSupplierPart> products)
		{
			foreach (var product in products)
			{
				if (ProductHasOwners(product))
				{
					var barcodeQuery = OrgPartRelationValidationHelper.GetBarcodeDuplicateBarcodeCheckQuery(product.PK, GetBarcodesToCheckForDuplicate(product));
					factoryForSaving.AddFetchHint(OrgSupplierPartBarcodeSchema.Instance, barcodeQuery);
				}
			}
		}

		void AddOrgSupplierPartBarcodeCheckFetchHints(BusinessObjectFactory factoryForSaving, IEnumerable<OrgSupplierPart> products)
		{
			foreach (var product in products)
			{
				if (ProductHasOwners(product))
				{
					var barcodesToCheck = GetBarcodesToCheckForDuplicate(product);
					var barcodeQuery = OrgPartRelationValidationHelper.GetBarcodeDuplicateBarcodeCheckQuery(product.PK, barcodesToCheck);
					var sameBarcodesInOtherProducts = factoryForSaving.Load<OrgSupplierPartBarcode>(barcodeQuery);

					var query = OrgPartRelationValidationHelper.GetProductDuplicateBarcodeCheckQuery(product.PK, barcodesToCheck, sameBarcodesInOtherProducts);
					factoryForSaving.AddFetchHint(OrgSupplierPartSchema.Instance, query);
				}
			}
		}

		static bool ProductHasOwners(OrgSupplierPart product)
		{
			return product.RelatedOrganisations.Cast<OrgPartRelation>().Where(o => o.IsOwner).Select(o => o.OU_OH).Any();
		}

		static IEnumerable<ZString> GetBarcodesToCheckForDuplicate(OrgSupplierPart product)
		{
			var productBarcodes = product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Select(barcode => barcode.PH_Barcode);
			return new[] { product.OP_PartNum }.Union(productBarcodes);
		}

		void ApplyActiveFilter(ZQuery filter)
		{
			filter.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ShouldDeactivate);
		}

		public void OnProgressChanged(decimal count)
		{
			if (ProgressChanged != null && EstimatedProductCount > 0)
			{
				int percentage = (int)(count / EstimatedProductCount * 100);
				ProgressChanged(percentage > 100 ? 100 : percentage);
			}
		}

		#region Event Handlers

		public delegate void DeactivatorProgressChangedEventHandler(int percentage);

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			shouldDeactivate = true;
		}

		partial void SetBulkDeactivatorFactoryForTesting(BusinessObjectFactory factory);
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	public partial class OrgSupplierBulkDeactivator
	{
		partial void SetBulkDeactivatorFactoryForTesting(BusinessObjectFactory factory)
		{
			BulkDeactivatorFactoryForTesting = factory;
		}
		public BusinessObjectFactory BulkDeactivatorFactoryForTesting;
	}
}

#endif
#endregion
