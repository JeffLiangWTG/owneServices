using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLLocalNumberFormat))]
	class GLLocalNumberFormatTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateLanguage()
		{
			AssertNoErrors("Precondition: Language should not have errors.", BizObj.LanguageInfo);

			BizObj.Language = "";
			AssertHasError(BizObj.LanguageInfo, "Please enter a Language.");
			BizObj.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertNoErrors(BizObj.LanguageInfo);
		}

		public void TestValidateNumberFormat()
		{
			AssertNoErrors("Precondition: Local GL Number Format should not have errors.", BizObj.NumberFormatInfo);
			BizObj.NumberFormat = "";
			AssertHasError(BizObj.NumberFormatInfo, "Please enter a Local GL Number Format.");
			BizObj.NumberFormat = "4-2-1";
			AssertNoErrors(BizObj.NumberFormatInfo);
		}

		public void TestValidateCheckRecordIsUnique()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			gNF = list.AddNew();
			gNF.NumberFormat = "3-2-2";
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.Australia;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			AssertEquals("The registry item should not have errors", false, gNF.HasRowErrors);

			gNF = list.AddNew();
			gNF.NumberFormat = "3-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			AssertEquals(gNF.RowErrors.GetFirstMessage(), "Duplicate Country/Region Code and Language.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new GLLocalNumberFormat();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLLocalNumberFormat();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new GLLocalNumberFormat BizObj
		{
			get { return (GLLocalNumberFormat)base.BizObj; }
		}

		#endregion

	}
}
