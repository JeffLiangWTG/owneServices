using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalInformationWrapper))]
	sealed class AdditionalInformationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalInformation_CopyQuantity()
		{
			IAdditionalInformation additionalInformation = new AdditionalInformationWrapper(1);
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "AdditionalInformation.CopyQuantity should be");
			additionalInformation = new AdditionalInformationWrapper(6);
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(6).Using(CustomComparers.TypeComparison), "AdditionalInformation.CopyQuantity should be");
			additionalInformation = new AdditionalInformationWrapper("6", "8");
			NUnit.Framework.Assert.That(additionalInformation.StatementCode, NUnit.Framework.Is.EqualTo("6").Using(CustomComparers.TypeComparison), "AdditionalInformation.StatementCode should be");
			NUnit.Framework.Assert.That(additionalInformation.StatementDescription, NUnit.Framework.Is.EqualTo("8").Using(CustomComparers.TypeComparison), "AdditionalInformation.StatementDescription should be");
			additionalInformation = new AdditionalInformationWrapper("packingHouse");
			NUnit.Framework.Assert.That(additionalInformation.PackingHouse, NUnit.Framework.Is.EqualTo("packingHouse").Using(CustomComparers.TypeComparison), "AdditionalInformation.PackingHouse should be");
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			IAdditionalInformation additionalInformation = new AdditionalInformationWrapper(1);
			NUnit.Framework.Assert.That(additionalInformation.StatementCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(additionalInformation.StatementDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(additionalInformation.ProcessNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(additionalInformation.Content.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(additionalInformation.ApprovalID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestApprovalID()
		{
			IAdditionalInformation additionalInformation = new AdditionalInformationWrapper(ZString.Empty, ZString.Empty, "ApprovalID", ZString.Empty);
			NUnit.Framework.Assert.That(additionalInformation.ApprovalID, NUnit.Framework.Is.EqualTo("ApprovalID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestContent()
		{
			IAdditionalInformation additionalInformation = new AdditionalInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, "Content");
			NUnit.Framework.Assert.That(additionalInformation.Content, NUnit.Framework.Is.EqualTo("Content").Using(CustomComparers.TypeComparison));
		}
	}
}
