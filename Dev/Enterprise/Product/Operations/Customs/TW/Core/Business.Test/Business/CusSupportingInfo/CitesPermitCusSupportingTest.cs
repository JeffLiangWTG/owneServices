using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CitesPermitCusSupporting))]
	sealed class CitesPermitCusSupportingTest : SingleCusSupportingInfoTest<CitesPermitCusSupporting>
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = GetBizObjsForCorrectlyTypeDecideTest(Factory).FirstOrDefault();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.CitesImportPermit).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		protected override IEnumerable<CitesPermitCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var citesPermit = factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().CitesPermitCusSupporting;
			citesPermit.CSI_Code = "X";
			citesPermit.CSI_ReferenceNumber = "X1";
			citesPermit.CSI_ReferenceNumber2 = "X2";
			yield return citesPermit;
		}

		[ExpectNoExceptions]
		public override void TestGetUsedFieldsInfos()
		{
			var bizObj = GetSingleCusSupportingInfoForTesting(Factory, false);
			var expected = new ZPropertyInfo[] { bizObj.CSI_ReferenceNumberInfo };
			NUnit.Framework.Assert.That(bizObj.GetUsedFieldsInfos().ToArray(), NUnit.Framework.Is.EqualTo(expected));
		}
	}
}
