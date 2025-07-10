using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GenCustomColumnDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDisplaySequenceConstraint()
		{
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_DisplaySequence = -5;
			AssertHasError(def.XC_DisplaySequenceInfo, "Display Sequence cannot be negative.");
		}

		public void TestErrorIfEmptyName()
		{
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Name = "X";
			Assert(!def.XC_NameInfo.HasNotifications());
			def.XC_Name = "";
			Assert(def.XC_NameInfo.HasNotifications());
		}

		public void TestErrorIfDuplicate()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var def1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "AAA";
			var def2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "AAA";
			AssertHasError(def2.XC_NameInfo, "The Name has been duplicated and must be unique.");
			def2.XC_Name = "BBB";
			AssertNoErrors(def2.XC_NameInfo);
		}

		public void TestWarningIfSameNameAsSystemProperty()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "Notes";
			string expectedWarning = string.Format("There is a system property with same name '{0}' on related business object.", def1.XC_Name);
			AssertHasWarning(def1.XC_NameInfo, expectedWarning);
			def1.XC_Name = "AAA";
			AssertNoWarning(def1.XC_NameInfo, expectedWarning);
		}

		public void TestWarningIfSameNameDifferentType()
		{
			const string expectedWarning =
@"There is a Custom Field definition in other Workflow Template with same Name, but different Type.
This may cause confusions in a case when such Custom Fields of several Workflow Templates are grouped together, e.g. in a module filter grid.";

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "AAA";
			def1.XC_Type = "STR";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SHP";
			var def2 = template2.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "AAA";
			def2.XC_Type = "STR";

			AssertNoWarning(def2.XC_NameInfo, expectedWarning);

			def2.XC_Type = "INT";

			AssertHasWarning(def2.XC_NameInfo, expectedWarning);

			template2.P0_ProcessType = "CON";
			def2.XC_Type = "INT";

			AssertNoWarning(def2.XC_NameInfo, expectedWarning);
		}

		public void TestErrorIfInvalidType()
		{
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Type = AddOnColumnDataType.Codes.String;
			Assert(!def.XC_TypeInfo.HasNotifications());
			def.XC_Type = "";
			Assert(def.XC_TypeInfo.HasNotifications());
			def.XC_Type = "X";
			Assert(def.XC_TypeInfo.HasNotifications());
		}

		public void TestCheckXC_XR()
		{
			var addOnRule1 = Factory.New<GenCustomAddOnRule>();
			addOnRule1.XR_Code = "TIMEONLY";
			addOnRule1.SetRules(new DateTimeFormatRule { Format = KDateTimeFormat.Time, IsEnabled = true });
			var addOnRule2 = Factory.New<GenCustomAddOnRule>();
			addOnRule2.SetRules(new CheckEnteredRule { IsEnabled = true });

			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Type = AddOnColumnDataType.Codes.String;

			def.XC_XR = ZGuid.Empty;
			AssertNoErrors(def.XC_XRInfo);
			def.XC_XR = addOnRule1.PK;
			AssertHasError(def.XC_XRInfo, "The 'TIMEONLY' rule cannot be applied to the String field.");
			def.XC_XR = addOnRule2.PK;
			AssertNoErrors(def.XC_XRInfo);
		}

		public void TestErrorIfComboBoxTooLong()
		{
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Type = AddOnColumnDataType.Codes.ComboBox;
			var maxLength = GenCustomColumnDefinitionValidation.Custom_Field_Name_Max_Length - (AddOnColumnDataType.PartIdentifier.Length + 1);
			def.XC_Name = new string('a', maxLength + 1);
			AssertHasError(def.XC_NameInfo, string.Format("Enter a Name no longer than {0} characters.", maxLength));
		}

		public void TestErrorIfTooLongExcludeComboBox()
		{
			var maxLength = GenCustomColumnDefinitionValidation.Custom_Field_Name_Max_Length;
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Type = AddOnColumnDataType.Codes.Boolean;
			def.XC_Name = new string('a', maxLength);
			AssertNoErrors(def.XC_NameInfo);

			def.XC_Name = new string('a', maxLength + 1);
			AssertHasError(def.XC_NameInfo, string.Format("Enter a Name no longer than {0} characters.", maxLength));

			def.XC_Type = AddOnColumnDataType.Codes.Byte;
			def.XC_Name = new string('a', maxLength);
			AssertNoErrors(def.XC_NameInfo);

			def.XC_Name = new string('a', maxLength + 1);
			AssertHasError(def.XC_NameInfo, string.Format("Enter a Name no longer than {0} characters.", maxLength));
		}

		public void TestNoPartIdentifierInName()
		{
			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_Type = AddOnColumnDataType.Codes.ComboBox;
			def.XC_Name = AddOnColumnDataType.PartIdentifier + "1ABC";
			AssertHasError(def.XC_NameInfo, string.Format("Name cannot contain {0} followed by a number.", AddOnColumnDataType.PartIdentifier));
			def.XC_Name = AddOnColumnDataType.PartIdentifier + "ABC";
			AssertNoErrors(def.XC_NameInfo);
			def.XC_Name = "a, 1";
			AssertHasError(def.XC_NameInfo, string.Format("Name cannot end with ',' followed by a number.", AddOnColumnDataType.PartIdentifier));
			def.XC_Name = "a, 1a";
			AssertNoErrors(def.XC_NameInfo);
			def.XC_Name = "a,99";
			AssertHasError(def.XC_NameInfo, string.Format("Name cannot end with ',' followed by a number.", AddOnColumnDataType.PartIdentifier));
		}
	}
}
