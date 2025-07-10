using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class ConfirmGetReportModelTest : TestCase
	{
		public void TestConfirmGetReportModel()
		{
			var model = new ConfirmGetReportModel()
			{
				Identifiers = new List<Identifier>()
				{
					new Identifier()
					{
						Type = IdentifierType.ABN,
						ID = "24352768"
					},

					new Identifier()
					{
						Type = IdentifierType.ACN,
						ID = "23462874"
					},

					new Identifier()
					{
						Type = IdentifierType.DUNS,
						ID = "2436876"
					}
				},

				IsGet = false,
				CompanyName = "WiseTech Global Limited",
				CompanyAddress = "U 3 72 O'Riordan St",
				ReportType = CreditReportType.ComprehensiveReport,
				City = "Alexandria",
				CountryState = "NSW",
				PostCode = "2015",
			};

			CombineAssertions(() =>
			{
				AssertEquals("2436876", model.Identifiers.Single(o => o.Type == IdentifierType.DUNS).ID);
				AssertEquals("24352768", model.Identifiers.Single(o => o.Type == IdentifierType.ABN).ID);
				AssertEquals("23462874", model.Identifiers.Single(o => o.Type == IdentifierType.ACN).ID);
				AssertEquals(false, model.IsGet);
				AssertEquals("WiseTech Global Limited", model.CompanyName);
				AssertEquals("U 3 72 O'Riordan St", model.CompanyAddress);
				AssertEquals("Alexandria", model.City);
				AssertEquals(CreditReportType.ComprehensiveReport, model.ReportType);
				AssertEquals("NSW", model.CountryState);
				AssertEquals("2015", model.PostCode);
			});
		}
	}
}
