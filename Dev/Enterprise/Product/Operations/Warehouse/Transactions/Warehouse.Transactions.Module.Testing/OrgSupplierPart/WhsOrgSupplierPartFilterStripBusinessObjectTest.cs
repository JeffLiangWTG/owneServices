using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsOrgSupplierPartFilterStripBusinessObject))]
	class WhsOrgSupplierPartFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsOrgSupplierPartFilterStripBusinessObject();
		}

		public void TestProductStyleSizeAndColourFiltersAreInProductModuleFilters()
		{
			using (var module = new ProductModule())
			{
				var filters = module.FilterBusinessObject.ModuleFilters;

				AssertNotNull(filters[WhsProduct.Schema.ProductStyle]);
				AssertNotNull(filters[WhsProduct.Schema.ProductStyleColour]);
				AssertNotNull(filters[WhsProduct.Schema.ProductStyleClassification]);
				AssertNotNull(filters[WhsProduct.Schema.ProductStyleSize]);

				var styleFilterName = filters[WhsProduct.Schema.ProductStyle].MultilingualDescription;
				var styleColourFilterName = filters[WhsProduct.Schema.ProductStyleColour].MultilingualDescription;
				var styleClassificationFilterName = filters[WhsProduct.Schema.ProductStyleClassification].MultilingualDescription;
				var styleSizeFilterName = filters[WhsProduct.Schema.ProductStyleSize].MultilingualDescription;
				AssertEquals("Product Style filter has correct name", "Product Style", styleFilterName);
				AssertEquals("Product Style colour filter has correct name", "Product Style Color", styleColourFilterName);
				AssertEquals("Product Style classification filter has correct name", "Product Style Classification", styleClassificationFilterName);
				AssertEquals("Product Style size filter has correct name", "Product Style Size", styleSizeFilterName);
			}
		}

		public void TestProductStyleSizeAndColourFiltersAreUnderCorrectCategory()
		{
			using (var module = new ProductModule())
			{
				var filters = module.FilterBusinessObject.ModuleFilters;

				AssertEquals(FilterCategories.Other, filters[WhsProduct.Schema.ProductStyle].Category);
				AssertEquals(FilterCategories.Other, filters[WhsProduct.Schema.ProductStyleColour].Category);
				AssertEquals(FilterCategories.Other, filters[WhsProduct.Schema.ProductStyleClassification].Category);
				AssertEquals(FilterCategories.Other, filters[WhsProduct.Schema.ProductStyleSize].Category);
			}
		}

		public void TestProductStyleFilter_WhenProductsHaveDifferentStyles()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style1 = Helper.CreateProductStyle("ONE", "1STYL", data.Org1.PK);
				var redColourFromStyle1 = Helper.CreateProductStyleColour(style1, "RED", "RED PAINT");
				var largeSizeFromStyle1 = Helper.CreateProductStyleSize(style1, 1, "LARGE");

				var style2 = Helper.CreateProductStyle("TWO", "2STYL", data.Org1.PK);
				var blueColourFromStyle2 = Helper.CreateProductStyleColour(style2, "BLU", "BLUE PAINT");
				var smallSizeFromStyle2 = Helper.CreateProductStyleSize(style2, 1, "SMALL");

				var largeRedProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var largeRedWhsProduct = WhsProduct.GetWhsProduct(largeRedProduct);
				largeRedWhsProduct.ProductStylePK = style1.PK;
				largeRedWhsProduct.ProductStyleColourPK = redColourFromStyle1.PK;
				largeRedWhsProduct.ProductStyleSizePK = largeSizeFromStyle1.PK;

				var smallBlueProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var smallBlueWhsProduct = WhsProduct.GetWhsProduct(smallBlueProduct);
				smallBlueWhsProduct.ProductStylePK = style2.PK;
				smallBlueWhsProduct.ProductStyleColourPK = blueColourFromStyle2.PK;
				smallBlueWhsProduct.ProductStyleSizePK = smallSizeFromStyle2.PK;

				Factory.Save();

				var filterBizO = new WhsOrgSupplierPartFilterStripBusinessObject();

				Asserter.AddToScope(largeRedProduct, smallBlueProduct);

				var styleFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyle];

				styleFilter.Property = style1.PK;
				styleFilter.IsActive = true;
				Asserter.AssertMatches("Filtering for style 1, should show the large red product", styleFilter, largeRedProduct);

				styleFilter.Property = style2.PK;
				Asserter.AssertMatches("Filtering for style 2, should show the small blue product", styleFilter, smallBlueProduct);
			}
		}

		public void TestProductStyleColourFilter_WhenProductsHaveDifferentColours()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style = Helper.CreateProductStyle("ONE", "COLRS", data.Org1.PK);
				var colourYellow = Helper.CreateProductStyleColour(style, "YEL", "YELLOW");
				var colourPurple = Helper.CreateProductStyleColour(style, "PUR", "PURPLE");
				var sizeMed = Helper.CreateProductStyleSize(style, 1, "MED");

				var yellowProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var yellowWhsProduct = WhsProduct.GetWhsProduct(yellowProduct);
				yellowWhsProduct.ProductStylePK = style.PK;
				yellowWhsProduct.ProductStyleColourPK = colourYellow.PK;
				yellowWhsProduct.ProductStyleSizePK = sizeMed.PK;

				var purpleProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var purpleWhsProduct = WhsProduct.GetWhsProduct(purpleProduct);
				purpleWhsProduct.ProductStylePK = style.PK;
				purpleWhsProduct.ProductStyleColourPK = colourPurple.PK;
				purpleWhsProduct.ProductStyleSizePK = sizeMed.PK;

				Factory.Save();

				var filterBizO = new WhsOrgSupplierPartFilterStripBusinessObject();

				Asserter.AddToScope(yellowProduct, purpleProduct);

				var styleFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyle];
				var styleColourFilter = (ModuleTextFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyleColour];
				styleFilter.IsActive = true;
				styleColourFilter.IsActive = true;

				styleFilter.Property = style.PK;

				styleColourFilter.Property = colourYellow.WSC_Code;
				Asserter.AssertMatches("Filtering for style and colour yellow, should show the yellow product", filterBizO.Filter, yellowProduct);

				styleColourFilter.Property = colourPurple.WSC_Code;
				Asserter.AssertMatches("Filtering for style and colour purple should show the purple product", filterBizO.Filter, purpleProduct);
			}
		}

		public void TestProductStyleClassificationFilter_WhenProductsHaveDifferentClassifications()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style = Helper.CreateProductStyle("ONE", "COLRS", data.Org1.PK);
				var colourYellow = Helper.CreateProductStyleColour(style, "YEL", "YELLOW");
				var sizeMed = Helper.CreateProductStyleSize(style, 1, "MED");
				var classificationM = Helper.CreateProductStyleClassification(style, "M", "MALE");
				var classificationF = Helper.CreateProductStyleClassification(style, "F", "FEMALE");
				var classificationC = Helper.CreateProductStyleClassification(style, "C", "CHILDREN");

				var maleProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var maleWhsProduct = WhsProduct.GetWhsProduct(maleProduct);
				maleWhsProduct.ProductStylePK = style.PK;
				maleWhsProduct.ProductStyleColourPK = colourYellow.PK;
				maleWhsProduct.ProductStyleSizePK = sizeMed.PK;
				maleWhsProduct.ProductStyleClassificationPK = classificationM.PK;

				var femaleProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var femaleWhsProduct = WhsProduct.GetWhsProduct(femaleProduct);
				femaleWhsProduct.ProductStylePK = style.PK;
				femaleWhsProduct.ProductStyleColourPK = colourYellow.PK;
				femaleWhsProduct.ProductStyleSizePK = sizeMed.PK;
				femaleWhsProduct.ProductStyleClassificationPK = classificationF.PK;

				var childrenProduct = Helper.CreateProduct(data.Org1, "PRODUCT3");
				var childrenWhsProduct = WhsProduct.GetWhsProduct(childrenProduct);
				childrenWhsProduct.ProductStylePK = style.PK;
				childrenWhsProduct.ProductStyleColourPK = colourYellow.PK;
				childrenWhsProduct.ProductStyleSizePK = sizeMed.PK;
				childrenWhsProduct.ProductStyleClassificationPK = classificationC.PK;

				Factory.Save();

				var filterBizO = new WhsOrgSupplierPartFilterStripBusinessObject();

				Asserter.AddToScope(maleProduct, femaleProduct, childrenProduct);

				var styleFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyle];
				var stylClassificationFilter = (ModuleTextFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyleClassification];
				styleFilter.IsActive = true;
				stylClassificationFilter.IsActive = true;

				styleFilter.Property = style.PK;

				stylClassificationFilter.Property = classificationM.WSS_Code;
				Asserter.AssertMatches("Filtering for style and classification male, should show the male product", filterBizO.Filter, maleProduct);

				stylClassificationFilter.Property = classificationF.WSS_Code;
				Asserter.AssertMatches("Filtering for style and classification female, should show the female product", filterBizO.Filter, femaleProduct);

				stylClassificationFilter.Property = classificationC.WSS_Code;
				Asserter.AssertMatches("Filtering for style and classification children, should show the children product", filterBizO.Filter, childrenProduct);
			}
		}

		public void TestProductStyleSizeFilter_WhenProductsHaveDifferentSizes()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style = Helper.CreateProductStyle("ONE", "STYLE", data.Org1.PK);
				var sizeSml = Helper.CreateProductStyleSize(style, 1, "SML");
				var sizeLrg = Helper.CreateProductStyleSize(style, 2, "LRG");
				var colourRainbow = Helper.CreateProductStyleColour(style, "RNB", "RAINBOW");

				var smallProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var smallWhsProduct = WhsProduct.GetWhsProduct(smallProduct);
				smallWhsProduct.ProductStylePK = style.PK;
				smallWhsProduct.ProductStyleSizePK = sizeSml.PK;
				smallWhsProduct.ProductStyleColourPK = colourRainbow.PK;

				var largeProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var largeWhsProduct = WhsProduct.GetWhsProduct(largeProduct);
				largeWhsProduct.ProductStylePK = style.PK;
				largeWhsProduct.ProductStyleSizePK = sizeLrg.PK;
				largeWhsProduct.ProductStyleColourPK = colourRainbow.PK;

				Factory.Save();

				var filterBizO = new WhsOrgSupplierPartFilterStripBusinessObject();

				Asserter.AddToScope(smallProduct, largeProduct);

				var styleFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyle];
				var styleSizeFilter = (ModuleTextFilter)filterBizO.ModuleFilters[WhsProduct.Schema.ProductStyleSize];
				styleFilter.IsActive = true;
				styleSizeFilter.IsActive = true;

				styleFilter.Property = style.PK;
				styleSizeFilter.Property = sizeSml.WSZ_Size;
				Asserter.AssertMatches("Filtering for style and size Sml, should only show the small product.", filterBizO.Filter, smallProduct);

				styleSizeFilter.Property = sizeLrg.WSZ_Size;
				Asserter.AssertMatches("Filtering for style and size Lrg, should only show the large product.", filterBizO.Filter, largeProduct);
			}
		}

		public void TestProductStyleFilter_WhenProductStyleInvalid()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style = Helper.CreateProductStyle("WAH", "WAHHHH", data.Org1.PK);
				var sizeSml = Helper.CreateProductStyleSize(style, 1, "SML");
				var sizeLrg = Helper.CreateProductStyleSize(style, 2, "LRG");
				var colourYellow = Helper.CreateProductStyleColour(style, "YEL", "YELLOW");
				var colourPurple = Helper.CreateProductStyleColour(style, "PUR", "PURPLE");

				var smallPurpleProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var smallPurpleWhsProduct = WhsProduct.GetWhsProduct(smallPurpleProduct);
				smallPurpleWhsProduct.ProductStylePK = style.PK;
				smallPurpleWhsProduct.ProductStyleSizePK = sizeSml.PK;
				smallPurpleWhsProduct.ProductStyleColourPK = colourPurple.PK;

				var largeYellowProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var largeYellowWhsProduct = WhsProduct.GetWhsProduct(largeYellowProduct);
				largeYellowWhsProduct.ProductStylePK = style.PK;
				largeYellowWhsProduct.ProductStyleSizePK = sizeLrg.PK;
				largeYellowWhsProduct.ProductStyleColourPK = colourYellow.PK;

				Factory.Save();

				var filter = new WhsOrgSupplierPartFilterStripBusinessObject();
				var styleFilter = (ModuleGuidFilter)filter.ModuleFilters[WhsProduct.Schema.ProductStyle];

				Asserter.AddToScope(smallPurpleProduct, largeYellowProduct);
				Asserter.AssertMatches("The is no Product Style Colour or Size selected.", styleFilter, smallPurpleProduct, largeYellowProduct);

				styleFilter.Property = ZGuid.Invalid;
				Asserter.AssertMatches("When invalid style entered, filter shows all products.", styleFilter, smallPurpleProduct, largeYellowProduct);
			}
		}

		public void TestOnProductChanges_ColourAndSizeFiltersShouldBeCleared()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style1 = Helper.CreateProductStyle("ONE", "1STYL", data.Org1.PK);
				var redColourFromStyle1 = Helper.CreateProductStyleColour(style1, "RED", "RED PAINT");
				var largeSizeFromStyle1 = Helper.CreateProductStyleSize(style1, 1, "LARGE");

				var style2 = Helper.CreateProductStyle("TWO", "2STYL", data.Org1.PK);
				var blueColourFromStyle2 = Helper.CreateProductStyleColour(style2, "BLU", "BLUE PAINT");
				var smallSizeFromStyle2 = Helper.CreateProductStyleSize(style2, 1, "SMALL");

				var largeRedProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var largeRedWhsProduct = WhsProduct.GetWhsProduct(largeRedProduct);
				largeRedWhsProduct.ProductStylePK = style1.PK;
				largeRedWhsProduct.ProductStyleColourPK = redColourFromStyle1.PK;
				largeRedWhsProduct.ProductStyleSizePK = largeSizeFromStyle1.PK;

				var smallBlueProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var smallBlueWhsProduct = WhsProduct.GetWhsProduct(smallBlueProduct);
				smallBlueWhsProduct.ProductStylePK = style2.PK;
				smallBlueWhsProduct.ProductStyleColourPK = blueColourFromStyle2.PK;
				smallBlueWhsProduct.ProductStyleSizePK = smallSizeFromStyle2.PK;

				Factory.Save();

				var filters = new WhsOrgSupplierPartFilterStripBusinessObject();
				var styleFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyle];
				var styleSizeFilter = (ModuleTextFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyleSize];
				var styleColourFilter = (ModuleTextFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyleColour];

				Asserter.AddToScope(smallBlueProduct, largeRedProduct);
				styleFilter.IsActive = true;
				styleColourFilter.IsActive = true;
				styleSizeFilter.IsActive = true;
				styleFilter.Property = style1.PK;
				styleColourFilter.Property = redColourFromStyle1.WSC_Code;
				styleSizeFilter.Property = largeSizeFromStyle1.WSZ_Size;

				AssertEquals("Pre-condition: style colour filter is not empty, has red colour from style 1 in it.", redColourFromStyle1.WSC_Code, styleColourFilter.Property);
				AssertEquals("Pre-condition: style size filter is not empty, has large size form style 2 in it.", largeSizeFromStyle1.WSZ_Size, styleSizeFilter.Property);

				styleFilter.Property = style2.PK;

				AssertEquals("Pre-condition: style colour filter is not empty, has red colour from style 1 in it.", "", styleColourFilter.Property);
				AssertEquals("Pre-condition: style size filter is not empty, has large size form style 2 in it.", "", styleSizeFilter.Property);
			}
		}

		public void TestWhenFiltersAreEmpty_AllProductsShouldShow()
		{
			using (var module = new ProductModule())
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var style1 = Helper.CreateProductStyle("ONE", "1STYL", data.Org1.PK);
				var redColourFromStyle1 = Helper.CreateProductStyleColour(style1, "RED", "RED PAINT");
				var largeSizeFromStyle1 = Helper.CreateProductStyleSize(style1, 1, "LARGE");

				var style2 = Helper.CreateProductStyle("TWO", "2STYL", data.Org1.PK);
				var blueColourFromStyle2 = Helper.CreateProductStyleColour(style2, "BLU", "BLUE PAINT");
				var smallSizeFromStyle2 = Helper.CreateProductStyleSize(style2, 1, "SMALL");

				var largeRedProduct = Helper.CreateProduct(data.Org1, "PRODUCT1");
				var largeRedWhsProduct = WhsProduct.GetWhsProduct(largeRedProduct);
				largeRedWhsProduct.ProductStylePK = style1.PK;
				largeRedWhsProduct.ProductStyleColourPK = redColourFromStyle1.PK;
				largeRedWhsProduct.ProductStyleSizePK = largeSizeFromStyle1.PK;

				var smallBlueProduct = Helper.CreateProduct(data.Org1, "PRODUCT2");
				var smallBlueWhsProduct = WhsProduct.GetWhsProduct(smallBlueProduct);
				smallBlueWhsProduct.ProductStylePK = style2.PK;
				smallBlueWhsProduct.ProductStyleColourPK = blueColourFromStyle2.PK;
				smallBlueWhsProduct.ProductStyleSizePK = smallSizeFromStyle2.PK;

				Factory.Save();

				var filters = new WhsOrgSupplierPartFilterStripBusinessObject();
				var styleFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyle];
				var styleSizeFilter = (ModuleTextFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyleSize];
				var styleColourFilter = (ModuleTextFilter)filters.ModuleFilters[WhsProduct.Schema.ProductStyleColour];

				Asserter.AddToScope(smallBlueProduct, largeRedProduct);
				styleFilter.IsActive = true;
				styleColourFilter.IsActive = true;
				styleSizeFilter.IsActive = true;

				styleFilter.Property = ZGuid.Empty;
				styleColourFilter.Property = "";
				styleSizeFilter.Property = "";

				Asserter.AssertMatches("When filters are empty, all products should show", filters.Filter, smallBlueProduct, largeRedProduct);
			}
		}

		public void TestProductStyleFilters_SizeOrderedBySequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var nikeStyle = Helper.CreateProductStyle("nikeStyle", "Test Style", data.Org1.PK);

			var size10 = Helper.CreateProductStyleSize(nikeStyle, 4, "Ten");
			var size5 = Helper.CreateProductStyleSize(nikeStyle, 2, "Five");
			var size1 = Helper.CreateProductStyleSize(nikeStyle, 1, "One");
			var size7 = Helper.CreateProductStyleSize(nikeStyle, 3, "Seven");

			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			var filterProductStyle = (ModuleGuidFilter)filters[WhsProduct.Schema.ProductStyle];
			filterProductStyle.IsActive = true;
			filterProductStyle.Property = nikeStyle.PK;

			var filterStyleSize = (ModuleTextFilter)filters[WhsProduct.Schema.ProductStyleSize];
			filterStyleSize.IsActive = true;
			AssertArrayEqualsByElements("Collection should be sorted by sequence.", new ZByte[] { 1, 2, 3, 4 }, filterStyleSize.List.ToList<WhsProductStyleSize>().Select(s => s.WSZ_Sequence).ToArray());
		}

		#region Helper
		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
		#endregion

		#region Asserter
		protected FilterStripAsserter<OrgSupplierPart> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<OrgSupplierPart>(Factory, (i) => i.AllOwners + "-" + i.OP_PartNum)); }
		}
		FilterStripAsserter<OrgSupplierPart> asserter;
		#endregion
	}
}
