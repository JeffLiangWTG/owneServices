using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISCertificateDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckCertificateNumber()
		{
			var certificateData = new DISCertificateData(Factory, Document);
			certificateData.CertificateNumber = ZString.Empty;
			AssertNoMessageErrorContaining(certificateData.CertificateNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AddAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, AutoDISCertificateData.Schema.CertificateNumber);
			certificateData.CertificateNumber = ZString.Empty;
			AssertHasMessageErrorContaining(certificateData.CertificateNumberInfo, MandatoryValidation.YouHaveNotEntered);
			certificateData.CertificateNumber = "123";
			AssertNoMessageErrorContaining(certificateData.CertificateNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckExpiryDate()
		{
			var certificateData = new DISCertificateData(Factory, Document);
			certificateData.ExpiryDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(certificateData.ExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
			AddAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, AutoDISCertificateData.Schema.ExpiryDate);
			certificateData.ExpiryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(certificateData.ExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
			certificateData.ExpiryDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(certificateData.ExpiryDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckIssueDate()
		{
			var certificateData = new DISCertificateData(Factory, Document);
			certificateData.IssueDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(certificateData.IssueDateInfo, MandatoryValidation.YouHaveNotEntered);
			AddAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, AutoDISCertificateData.Schema.IssueDate);
			certificateData.IssueDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(certificateData.IssueDateInfo, MandatoryValidation.YouHaveNotEntered);
			certificateData.IssueDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(certificateData.IssueDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckGrossTonnage()
		{
			var certificateData = new DISCertificateData(Factory, Document);
			certificateData.GrossTonnage = ZDecimal.Zero;
			AssertNoMessageErrorContaining(certificateData.GrossTonnageInfo, MandatoryValidation.YouHaveNotEntered);
			AddAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, AutoDISCertificateData.Schema.GrossTonnage);
			certificateData.GrossTonnage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(certificateData.GrossTonnageInfo, MandatoryValidation.YouHaveNotEntered);
			certificateData.GrossTonnage = 1m;
			AssertNoMessageErrorContaining(certificateData.GrossTonnageInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckNetTonnage()
		{
			var certificateData = new DISCertificateData(Factory, Document);
			certificateData.NetTonnage = ZDecimal.Zero;
			AssertNoMessageErrorContaining(certificateData.NetTonnageInfo, MandatoryValidation.YouHaveNotEntered);
			AddAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, AutoDISCertificateData.Schema.NetTonnage);
			certificateData.NetTonnage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(certificateData.NetTonnageInfo, MandatoryValidation.YouHaveNotEntered);
			certificateData.NetTonnage = 1m;
			AssertNoMessageErrorContaining(certificateData.NetTonnageInfo, MandatoryValidation.YouHaveNotEntered);
		}

		DISDocument Document => document ?? (document = CreateDocument());
		DISDocument document;

		DISDocument CreateDocument()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			usDISHost.Setup(m => m.EDocs).Returns(new List<IeDoc>());
			usDISHost.Setup(m => m.FormGroups).Returns(System.Array.Empty<ZString>());
			var defaultValues = new Mock<IUSDISDefaultValues>();
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var document = new DISDocument(hostWrapper);
			var helper = new DISUniversalReferenceHelper(Factory);
			var code = helper.CreateDisCodeEntry("COM02", "SOLAS_1");
			Factory.Save();
			document.DocumentLabel = "COM02";
			return document;
		}

		void AddAttribute(ZString name, ZString value)
		{
			if (Document != null)
			{
				Document.DISFormCusCode.Attributes.AddNew(name, value);
			}
		}
	}
}
