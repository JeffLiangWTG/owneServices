using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CMCertificateOfOriginCusSupporting))]
	sealed class CMCertificateOfOriginCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<CMCertificateOfOriginCusSupporting>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusTWControllingMessageHeader>().CertificateOfOrigins.AddNew();
		}

		protected override IEnumerable<CMCertificateOfOriginCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var controllingMessageHeader = factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var cmCertificateOfOriginCusSupporting = controllingMessageHeader.CertificateOfOrigins.AddNew();
			cmCertificateOfOriginCusSupporting.CSI_ReferenceNumber = "X1";
			yield return cmCertificateOfOriginCusSupporting;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<CMCertificateOfOriginCusSupporting>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(CusTWControllingMessageHeaderSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber()
		{
			var supporting = Factory.New<CMCertificateOfOriginCusSupporting>();
			var info = supporting.CSI_ReferenceNumberInfo;
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(info, "Certificate of Origin Number", "COO Number", "COO No.", "Indicates the certificate of origin number from the country of origin. When Certificate Type is '17', this column must be filled in.");
				NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(35), "MaxLength");
			});
		}

		[ExpectNoExceptions]
		public void TestIsRowEmpty()
		{
			var supporting = Factory.New<CMCertificateOfOriginCusSupporting>();
			supporting.CSI_ReferenceNumber = "XX";
			NUnit.Framework.Assert.That(supporting.IsRowEmpty, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			supporting.CSI_ReferenceNumber = ZString.Empty;
			NUnit.Framework.Assert.That(supporting.IsRowEmpty, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}
	}
}
