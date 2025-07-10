using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmMenuItem))]
	sealed class StmMenuItemTest : EnterpriseBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestMenuTypeDescription()
		{
			var testStmMenuItem = Factory.New<StmMenuItem>();
			testStmMenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			AssertEquals("Normal Document", testStmMenuItem.MenuTypeDescription);
			testStmMenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			AssertEquals("Normal Document", testStmMenuItem.MenuTypeDescription);
			testStmMenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			AssertEquals("Visualizer Form", testStmMenuItem.MenuTypeDescription);
		}

		public void TestFindDocumentMenu()
		{
			BusinessContext businessContext = BusinessContext.APTransaction;
			StmTemplate newTemplate = Factory.NewWithValidTestData<StmTemplate>();
			newTemplate.SO_Name = "This is a new template";
			Factory.Save();

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmTemplate));
			query.AddToFilter(StmTemplateSchema.SO_Name, "Standard");
			StmTemplate existingTemplate = Factory.LoadTop1<StmTemplate>(query);

			StmMenuItem menu = StmMenuItem.FindDocumentMenu(Factory, newTemplate, businessContext);
			AssertNull("menu should not exist", menu);

			menu = StmMenuItem.FindDocumentMenu(Factory, existingTemplate, businessContext);
			AssertNotNull("menu should exist", menu);
		}

		public void TestCreateDocumentMenu()
		{
			BusinessContext businessContext = BusinessContext.APTransaction;
			StmTemplate newTemplate = Factory.NewWithValidTestData<StmTemplate>();
			newTemplate.SO_Name = "This is a new template";
			Factory.Save();

			StmMenuItem menu = StmMenuItem.FindDocumentMenu(Factory, newTemplate, BusinessContext.ARInvoice);
			AssertNull("Precondition - menu should be null", menu);
			StmMenuItem.CreateDocumentMenu(Factory, newTemplate, businessContext, "Standard", "ARInvoice");
			Factory.Save();
			menu = StmMenuItem.FindDocumentMenu(Factory, newTemplate, BusinessContext.ARInvoice);
			AssertNotNull("Postcondition - menu should be created", menu);
			AssertEquals("ARInvoice", menu.SU_BusinessContext);
			AssertEquals("This is a new template", menu.SU_MenuName);
		}

		public void TestAttachmentTypes()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var attachmentTypes = menuItem.AttachmentTypes;

			AssertEquals(8, attachmentTypes.Count);
			Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain PDFC.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfc));
			Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
		}

		public void TestRealPath()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "Menu/Path/";

			AssertEquals("menuItem.RealPath", "Menu/Path", menuItem.RealPathMultilingual.GetUnresolvedString());

			menuItem.SU_MenuPath = "";

			AssertEquals("menuItem.RealPath", ".", menuItem.RealPathMultilingual.GetUnresolvedString());

			menuItem.SU_MenuPath = "Legacy Documents/Arrival/";

			AssertEquals("menuItem.RealPath", "Arrival", menuItem.RealPathMultilingual.GetUnresolvedString());

			menuItem.SU_MenuPath = "/Departure/";

			AssertEquals("menuItem.RealPath", "Departure", menuItem.RealPathMultilingual.GetUnresolvedString());
		}

		public void TestDocumentID()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Name";
			menuItem.SU_BusinessContext = "Context";
			menuItem.SU_ContactType = "CZZ";
			menuItem.SU_MenuPath = "Menu/Path/";

			AssertEquals("menuItem.DocumentID", "Context : Name : Menu/Path : CZZ", menuItem.DocumentId);

			menuItem.SU_MenuPath = "";

			AssertEquals("menuItem.DocumentID", "Context : Name : . : CZZ", menuItem.DocumentId);

			menuItem.SU_MenuName = "Arrival Notice";
			menuItem.SU_BusinessContext = "Shipment";
			menuItem.SU_ContactType = "CNE";
			menuItem.SU_MenuPath = "Legacy Documents/Arrival/";

			AssertEquals("menuItem.DocumentID", "Shipment : Arrival Notice : Arrival : CNE", menuItem.DocumentId);
		}

		public void TestDocumentIdMultilingual()
		{
			var menuNameInEnglish = "Name";
			var menuNameInChinese = "名称";
			var menuPathInEnglish = "Path";
			var menuPathInChinese = "地址";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = menuNameInEnglish;
			menuItem.SU_BusinessContext = "Context";
			menuItem.SU_ContactType = "CZZ";
			menuItem.SU_MenuPath = menuPathInEnglish;

			var resKeyName = menuItem.SU_MenuNameInfo.CustomizableDataResourceStrings.GetMultilingualString(menuItem, menuNameInEnglish).ResourceKey;
			var resKeyPath = menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.GetMultilingualString(menuItem, menuPathInEnglish).ResourceKey;
			AssertEquals("Context : Name : Path : CZZ", menuItem.DocumentIdMultilingual);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKeyName, new ResourceStringData(resKeyName, menuNameInChinese));
				mockRes.Put(resKeyPath, new ResourceStringData(resKeyPath, menuPathInChinese));
				AssertEquals("Context : 名称 : 地址 : CZZ", menuItem.DocumentIdMultilingual);
			}
		}

		#region Default Values / Loading

		public void TestDefaultValuesAndLoading()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			Assert("Allow Multiple Copies true by default", menuItem.AllowMultipleCopies);
			AssertEquals("Number of copies 1 by default", (short)1, menuItem.NumberOfCopies);

			menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			Assert("Allow Multiple Copies true on all loaded menu items", menuItem.AllowMultipleCopies);
			AssertEquals("Number of copies 1 on all loaded menu items", (short)1, menuItem.NumberOfCopies);
		}

		#endregion

		#region Properties

		public void TestCulture()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			AssertEquals(false, menuItem.SU_IsLocalDocument);
			AssertEquals(false, menuItem.IsLocalDocument);
			AssertEquals(Culture.Default, menuItem.RenderCulture);

			menuItem.RenderCulture = new CultureInfo("en-US");
			AssertEquals(false, menuItem.SU_IsLocalDocument);
			AssertEquals(true, menuItem.IsLocalDocument);
			AssertEquals("en-US", menuItem.RenderCulture.Name);

			menuItem.IsLocalDocument = false;
			AssertEquals(false, menuItem.SU_IsLocalDocument);
			AssertEquals(false, menuItem.IsLocalDocument);
			AssertEquals(Culture.Default, menuItem.RenderCulture);

			menuItem.SU_IsLocalDocument = true;
			AssertEquals(true, menuItem.SU_IsLocalDocument);
			AssertEquals(true, menuItem.IsLocalDocument);
			AssertEquals(Culture.CurrentCompanyCountryCulture, menuItem.RenderCulture);
		}

		public void TestMenuItemUniqueCode()
		{
			MenuItem.SU_MenuName = "Test Menu Name";
			AssertEquals("MenuItemUniqueCode", "Test Menu Name :  :  : ", MenuItem.MenuItemUniqueCode);

			MenuItem.SU_BusinessContext = "Business Context";
			AssertEquals("MenuItemUniqueCode", "Test Menu Name : Business Context :  : ", MenuItem.MenuItemUniqueCode);

			MenuItem.SU_MenuPath = "Start Path/End Path/";
			AssertEquals("MenuItemUniqueCode", "Test Menu Name : Business Context : Start Path/End Path/ : ", MenuItem.MenuItemUniqueCode);

			MenuItem.SU_ContactType = "CNE";
			AssertEquals("MenuItemUniqueCode", "Test Menu Name : Business Context : Start Path/End Path/ : CNE", MenuItem.MenuItemUniqueCode);

			MenuItem.SU_MenuPath = "";
			AssertEquals("MenuItemUniqueCode", "Test Menu Name : Business Context :  : CNE", MenuItem.MenuItemUniqueCode);
		}

		#endregion

		#region Validation

		public void TestValidateSU_MenuName()
		{
			MenuItem.SU_MenuName = "Test Menu Name";
			Assert("No error expected", !MenuItem.SU_MenuNameInfo.HasErrors());

			MenuItem.SU_MenuName = "Test :Menu Name";
			Assert("Error expected", MenuItem.SU_MenuNameInfo.HasErrors());
		}

		public void TestValidateSU_BusinessContext()
		{
			MenuItem.SU_BusinessContext = "Business Context";
			Assert("No error expected", !MenuItem.SU_BusinessContextInfo.HasErrors());

			MenuItem.SU_BusinessContext = "Business :Context";
			Assert("Error expected", MenuItem.SU_BusinessContextInfo.HasErrors());
		}

		public void TestValidateSU_MenuPath()
		{
			MenuItem.SU_MenuPath = "Test/Menu/Path";
			Assert("No error expected", !MenuItem.SU_MenuPathInfo.HasErrors());

			MenuItem.SU_MenuPath = "Test/:Menu/Name";
			Assert("Error expected", MenuItem.SU_MenuPathInfo.HasErrors());
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			menuItem.SU_BusinessContext = Env.Security.OrderReports.Code;
			SecurityCheckpoint checkpoint = Env.Security.FindOrCreateReportCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, Env.Security.OrderReports);
			checkpoint.IsAllowed = true;

			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbSecurityCollection collection = new GlbSecurityCollection(newStaff, Factory);
			GlbSecurityCollectionView newView = new GlbSecurityCollectionView(collection);
			newView.FilterBySecurityKey(new CheckpointLookupKey(checkpoint.Code, menuItem.PK.ToGuid()), newStaff.PK);
			newView.AddNew();
			Factory.Save();

			ZQuery filter = new ZQuery(GlbSecuritySchema.GU_SecurityRight, checkpoint.Code);
			GlbSecurity[] checkpointBizo = Factory.Load<GlbSecurity>(filter);

			AssertEquals(false, checkpointBizo[0].IsDeleted);
			menuItem.Delete();
			AssertEquals(true, checkpointBizo[0].IsDeleted);
		}

		public void TestDelete_RemovesRelatedJobDocumentDeliveryRecords()
		{
			var menuItemA = Factory.NewWithValidTestData<StmMenuItem>();
			var menuItemB = Factory.NewWithValidTestData<StmMenuItem>();

			var jobDocumentDeliveryA1 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDeliveryA1.JDC_ParentID = ZGuid.NewZGuid();
			jobDocumentDeliveryA1.JDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocumentDeliveryA1.JDC_SU_MenuItem = menuItemA.PK;

			var jobDocumentDeliveryA2 = Factory.New<JobDocumentDelivery>();
			jobDocumentDeliveryA2.JDC_ParentID = ZGuid.NewZGuid();
			jobDocumentDeliveryA2.JDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocumentDeliveryA2.JDC_SU_MenuItem = menuItemA.PK;

			var jobDocumentDeliveryB = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDeliveryB.JDC_ParentID = ZGuid.NewZGuid();
			jobDocumentDeliveryB.JDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocumentDeliveryB.JDC_SU_MenuItem = menuItemB.PK;

			AssertEquals("Precondition", 2, GetRelatedJobDocumentDeliveryCount(menuItemA.PK));
			AssertEquals("Precondition", 1, GetRelatedJobDocumentDeliveryCount(menuItemB.PK));

			menuItemA.Delete();

			AssertEquals("Related JobDocumentDelivery records should be deleted alongside their StmMenuItem", 0, GetRelatedJobDocumentDeliveryCount(menuItemA.PK));
			AssertEquals("Unrelated JobDocumentDelivery records should not be deleted", 1, GetRelatedJobDocumentDeliveryCount(menuItemB.PK));

			int GetRelatedJobDocumentDeliveryCount(ZGuid menuItemPK)
			{
				return Factory.Load<JobDocumentDelivery>(new ZQuery(JobDocumentDeliverySchema.JDC_SU_MenuItem, menuItemPK)).Length;
			}
		}

		#endregion

		public void TestSetPrimaryDocument_ShouldAlsoSetTablePrefix()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var menuPivot = Factory.New<StmMenuMenuPivot>();
			var templatePivot = Factory.New<StmMenuTemplatePivot>();

			menuItem.SU_PrimaryDocPackItemId = menuPivot.PK;
			AssertEquals(StmMenuMenuPivotSchema.Constants.Prefix, menuItem.SU_PrimaryDocPackItemTableCode);

			menuItem.SU_PrimaryDocPackItemId = templatePivot.PK;
			AssertEquals(StmMenuTemplatePivotSchema.Constants.Prefix, menuItem.SU_PrimaryDocPackItemTableCode);

			menuItem.SU_PrimaryDocPackItemId = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, menuItem.SU_PrimaryDocPackItemTableCode);
		}

		public void TestSU_HintMultilingual()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_Hint = "Test Hint";
			string resKey = menuItem.SU_HintInfo.CustomizableDataResourceStrings.GetMultilingualString(menuItem, "Test Hint").ResourceKey;
			AssertEquals("Test Hint", menuItem.SU_HintMultilingual);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(resKey, new ResourceStringData(resKey, "测试说明"));
				AssertEquals("测试说明", menuItem.SU_HintMultilingual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSU_MenuPathMultilingual()
		{
			var menuItem = Factory.New<StmMenuItem>();

			AssertContainsExactElementsInAnyOrder(menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetRuntimeCaptions(), menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetCompileTimeSystemCaptions());

			using (var mockRes = Res.UseMockData())
			{
				string key;
				mockRes.Put(key = menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetKey(null, "One"), new ResourceStringData(key, "一"));
				mockRes.Put(menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Two"), new ResourceStringData(key, "二"));
				mockRes.Put(menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Three"), new ResourceStringData(key, "三"));

				menuItem.SU_MenuPath = "One";
				AssertEquals("一", menuItem.SU_MenuPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一" }, menuItem.SU_MenuPathMultilingualParts.Select(item => item.ToString()).ToArray());
				AssertEquals("一", menuItem.RealPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一" }, menuItem.RealPathMultilingualParts.Select(item => item.ToString()).ToArray());

				menuItem.SU_MenuPath = "One/Two";
				AssertEquals("一/二", menuItem.SU_MenuPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.SU_MenuPathMultilingualParts.Select(item => item.ToString()).ToArray());
				AssertEquals("一/二", menuItem.RealPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.RealPathMultilingualParts.Select(item => item.ToString()).ToArray());

				menuItem.SU_MenuPath = "One/ Two/";
				AssertEquals("一/二", menuItem.SU_MenuPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.SU_MenuPathMultilingualParts.Select(item => item.ToString()).ToArray());
				AssertEquals("一/二", menuItem.RealPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.RealPathMultilingualParts.Select(item => item.ToString()).ToArray());

				menuItem.SU_MenuPath = "/One /Two";
				AssertEquals("一/二", menuItem.SU_MenuPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.SU_MenuPathMultilingualParts.Select(item => item.ToString()).ToArray());
				AssertEquals("一/二", menuItem.RealPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二" }, menuItem.RealPathMultilingualParts.Select(item => item.ToString()).ToArray());

				menuItem.SU_MenuPath = "One/Two / Three";
				AssertEquals("一/二/三", menuItem.SU_MenuPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二", "三" }, menuItem.SU_MenuPathMultilingualParts.Select(item => item.ToString()).ToArray());
				AssertEquals("一/二/三", menuItem.RealPathMultilingual);
				AssertArrayEqualsByElements(new string[] { "一", "二", "三" }, menuItem.RealPathMultilingualParts.Select(item => item.ToString()).ToArray());
				Factory.Save();

				var runtimeCaptions = menuItem.SU_MenuPathInfo.CustomizableDataResourceStrings.Source.GetRuntimeCaptions().Select(r => r.EnglishText);
				Assert(runtimeCaptions.Contains("One"));
				Assert(runtimeCaptions.Contains("Two"));
				Assert(runtimeCaptions.Contains("Three"));
			}
		}

		public void TestDeliveryRestrictedConditions()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			AssertEquals(string.Empty, menuItem.SU_DeliveryRestrictionMacro);
			AssertEquals(true, menuItem.SU_DeliveryRestrictionMacro_ReadOnly);
			AssertEquals(string.Empty, menuItem.SU_DeliveryRestrictionDescription);
			AssertEquals(true, menuItem.SU_DeliveryRestrictionDescription_ReadOnly);

			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			AssertEquals(false, menuItem.SU_DeliveryRestrictionMacro_ReadOnly);
			AssertEquals(true, menuItem.SU_DeliveryRestrictionDescription_ReadOnly);
			menuItem.SU_DeliveryRestrictionMacro = "condition";
			AssertEquals("condition", menuItem.SU_DeliveryRestrictionMacro);
			AssertEquals(false, menuItem.SU_DeliveryRestrictionDescription_ReadOnly);

			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			AssertEquals(string.Empty, menuItem.SU_DeliveryRestrictionMacro);
			AssertEquals(true, menuItem.SU_DeliveryRestrictionMacro_ReadOnly);
			AssertEquals(true, menuItem.SU_DeliveryRestrictionDescription_ReadOnly);
		}

		public void TestDeliveryRestrictedConditionFieldType()
		{
			var menuItem = Factory.New<StmMenuItem>();
			AssertEquals(nameof(FieldType.TextMacro), menuItem.DeliveryRestrictionConditionFieldType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		StmMenuItem MenuItem;

		protected override void SetUp()
		{
			base.SetUp();
			MenuItem = (StmMenuItem)GetNewBusinessObject();
		}

		#endregion
	}
}
