using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TypeApprovalCertificateNumberCusSupporting))]
	sealed class TypeApprovalCertificateNumberCusSupportingTest : SingleCusSupportingInfoTest<TypeApprovalCertificateNumberCusSupporting>
	{
		protected override IEnumerable<TypeApprovalCertificateNumberCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var typeApprovalCertificateNumber = factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().TypeApprovalCertificateNumbers;
			typeApprovalCertificateNumber.CSI_Code = "X";
			typeApprovalCertificateNumber.CSI_ReferenceNumber = "X1";
			typeApprovalCertificateNumber.CSI_ReferenceNumber2 = "X2";
			yield return typeApprovalCertificateNumber;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<TypeApprovalCertificateNumberCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var supporting = Factory.New<TypeApprovalCertificateNumberCusSupporting>();
			NUnit.Framework.Assert.That(supporting.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(TypeApprovalCertificateNumberCusSupportingValidation)));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var supporting = jobComInvoiceLine.TypeApprovalCertificateNumbers;
			NUnit.Framework.Assert.That(supporting.Parent, NUnit.Framework.Is.EqualTo(jobComInvoiceLine));
		}

		[ExpectNoExceptions]
		public void TestCSI_Description()
		{
			var supporting = Factory.New<TypeApprovalCertificateNumberCusSupporting>();
			supporting.CSI_Description = "XXX";
			supporting.CSI_ReferenceNumber2 = "XX2";
			NUnit.Framework.Assert.That(supporting.CSI_ReferenceNumber2, NUnit.Framework.Is.EqualTo("XX2").Using(CustomComparers.TypeComparison));
			supporting.CSI_Description = ZString.Empty;
			NUnit.Framework.Assert.That(supporting.CSI_ReferenceNumber2, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}
	}
}
