using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartUnitValidation : AutoOrgPartUnitValidation
	{
		public OrgPartUnitValidation(AutoOrgPartUnit parent) : base(parent)
		{
		}

		#region Parent

		new OrgPartUnit Parent
		{
			get { return (OrgPartUnit)base.Parent; }
		}

		#endregion

		#region HasPendingPicksByUOM

		public static bool HasPendingPicksByUOM(OrgPartUnit parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			var factory = parent.Factory;
			return factory.GetCachedValue("HasPendingPicksByUOM|" + parent.PK, () =>
			{
				var pickLineQuery = new ZDBOnlyQuery(typeof(IWhsPickLine));
				// is the pick using UOM
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_F3_NKAllocatedPackType, SQLComparisonOperator.NotEqual, "");

				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
				orderLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_OP, parent.OF_OP);

				var orderSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsDocketLineSchema.WE_WD);
				orderSubQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, "CAN");

				var pickSubQuery = new ZDBOnlySubQuery(typeof(IWhsPick), WhsDocketSchema.WD_WP);
				pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, new[] { "FIN", "CAN" });

				orderSubQuery.AddSubQuery(pickSubQuery, JoinCondition.And);
				orderLineSubQuery.AddSubQuery(orderSubQuery, JoinCondition.And);
				pickLineQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);

				return parent.Factory.LoadTop1<IWhsPickLine>(pickLineQuery) != null;
			});
		}

		#endregion

		#region Fields validation checks

		protected override void CheckOF_PackType()
		{
			base.CheckOF_PackType();
			MandatoryValidation.CheckEntered(Parent.OF_PackTypeInfo);

			CheckForChangedPackUnitConversionWithPendingPicksByUOM(Parent.OF_PackTypeInfo);
			if (!Parent.OF_PackTypeInfo.HasErrors())
			{
				CheckForSelfReferenceConversion(Parent.OF_PackTypeInfo);
				CheckIfValidPackType(Parent.OF_PackTypeInfo);
				CheckForDuplicateConversions(Parent.OF_PackTypeInfo);
				CheckForLoops(Parent.OF_PackTypeInfo);
				CheckForInconsistentConversionsToSKU(Parent.OF_PackTypeInfo, new string[] { Parent.OF_PackType });
				CheckForUnreachablePackType(Parent.OF_PackTypeInfo, Parent.OF_PackType);
				CheckForMetricWeightConversion(Parent.OF_PackTypeInfo);
				CheckIfChangesInterfereWithPickOnSalesOrders(Parent.OF_PackTypeInfo);
			}
		}

		void CheckForChangedPackUnitConversionWithPendingPicksByUOM(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsInDatabase
				&& !IsOriginallyWeightOrVolumeConversion(Parent)
				&& !propertyInfo.OriginalValue.Equals(propertyInfo.Value)
				&& HasPendingPicksByUOM(Parent))
			{
				propertyInfo.AddError(PendingUOMPicksDetected);
			}
		}

		protected override void CheckOF_ParentPackType()
		{
			base.CheckOF_ParentPackType();
			MandatoryValidation.CheckEntered(Parent.OF_ParentPackTypeInfo);

			CheckForChangedPackUnitConversionWithPendingPicksByUOM(Parent.OF_ParentPackTypeInfo);
			if (!Parent.OF_ParentPackTypeInfo.HasErrors())
			{
				CheckForSelfReferenceConversion(Parent.OF_ParentPackTypeInfo);
				CheckIfValidPackType(Parent.OF_ParentPackTypeInfo);
				CheckForDuplicateConversions(Parent.OF_ParentPackTypeInfo);
				CheckForLoops(Parent.OF_ParentPackTypeInfo);
				CheckForInconsistentConversionsToSKU(Parent.OF_ParentPackTypeInfo, new string[] { Parent.OF_ParentPackType });
				CheckForUnreachablePackType(Parent.OF_ParentPackTypeInfo, Parent.OF_ParentPackType);
				CheckForMetricWeightConversion(Parent.OF_ParentPackTypeInfo);
				CheckIfChangesInterfereWithPickOnSalesOrders(Parent.OF_ParentPackTypeInfo);
			}
		}

		protected override void CheckOF_QuantityInParent()
		{
			base.CheckOF_QuantityInParent();
			MandatoryValidation.CheckEntered(Parent.OF_QuantityInParentInfo);
			CompareValidation.CheckNumberGreaterThanZero(Parent.OF_QuantityInParentInfo);

			CheckForChangedPackUnitConversionWithPendingPicksByUOM(Parent.OF_QuantityInParentInfo);
			if (!Parent.OF_QuantityInParentInfo.HasErrors())
			{
				CheckForInconsistentConversionsToSKU(Parent.OF_QuantityInParentInfo, new string[] { Parent.OF_PackType, Parent.OF_ParentPackType });
				CheckForMetricWeightConversion(Parent.OF_QuantityInParentInfo);
				CheckIfChangesInterfereWithPickOnSalesOrders(Parent.OF_QuantityInParentInfo);
			}
		}

		internal static bool IsOriginallyWeightOrVolumeConversion(OrgPartUnit parent)
		{
			var result = false;
			if (parent.OF_ParentPackTypeInfo.OriginalValue.IsValid && parent.OF_PackTypeInfo.OriginalValue.IsValid)
			{
				result = Core.Constants.Weight.ContainsCode(parent.OF_ParentPackTypeInfo.OriginalValue.ToString())
						 || Core.Constants.Volume.ContainsCode(parent.OF_ParentPackTypeInfo.OriginalValue.ToString())
						 || Core.Constants.Weight.ContainsCode(parent.OF_PackTypeInfo.OriginalValue.ToString())
						 || Core.Constants.Volume.ContainsCode(parent.OF_PackTypeInfo.OriginalValue.ToString());
			}
			return result;
		}

		protected override void CheckOF_Cubic()
		{
			base.CheckOF_Cubic();
			CheckForDifferentValuesForSameParentPackage(unit => unit.OF_Cubic, product => product.OP_Cubic, Parent.OF_CubicInfo);
			CheckHasUQ(Res.GetString("67d1eac6-70c6-48bf-9e38-803c1d5ad6cb", "Product Cubic UQ is required."), unit => unit.OF_Cubic, product => product.OP_CubicUQ, Parent.OF_CubicInfo);
		}

		protected override void CheckOF_Depth()
		{
			base.CheckOF_Depth();
			CheckForDifferentValuesForSameParentPackage(unit => unit.OF_Depth, product => product.OP_Depth, Parent.OF_DepthInfo);
			CheckHasMeasurementUQ(unit => unit.OF_Depth, Parent.OF_DepthInfo);
		}

		protected override void CheckOF_Height()
		{
			base.CheckOF_Height();
			CheckForDifferentValuesForSameParentPackage(unit => unit.OF_Height, product => product.OP_Height, Parent.OF_HeightInfo);
			CheckHasMeasurementUQ(unit => unit.OF_Height, Parent.OF_HeightInfo);
		}

		protected override void CheckOF_Width()
		{
			base.CheckOF_Width();
			CheckForDifferentValuesForSameParentPackage(unit => unit.OF_Width, product => product.OP_Width, Parent.OF_WidthInfo);
			CheckHasMeasurementUQ(unit => unit.OF_Width, Parent.OF_WidthInfo);
		}

		protected override void CheckOF_Weight()
		{
			base.CheckOF_Weight();
			CheckForDifferentValuesForSameParentPackage(unit => unit.OF_Weight, product => product.OP_Weight, Parent.OF_WeightInfo);
			CheckHasUQ(Res.GetString("d84f61e7-a185-430a-885c-ac3a62a40883", "Product Weight UQ is required."), unit => unit.OF_Weight, product => product.OP_WeightUQ, Parent.OF_WeightInfo);
		}

		void CheckHasMeasurementUQ(Func<OrgPartUnit, ZDecimal> partUnitValueSelector, ZPropertyInfo propertyInfo)
		{
			CheckHasUQ(Res.GetString("54511a26-2029-476e-9bea-79a6ac1dfec8", "Product Measurement UQ is required."), partUnitValueSelector, p => p.OP_MeasureUQ, propertyInfo);
		}

		void CheckHasUQ(string errorMessage, Func<OrgPartUnit, ZDecimal> partUnitValueSelector, Func<OrgSupplierPart, ZString> productUQSelector, ZPropertyInfo propertyInfo)
		{
			var product = Parent.SupplierPart;
			if (product != null) // should never happen in production
			{
				var partUnitValue = partUnitValueSelector(Parent);
				if (partUnitValue > 0 && productUQSelector(product).IsEmpty)
				{
					propertyInfo.AddError(errorMessage);
				}
			}
		}

		void CheckForDifferentValuesForSameParentPackage(Func<OrgPartUnit, ZDecimal> partUnitValueSelector, Func<OrgSupplierPart, ZDecimal> productValueSelector, ZPropertyInfo propertyInfo)
		{
			var packType = Parent.OF_ParentPackType;
			var product = Parent.SupplierPart;
			if (product != null) // should never happen in production
			{
				if (packType == product.OP_StockKeepingUnit)
				{
					var partUnitValue = partUnitValueSelector(Parent);
					if (partUnitValue > 0 && partUnitValue != productValueSelector(product))
					{
						propertyInfo.AddError(Res.GetString("AA7C0CF8-F4D4-41AB-BF0F-818561500BBF", "Value for '{0}' should match corresponding property of a product definition as '{1}' is a Stock Unit.", propertyInfo.Description, packType));
					}
				}
				else
				{
					var rowsToCompare = product.PartUnits.Cast<OrgPartUnit>().Where(p => p.OF_ParentPackType == packType).ToArray();
					if (rowsToCompare.Length > 1 && rowsToCompare.DistinctBy(partUnitValueSelector).Count() > 1)
					{
						propertyInfo.AddError(Res.GetString("5039B5CB-A947-4824-AB6D-239AE7BCFDEC", "Should not have different values for '{0}' column for same Parent Pack Type '{1}'.", propertyInfo.Description, packType));
					}
				}
			}
		}

		#endregion

		#region CheckForDuplicateConversions

		void CheckForDuplicateConversions(ZPropertyInfo info)
		{
			if (Parent.SupplierPart != null)
			{
				foreach (OrgPartUnit unit in Parent.SupplierPart.PartUnits)
				{
					if (unit != Parent && unit.OF_PackType == Parent.OF_PackType && unit.OF_ParentPackType == Parent.OF_ParentPackType)
					{
						info.AddError(DuplicateUnitError);
					}
				}
			}
		}

		#endregion

		#region CheckForIncosistentConversionsToSKU

		void CheckForInconsistentConversionsToSKU(ZPropertyInfo info, IEnumerable<string> packTypes)
		{
			if (Parent.SupplierPart != null)
			{
				var packTypesToLookup = packTypes.Except(new string[] { Parent.SupplierPart.OP_StockKeepingUnit }).ToList();
				var conversions = new ConversionsToSKUTable(Parent.SupplierPart);
				foreach (var error in conversions.ErrorsForPackTypes(packTypesToLookup))
				{
					info.AddError(error);
				}
				foreach (var error in conversions.WarningsForPackTypes(packTypesToLookup))
				{
					info.AddWarning(error);
				}
			}
		}

		#endregion

		#region CheckForLoops

		void CheckForLoops(ZPropertyInfo info)
		{
			new OrgPartUnitLoopChecker().CheckIfItIsStartOfTheLoop(Parent, info);
		}

		#region OrgPartUnitLoopChecker

		class OrgPartUnitLoopChecker
		{
			public void CheckIfItIsStartOfTheLoop(OrgPartUnit parent, ZPropertyInfo info)
			{
				StepDeeper(parent, new List<ZString>(), info);
			}

			void StepDeeper(OrgPartUnit current, List<ZString> route, ZPropertyInfo info)
			{
				//cycle found
				if (route.Contains(current.OF_PackType))
				{
					// we log it only if we looped to the first OrgPartUnit
					if (route.First() == current.OF_PackType)
					{
						info.AddWarning(LoopWarning + ": " + BuildLoopDescription(route, current.OF_PackType) + ".");
					}
					// otherwise it is a loop with a tail, and we ignore it
					return;
				}
				route.Add(current.OF_PackType);
				foreach (var parentLink in GetParentOrgPartUnits(current))
				{
					StepDeeper(parentLink, route.ToList(), info);
				}
			}

			IEnumerable<OrgPartUnit> GetParentOrgPartUnits(OrgPartUnit current)
			{
				if (current.SupplierPart != null)
				{
					return current.SupplierPart.PartUnits.Cast<OrgPartUnit>().Where(x => x.OF_PackType == current.OF_ParentPackType);
				}
				return Array.Empty<OrgPartUnit>();
			}

			string BuildLoopDescription(IEnumerable<ZString> route, ZString lastStep)
			{
				return string.Join("->", route.Concat(new[] { lastStep }));
			}
		}

		#endregion

		#endregion

		#region CheckForMetricWeightConversion

		void CheckForMetricWeightConversion(ZPropertyInfo info)
		{
			if (IsMetricWeight(Parent.OF_PackType) && IsMetricWeight(Parent.OF_ParentPackType))
			{
				if (!IsCorrectMetricWeightConversion(Parent.OF_QuantityInParent, Parent.OF_PackType, Parent.OF_ParentPackType))
				{
					info.AddWarning(Res.GetString("bc3ed6c4-5fa0-48eb-962b-8277460a3d34",
						"Incorrect metric weight conversion. Should be {0} {1} in {2}.",
						Core.Constants.Weight.Convert(1, Parent.OF_ParentPackType, Parent.OF_PackType).ToString("#.#"),
						Parent.OF_PackType,
						Parent.OF_ParentPackType));
				}
			}
		}

		static bool IsMetricWeight(ZString packType)
		{
			return Core.Constants.Weight.ContainsCode(packType)
				   && !Core.Constants.Weight.IsImperial(packType);
		}

		static bool IsCorrectMetricWeightConversion(ZDecimal quantityInParent, ZString packType, ZString parentPackType)
		{
			return 1 == Core.Constants.Weight.Convert(quantityInParent, packType, parentPackType);
		}

		#endregion

		#region CheckForSelfReferenceConversion

		void CheckForSelfReferenceConversion(ZPropertyInfo info)
		{
			if (Parent.OF_PackType == Parent.OF_ParentPackType)
			{
				info.AddError(SelfReferenceUnitError);
			}
		}

		#endregion

		#region CheckForUnreachablePackType

		void CheckForUnreachablePackType(ZPropertyInfo info, ZString packType)
		{
			packType = packType.ToUpper();
			var supplierPart = Parent.SupplierPart;
			if (supplierPart != null)
			{
				if (!Core.Constants.Volume.ContainsCode(packType) && !Core.Constants.Weight.ContainsCode(packType))
				{
					var conversions = new ConversionsToSKUTable(supplierPart);
					if (conversions[packType] == null)
					{
						info.AddWarning(Res.GetString("ca3ee846-34f7-411e-bbe2-f223ffc70444", "There is no conversion between {0} and the Stock Unit of the product, {1}.", packType, supplierPart.OP_StockKeepingUnit.ToUpper()));
					}
				}
			}
		}

		#endregion

		#region CheckIfChangesInterfereWithPickOnSalesOrders

		void CheckIfChangesInterfereWithPickOnSalesOrders(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && (propertyInfo.HasChanges || !Parent.IsInDatabase))
			{
				if (HasPicksOnSalesOrder(Parent.SupplierPart))
				{
					propertyInfo.AddError(PickOnSalesOrderDetected);
				}
			}
		}

		internal static bool HasPicksOnSalesOrder(OrgSupplierPart component)
		{
			var result = false;
			if (component != null)
			{
				var factory = component.Factory;
				result = ObjectFactory.Get<IWhsPickOnSalesOrderDetector>().IsComponentUsedToBuiltKitOnSalesOrder(factory, component.PK);
			}
			return result;
		}

		#endregion

		#region CheckIfValidPackType

		void CheckIfValidPackType(ZPropertyInfo info)
		{
			var message = ResString.GetMultilingualString("b608f03e-f97c-4b7a-829d-10d7bea726d4", "{0} is not a valid pack type.", info.Value);
			if (DataRegistry.Instance.UnitConversionPackTypesValidation)
			{
				ListValidation.ErrorIfInvalidCode(message, info);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, message);
			}
		}

		#endregion

		#region Error strings

		public static MultilingualString SelfReferenceUnitError => ResString.GetMultilingualString("6b8ac4ee-5d99-4bcc-bfcc-69cc91b0cc21", "Specifying a unit conversion for the same pack type is invalid");

		public static MultilingualString DuplicateUnitError => ResString.GetMultilingualString("f5ccc495-feb1-4afe-af05-57c398ea0a2f", "There are two entries for the same package / parent package. Please remove one of them");

		public static MultilingualString LoopWarning => ResString.GetMultilingualString("e93eb2db-80db-46f2-b330-cdae14c0bd91", "This pack type is part of the loop");

		public static MultilingualString PendingUOMPicksDetected => ResString.GetMultilingualString("d901d7b8-e76c-4d88-a5e9-313a3f01bfdc", "There are currently pending picks for this product that Pick by UOM which relies on Unit Conversions for the product. Finalize or Cancel the Pick and open the form again.");

		public static MultilingualString PickOnSalesOrderDetected => ResString.GetMultilingualString("bff42829-6636-4683-badc-0ba264790450", "Cannot change the unit conversions for this product.\r\nThis product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition and conversions cannot be changed, please finalize these Picks first.");

		#endregion
	}
}
