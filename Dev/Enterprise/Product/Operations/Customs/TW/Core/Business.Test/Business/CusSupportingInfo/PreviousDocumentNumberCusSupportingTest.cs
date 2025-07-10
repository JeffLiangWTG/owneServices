using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PreviousDocumentNumberCusSupporting))]
	sealed class PreviousDocumentNumberCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocumentNumberCusSupporting>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusTWControllingMessageHeader>().PreviousDocumentNumbers.AddNew();
		}

		protected override IEnumerable<PreviousDocumentNumberCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var controllingMessageHeader = factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var previousDocumentNumberCusSupporting = controllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumberCusSupporting.CSI_ReferenceNumber = "X1";
			yield return previousDocumentNumberCusSupporting;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<PreviousDocumentNumberCusSupporting>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.PreviousDocumentNumber).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(CusTWControllingMessageHeaderSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber()
		{
			var supporting = Factory.New<PreviousDocumentNumberCusSupporting>();
			var info = supporting.CSI_ReferenceNumberInfo;
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(info, "Previous Document Number", "Previous Document Number", "Previous Document Number", ZString.Empty);
				NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(35), "MaxLength");
			});
		}

		[ExpectNoExceptions]
		public void TestIsRowEmpty()
		{
			var supporting = Factory.New<PreviousDocumentNumberCusSupporting>();
			supporting.CSI_ReferenceNumber = "XX";
			NUnit.Framework.Assert.That(supporting.IsRowEmpty, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			supporting.CSI_ReferenceNumber = ZString.Empty;
			NUnit.Framework.Assert.That(supporting.IsRowEmpty, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}
	}
}
