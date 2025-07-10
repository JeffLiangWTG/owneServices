using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ConfirmGetReportInfoModelTest : TestCase
	{
		public void TestConfirmGetReportInfoModel()
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
				City = "Alexandria",
				ReportType = CreditReportType.ComprehensiveReport,
				CountryState = "NSW",
				PostCode = "2015",
			};

			var confirmGetReportInfoModel = new ConfirmGetReportInfoModel(model);

			CombineAssertions(() =>
			{
				AssertEquals("2436876", confirmGetReportInfoModel.Identifiers.Single(o => o.Type == IdentifierType.DUNS).ID);
				AssertEquals("24352768", confirmGetReportInfoModel.Identifiers.Single(o => o.Type == IdentifierType.ABN).ID);
				AssertEquals("23462874", confirmGetReportInfoModel.Identifiers.Single(o => o.Type == IdentifierType.ACN).ID);
				AssertEquals("Renew", confirmGetReportInfoModel.ActionName);
				AssertEquals("WiseTech Global Limited", confirmGetReportInfoModel.CompanyName);
				AssertEquals("U 3 72 O'Riordan St", confirmGetReportInfoModel.CompanyAddress);
				AssertEquals("Alexandria", confirmGetReportInfoModel.City);
				AssertEquals("NSW", confirmGetReportInfoModel.CountryState);
				AssertEquals("2015", confirmGetReportInfoModel.PostCode);
				AssertEquals("You are about to purchase a Comprehensive Report (priced as per your current price list).", confirmGetReportInfoModel.OperationDetail);
				AssertEquals("Renew Comprehensive Report", confirmGetReportInfoModel.Title);
				AssertEquals("Cancel", confirmGetReportInfoModel.Cancel);
				AssertEquals("Alexandria NSW 2015", confirmGetReportInfoModel.CityStatePostInfo);
			});
		}
	}
}
