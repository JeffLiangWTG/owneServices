using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EthanolPermitNumberCusSupporting))]
	sealed class EthanolPermitNumberCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<EthanolPermitNumberCusSupporting>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return EthanolPermitNumberForTest;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<EthanolPermitNumberCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.EthanolPermitNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(CusTWControllingMessageHeaderSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber()
		{
			var supporting = Factory.New<EthanolPermitNumberCusSupporting>();
			NUnit.Framework.Assert.That(supporting.CSI_ReferenceNumberInfo.MaxLength, NUnit.Framework.Is.EqualTo(14));
		}

		[ExpectNoExceptions]
		public void TestIsRowEmpty()
		{
			EthanolPermitNumberForTest.CSI_ReferenceNumber = "XX";
			NUnit.Framework.Assert.That(EthanolPermitNumberForTest.IsRowEmpty, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			EthanolPermitNumberForTest.CSI_ReferenceNumber = ZString.Empty;
			NUnit.Framework.Assert.That(EthanolPermitNumberForTest.IsRowEmpty, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(EthanolPermitNumberForTest.CSI_ReferenceNumberInfo, "Ethanol Permit Number", "Ethanol Permit Number: Undenatured ethanol used for alcohol production should be enclosed with the approval documents issued by the Ministry of Finance.");
		}

		EthanolPermitNumberCusSupporting EthanolPermitNumberForTest
		{
			get
			{
				if (fEthanolPermitNumber == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var entryInstruction = declaration.CusEntryInstruction;
					var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
					fEthanolPermitNumber = messageHeader.EthanolPermitNumbers.AddNew();
				}

				return fEthanolPermitNumber;
			}
		}

		EthanolPermitNumberCusSupporting fEthanolPermitNumber;
		protected override IEnumerable<EthanolPermitNumberCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var ethanolPermitNumber = messageHeader.EthanolPermitNumbers.AddNew();
			ethanolPermitNumber.CSI_ReferenceNumber = "1";
			yield return ethanolPermitNumber;
		}
	}
}
