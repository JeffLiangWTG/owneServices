using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Tests
{
	public class PersonMergePreviewUserControlTest : TestCaseWithFactory
	{
		public void TestDefaultHeight()
		{
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(53), control.Height);
			}
		}

		public void TestMaxHeight()
		{
			var mergedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var personMergePreview = new PersonMergePreview();

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				CombineAssertions(() =>
				{
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(53), control.Height);
					AssertEquals(1, control.RightBottomLayoutPanelForTest.RowCount);
					Assert("Over the maximum height, the vertical scrollbar is displayed.", control.RightBottomPanelForTest.VerticalScroll.Visible);
				});
			}
		}

		public void TestSetGroupBoxCaption()
		{
			var mergedPerson = Factory.New<GlbPerson>();
			mergedPerson.PER_FullName = "PETER";
			var personMergePreview = new PersonMergePreview();

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "OWEN");
				form.Controls.Add(control);

				AssertEquals("After merge : PETER", control.MergedPropertiesGroupBoxForTest.Text);
				AssertEquals("Information discarded from : OWEN", control.DiscardedPropertiesGroupBoxForTest.Text);
			}
		}

		public void TestShowMergedPersonProperties()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "PETER");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "Test Address1");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Test Address2");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				var previewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("PETER", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Test Address1\nTest Address2", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestShowDiscardedProperties()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "PETER");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "Test Address1");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Test Address2");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.RightBottomLayoutPanelForTest.RowCount);

				var previewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("PETER", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Test Address1\nTest Address2", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestShowMergedAndDiscardedProperties()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "PETER");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "Test Address1");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Test Address2");

			personMergePreview.MergedProperties.AddPair("PER_FullName", "PETER");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "Test Address1");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Test Address2");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("PETER", leftPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Test Address1\nTest Address2", leftPreviewLabelControls[1].ValueLabel.Text);
				});

				AssertEquals(2, control.RightBottomLayoutPanelForTest.RowCount);

				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("PETER", rightPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Test Address1\nTest Address2", rightPreviewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_DisplayedAsOneField_Australia()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "72 O'Rioidan Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "");
			personMergePreview.MergedProperties.AddPair("PER_City", "Alexandria");
			personMergePreview.MergedProperties.AddPair("PER_State", "NSW");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "2015");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "AU");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				var previewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("72 O'Rioidan Street\nAlexandria NSW 2015\nAU", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_DisplayedAsOneField_France()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "French Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "");
			personMergePreview.MergedProperties.AddPair("PER_City", "Paris");
			personMergePreview.MergedProperties.AddPair("PER_State", "");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "FR");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				var previewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("French Street\n1234 PARIS\nFR", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_DisplayedAsOneField_Spain()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "Spanish Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "");
			personMergePreview.MergedProperties.AddPair("PER_City", "Aranjeuz");
			personMergePreview.MergedProperties.AddPair("PER_State", "Madrid");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "ES");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				var previewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Spanish Street\n1234 Aranjeuz (Madrid)\nES", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_DisplayedAsOneField_UnitedKingdom()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "British Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Station Approach");
			personMergePreview.MergedProperties.AddPair("PER_City", "Totternhoe");
			personMergePreview.MergedProperties.AddPair("PER_State", "Beds");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "LU6 1AA");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "GB");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				var previewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", previewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", previewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", previewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("British Street\nStation Approach\nTOTTERNHOE\nLU6 1AA\nGB", previewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_DiscardedAndRetainedBothConvertAddress()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "82 Fake Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Smallville");
			personMergePreview.MergedProperties.AddPair("PER_City", "Joburg");
			personMergePreview.MergedProperties.AddPair("PER_State", "XX");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "ZA");
			personMergePreview.MergedProperties.AddPair("PER_MobilePhone", "111");

			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "123 Some Address");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Station Approach");
			personMergePreview.DiscardedProperties.AddPair("PER_City", "Atlanta");
			personMergePreview.DiscardedProperties.AddPair("PER_State", "GA");
			personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.DiscardedProperties.AddPair("PER_RN_NKCountry", "US");
			personMergePreview.DiscardedProperties.AddPair("PER_MobilePhone", "123");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(3, control.LeftBottomLayoutPanelForTest.RowCount);
				AssertEquals(3, control.RightBottomLayoutPanelForTest.RowCount);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", leftPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("82 Fake Street\nSmallville\nJoburg\n1234\nZA", leftPreviewLabelControls[1].ValueLabel.Text);
					AssertEquals("Mobile Phone :", leftPreviewLabelControls[2].HumanReadableNameLabel.Text);
					AssertEquals("111", leftPreviewLabelControls[2].ValueLabel.Text);

					AssertEquals("Full Name :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("Jane Doe", rightPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("123 Some Address\nStation Approach\nAtlanta GA 1234\nUS", rightPreviewLabelControls[1].ValueLabel.Text);
					AssertEquals("Mobile Phone :", rightPreviewLabelControls[2].HumanReadableNameLabel.Text);
					AssertEquals("123", rightPreviewLabelControls[2].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_PartialAddressProvided_StillFormatsCorrectly()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_State", "NSW");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "2015");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "AU");

			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
			personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.DiscardedProperties.AddPair("PER_RN_NKCountry", "US");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				AssertEquals(2, control.RightBottomLayoutPanelForTest.RowCount);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", leftPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("NSW 2015\nAU", leftPreviewLabelControls[1].ValueLabel.Text);

					AssertEquals("Full Name :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("Jane Doe", rightPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("1234\nUS", rightPreviewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestPersonsMergePreviewAddress_NoCountryProvidedInAddress_UsesDefaultFormatting()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();
			personMergePreview.MergedProperties.AddPair("PER_FullName", "John Doe");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "72 O'Rioidan Street");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "2015");

			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
			personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				AssertEquals(2, control.LeftBottomLayoutPanelForTest.RowCount);
				AssertEquals(2, control.RightBottomLayoutPanelForTest.RowCount);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("John Doe", leftPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("72 O'Rioidan Street\n2015", leftPreviewLabelControls[1].ValueLabel.Text);

					AssertEquals("Full Name :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("Jane Doe", rightPreviewLabelControls[0].ValueLabel.Text);
					AssertEquals("Address :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("1234", rightPreviewLabelControls[1].ValueLabel.Text);
				});
			}
		}

		public void TestShowsPropertiesInCorrectOrder_Default()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();

			// Properties intentionally out of order to ensure that they end up sorted
			personMergePreview.MergedProperties.AddPair("PER_HomePhone", "1234 5678");
			personMergePreview.MergedProperties.AddPair("PER_MobilePhone", "7777 6666");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "42 Pretend Street");
			personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Unit 7");
			personMergePreview.MergedProperties.AddPair("PER_City", "Janeville");
			personMergePreview.MergedProperties.AddPair("PER_State", "Janeland");
			personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "ZA");
			personMergePreview.MergedProperties.AddPair("PER_FullName", "Jane Deer");
			personMergePreview.MergedProperties.AddPair("PER_EmailAddress", "Jane@Deer.com");
			personMergePreview.MergedProperties.AddPair("PER_Gender", "Female");
			personMergePreview.MergedProperties.AddPair("PER_BirthDate", "01/01/1990");

			personMergePreview.DiscardedProperties.AddPair("PER_MobilePhone", "8888 5555");
			personMergePreview.DiscardedProperties.AddPair("PER_HomePhone", "9876 5432");
			personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "42 Fake Street");
			personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Room 7");
			personMergePreview.DiscardedProperties.AddPair("PER_City", "JaneTown");
			personMergePreview.DiscardedProperties.AddPair("PER_State", "Janeland");
			personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");
			personMergePreview.DiscardedProperties.AddPair("PER_RN_NKCountry", "US");
			personMergePreview.DiscardedProperties.AddPair("PER_BirthDate", "10/10/1980");

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

				AssertEquals(7, control.LeftBottomLayoutPanelForTest.RowCount);
				AssertEquals(5, control.RightBottomLayoutPanelForTest.RowCount);

				CombineAssertions(() =>
				{
					AssertEquals("Full Name :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("Gender :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Birth Date :", leftPreviewLabelControls[2].HumanReadableNameLabel.Text);
					AssertEquals("Address :", leftPreviewLabelControls[3].HumanReadableNameLabel.Text);
					AssertEquals("Home Phone :", leftPreviewLabelControls[4].HumanReadableNameLabel.Text);
					AssertEquals("Mobile Phone :", leftPreviewLabelControls[5].HumanReadableNameLabel.Text);
					AssertEquals("Email Address :", leftPreviewLabelControls[6].HumanReadableNameLabel.Text);

					AssertEquals("Full Name :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
					AssertEquals("Birth Date :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
					AssertEquals("Address :", rightPreviewLabelControls[2].HumanReadableNameLabel.Text);
					AssertEquals("Home Phone :", rightPreviewLabelControls[3].HumanReadableNameLabel.Text);
					AssertEquals("Mobile Phone :", rightPreviewLabelControls[4].HumanReadableNameLabel.Text);
				});
			}
		}

		public void TestShowsPropertiesInCorrectOrder_CustomOrder()
		{
			var manualOrder = new PersonMergePreviewItemCollection()
			{
				new PersonMergePreviewItem() { FriendlyName = "Nationality Code", ColumnName = GlbPersonSchema.PER_RN_NKNationalityCodeISO.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Home Address 1", ColumnName = GlbPersonSchema.PER_HomeAddress1.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Home Address 2", ColumnName = GlbPersonSchema.PER_HomeAddress2.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "City", ColumnName = GlbPersonSchema.PER_City.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "State", ColumnName = GlbPersonSchema.PER_State.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Postcode", ColumnName = GlbPersonSchema.PER_Postcode.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Country", ColumnName = GlbPersonSchema.PER_RN_NKCountry.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Birth Date", ColumnName = GlbPersonSchema.PER_BirthDate.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Email Address", ColumnName = GlbPersonSchema.PER_EmailAddress.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Email Address 2", ColumnName = GlbPersonSchema.PER_EmailAddress2.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Gender", ColumnName = GlbPersonSchema.PER_Gender.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Full Name", ColumnName = GlbPersonSchema.PER_FullName.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Friendly Name", ColumnName = GlbPersonSchema.PER_FriendlyName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Legal Name", ColumnName = GlbPersonSchema.PER_LegalName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Name Suffix", ColumnName = GlbPersonSchema.PER_NameSuffix.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Name Title", ColumnName = GlbPersonSchema.PER_NameTitle.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Home Phone", ColumnName = GlbPersonSchema.PER_HomePhone.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone", ColumnName = GlbPersonSchema.PER_MobilePhone.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone 2", ColumnName = GlbPersonSchema.PER_MobilePhone2.Name, Visibility = true },

				new PersonMergePreviewItem() { FriendlyName = "Preferred Language", ColumnName = GlbPersonSchema.PER_PreferredLanguage.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Drivers License Number", ColumnName = GlbPersonSchema.PER_DriversLicenseNumber.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Personal Info", ColumnName = GlbPersonSchema.PER_PersonalInfo.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Picture", ColumnName = GlbPersonSchema.PER_Picture.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Fax Number", ColumnName = GlbPersonSchema.PER_FaxNumber.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Passport", ColumnName = GlbPersonSchema.PER_Passport.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Passport Expiry Date", ColumnName = GlbPersonSchema.PER_PassportExpiryDate.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Passport Place Of Issue", ColumnName = GlbPersonSchema.PER_PassportPlaceOfIssue.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase", ColumnName = GlbPersonSchema.PER_ChallengePhrase.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase Type", ColumnName = GlbPersonSchema.PER_ChallengePhraseType.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Password Hash", ColumnName = GlbPersonSchema.PER_PasswordHash.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Password Hash Iterations", ColumnName = GlbPersonSchema.PER_PasswordHashIterations.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Password Salt", ColumnName = GlbPersonSchema.PER_PasswordSalt.Name, Visibility = true },
				new PersonMergePreviewItem() { FriendlyName = "Web Access Enabled", ColumnName = GlbPersonSchema.PER_WebAccessEnabled.Name, Visibility = true }
			};

			using (SystemDataRegistry.Instance.PersonMergePreviewItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, manualOrder))
			{
				Env.Registry.SetOrgAllowMixedCase(true);
				var mergedPerson = Factory.New<GlbPerson>();
				var personMergePreview = new PersonMergePreview();

				// Properties intentionally out of order to ensure that they end up sorted
				personMergePreview.MergedProperties.AddPair("PER_HomePhone", "1234 5678");
				personMergePreview.MergedProperties.AddPair("PER_MobilePhone", "7777 6666");
				personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "42 Pretend Street");
				personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Unit 7");
				personMergePreview.MergedProperties.AddPair("PER_City", "Janeville");
				personMergePreview.MergedProperties.AddPair("PER_State", "Janeland");
				personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
				personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "ZA");
				personMergePreview.MergedProperties.AddPair("PER_FullName", "Jane Deer");
				personMergePreview.MergedProperties.AddPair("PER_EmailAddress", "Jane@Deer.com");
				personMergePreview.MergedProperties.AddPair("PER_Gender", "Female");
				personMergePreview.MergedProperties.AddPair("PER_BirthDate", "01/01/1990");

				personMergePreview.DiscardedProperties.AddPair("PER_MobilePhone", "8888 5555");
				personMergePreview.DiscardedProperties.AddPair("PER_HomePhone", "9876 5432");
				personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
				personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "42 Fake Street");
				personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Room 7");
				personMergePreview.DiscardedProperties.AddPair("PER_City", "JaneTown");
				personMergePreview.DiscardedProperties.AddPair("PER_State", "Janeland");
				personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");
				personMergePreview.DiscardedProperties.AddPair("PER_RN_NKCountry", "US");
				personMergePreview.DiscardedProperties.AddPair("PER_BirthDate", "10/10/1980");

				using (var form = new ZForm())
				{
					var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
					form.Controls.Add(control);

					var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
					var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

					AssertEquals(7, control.LeftBottomLayoutPanelForTest.RowCount);
					AssertEquals(5, control.RightBottomLayoutPanelForTest.RowCount);

					CombineAssertions(() =>
					{
						AssertEquals("Address :", leftPreviewLabelControls[0].HumanReadableNameLabel.Text);
						AssertEquals("Birth Date :", leftPreviewLabelControls[1].HumanReadableNameLabel.Text);
						AssertEquals("Email Address :", leftPreviewLabelControls[2].HumanReadableNameLabel.Text);
						AssertEquals("Gender :", leftPreviewLabelControls[3].HumanReadableNameLabel.Text);
						AssertEquals("Full Name :", leftPreviewLabelControls[4].HumanReadableNameLabel.Text);
						AssertEquals("Home Phone :", leftPreviewLabelControls[5].HumanReadableNameLabel.Text);
						AssertEquals("Mobile Phone :", leftPreviewLabelControls[6].HumanReadableNameLabel.Text);

						AssertEquals("Address :", rightPreviewLabelControls[0].HumanReadableNameLabel.Text);
						AssertEquals("Birth Date :", rightPreviewLabelControls[1].HumanReadableNameLabel.Text);
						AssertEquals("Full Name :", rightPreviewLabelControls[2].HumanReadableNameLabel.Text);
						AssertEquals("Home Phone :", rightPreviewLabelControls[3].HumanReadableNameLabel.Text);
						AssertEquals("Mobile Phone :", rightPreviewLabelControls[4].HumanReadableNameLabel.Text);
					});
				}
			}
		}

		public void TestShouldNotShowInvisibleProperties()
		{
			var manualOrder = new PersonMergePreviewItemCollection()
			{
				new PersonMergePreviewItem() { FriendlyName = "Nationality Code", ColumnName = GlbPersonSchema.PER_RN_NKNationalityCodeISO.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Home Address 1", ColumnName = GlbPersonSchema.PER_HomeAddress1.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Home Address 2", ColumnName = GlbPersonSchema.PER_HomeAddress2.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "City", ColumnName = GlbPersonSchema.PER_City.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "State", ColumnName = GlbPersonSchema.PER_State.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Postcode", ColumnName = GlbPersonSchema.PER_Postcode.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Country", ColumnName = GlbPersonSchema.PER_RN_NKCountry.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Birth Date", ColumnName = GlbPersonSchema.PER_BirthDate.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Email Address", ColumnName = GlbPersonSchema.PER_EmailAddress.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Email Address 2", ColumnName = GlbPersonSchema.PER_EmailAddress2.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Gender", ColumnName = GlbPersonSchema.PER_Gender.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Full Name", ColumnName = GlbPersonSchema.PER_FullName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Friendly Name", ColumnName = GlbPersonSchema.PER_FriendlyName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Legal Name", ColumnName = GlbPersonSchema.PER_LegalName.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Name Suffix", ColumnName = GlbPersonSchema.PER_NameSuffix.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Name Title", ColumnName = GlbPersonSchema.PER_NameTitle.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Home Phone", ColumnName = GlbPersonSchema.PER_HomePhone.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone", ColumnName = GlbPersonSchema.PER_MobilePhone.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Mobile Phone 2", ColumnName = GlbPersonSchema.PER_MobilePhone2.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Preferred Language", ColumnName = GlbPersonSchema.PER_PreferredLanguage.Name, Visibility = false },

				new PersonMergePreviewItem() { FriendlyName = "Drivers License Number", ColumnName = GlbPersonSchema.PER_DriversLicenseNumber.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Personal Info", ColumnName = GlbPersonSchema.PER_PersonalInfo.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Picture", ColumnName = GlbPersonSchema.PER_Picture.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Fax Number", ColumnName = GlbPersonSchema.PER_FaxNumber.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Passport", ColumnName = GlbPersonSchema.PER_Passport.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Passport Expiry Date", ColumnName = GlbPersonSchema.PER_PassportExpiryDate.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Passport Place Of Issue", ColumnName = GlbPersonSchema.PER_PassportPlaceOfIssue.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase", ColumnName = GlbPersonSchema.PER_ChallengePhrase.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Challenge Phrase Type", ColumnName = GlbPersonSchema.PER_ChallengePhraseType.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Password Hash", ColumnName = GlbPersonSchema.PER_PasswordHash.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Password Hash Iterations", ColumnName = GlbPersonSchema.PER_PasswordHashIterations.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Password Salt", ColumnName = GlbPersonSchema.PER_PasswordSalt.Name, Visibility = false },
				new PersonMergePreviewItem() { FriendlyName = "Web Access Enabled", ColumnName = GlbPersonSchema.PER_WebAccessEnabled.Name, Visibility = false }
			};

			using (SystemDataRegistry.Instance.PersonMergePreviewItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, manualOrder))
			{
				Env.Registry.SetOrgAllowMixedCase(true);

				var mergedPerson = Factory.New<GlbPerson>();
				var personMergePreview = new PersonMergePreview();
				personMergePreview.MergedProperties.AddPair("PER_HomePhone", "1234 5678");
				personMergePreview.MergedProperties.AddPair("PER_MobilePhone", "7777 6666");
				personMergePreview.MergedProperties.AddPair("PER_HomeAddress1", "42 Pretend Street");
				personMergePreview.MergedProperties.AddPair("PER_HomeAddress2", "Unit 7");
				personMergePreview.MergedProperties.AddPair("PER_City", "Janeville");
				personMergePreview.MergedProperties.AddPair("PER_State", "Janeland");
				personMergePreview.MergedProperties.AddPair("PER_Postcode", "1234");
				personMergePreview.MergedProperties.AddPair("PER_RN_NKCountry", "ZA");
				personMergePreview.MergedProperties.AddPair("PER_FullName", "Jane Deer");
				personMergePreview.MergedProperties.AddPair("PER_EmailAddress", "Jane@Deer.com");
				personMergePreview.MergedProperties.AddPair("PER_Gender", "Female");
				personMergePreview.MergedProperties.AddPair("PER_BirthDate", "01/01/1990");

				personMergePreview.DiscardedProperties.AddPair("PER_MobilePhone", "8888 5555");
				personMergePreview.DiscardedProperties.AddPair("PER_HomePhone", "9876 5432");
				personMergePreview.DiscardedProperties.AddPair("PER_FullName", "Jane Doe");
				personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress1", "42 Fake Street");
				personMergePreview.DiscardedProperties.AddPair("PER_HomeAddress2", "Room 7");
				personMergePreview.DiscardedProperties.AddPair("PER_City", "JaneTown");
				personMergePreview.DiscardedProperties.AddPair("PER_State", "Janeland");
				personMergePreview.DiscardedProperties.AddPair("PER_Postcode", "1234");
				personMergePreview.DiscardedProperties.AddPair("PER_RN_NKCountry", "US");
				personMergePreview.DiscardedProperties.AddPair("PER_BirthDate", "10/10/1980");

				using (var form = new ZForm())
				{
					var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
					form.Controls.Add(control);

					var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
					var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

					CombineAssertions(() =>
					{
						AssertEquals(false, leftPreviewLabelControls.Any());
						AssertEquals(false, rightPreviewLabelControls.Any());
					});
				}
			}
		}

		public void TestPersonsMergePreview_BlankKeyPairHasNoLabels()
		{
			var mergedPerson = Factory.New<GlbPerson>();
			var personMergePreview = new PersonMergePreview();

			using (var form = new ZForm())
			{
				var control = new PersonMergePreviewUserControlForTest(mergedPerson, personMergePreview, "");
				form.Controls.Add(control);

				var leftPreviewLabelControls = control.LeftBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();
				var rightPreviewLabelControls = control.RightBottomLayoutPanelForTest.Controls.OfType<PersonMergePreviewLabelUserControl>().ToArray();

				AssertEquals(0, leftPreviewLabelControls.Length);
				AssertEquals(0, rightPreviewLabelControls.Length);
			}
		}
	}

	#region Implementation

	public class PersonMergePreviewUserControlForTest : PersonMergePreviewUserControl
	{
		public KTableLayoutPanel LeftBottomLayoutPanelForTest => LeftBottomLayoutPanel;

		public KTableLayoutPanel RightBottomLayoutPanelForTest => RightBottomLayoutPanel;

		public ZPanel RightBottomPanelForTest => RightBottomPanel;

		public ZGroupBox MergedPropertiesGroupBoxForTest => MergedPropertiesGroupBox;

		public ZGroupBox DiscardedPropertiesGroupBoxForTest => DiscardedPropertiesGroupBox;

		public PersonMergePreviewUserControlForTest(GlbPerson mergedPerson, PersonMergePreview personMergePreview, string mergingPersonname) : base(mergedPerson, personMergePreview, mergingPersonname)
		{
		}
	}

	#endregion
}
