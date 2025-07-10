using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTemplateFileStorageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMandatoryValidationTemplateCode()
		{
			var testCases = new[]
			{
				new { value = ZString.Empty, isOk = false },
				new { value = (ZString)"XXX", isOk = true },
				new { value = (ZString)"IMP", isOk = true },
				new { value = (ZString)"EXP", isOk = true },
			};

			var template = Factory.New<AccTemplateFileStorage>();

			foreach (var test in testCases)
			{
				template.TFS_Code = test.value;
				template.Validation.ValidateTFS_Code();
				AssertEquals(test.isOk, !template.HasErrors);
			}
		}

		public void TestStringIsUsableAsGuidInTFSExternalFileReference()
		{
			var exampleTemplateFile = Factory.New<AccTemplateFileStorage>();
			exampleTemplateFile.TFS_FileData = ZBlob.FromUTF8("0x505AECBBD7AE24E97626762F40EFB0591CA149E4A90E9FA60E7B466132BCC9F01931180CC265789361320");
			exampleTemplateFile.TFS_FileName = "Test XSLT File";
			exampleTemplateFile.TFS_Code = "XYZ";
			var expectedErrorMessage = @"Enter a valid GUID value.";

			//Attempting valid guid
			exampleTemplateFile.TFS_ExternalReference_ForBinding = ZGuid.NewZGuid().ToString();
			AssertNoErrors(exampleTemplateFile.TFS_ExternalReferenceInfo);

			//Attempting empty string
			exampleTemplateFile.TFS_ExternalReference_ForBinding = "";
			AssertNoErrors(exampleTemplateFile.TFS_ExternalReferenceInfo);

			//Attempting non-guid value
			exampleTemplateFile.TFS_ExternalReference_ForBinding = "ABC";
			AssertHasError(exampleTemplateFile.TFS_ExternalReferenceInfo, expectedErrorMessage);

			//Attempting incomplete guid value
			exampleTemplateFile.TFS_ExternalReference_ForBinding = "CADFB62F-E1FA-4D9F-A5D1";
			AssertHasError(exampleTemplateFile.TFS_ExternalReferenceInfo, expectedErrorMessage);

			//Attempting invalid guid value
			exampleTemplateFile.TFS_ExternalReference_ForBinding = ZGuid.Invalid.ToString();
			AssertHasError(exampleTemplateFile.TFS_ExternalReferenceInfo, expectedErrorMessage);
		}

		public void TestXSLTTemplateShouldHaveFileOrGuid()
		{
			//Template Creation with both Guid and File Data
			var exampleTemplateFile = Factory.New<AccTemplateFileStorage>();
			exampleTemplateFile.TFS_Code = "XYZ";
			exampleTemplateFile.TFS_FileData = ZBlob.FromUTF8("0x505AECBBD7AE24E97626762F40EFB0591CA149E4A90E9FA60E7B466132BCC9F01931180CC265789361320");
			exampleTemplateFile.TFS_FileName = "Test File";
			exampleTemplateFile.TFS_ExternalReference_ForBinding = ZGuid.NewZGuid().ToString();

			var expectedError = "Please enter either a GUID or add an XSLT file";

			//Asserting deleting file data only causes no errors
			exampleTemplateFile.TFS_FileName = string.Empty;
			exampleTemplateFile.TFS_FileData = ZBlob.Empty;
			AssertNoErrors(exampleTemplateFile.TFS_ExternalReferenceInfo);
			AssertNoErrors(exampleTemplateFile.TFS_FileDataInfo);

			//Asserting having both file name and external reference empty causes  error by emptying guid
			exampleTemplateFile.TFS_ExternalReference_ForBinding = "";
			AssertHasErrors(expectedError, exampleTemplateFile.TFS_ExternalReferenceInfo);

			//Re-entering a guid and seeing errors gone after it
			exampleTemplateFile.TFS_ExternalReference_ForBinding = ZGuid.NewZGuid().ToString();
			Assert(!exampleTemplateFile.TFS_ExternalReferenceInfo.HasNotifications());

			//Re-Entering File Data, record will have both file data and guid now
			exampleTemplateFile.TFS_FileData = ZBlob.FromUTF8("0x505AECBBD7AE24E97626762F40EFB0591CA149E4A90E9FA60E7B466132BCC9F01931180CC265789361320");
			exampleTemplateFile.TFS_FileName = "New Test File";
			AssertNoErrors(exampleTemplateFile.TFS_ExternalReferenceInfo);
			AssertNoErrors(exampleTemplateFile.TFS_FileDataInfo);
			AssertNotNullOrEmpty(exampleTemplateFile.TFS_FileData.ToString());
			AssertNotNullOrEmpty(exampleTemplateFile.TFS_FileName);
			AssertNotNullOrEmpty(exampleTemplateFile.TFS_ExternalReference.ToString());

			//Asserting deleting guid only causes no errors. Record will have only file data after this.
			exampleTemplateFile.TFS_ExternalReference_ForBinding = "";
			AssertNoErrors(exampleTemplateFile.TFS_ExternalReferenceInfo);
			AssertNoErrors(exampleTemplateFile.TFS_FileDataInfo);
		}
	}
}
