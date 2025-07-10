using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing.BusinessTests.Autorating
{
	class PossibleMatchesIntegrationTest : BaseRatingIntegrationTest
	{
		[GuiTest]
		public void TestPossibleMatchesAndUnacceptedQuotes()
		{
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "AGN ORG";

			var unacceptedQuote = Factory.New<Quote>();
			unacceptedQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			unacceptedQuote.QuotationClientAddress.OrganisationPK = consignor.PK;
			unacceptedQuote.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USNYC");

			var tariff = Helper.NewCompanyTariff();
			tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "USLAX");
			var tariffFRTEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "USNYC");
			tariff.Factory.Save();

			var rate = Helper.NewClientRate(consignee);
			var rateFRTEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "USNYC");
			rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUBNE", "USLAX");

			var shipment = CreateStandardShipment(origin: "AUSYD", destination: "USNYC",
				consignor: consignor, consignee: consignee);
			shipment.JS_INCO = "CIF";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				var runner = new AutoRatingStarter(shipment, new AutoRatingGUIInteractor(form));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				runner.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(string.Format(@"Shipment {0} : There are matching:
  • Unaccepted Quotes for this client

Do you want to continue with Autorating?", shipment.JS_UniqueConsignRef)));

				var pmForm = ZFormModaliser.LastFormShownDialogForTest as PossibleMatchesForm;
				AssertNotNull(pmForm);
				var pmWrapper = pmForm.LastDataSourceForTest as PossibleMatchesWrapper;
				AssertNotNull(pmWrapper);
				AssertContainsExactElementsInAnyOrder(new[] { tariffFRTEntry2.PK, rateFRTEntry1.PK }, pmWrapper.PossibleMatches.Select(x => x.PK));
			}
		}

		[GuiTest]
		public void TestPossibleMatchesApplicableToFreightRateAutoratingMode()
		{
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "AGN ORG";

			var rate = Helper.NewClientRate(consignee);
			rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USNWK");

			var shipment = CreateStandardShipment(consignor: consignor, consignee: consignee);
			shipment.JS_INCO = "CIF";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			Factory.Save();

			AssertPossibleMatchesFormShown(false, shipment, consignor);

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			Factory.Save();

			AssertPossibleMatchesFormShown(true, shipment, consignor);

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			Factory.Save();

			AssertPossibleMatchesFormShown(true, shipment, consignor);
		}

		[GuiTest]
		public void TestPossibleMatchesApplicableWithFilters()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrier1SvcLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrier1SvcLevel.PL_Code = "XYZ";

			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "AGN ORG";

			Helper.NewClientRate(consignee).AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USNWK");
			var shipment = CreateStandardShipment(consignor: consignor, consignee: consignee);

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;

			Factory.Save();

			AssertPossibleMatchesFormShown(false, shipment, consignor, new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT"
					}
				});

			//To filter RemoveChargeCodesNotMatchingCurrentCompany
			Helper.ChargeCodes["FRT"].AC_GC = Factory.NewWithValidTestData<GlbCompany>().PK;

			Factory.Save();

			AssertPossibleMatchesFormShown(true, shipment, consignor);
		}

		[GuiTest]
		public void TestPossibleMatches_ShouldNotDisplayPopupWhenMatchingRatesPresent_BCN()
		{
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.NewClientRate(Consignee).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.BCN, "AUSYD", "USNWK", "FRT", 100);
				var shipment = Helper.CreateBuyerConsolLeadShipment(Helper.CreateBuyersConsolConsol("AUSYD", "USNWK"), Consignor, Consignee);
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				job.PlugInData = shipment;
				job.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;

				Factory.Save();

				AssertPossibleMatchesFormShown(false, shipment, Consignor, new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 100
					}
				});
			}
		}

		[GuiTest]
		public void TestPossibleMatches_ShouldNotThrowException_WhenBCNIsGeneratingError()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.ApportionInvoiceMaster;

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 300);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.BCN, "USLAX", "AUSYD", "CAF", 300);

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", transportMode: RatingConstants.RateCategory.AIR);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, transportMode: RatingConstants.RateCategory.AIR);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee);

			AssertEquals("Pre-condition: Chargeable should be zero", 0m, leadShipment.ChargeableAmount);
			AssertEquals("Pre-condition: Chargeable should be zero", 0m, coloadShipment.ChargeableAmount);

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				AssertPossibleMatchesFormShown(true, () =>
				{
					AutorateAndAssert(Array.Empty<AssertionCharge>(), leadShipment, Consignee, autorateCosts: false,
						expectedErrors: new[]
						{
							@"Error Autorating has encountered an error:
The chargeable amount for all shipment in this Buyers Consol could not be calculated.
Please check the chargeable entered on each shipment has been specified correctly."
						}
					);
				});
			});
		}

		[GuiTest]
		public void TestPossibleMatches_ShouldNotDisplayPopupWhenMatchingRatesPresentAndHasAdditionalNotification()
		{
			var consignor = Helper.NewOrgHeader(1);
			consignor.OH_FullName = "CNR ORG";
			var consignee = Helper.NewOrgHeader(1);
			consignee.OH_FullName = "CNE ORG";
			var agent = Helper.NewOrgHeader(1);
			agent.OH_FullName = "AGN ORG";

			var rate = Helper.NewClientRate(consignee);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USNWK");
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-1);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(7);

			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddFlatCharge("FRT", 100);

			var shipment = CreateStandardShipment(consignor: consignor, consignee: consignee);
			shipment.JS_UniqueConsignRef = "S00000101";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;

			Factory.Save();

			AssertPossibleMatchesFormShown(false, () =>
			{
				AutorateAndAssert
				(
					new[] {
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 100
						}
					},
					shipment,
					consignor,
					autorateCosts: false
				);

				Assert("Test Condition: should have Autorating Notifications.", UnitTestUserNotification.Instance.PreviousMessages[0].Contains(@"Autorating Notifications: Shipment S00000101 : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days"));
			});
		}

		void AssertPossibleMatchesFormShown(bool expectedResult, ForwardingShipment shipment, OrgHeader orgHeader)
		{
			AssertPossibleMatchesFormShown(expectedResult, shipment, orgHeader, Array.Empty<AssertionCharge>());
		}

		void AssertPossibleMatchesFormShown(bool expectedResult, ForwardingShipment shipment, OrgHeader orgHeader, AssertionCharge[] expectedCharges)
		{
			AssertPossibleMatchesFormShown(expectedResult, () => AutorateAndAssert(expectedCharges, shipment, orgHeader, autorateCosts: false));
		}

		void AssertPossibleMatchesFormShown(bool expectedResult, Action action)
		{
			using (DataRegistryRating.Instance.ProposeSimilarRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				bool possibleMatcherFormShown = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
					form =>
					{
						if (form is PossibleMatchesForm)
						{
							possibleMatcherFormShown = true;
						}
					});

				action();

				AssertEquals(expectedResult, possibleMatcherFormShown);
			}
		}

		ForwardingShipment CreateStandardShipment(string transportMode = "SEA", string packingMode = "LCL",
			string origin = "AUSYD", string destination = "USNWK",
			OrgHeader consignor = null, OrgHeader consignee = null)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = packingMode;
			shipment.JS_ShipmentType = "STD";
			shipment.JS_ActualWeight = 120m;
			shipment.ConsignorPK = consignor?.PK ?? Consignor.PK;
			shipment.ConsigneePK = consignee?.PK ?? Consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			return shipment;
		}
	}
}
