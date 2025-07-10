using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GovernmentProcedureTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentProcedure.TransportTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TransportTypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(governmentProcedure.CurrentCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CurrentCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(governmentProcedure.Description, NUnit.Framework.Is.EqualTo("SAY TOTAL ZERO (0) CTN ONLY").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			var jobDeclaration = header.Declaration;
			jobDeclaration.JE_TotalNoOfPacks = 1;
			jobDeclaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			governmentProcedure = NX101GovernmentProcedure.GovernmentProcedures(header).First();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentProcedure.Description, NUnit.Framework.Is.EqualTo("SAY TOTAL ONE (1) PCE ONLY").Using(CustomComparers.TypeComparison));
				jobDeclaration.JE_TotalNoOfPacks = 9999;
				jobDeclaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Box;
				governmentProcedure = NX101GovernmentProcedure.GovernmentProcedures(header).First();
				NUnit.Framework.Assert.That(governmentProcedure.Description, NUnit.Framework.Is.EqualTo("SAY TOTAL NINE THOUSAND, NINE HUNDRED AND NINETY NINE (9999) BOXES ONLY").Using(CustomComparers.TypeComparison));

				header.TW1_Remarks = "header remark";
				governmentProcedure = NX101GovernmentProcedure.GovernmentProcedures(header).First();
				NUnit.Framework.Assert.That(governmentProcedure.Description, NUnit.Framework.Is.EqualTo("header remark\r\nSAY TOTAL NINE THOUSAND, NINE HUNDRED AND NINETY NINE (9999) BOXES ONLY").Using(CustomComparers.TypeComparison));

				header.TW1_Remarks = ZString.Replicate('A', 1024);
				var governmentProcedures = NX101GovernmentProcedure.GovernmentProcedures(header);
				NUnit.Framework.Assert.That(governmentProcedures.Count(), NUnit.Framework.Is.EqualTo(4), "should create up to 4 GovernmentProcedures");
				NUnit.Framework.Assert.That(governmentProcedures.First().Description, NUnit.Framework.Is.EqualTo(ZString.Replicate('A', 256)), "should split by 256 chars");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			governmentProcedure = NX101GovernmentProcedure.GovernmentProcedures(header).First();
		}

		CusTWControllingMessageHeader header;
		NX101GovernmentProcedure governmentProcedure;
	}
}
