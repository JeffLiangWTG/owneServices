using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsOrgSupplierPartFilterStripBusinessObject : MasterFiles.Module.OrgSupplierPartFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetOrgSupplierPartModuleFiltersCore();

			AddProductStyleFilter(result);
			AddProductStyleColourFilter(result);
			AddProductStyleClassificationFilter(result);
			AddProductStyleSizeFilter(result);

			return result;
		}

		#region Product Style Filter

		void AddProductStyleFilter(ModuleFilterCollection filters)
		{
			var productStyleFilter = filters.AddGuidFilter(WhsProduct.Schema.ProductStyle, ModuleIDs.WhsConfigProductStyle, GetProductStyleQuery, GetProductStyles);
			productStyleFilter.MultilingualDescription = ResString.GetMultilingualString("11E38220-A23F-4e64-93E5-EE429CB1BDC1", "Product Style");
			productStyleFilter.PropertyInfo.ValueChanged += OnProductStyleChanged;
			productStyleFilter.IsPublishedOnWeb = false;
		}

		protected ZQuery GetProductStyleQuery(ZGuid value)
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var colourSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleColour), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourSubQuery.AddToFilter(WhsProductStyleColourSchema.WSC_WST_ProductStyle, value);

			var sizeSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleSize), OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize);
			sizeSubQuery.AddToFilter(WhsProductStyleSizeSchema.WSZ_WST_ProductStyle, value);

			partQuery.AddSubQuery(colourSubQuery, JoinCondition.And);
			partQuery.AddSubQuery(sizeSubQuery, JoinCondition.And);

			return partQuery;
		}

		WhsProductStyleCollection GetProductStyles() => Factory.GetCachedValue("WhsOrgSupplierPartFilterStripBusinessObject|GetProductStyles", () => new WhsProductStyleCollection(Factory));

		#endregion

		#region Product Style Colour Filter

		void AddProductStyleColourFilter(ModuleFilterCollection filters)
		{
			var productStyleColourFilter = filters.AddTextFilter(WhsProduct.Schema.ProductStyleColour, GetProductStyleColourQuery, GetProductStyleColours);
			productStyleColourFilter.Category = FilterCategories.Other;
			productStyleColourFilter.MultilingualDescription = ResString.GetMultilingualString("CE03E351-9011-445b-9FD3-81B4BFC806A4", "Product Style Color");
			productStyleColourFilter.IsPublishedOnWeb = false;
		}
		ZQuery GetProductStyleColourQuery(ZString value)
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var colourSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleColour), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourSubQuery.AddToFilter(WhsProductStyleColourSchema.WSC_Code, value);

			partQuery.AddSubQuery(colourSubQuery, JoinCondition.And);

			return partQuery;
		}

		IList GetProductStyleColours()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("WhsOrgSupplierPartFilterStripBusinessObject|GetProductStyleColours|" + ProductStylePK, () => new WhsProductStyleColourCollection(ProductStyle))
				: new List<WhsProductStyleColour>().AsReadOnly();
		}

		#endregion

		#region Product Style Classification Filter

		void AddProductStyleClassificationFilter(ModuleFilterCollection filters)
		{
			var productStyleClassificationFilter = filters.AddTextFilter(WhsProduct.Schema.ProductStyleClassification, GetProductStyleClassificationQuery, GetProductStyleClassifications);
			productStyleClassificationFilter.Category = FilterCategories.Other;
			productStyleClassificationFilter.MultilingualDescription = ResString.GetMultilingualString("d87fe660-3190-4250-aa7d-e0ffd46a2745", "Product Style Classification");
			productStyleClassificationFilter.IsPublishedOnWeb = false;
		}
		ZQuery GetProductStyleClassificationQuery(ZString value)
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var classificationSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleClassification), OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification);
			classificationSubQuery.AddToFilter(WhsProductStyleClassificationSchema.WSS_Code, value);

			partQuery.AddSubQuery(classificationSubQuery, JoinCondition.And);

			return partQuery;
		}

		IList GetProductStyleClassifications()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("WhsOrgSupplierPartFilterStripBusinessObject|GetProductStyleClassifications|" + ProductStylePK, () => new WhsProductStyleClassificationCollection(ProductStyle))
				: new List<WhsProductStyleClassification>().AsReadOnly();
		}

		#endregion

		#region Product Style Size Filter

		void AddProductStyleSizeFilter(ModuleFilterCollection filters)
		{
			var styleSizeFilter = filters.AddTextFilter(WhsProduct.Schema.ProductStyleSize, GetProductStyleSizeQuery, GetProductStyleSizes);
			styleSizeFilter.Category = FilterCategories.Other;
			styleSizeFilter.MultilingualDescription = ResString.GetMultilingualString("1871EF5B-868A-456f-91A3-51815D1713DC", "Product Style Size");
			styleSizeFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductStyleSizeQuery(ZString value)
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			var sizeQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleSize), OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize);
			sizeQuery.AddToFilter(WhsProductStyleSizeSchema.WSZ_Size, value);

			partQuery.AddSubQuery(sizeQuery, JoinCondition.And);

			return partQuery;
		}

		IList GetProductStyleSizes()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("WhsOrgSupplierPartFilterStripBusinessObject|GetProductStyleSize|" + ProductStylePK, () => new WhsProductStyleSizeCollection(ProductStyle))
				: new List<WhsProductStyleSize>().AsReadOnly();
		}

		#endregion

		void OnProductStyleChanged(object sender, EventArgs e)
		{
			ColourCodeFilter.Property = "";
			ClassificationCodeFilter.Property = "";
			StyleSizeFilter.Property = "";
		}

		ModuleTextFilter ColourCodeFilter => (ModuleTextFilter)ModuleFilters[WhsProduct.Schema.ProductStyleColour];
		ModuleTextFilter ClassificationCodeFilter => (ModuleTextFilter)ModuleFilters[WhsProduct.Schema.ProductStyleClassification];
		ModuleTextFilter StyleSizeFilter => (ModuleTextFilter)ModuleFilters[WhsProduct.Schema.ProductStyleSize];
		ModuleGuidFilter ProductStyleFilter => (ModuleGuidFilter)ModuleFilters[WhsProduct.Schema.ProductStyle];
		WhsProductStyle ProductStyle => !ProductStylePK.IsEmpty ? Factory.Load<WhsProductStyle>(ProductStylePK) : null;

		ZGuid ProductStylePK => ProductStyleFilter != null && ProductStyleFilter.IsActive ? ProductStyleFilter.Property : ZGuid.Empty;
	}
}
