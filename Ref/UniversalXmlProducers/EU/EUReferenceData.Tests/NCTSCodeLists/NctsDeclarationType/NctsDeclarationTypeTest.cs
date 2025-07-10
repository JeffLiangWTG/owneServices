using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsDeclarationTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsDeclarationTypeCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.DeclarationTypeType));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.DeclarationTypes));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsDeclarationType;
		}
		NctsDeclarationType declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsDeclarationType();
	}
}
