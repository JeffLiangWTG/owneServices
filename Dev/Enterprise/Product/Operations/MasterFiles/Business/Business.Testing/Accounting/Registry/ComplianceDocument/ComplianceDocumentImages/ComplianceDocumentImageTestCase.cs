using System;
using System.Drawing;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class ComplianceDocumentImageTestCase : RegistryBusinessObjectTestCaseBase
	{
		public void ImageIsMandatory()
		{
			BizObj.Image = null;
			BizObj.RunPreSaveValidation();
			AssertEquals("There should be an error if image is null.", true, BizObj.RowErrors.Contains("Please select an image."));

			BizObj.Image = new Bitmap(1, 1);
			BizObj.RunPreSaveValidation();
			AssertEquals("There should not be any row errors.", false, BizObj.HasRowErrors);
		}

		public void TestValidateComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertNoErrors(BizObj.ComplianceSubTypeInfo);

				var collection = new ComplianceDocumentImageCollection();
				collection.Add(BizObj);

				BizObj.ComplianceSubType = "";
				AssertHasError(BizObj.ComplianceSubTypeInfo, "Please enter a Compliance Sub Type.");

				BizObj.Country = Core.Constants.CountryCodes.Taiwan;
				BizObj.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
				AssertNoErrors(BizObj.ComplianceSubTypeInfo);

				var newBizObj = collection.AddNew();
				newBizObj.Country = Core.Constants.CountryCodes.Taiwan;
				newBizObj.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
				AssertHasError(newBizObj.ComplianceSubTypeInfo, "The combination of Country/Region 'TW' and Compliance Sub Type 'NTC' have already exists.");
			}
		}

		#region Implementation

		protected override bool IsCodeMandatory => false;

		protected sealed override bool RequiresFactory => false;

		protected sealed override bool RequiresFallbackLevel => false;

		protected new ComplianceDocumentImage BizObj
		{
			get { return (ComplianceDocumentImage)base.BizObj; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ComplianceDocumentImage result = (ComplianceDocumentImage)GetNewBusinessObject();
			result.Image = CreateATestImage();
			result.ImagePkForTest = Guid.NewGuid();
			return result;
		}

		Image CreateATestImage(int width = 1, int height = 1)
		{
			var bitmap = new Bitmap(width, height);
			byte[] bytes;

			using (TempFile tempFile = TempFile.New())
			{
				bitmap.Save(tempFile.Filename);
				bytes = File.ReadAllBytes(tempFile.Filename);
			}

			var stream = new MemoryStream(bytes);
			return Image.FromStream(stream);
		}

		#endregion
	}
}
