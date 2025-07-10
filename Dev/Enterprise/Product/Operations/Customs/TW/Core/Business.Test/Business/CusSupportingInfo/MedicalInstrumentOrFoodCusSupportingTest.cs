using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(MedicalInstrumentOrFoodCusSupporting))]
	sealed class MedicalInstrumentOrFoodCusSupportingTest : SingleCusSupportingInfoTest<MedicalInstrumentOrFoodCusSupporting>
	{
		protected override IEnumerable<MedicalInstrumentOrFoodCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var cusSupportingInfo = factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().MedicalInstrumentOrFoodCusSupporting;
			cusSupportingInfo.CSI_Code = "A";
			cusSupportingInfo.CSI_ReferenceNumber = "B";
			yield return cusSupportingInfo;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<MedicalInstrumentOrFoodCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var supporting = jobComInvoiceLine.MedicalInstrumentOrFoodCusSupporting;
			NUnit.Framework.Assert.That(supporting.Parent, NUnit.Framework.Is.EqualTo(jobComInvoiceLine));
		}

		[ExpectNoExceptions]
		public override void TestGetUsedFieldsInfos()
		{
			var bizObj = GetSingleCusSupportingInfoForTesting(Factory, false);
			var expected = new ZPropertyInfo[] { bizObj.CSI_CodeInfo, bizObj.CSI_ReferenceNumberInfo, bizObj.CSI_ReferenceNumber2Info };
			NUnit.Framework.Assert.That(bizObj.GetUsedFieldsInfos().ToArray(), NUnit.Framework.Is.EqualTo(expected));
		}
	}
}
