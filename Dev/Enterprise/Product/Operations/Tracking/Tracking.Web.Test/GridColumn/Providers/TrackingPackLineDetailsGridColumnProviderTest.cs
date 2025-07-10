using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingPackLineDetailsGridColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingPackLineDetailsGridColumnProviderTest : GridColumnProviderTest
	{
		#region Test Cases

		public override void TestColumnKeys()
		{
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			LoginAsQuickViewUser();
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			LoginAsQuickViewUser();
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			LoginAsQuickViewUser();
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			LoginAsQuickViewUser();
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = null;
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = false;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();

			hideCustomsCodeAndPrice = true;
			isShipmentPackLines = true;
			multipleProductsColumn = new ZTextEditColumn("Test", "Test");
			SetupNewProvider();
			base.TestUniqueColumns();
		}

		#endregion

		protected override bool SupportsOldLayoutFix
		{
			get
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			bool userIsAuthorised = WebEnv.AppInstance.SiteUser != null && WebEnv.AppInstance.SiteUser is TrackingSiteUser && !(WebEnv.AppInstance.SiteUser as TrackingSiteUser).IsShipmentQuickViewUser;
			AddDefaultsColumn(new ZCalcEditColumn("Pieces", JobPackLinesSchema.Constants.JL_PackageCount) { ColumnKey = WebTracker.Grids.TrackingPackLines.Pieces });
			AddDefaultsColumn(new ZDropDownListColumn("Pack Type", JobPackLinesSchema.Constants.JL_F3_NKPackType, JobPackLinesSchema.Constants.JL_F3_NKPackType + "_List")
			{
				ColumnKey = WebTracker.Grids.TrackingPackLines.PackType,
				ValueFieldName = RefPackTypeSchema.F3_Code.Name,
				TextFieldName = RefPackTypeSchema.F3_Description.Name + "Multilingual"
			}); // May be an identifier
			if (userIsAuthorised)
			{
				AddDefaultsColumn(new ZCalcEditColumn("Length", JobPackLinesSchema.Constants.JL_Length) { ColumnKey = WebTracker.Grids.TrackingPackLines.Length });
				AddDefaultsColumn(new ZCalcEditColumn("Width", JobPackLinesSchema.Constants.JL_Width) { ColumnKey = WebTracker.Grids.TrackingPackLines.Width });
				AddDefaultsColumn(new ZCalcEditColumn("Height", JobPackLinesSchema.Constants.JL_Height) { ColumnKey = WebTracker.Grids.TrackingPackLines.Height });
				AddDefaultsColumn(new ZDropDownListColumn("UD", JobPackLinesSchema.Constants.JL_UnitOfDimension, JobPackLinesSchema.Constants.JL_UnitOfDimension + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.UnitOfDimension,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});
				AddDefaultsColumn(new ZCalcEditColumn("Weight", JobPackLinesSchema.Constants.JL_ActualWeight) { ColumnKey = WebTracker.Grids.TrackingPackLines.Weight });
				AddDefaultsColumn(new ZDropDownListColumn("UQ", JobPackLinesSchema.Constants.JL_ActualWeightUQ, JobPackLinesSchema.Constants.JL_ActualWeightUQ + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.WeightUnit,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});
				AddDefaultsColumn(new ZCalcEditColumn("Volume", JobPackLinesSchema.Constants.JL_ActualVolume) { ColumnKey = WebTracker.Grids.TrackingPackLines.Volume });
				AddDefaultsColumn(new ZDropDownListColumn("UQ", JobPackLinesSchema.Constants.JL_ActualVolumeUQ, JobPackLinesSchema.Constants.JL_ActualVolumeUQ + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.VolumeUnit,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});
			}
			AddDefaultsColumn(new ZTextEditColumn("Description", JobPackLinesSchema.Constants.JL_Description) { ColumnKey = WebTracker.Grids.TrackingPackLines.Description });
			if (userIsAuthorised)
			{
				AddDefaultsColumn(new ZTextEditColumn("Marks and Numbers", JobPackLinesSchema.Constants.JL_MarksAndNumbers) { ColumnKey = WebTracker.Grids.TrackingPackLines.MarksAndNumbers });

				if (!hideCustomsCodeAndPrice || isShipmentPackLines)
				{
					AddDefaultsColumn(new ZCalcEditColumn("Line Price", JobPackLinesSchema.Constants.JL_LinePrice) { ColumnKey = WebTracker.Grids.TrackingPackLines.LinePrice });
				}
				if (isShipmentPackLines)
				{
					AddDefaultsColumn(new ZTextEditColumn("Currency", TrackingPackLine.Schema.CurrencyCode) { ColumnKey = WebTracker.Grids.TrackingPackLines.Currency });
					AddColumn(new ZCalcEditColumn("Loading Meters", TrackingPackLine.Schema.JL_LoadingMeters) { ColumnKey = WebTracker.Grids.TrackingPackLines.LoadingMeters });
				}
				if (!hideCustomsCodeAndPrice)
				{
					AddDefaultsColumn(new ZTextEditColumn("Tariff Num.", JobPackLinesSchema.Constants.JL_HarmonisedCode) { ColumnKey = WebTracker.Grids.TrackingPackLines.TariffNumber });
				}
			}
			if (isShipmentPackLines)
			{
				AddDefaultsColumn(new ZTextEditColumn("Container", Enterprise.Freight.Business.PackLine.JL_Calc_ContainerNumberName) { ColumnKey = WebTracker.Grids.TrackingPackLines.ContainerNumber });
			}
			if (userIsAuthorised)
			{
				AddColumn(new ZTextEditColumn("Reference Number", JobPackLinesSchema.Constants.JL_RefNumber) { ColumnKey = WebTracker.Grids.TrackingPackLines.ReferenceNumber });
				AddColumn(new ZTextEditColumn("Custom Text 1", JobPackLinesSchema.Constants.JL_CustomAttrib1) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute1 });
				AddColumn(new ZTextEditColumn("Custom Text 2", JobPackLinesSchema.Constants.JL_CustomAttrib2) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute2 });
				AddColumn(new ZTextEditColumn("Custom Text 3", JobPackLinesSchema.Constants.JL_CustomAttrib3) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute3 });
				AddColumn(new ZTextEditColumn("Custom Text 4", JobPackLinesSchema.Constants.JL_CustomAttrib4) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute4 });
			}
			if (multipleProductsColumn != null)
			{
				multipleProductsColumn.ColumnKey = WebTracker.Grids.TrackingPackLines.MultipleProducts;
				AddDefaultsColumn(multipleProductsColumn);
			}
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingPackLineDetailsGridColumnProvider(isShipmentPackLines, hideCustomsCodeAndPrice, multipleProductsColumn);
		}

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}

		void LoginAsQuickViewUser()
		{
			OrgContact contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			var password = "test";
			contact.SetHashedPassword(password);

			Factory.Save();

			TrackingSiteUser user = new TrackingSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, password);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertNotNull("No tracking site user", LoggedSiteUser);
			AssertEquals("IsLogged", true, LoggedSiteUser.IsLoggedIn);
			AssertEquals("Not IsShipmentQuickViewUser", false, LoggedSiteUser.IsShipmentQuickViewUser);
		}

		protected override void SetUp()
		{
			hideCustomsCodeAndPrice = false;
			isShipmentPackLines = false;
			multipleProductsColumn = null;
			base.SetUp();
		}

		bool hideCustomsCodeAndPrice;
		bool isShipmentPackLines;
		ZTemplateColumn multipleProductsColumn;
	}
}
