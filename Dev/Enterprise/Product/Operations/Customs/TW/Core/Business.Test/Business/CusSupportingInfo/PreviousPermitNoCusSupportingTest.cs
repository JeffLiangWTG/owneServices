using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PreviousPermitNoCusSupporting))]
	sealed class PreviousPermitNoCusSupportingTest : SingleCusSupportingInfoTest<PreviousPermitNoCusSupporting>
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<PreviousPermitNoCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.PreviousPermitNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		protected override IEnumerable<PreviousPermitNoCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			PreviousPermitNoCusSupporting cusSupportingInfo = factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PrePermitNoCusSupporting;
			cusSupportingInfo.CSI_Code = "X";
			cusSupportingInfo.CSI_ReferenceNumber = "X1";
			cusSupportingInfo.CSI_ReferenceNumber2 = "X2";
			yield return cusSupportingInfo;
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
