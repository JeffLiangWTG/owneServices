using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProduct : NonPersistentBusinessObject, IWhsProduct, ICartonisableItemDefinition
	{
		#region Schema

		public static class Schema
		{
			public const string ProductStylePK = "ProductStylePK";
			public const string ProductStyleColourPK = "ProductStyleColourPK";
			public const string ProductStyleClassificationPK = "ProductStyleClassificationPK";
			public const string ProductStyleSizePK = "ProductStyleSizePK";
			public const string ProductStyleOwner = "ProductStyleOwner";
			public const string FirstUNDG = "FirstUNDG";
			public const string ProductStyle = "ProductStyle";
			public const string ProductStyleColour = "ProductStyleColour";
			public const string ProductStyleClassification = "ProductStyleClassification";
			public const string ProductStyleSize = "ProductStyleSize";
		}

		#endregion

		#region Constructors

		WhsProduct(OrgSupplierPart parent)
			: base(parent.Factory)
		{
			this.parent = parent;
			parent.OP_CountDecimalPlacesInfo.ValueChanged += OP_CountDecimalPlacesInfo_ValueChanged;
		}

		#region GetWhsProduct

		public static WhsProduct GetWhsProduct(OrgSupplierPart part)
		{
			Argument.NotNull(part, nameof(part));
			return part.Factory.GetCachedValue(GetWhsProductKey(part.PK), () => new WhsProduct(part));
		}

		public static WhsProduct GetWhsProduct(BusinessObjectFactory factory, ZGuid partPk)
		{
			Argument.NotNull(factory, nameof(factory));

			var whsProduct = !partPk.IsEmpty ? factory.GetCachedValue(GetWhsProductKey(partPk), () => GetWhsProduct()) : null;
			return whsProduct?.Parent.IsDeleted ?? true ? null : whsProduct;

			WhsProduct GetWhsProduct()
			{
				var supplierPart = factory.Load<OrgSupplierPart>(partPk);
				return supplierPart == null ? null : new WhsProduct(supplierPart);
			}
		}

		static string GetWhsProductKey(ZGuid partPk) => "WhsProduct|" + partPk;

		#endregion

		readonly OrgSupplierPart parent;

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			base.Delete();

			PickFaces.DeleteAll();
			ParamsByWhsAndClient.DeleteAll();
		}

		#endregion

		#region Related Entities

		#region OrgSupplierPart / Parent

		public OrgSupplierPart Parent
		{
			get { return parent; }
		}

		#endregion

		#region Warehouse

		[ActionFieldFollow(false)]
		public WhsWarehouseCollection Warehouses
		{
			get
			{
				if (warehouses == null)
				{
					var lwarehouses = new WhsWarehouseCollection(Factory);
					lwarehouses.Load();
					warehouses = lwarehouses;
					RegisterEditableChildObject(warehouses);
				}
				return warehouses;
			}
		}

		#endregion

		#region PackTypes

		public CodeDescriptionPairList GetPackTypes()
		{
			var packTypes = new CodeDescriptionPairList();
			var packTypeCodes = new HashSet<ZString>(GetPackTypeCodes());

			// Looping through CodeDescriptionPairWithStandardUnit to add pairs in one pass, rather than using .GetDescription() for each element in our hashset.
			foreach (CodeDescriptionPair pair in RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory))
			{
				if (packTypeCodes.Remove(pair.Code))
				{
					packTypes.AddPair(pair.Code, pair.Description);

					if (packTypeCodes.Count == 0)
					{
						break;
					}
				}
			}

			packTypeCodes.ForEach(p => packTypes.AddPair(p, string.Empty));

			return packTypes;
		}

		IEnumerable<ZString> GetPackTypeCodes()
		{
			yield return Parent.OP_StockKeepingUnit;

			foreach (OrgPartUnit unit in Parent.PartUnits)
			{
				yield return unit.OF_PackType;
				yield return unit.OF_ParentPackType;
			}
		}

		#endregion

		#region PickFaces

		[ActionFieldFollow(false)]
		[ChildEditable]
		public WhsPickFaceCollection PickFaces
		{
			get
			{
				return Factory.GetCachedValue(
					"WhsProduct|PickFaces|" + Parent.PK,
					() =>
					{
						var pickFaces = new WhsPickFaceCollection(Parent, Factory);
						RegisterEditableChildObject(pickFaces);
						return pickFaces;
					});
			}
		}

		#endregion

		#region ParamsByWhsAndClient

		[ChildEditable]
		public WhsProductParamsByWhsAndClientCollection ParamsByWhsAndClient
		{
			get
			{
				return Factory.GetCachedValue("WhsProduct|ParamsByWhsAndClient|" + Parent.PK,
					() =>
					{
						var paramsByWhsAndClient = new WhsProductParamsByWhsAndClientCollection(Parent, Factory);
						RegisterEditableChildObject(paramsByWhsAndClient);
						return paramsByWhsAndClient;
					});
			}
		}

		public WhsProductParamsByWhsAndClient GetParamsByWhsAndClient(WhsWarehouse warehouse, OrgHeader client)
		{
			return warehouse != null && client != null ? GetParamsByWhsAndClient(warehouse.PK, client.PK) : null;
		}

		public WhsProductParamsByWhsAndClient GetParamsByWhsAndClient(ZGuid warehousePk, ZGuid clientPk)
		{
			return ParamsByWhsAndClient.FirstOrDefault(p => p.W3_WW == warehousePk && p.W3_OH == clientPk);
		}

		public WhsCartonGroup GetOverridenCartonGroup(OrgHeader client)
		{
			var relationship = GetOwnerRelationship(client);
			var cartonGroupFK = relationship != null ? relationship.OU_WCG_CartonGroup : ZGuid.Empty;
			return cartonGroupFK.IsEmpty ? null : Factory.Load<WhsCartonGroup>(cartonGroupFK);
		}

		#endregion

		#region Product Style

		public WhsProductStyle ProductStyle
		{
			get { return Factory.Load<WhsProductStyle>(ProductStylePK); }
		}

		#endregion

		#region ProductStyleColour

		public WhsProductStyleColour ProductStyleColour
		{
			get { return Factory.Load<WhsProductStyleColour>(ProductStyleColourPK); }
		}

		#endregion

		#region ProductStyleClassification

		public WhsProductStyleClassification ProductStyleClassification
		{
			get { return Factory.Load<WhsProductStyleClassification>(ProductStyleClassificationPK); }
		}

		#endregion

		#region ProductStyleSize

		public WhsProductStyleSize ProductStyleSize
		{
			get { return Factory.Load<WhsProductStyleSize>(ProductStyleSizePK); }
		}

		#endregion

		#region GetOwnerRelationship

		public OrgPartRelation GetOwnerRelationship(OrgHeader client)
		{
			Argument.NotNull(client, nameof(client));

			return Parent.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
		}

		#endregion

		#endregion

		#region Properties

		#region QtyToStringFormat

		public string QtyToStringFormat
		{
			get
			{
				string format = "0";
				if (Parent.OP_CountDecimalPlaces > 0)
				{
					format += ".";
					format = format.PadRight(Parent.OP_CountDecimalPlaces + format.Length, '0');
				}
				return format;
			}
		}

		public ZString FormattedQtyAndUnit(ZDecimal units)
		{
			ZString result = Res.GetString("118b5979-c9ca-475b-9c2f-3d9bd59ded86", "Units");
			if (!Parent.OP_StockKeepingUnit.IsEmpty)
			{
				result = Parent.Lookups.OP_ProductUQ_List.GetDescriptionFromCode(Parent.OP_StockKeepingUnit);
			}
			return units.ToString(QtyToStringFormat, Culture.Current) + " " + ((units != 1m || Parent.OP_CountDecimalPlaces > 0) ? Grammar.Instance.Pluralize(result) : result.ToString());
		}

		#endregion

		#region FirstUNDG

		public UNDGDataItem FirstUNDG
		{
			get { return Parent.UNDGs.FirstOrDefault(); }
		}

		#endregion

		#region ProductImage

		public IeDoc ProductImage
		{
			get { return productImage ?? (productImage = GetMostRecentPublishedEDoc()); }
		}

		IeDoc productImage;

		#endregion

		#region GetMostRecentPublishedEDoc

		IeDoc GetMostRecentPublishedEDoc()
		{
			var alleDocs = ((IDocManagerSupport)parent).DocManagerInfo.AllEDocs;

			IeDoc result = null;
			foreach (IeDoc eDoc in alleDocs)
			{
				if (!eDoc.IsDeleted && eDoc.IsPublished && eDoc.DocType == Core.Constants.RefDocTypes.ImageFile)
				{
					if (result == null || GetLastChanged(eDoc) > GetLastChanged(result))
					{
						result = eDoc;
					}
				}
			}

			return result;
		}

		ZDateTime GetLastChanged(IeDoc eDoc)
		{
			var result = eDoc.LastEdited;
			if (result.IsEmpty)
			{
				result = eDoc.DateAdded;
			}

			return result;
		}

		#endregion

		// calc

		#region ProductStyle

		[List("ProductStyles")]
		public ZGuid ProductStylePK
		{
			get
			{
				if (!productStylePK.HasValue)
				{
					var styleColour = ProductStyleColour;
					productStylePK = styleColour != null ? styleColour.WSC_WST_ProductStyle : ZGuid.Empty;
				}

				return productStylePK.Value;
			}
			set
			{
				SetNonPersistentPropertyValue(ProductStylePKInfo, ref productStylePK, value);

				if (ProductStyleColourPK.IsValid && (ProductStyle == null || !ProductStyle.Colours.Select(c => c.PK).Contains(ProductStyleColourPK)))
				{
					ProductStyleColourPK = ZGuid.Empty;
				}

				if (ProductStyleClassificationPK.IsValid && (ProductStyle == null || !ProductStyle.Classifications.Select(c => c.PK).Contains(ProductStyleClassificationPK)))
				{
					ProductStyleClassificationPK = ZGuid.Empty;
				}

				if (ProductStyleSizePK.IsValid && (ProductStyle == null || !ProductStyle.Sizes.Select(s => s.PK).Contains(ProductStyleSizePK)))
				{
					ProductStyleSizePK = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductStylePK();
				}

				ProductStylePKInfo.RefreshBinding();
			}
		}
		ZGuid? productStylePK;

		public ZPropertyInfo ProductStylePKInfo
		{
			get { return GetZPropertyInfo(Schema.ProductStylePK); }
		}

		public WhsProductStyleCollection ProductStyles
		{
			get { return new WhsProductStyleCollection(Factory); }
		}

		#endregion

		#region ProductStyleColourPK

		[List("ProductStyle.Colours")]
		public ZGuid ProductStyleColourPK
		{
			get { return parent.OP_WSC_WhsProductStyleColour; }
			set
			{
				parent.OP_WSC_WhsProductStyleColour = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductStyleColourPK();
				}

				ProductStyleColourPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ProductStyleColourPKInfo
		{
			get { return GetZPropertyInfo(Schema.ProductStyleColourPK); }
		}

		protected bool ProductStyleColourPK_ReadOnly
		{
			get { return ProductStyle == null; }
		}

		#endregion

		#region ProductStyleClassificationPK

		[List("ProductStyle.Classifications")]
		public ZGuid ProductStyleClassificationPK
		{
			get { return parent.OP_WSS_WhsProductStyleClassification; }
			set
			{
				parent.OP_WSS_WhsProductStyleClassification = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductStyleClassificationPK();
				}

				ProductStyleClassificationPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ProductStyleClassificationPKInfo
		{
			get { return GetZPropertyInfo(Schema.ProductStyleClassificationPK); }
		}

		protected bool ProductStyleClassificationPK_ReadOnly
		{
			get { return ProductStyle == null; }
		}

		#endregion

		#region ProductStyleSize

		[List("ProductStyle.Sizes")]
		public ZGuid ProductStyleSizePK
		{
			get { return parent.OP_WSZ_WhsProductStyleSize; }
			set
			{
				parent.OP_WSZ_WhsProductStyleSize = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductStyleSizePK();
				}

				ProductStyleSizePKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ProductStyleSizePKInfo
		{
			get { return GetZPropertyInfo(Schema.ProductStyleSizePK); }
		}

		protected bool ProductStyleSizePK_ReadOnly
		{
			get { return ProductStyle == null; }
		}

		#endregion

		#region ProductStyleOwner

		public ZString ProductStyleOwner
		{
			get { return ProductStyle != null ? ProductStyle.Owner.OH_FullName : ZString.Empty; }
		}

		public ZPropertyInfo ProductStyleOwnerInfo
		{
			get { return GetZPropertyInfo(Schema.ProductStyleOwner); }
		}

		#endregion

		#region GetWeightInKG

		public ZDecimal GetWeightInKG(ZDecimal units)
		{
			var stockKeepingUnit = Parent.OP_StockKeepingUnit.ToUpper();
			return Constants.Weight.ContainsCode(stockKeepingUnit)
				? Constants.Weight.Convert(1, stockKeepingUnit, Constants.Weight.Kilograms) * units
				: Constants.Weight.Convert(Parent.OP_Weight, Parent.OP_WeightUQ.ToUpper(), Constants.Weight.Kilograms) * units;
		}

		public bool IsWeightUnitInvalid
		{
			get
			{
				return !Constants.Weight.ContainsCode(Parent.OP_StockKeepingUnit.ToUpper())
					&& !Constants.Weight.ContainsCode(Parent.OP_WeightUQ.ToUpper());
			}
		}

		public ZString GetInvalidWeightUnitErrorMessage()
		{
			var message = ZString.Empty;
			if (IsWeightUnitInvalid)
			{
				var builder = new ZStringBuilder();
				builder.AppendLine(Res.GetString("50f15571-6c94-4ede-9ac9-5be3a0f4b3d3", "Invalid Weight Unit in this Product Code: {0}. Please use following valid unit types:", Parent.OP_PartNum));
				builder.Append(string.Join(", ", Constants.Weight.Codes.OrderBy(w => w)));
				message = builder.ToString();
			}
			return message;
		}

		#endregion

		#region GetVolumeInM3

		public ZDecimal GetVolumeInM3(ZDecimal units)
		{
			var stockKeepingUnit = Parent.OP_StockKeepingUnit.ToUpper();
			return Constants.Volume.ContainsCode(stockKeepingUnit)
				? Constants.Volume.Convert(1, stockKeepingUnit, Constants.Volume.CubicMetres) * units
				: Constants.Volume.Convert(Parent.OP_Cubic, Parent.OP_CubicUQ.ToUpper(), Constants.Volume.CubicMetres) * units;
		}

		public bool IsVolumeUnitInvalid
		{
			get
			{
				return !Constants.Volume.ContainsCode(Parent.OP_StockKeepingUnit.ToUpper())
					&& !Constants.Volume.ContainsCode(Parent.OP_CubicUQ.ToUpper());
			}
		}

		public ZString GetInvalidVolumeUnitErrorMessage()
		{
			var message = ZString.Empty;
			if (IsVolumeUnitInvalid)
			{
				var builder = new ZStringBuilder();
				builder.AppendLine(Res.GetString("d8066967-1c5d-4fd7-80da-3877e0e2b267", "Invalid Volume Unit in this Product Code: {0}. Please use following valid unit types:", Parent.OP_PartNum));
				builder.Append(string.Join(", ", Constants.Volume.Codes.OrderBy(v => v)));
				message = builder.ToString();
			}
			return message;
		}

		#endregion

		#endregion

		#region Flags

		#region Part Attributes

		#region IsAnyAttributeUsed

		public bool IsAnyAttributeUsed(OrgHeader client) =>
			IsExpiryDateUsed(client) ||
			IsPackingDateUsed(client) ||
			IsSerialNumberUsed(client) ||
			IsPartAttributeUsed(client, 1) ||
			IsPartAttributeUsed(client, 2) ||
			IsPartAttributeUsed(client, 3);

		#endregion

		#region IsAnyPartAttribReleaseCaptured

		public bool IsAnyPartAttribReleaseCaptured(OrgHeader client)
		{
			return IsPartAttribReleaseCaptured(client, 1)
				|| IsPartAttribReleaseCaptured(client, 2)
				|| IsPartAttribReleaseCaptured(client, 3)
				|| IsSerialNumberReleaseCaptured(client);
		}

		#endregion

		#region IsPartAttribReleaseCaptured

		public bool IsPartAttribReleaseCaptured(OrgHeader client, ZInt attributeNumber)
		{
			return client != null
				&& client.PartAttributeManager.IsPartAttributeReleaseCaptured(Parent, attributeNumber);
		}

		public bool IsMandatoryPartAttribReleaseCaptured(OrgHeader client, ZInt attributeNumber)
		{
			return client != null
				&& client.PartAttributeManager.IsPartAttributeMandatory(attributeNumber)
				&& client.PartAttributeManager.IsPartAttributeReleaseCaptured(Parent, attributeNumber);
		}

		#endregion

		#region IsPartAttributeUsed

		public bool IsPartAttributeUsed(OrgHeader client, int attributeNumber) =>
			client != null && client.PartAttributeManager.IsPartAttributeUsedByProduct(Parent, attributeNumber);

		#endregion

		#region IsPartAttributeAJulianBatchNumberAndUsed

		public bool IsPartAttributeAJulianBatchNumberAndUsed(OrgHeader client, ZInt attributeNumber)
		{
			return client != null && client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(Parent, attributeNumber);
		}

		#endregion

		#region IsSerialNumberReleaseCaptured

		public bool IsSerialNumberReleaseCaptured(OrgHeader client)
		{
			return client != null
				&& client.PartAttributeManager.IsSerialNumberReleaseCaptured(Parent);
		}

		#endregion

		#region IsSerialNumberUsed

		public bool IsSerialNumberUsed(OrgHeader client) =>
			client != null && client.PartAttributeManager.IsSerialNumberUsedByProduct(Parent);

		#endregion

		#region IsSerialNumberUsedAndNotReleaseCaptured

		public bool IsSerialNumberUsedAndNotReleaseCaptured(OrgHeader client)
			=> IsSerialNumberUsed(client) && !IsSerialNumberReleaseCaptured(client);

		#endregion

		#region IsAJulianBatchNumberAttributeUsed

		public bool IsAJulianBatchNumberAttributeUsed(OrgHeader client)
		{
			return client != null && client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(Parent);
		}

		#endregion

		#region IsAJulianBatchNumberAttributeUsedAndHasAnyStock

		public bool IsAJulianBatchNumberAttributeUsedAndHasAnyStock(OrgHeader client, WhsWarehouse whs)
		{
			return client != null && whs != null && IsAJulianBatchNumberAttributeUsed(client) && HasAnyStock(whs, client);
		}

		#endregion

		#region IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified

		public bool IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(OrgHeader client, WhsWarehouse warehouse)
		{
			bool result = false;
			if (warehouse != null && client != null && IsAJulianBatchNumberAttributeUsed(client))
			{
				var param = GetParamsByWhsAndClient(warehouse, client);
				result = (param == null || param.W3_MaximumShelfLife <= 0m);
			}
			return result;
		}

		#endregion

		#region IsExpiryDateUsed

		public bool IsExpiryDateUsed(OrgHeader client) =>
			client != null && client.PartAttributeManager.IsExpiryDateUsedByProduct(Parent);

		#endregion

		#region IsPackingDateUsed

		public bool IsPackingDateUsed(OrgHeader client) =>
			client != null && client.PartAttributeManager.IsPackingDateUsedByProduct(Parent);

		#endregion

		#region ExpiryDateFormatString

		public ZString ExpiryDateFormatString(OrgHeader client)
		{
			return (client != null) ? client.PartAttributeManager.ExpiryDateFormatString(Parent) : ZString.Empty;
		}

		#endregion

		#region PackingDateFormatString

		public ZString PackingDateFormatString(OrgHeader client)
		{
			return (client != null) ? client.PartAttributeManager.PackingDateFormatString(Parent) : ZString.Empty;
		}

		#endregion

		#region IsAttributeNeutralUsed

		public bool IsAttributeNeutralUsed(OrgHeader client)
		{
			return client != null && client.PartAttributeManager.IsAttributeNeutralUsedByProduct(Parent);
		}

		#endregion

		#endregion

		#region IsBOMProductPickedOnSalesOrder

		public bool IsBOMProductPickedOnSalesOrder
		{
			get { return Parent.OP_IsComponentPickedOnSalesOrder && Parent.BillOfMaterials.Count > 0; }
		}

		#endregion

		#region IsCompletePalletPickingUsed

		public bool IsCompletePalletPickingUsed(OrgHeader client)
		{
			return client != null && client.PartAttributeManager.IsCompletePalletPickingUsedByProduct(Parent);
		}

		#endregion

		#region IsPackTypeUsedByProduct

		public bool IsPackTypeUsedByProduct(ZString packType) => GetPackTypeCodes().Any(p => p == packType);

		#endregion

		#region IsAValidJulianBatchNumberFormat

		public bool IsAValidJulianBatchNumberFormat(OrgHeader client, ZString julianBatchNumber)
		{
			var result = false;
			if (client != null && IsAJulianBatchNumberAttributeUsed(client))
			{
				var relation = Parent.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);
				if (relation != null)
				{
					result = !ExtractDateFromJulianBatchNumber(relation, julianBatchNumber).IsEmpty;
				}
			}
			return result;
		}

		#endregion

		#region HasAnyStock

		public bool HasAnyStock(WhsWarehouse warehouse, OrgHeader client)
		{
			var result = false;
			if (warehouse != null && client != null)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, Parent.PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

				var inventories = Factory.Load<WhsInventoryView>(query);
				result = inventories.Any(i => i.WI_WW_Whs == warehouse.PK);
			}
			return result;
		}

		#endregion

		#endregion

		#region CalculateDateFromJulianBatchNumber

		public ZDate CalculateExpiryDate(OrgHeader client, WhsWarehouse whs, ZString julianBatchNumber)
		{
			return CalcuateDateCore(client, whs, julianBatchNumber, true);
		}

		public ZDate GetPackingDateFromJulianBatchNumber(OrgHeader client, WhsWarehouse whs, ZString julianBatchNumber)
		{
			return CalcuateDateCore(client, whs, julianBatchNumber, false);
		}

		ZDate CalcuateDateCore(OrgHeader client, WhsWarehouse whs, ZString julianBatchNumber, bool isExpiry)
		{
			var result = ZDate.Empty;
			if (client != null && whs != null && !julianBatchNumber.IsEmpty && julianBatchNumber.Length >= 4)
			{
				var productParam = GetParamsByWhsAndClient(whs, client);
				if (productParam != null)
				{
					var relation = GetOwnerRelationship(client);
					if (relation != null && !relation.OU_JulianBatchNoFormat.IsEmpty)
					{
						result = ExtractDateFromJulianBatchNumber(relation, julianBatchNumber);
						if (isExpiry && !result.IsEmpty)
						{
							result = result.AddDays(productParam.W3_MaximumShelfLife.ToZInt());
						}
					}
				}
			}
			return result;
		}

		ZDate ExtractDateFromJulianBatchNumber(OrgPartRelation relation, ZString julianBatchNumber)
		{
			var result = ZDate.Empty;
			var yearString = "";
			var daysString = "";
			if (relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.BatchNumber_YDDD && julianBatchNumber.Length >= 4)
			{
				yearString = julianBatchNumber.Substring(julianBatchNumber.Length - 4, 1);
				daysString = julianBatchNumber.Substring(julianBatchNumber.Length - 3, 3);
			}
			else if (relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD && julianBatchNumber.Length >= 5)
			{
				yearString = julianBatchNumber.Substring(julianBatchNumber.Length - 5, 2);
				daysString = julianBatchNumber.Substring(julianBatchNumber.Length - 3, 3);
			}
			else if (relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.YDDD_BatchNumber && julianBatchNumber.Length >= 4)
			{
				yearString = julianBatchNumber.Substring(0, 1);
				daysString = julianBatchNumber.Substring(1, 3);
			}
			if (relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.YYDDD_BatchNumber && julianBatchNumber.Length >= 5)
			{
				yearString = julianBatchNumber.Substring(0, 2);
				daysString = julianBatchNumber.Substring(2, 3);
			}

			int year, days;
			if (int.TryParse(yearString, out year) && int.TryParse(daysString, out days))
			{
				var yearMultiplier = (relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.BatchNumber_YDDD || relation.OU_JulianBatchNoFormat == JulianBatchNumberFormatList.Codes.YDDD_BatchNumber) ? 10 : 100;
				var yearBase = ZDate.Today.Year % yearMultiplier;
				yearBase = (yearBase < year) ? yearBase + yearMultiplier : yearBase;
				if ((days > 0 && days <= 365) || (days == 366 && IsALeapYear(ZDateTime.Today.Year - yearBase + year)))
				{
					result = new ZDate(ZDateTime.Today.Year - yearBase + year, 1, 1).AddDays(days - 1);
				}
			}
			return result;
		}

		bool IsALeapYear(int year)
		{
			return year % 400 == 0 || (year % 100 != 0 && year % 4 == 0);
		}

		#endregion

		#region OP_CountDecimalPlacesInfo_ValueChanged

		void OP_CountDecimalPlacesInfo_ValueChanged(object sender, EventArgs e)
		{
			var decimalPlaces = parent.OP_CountDecimalPlaces;
			var pickFaces = PickFaces;
			if (pickFaces != null && pickFaces.Count > 0 && decimalPlaces < 29) // maximum decimal places can be 28
			{
				foreach (var pickFace in pickFaces.Cast<WhsPickFace>().ToArray())
				{
					pickFace.WF_ReplenishmentMultiple = Utilities.Round(pickFace.WF_ReplenishmentMultiple, decimalPlaces);
				}
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore(); // call RunPreSaveValidationCore() on all children then fire OnNotificationsChanged()
		}

		public WhsProductValidation Validation
		{
			get { return new WhsProductValidation(this); }
		}

		#endregion

		#region Implementation

		WhsWarehouseCollection warehouses;

		#endregion

		#region ICartonisableItemDefinition Members

		Guid ICartonisableItemDefinition.PK => Parent.PK.ToGuid();

		decimal ICartonisableItemDefinition.Height => Parent.OP_Height;

		decimal ICartonisableItemDefinition.Length => Parent.OP_Depth;

		decimal ICartonisableItemDefinition.Width => Parent.OP_Width;

		string ICartonisableItemDefinition.DimensionUQ => Parent.OP_MeasureUQ;

		decimal ICartonisableItemDefinition.Volume
		{
			get { return Constants.Volume.ContainsCode(Parent.OP_StockKeepingUnit.ToUpper()) ? 1m : (decimal)Parent.OP_Cubic; }
		}

		string ICartonisableItemDefinition.VolumeUQ
		{
			get
			{
				var sku = Parent.OP_StockKeepingUnit.ToUpper();
				return Constants.Volume.ContainsCode(sku) ? sku : Parent.OP_CubicUQ;
			}
		}

		decimal ICartonisableItemDefinition.Weight
		{
			get { return Constants.Weight.ContainsCode(Parent.OP_StockKeepingUnit.ToUpper()) ? 1m : (decimal)Parent.OP_Weight; }
		}

		string ICartonisableItemDefinition.WeightUQ
		{
			get
			{
				var sku = Parent.OP_StockKeepingUnit.ToUpper();
				return Constants.Weight.ContainsCode(sku) ? sku : Parent.OP_WeightUQ;
			}
		}

		bool ICartonisableItemDefinition.KeepUpright => Parent.OP_KeepUpright;

		#endregion
	}
}
