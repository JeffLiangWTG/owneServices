using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMApplicationAdditionalInformationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestStatementDescription()
		{
			header.TW1_RequestDescription = "Request Description";
			NUnit.Framework.Assert.That(applicationAdditionalInformation.StatementDescription, NUnit.Framework.Is.EqualTo("Request Description").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeductionSample()
		{
			header.TW1_SamplingReductionReason = "Deduction Sample";
			NUnit.Framework.Assert.That(applicationAdditionalInformation.DeductionSample, NUnit.Framework.Is.EqualTo("Deduction Sample").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestElectronicReceipt()
		{
			header.TW1_ElectronicReceipt = true;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ElectronicReceipt, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			header.TW1_ElectronicReceipt = false;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ElectronicReceipt, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestProvedPaper()
		{
			header.TW1_ProofOfPaper = true;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ProvedPaper, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			header.TW1_ProofOfPaper = false;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ProvedPaper, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestReturnSample()
		{
			header.TW1_ApplyForSampleReturn = true;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ReturnSample, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			header.TW1_ApplyForSampleReturn = false;
			NUnit.Framework.Assert.That(applicationAdditionalInformation.ReturnSample, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddressChineseLine()
		{
			header.TW1_SampleReturnAddress = "Chinese Line";
			NUnit.Framework.Assert.That(applicationAdditionalInformation.AddressChineseLine, NUnit.Framework.Is.EqualTo("Chinese Line").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			applicationAdditionalInformation = new NX5105CMApplicationAdditionalInformation(header);
		}

		CusTWControllingMessageHeader header;
		IApplicationAdditionalInformation applicationAdditionalInformation;
	}
}
