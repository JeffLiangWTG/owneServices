using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.AutoRating;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using static Enterprise.Core.Constants;
using LogType = Enterprise.Integration.LogType;
using WiseRatesModel = WiseRates.Api.Model;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class RatingIntegrationGUITest : BaseRatingIntegrationTest
	{
		public void TestViewJSONWithDeveloperAuthentication()
		{
			WiseRatesGUIHelper.ViewJSONWithDeveloperAuthenticationAction_ForTestOnly = () => throw new Win32Exception("Application not found");

			AssertNoExceptionThrown
			(
				"WHEN application not found THEN should not throw exception",
				() => { WiseRatesGUIHelper.ViewJSONWithDeveloperAuthentication("{}"); }
			);

			AssertEquals
			(
				"WHEN application not found THEN should show error message",
				"There was a problem showing raw data: Application not found",
				UnitTestUserNotification.Instance.LastMessage.Text
			);
		}

		[GuiTest]
		public void TestAutoratingOnShipment_WhenAutorateAndCloseShipmentBeforeShowingRateSelector_ShouldNotThrowException()
		{
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";

			var c = consol.Containers.AddNew();
			c.JC_RC = GP20.PK;
			c.JC_ContainerCount = 1;

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				using (var shipmentForm = new ZForm(shipment))
				{
					shipmentForm.DisplayMode = ODisplayMode.Edit;
					var tabControl = new ZTemplateTabControl();
					shipmentForm.Controls.Add(tabControl);
					shipmentForm.PlugIns.Add(ControllerIDs.JobInvoicing);
					shipmentForm.Show();

					using (var jobInvoicingPlugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing))
					{
						jobInvoicingPlugIn.SelectTabPage();

						var mockFactory = new Mock<IWiseRatesClientFactory>();
						var mockClient = new Mock<IWiseRatesClient>();

						mockFactory
							.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
							.Returns((mockClient.Object, string.Empty));

						mockClient
							.Setup(f => f.SearchAsync(It.IsAny<WiseRatesModel.RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
							.Callback<WiseRatesModel.RatesSearchRequest, string, CancellationToken>((req, correlationId, cancelToken) =>
							{
								shipmentForm.Close();
							})
							.Returns(Task.FromResult(new WiseRatesModel.RatesSearchResponse()));

						ObjectFactory.Substitute(mockFactory.Object);

						var autroRateMenu = shipmentForm.FindMenuItem_ForTest("Autorate Costs");
						AssertNoExceptionThrown(() => autroRateMenu.PerformClick());
					}
				}
			}
		}

		#region Error messages

		[GuiTest]
		public void TestSearchRate_WithoutRateSelector_Finds_CW1RateWithoutCurrencyExchangeRate_AutoratingHalts()
		{
			var org1 = Helper.NewOrgHeader("ORG1");

			var costing = Helper.NewCosting(org1);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");

			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry2.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("BAF", MinimumCalculator.Code, currencyCode: "EUR");
			var rateLine2 = rateEntry2.AddRateLine("CAF", MinimumCalculator.Code, currencyCode: "AUD");

			var rateLine1Calculator = rateLine1.GetCalculator<MinimumCalculator>();
			rateLine1Calculator.MinimumValue = 100;
			rateLine1Calculator.IsChargeCodeMinimum = true;

			var rateLine2Calculator = rateLine2.GetCalculator<MinimumCalculator>();
			rateLine2Calculator.MinimumValue = 200;
			rateLine2Calculator.IsJobMinimum = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = org1.PK;
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = org1.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_AWBServiceLevel = "STD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;

			Factory.Save();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutoCostAndAssert
				(
					"Expect a popup warning",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: false,
					autorateCosts: true,
					expectedErrors: new[] { @"Error Autorating has encountered an error:
Autorating cannot be performed as there are some rates in foreign currencies where no exchange rates have been specified." }
				);
			}
		}

		[GuiTest]
		public void TestSearchRate_WithoutRateSelector_Finds_CW1RateWithBogus_TM_Text_AutoratingHalts()
		{
			var org1 = Helper.NewOrgHeader("ORG1");

			var costing = Helper.NewCosting(org1);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AUSYD");
			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AUEC");

			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry3.RateLines.RemoveAndDeleteAll();

			rateEntry1.TI_OH_TransportProvider = org1.PK;

			var rateLine1 = rateEntry1.AddRateLine("BAF", MinimumCalculator.Code);
			var rateLine2 = rateEntry2.AddRateLine("CAF", MinimumCalculator.Code);
			var rateLine3 = rateEntry3.AddRateLine("WAR", MinimumCalculator.Code);

			// The Minimum calculator has String1 bound to TM_Text. It also controls the
			// minimum type. Whether Minimum-for-job or minimum-per-chargecode
			var rateLine1Calculator = rateLine1.GetCalculator<MinimumCalculator>();
			rateLine1Calculator.String1 = "???";
			rateLine1Calculator.MinimumValue = 100;

			var rateLine2Calculator = rateLine2.GetCalculator<MinimumCalculator>();
			rateLine2Calculator.MinimumValue = 200;
			rateLine2Calculator.IsChargeCodeMinimum = true;

			var rateLine3Calculator = rateLine3.GetCalculator<MinimumCalculator>();
			rateLine3Calculator.MinimumValue = 300;
			rateLine3Calculator.IsChargeCodeMinimum = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = org1.PK;
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = org1.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_AWBServiceLevel = "STD";

			Factory.Save();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutoCostAndAssert
				(
					"Expect a popup warning",
					null,
					Array.Empty<AssertionCost>(),
					consol,
					autorateRevenue: false,
					autorateCosts: true,
					expectedErrors: new[] { @"Error Autorating has encountered an error:
Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the 'BAF' Rate Line that uses a 'MIN' Calculator with currently an Apply To of '???' on Costing ORG1." }
				);
			}
		}

		#endregion

		#region Transport Bookings and Forwarding Shipments

		public void TestAutorateTransportBookings()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var consolidationBooking1 = Factory.New<IDtbBookingConsolidation>();
			var consolidationBooking2 = Factory.New<IDtbBookingConsolidation>();
			var consolidationBookingMulti = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking1.KB_ParentID = shipment1.PK;
			consolidationBooking2.KB_ParentID = shipment2.PK;
			consolidationBooking1.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			consolidationBooking2.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			consolidationBookingMulti.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var booking1 = Factory.New<IDtbBooking>();
			var booking2 = Factory.New<IDtbBooking>();
			var booking3 = Factory.New<IDtbBooking>();
			var booking4 = Factory.New<IDtbBooking>();
			booking1.KM_KB_Booking = consolidationBooking1.PK;
			booking2.KM_KB_Booking = consolidationBooking2.PK;

			booking1.KM_KB_BookingConsolidationMultiJob = consolidationBookingMulti.PK;
			booking2.KM_KB_BookingConsolidationMultiJob = consolidationBookingMulti.PK;
			booking3.KM_KB_BookingConsolidationMultiJob = consolidationBookingMulti.PK;
			booking4.KM_KB_BookingConsolidationMultiJob = consolidationBookingMulti.PK;

			job1.JH_ParentID = shipment1.PK;
			job2.JH_ParentID = shipment2.PK;
			job3.JH_ParentID = booking3.PK;
			job4.JH_ParentID = booking4.PK;

			AssertEquals(false, ((BusinessObject)booking1).IsRegisteredEditableChildObject(job1));
			AssertEquals(false, ((BusinessObject)booking2).IsRegisteredEditableChildObject(job2));
			AssertEquals(false, ((BusinessObject)booking3).IsRegisteredEditableChildObject(job3));
			AssertEquals(false, ((BusinessObject)booking4).IsRegisteredEditableChildObject(job4));

			using (var form = new ZForm(consolidationBookingMulti))
			{
				form.DisplayMode = ODisplayMode.Edit;
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.Show();

				using (var plugIn = (ApportionmentPlugin)form.PlugIns.GetPlugIn(ControllerIDs.Apportionment))
				{
					var autoRateCostsMenu = plugIn.TopLevelMenu.MenuItems.FindByText("Autorate Costs and Revenue");
					AssertNotNull(autoRateCostsMenu);

					autoRateCostsMenu.PerformClick();
					AssertEquals(true, ((BusinessObject)booking1).IsRegisteredEditableChildObject(job1));
					AssertEquals(true, ((BusinessObject)booking2).IsRegisteredEditableChildObject(job2));
					AssertEquals(true, ((BusinessObject)booking3).IsRegisteredEditableChildObject(job3));
					AssertEquals(true, ((BusinessObject)booking4).IsRegisteredEditableChildObject(job4));
				}
			}
		}

		public void TestAutorateTransportBookings_DoesNotClearChargesForOtherRatingAdapters()
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var chargeCodeFRT = Helper.ChargeCodes.New("TFRT", "Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var chargeCodeTBC = Helper.ChargeCodes.New("TBC", "Transport Fees", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			chargeCodeFRT.AC_AT_GSTRate = chargeCodeTBC.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRate = Helper.NewClientRate(Consignor);
			var freightRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ROA, "AU", "");
			var freightRateLine = freightRateEntry.AddRateLine(chargeCodeFRT, FlatCalculator.Code);
			freightRateLine.GetCalculator<FlatCalculator>().BaseRate = 250;

			var transportRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			var transportRateLine = transportRateEntry.AddRateLine(chargeCodeTBC, FlatCalculator.Code);
			transportRateLine.GetCalculator<FlatCalculator>().BaseRate = 100;

			var shipment = CreateForwardingShipment(TransportModes.Road, Consignor.PK, Consignee.PK, "AUSYD", "AUBTB", 20m);
			var booking1 = GetBooking(shipment, true);
			var booking2 = GetBooking(shipment, false);
			Factory.Save();

			var emptyArray = Array.Empty<string>();

			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			var jobInAnotherFactory = new JobHeader.Loader(reloadedShipment).TryCreateWithoutMutexForTestOnly();
			jobInAnotherFactory.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;
			newFactory.Save();

			var job = (Job)shipment.Job;

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();
				plugin.ExecuteAutorating(AutoRateOptions.AutorateRevenue);

				var message = "Autorating the Shipment should also autorate the related jobs (the charges for the attached transport bookings)";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 250
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 100
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 100
					},
				};

				AssertCharges(GetAutoRatingSummary(message, shipment, emptyArray, emptyArray), expected, job);
			}

			freightRateLine.GetCalculator<FlatCalculator>().BaseRate = 350;
			transportRateLine.GetCalculator<FlatCalculator>().BaseRate = 80;
			Factory.Save();

			void AssertJobFiltersCharges(string testMessage, DtbBooking booking)
			{
				job.JH_IsChargeCostReferenceFilterEnabled = false;
				AssertEquals("Pre-condition: autorated charges exist", 3, job.FilteredCharges.Count);

				job.JH_IsChargeCostReferenceFilterEnabled = true;

				var filteredCharge = job.FilteredCharges.Cast<Charge>().Single();
				AssertEquals(testMessage, booking.KM_JobID, filteredCharge.JR_CostReference);
				AssertEquals(testMessage, "Transport Fees {" + booking.KM_JobID + "}", filteredCharge.JR_Desc);
			}

			using (var plugin = new InvoicingPluginToFreight(booking1))
			{
				plugin.OnGUIShown();

				AssertJobFiltersCharges("Booking charges should be filtered despite autorating the shipment first", booking1);

				plugin.ExecuteAutorating(AutoRateOptions.AutorateRevenue);

				var message = "Since we're autorating from TB00000001, only the charges for that booking should be updated, the other booking and the shipment's rates should not change";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 250
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 80
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 100
					},
				};

				AssertCharges(GetAutoRatingSummary(message, shipment, emptyArray, emptyArray), expected, job);
				AssertJobFiltersCharges("After autorating for booking we should definitely be able to filter", booking1);
			}

			Factory.Save();

			using (var plugin = new InvoicingPluginToFreight(shipment))
			{
				plugin.OnGUIShown();
				plugin.ExecuteAutorating(AutoRateOptions.AutorateRevenue);

				var message = "AutoRating the shipment should autorate all the jobs again, all values now match the updated rates";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 350
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 80
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 80
					},
				};

				AssertCharges(GetAutoRatingSummary(message, shipment, emptyArray, emptyArray), expected, job);
			}

			Factory.Save();

			using (var plugin = new InvoicingPluginToFreight(booking1))
			{
				plugin.OnGUIShown();
				AssertJobFiltersCharges("Should be able to filter by booking", booking1);
			}

			using (var plugin = new InvoicingPluginToFreight(booking2))
			{
				plugin.OnGUIShown();
				AssertJobFiltersCharges("Should be able to filter by booking", booking2);
			}
		}

		public void TestAutorateTransportBookings_DoesNotClearChargesForOtherRatingAdapters2()
		{
			var chargeCodeFRT = Helper.ChargeCodes.New("TFRT", "Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var chargeCodeTBC = Helper.ChargeCodes.New("TBC", "Transport Fees", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			chargeCodeFRT.AC_AT_GSTRate = chargeCodeTBC.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRate = Helper.NewClientRate(Consignor);
			var freightRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ROA, "AU", "");
			var freightRateLine = freightRateEntry.AddRateLine(chargeCodeFRT, FlatCalculator.Code);
			freightRateLine.GetCalculator<FlatCalculator>().BaseRate = 250;

			var transportRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			var transportRateLine = transportRateEntry.AddRateLine(chargeCodeTBC, FlatCalculator.Code);
			transportRateLine.GetCalculator<FlatCalculator>().BaseRate = 100;

			var shipment = CreateForwardingShipment(TransportModes.Road, Consignor.PK, Consignee.PK, "AUSYD", "AUBTB", 20m);
			var booking1 = GetBooking(shipment, true);
			var booking2 = GetBooking(shipment, false);
			Factory.Save();

			using (var job = (Job)new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job.LocalChargesPK = Consignor.PK;

				var message = "Autorating the Shipment should also autorate the related children jobs";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 250
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 100
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 100
					},
				};

				AutorateAndAssert(message, expected, shipment, Consignor, job: job, autorateCosts: false);
				Assert("Pre-condition", !job.PlugInData.InvoicingSupporter.ShowOperationalJobRefFilter);
			}

			freightRateLine.GetCalculator<FlatCalculator>().BaseRate = 350;
			transportRateLine.GetCalculator<FlatCalculator>().BaseRate = 80;
			Factory.Save();

			using (var job = GetJobFromParent(shipment.PK))
			{
				var message = "Since we're autorating from TB00000001, only the charges for the transport booking should be updated";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 250
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 80
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 100
					},
				};

				AutorateAndAssert(message, expected, booking1, Consignor, job: job, autorateCosts: false);
				Assert("Pre-condition", job.PlugInData.InvoicingSupporter.ShowOperationalJobRefFilter);
				Assert("Job should not be filtered by default", !job.JH_IsChargeCostReferenceFilterEnabled);
				AssertEquals("Should include all three charges", 3, job.FilteredCharges.Count);

				job.JH_IsChargeCostReferenceFilterEnabled = true;
				AssertEquals("Filtering the charges should only show the charge for booking1", 1, job.FilteredCharges.Count);
				AssertEquals(booking1.KM_JobID, job.FilteredCharges[0].JR_CostReference);
			}

			using (var job = GetJobFromParent(shipment.PK))
			{
				var message = "AutoRating the shipment again should still autorated all child jobs";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "TFRT",
						JR_LocalSellAmt = 350
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 80
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 80
					},
				};

				AutorateAndAssert(message, expected, shipment, Consignor, job: job, autorateCosts: false);
			}
		}

		public void TestAutorateTransportBookings_DoesNotDisplayWarningWhenAutoratingAcrossAdapters()
		{
			var chargeCode = Helper.ChargeCodes.New("TBC", "Transport Fees", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUBTB", 20m);
			var booking1 = GetBooking(shipment, true);
			var booking2 = GetBooking(shipment, false);
			Factory.Save();

			using (var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job.LocalChargesPK = Consignee.PK;

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 100
					}
				};

				var expectedMessage = @"Information: AUTORATING REVENUE FOR Transport Booking TB00000001
Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 1
Information: RateLine Found TBC-FLT-Client Rate CONSIGNEE1
Information: CHARGES CALCULATED:
	TBC: Base Rate AUD 100.00
Information: Transport Booking TB00000001 was auto-rated.
	The following rates were found:
	  • TBC charge from Client Rate CONSIGNEE1
	Charges created: TBC";

				AutorateAndAssert(expected, booking1, Consignee, job: job, autorateCosts: false);
				AssertAutoratingAuditLogNoteContainsLines(booking1, "Expected a message", expectedMessage);

				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000001}",
						JR_LocalSellAmt = 100
					},
					new AssertionCharge
					{
						JR_Desc = "Transport Fees {TB00000002}",
						JR_LocalSellAmt = 100
					}
				};

				AutorateAndAssert("Should not clear charge for other transport booking", expected, booking2, Consignee, job: job, autorateCosts: false);

				var logNote = booking2.GetNotes()
					.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)[0]
					.ST_NoteData.ToUTF8();

				AssertNotContains("Autorating has produced new charges for the same charge codes as pre-existing charges which resulted in following charges conflicting", logNote);

				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		#endregion

		#region Security

		public void TestRatesSecurityStopsAutorating()
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");

			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(list));

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var sales = group.Staff.AddNew();
			sales.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			sales.GS_LoginName = "chuck norris";
			sales.GS_Code = "CN";

			var glbSecurity1 = Factory.New<GlbSecurity>();
			glbSecurity1.GU_SecurityItemIsAllowed = false;
			glbSecurity1.GU_SecurityRight = Env.Security.RatesSecurity.Code;
			glbSecurity1.GU_GS = sales.PK;

			var glbSecurity2 = Factory.New<GlbSecurity>();
			glbSecurity2.GU_SecurityItemIsAllowed = false;
			glbSecurity2.GU_SecurityRight = Env.Security.RatesSecurity.Code + "XYZ";
			glbSecurity2.GU_GS = sales.PK;

			Consignor.OH_IsDebtor = true;
			Consignee.OH_IsDebtor = true;
			Consignee.OH_Code = "CNESYD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(sales.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;

				var securityABC = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "ABC");
				Assert("Just to check security is found and working fine", !securityABC.IsAllowed);
				Assert(Env.Security.RatesSecurity.IsAncestorOf(securityABC));

				Factory.Save();

				var rateCNE = Helper.NewClientRate(Consignee);
				var entryCNE = rateCNE.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
				entryCNE.RateLines.RemoveAndDeleteAll();
				var lineCNE = entryCNE.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
				((UnitCalculator)lineCNE.Calculator).PerUnit = 15m;

				var rateCNR = Helper.NewClientRate(Consignor);
				var entryCNR = rateCNR.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUBNE", "USLAX");
				entryCNR.RateLines.RemoveAndDeleteAll();
				var lineCNR = entryCNR.AddRateLine("ODOC", FlatCalculator.Code);
				((FlatCalculator)lineCNR.Calculator).BaseRate = 15m;

				Factory.Save();

				var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "USLAX", 500);
				shipment.JS_INCO = "FOB";

				var helper = new JobInvoicingSecurityHelper(() => (shipment as IJobInvoicingPlugIn).InvoicingSupporter.JobInvoicingSecurity);
				helper.GetInvSecurity(SecurityCore.AutoRateRevenue).IsAllowed = true;
				helper.GetInvSecurity(SecurityCore.AutoRateCost).IsAllowed = true;

				var expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_Desc = "International Freight",
								JR_LocalSellAmt = 7500,
								JR_RX_NKSellCurrency = "AUD",
							},
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								JR_Desc = "Origin Documentation Fee",
								JR_LocalSellAmt = 15m,
								JR_RX_NKSellCurrency = "AUD",
							},
					};

				AutorateAndAssert("Precondition - charges from both client rates should autorate. Otherwise the below wouldn't make sense.", expected, shipment, Consignor);

				(shipment.Job as Job).Charges.RemoveAndDeleteAll();
				Consignor.CompanyData.OB_RateSecurityGroup = "ABC";
				Factory.Save();

				// Reloading shipment in new Factory because of security settings' caching.
				var shipmentReloaded = Helper.LoadInNewFactory(shipment);

				AutorateAndAssert(Array.Empty<AssertionCharge>(), shipmentReloaded, Consignor);

				var previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
				var expectedError = securityABC.ErrorMessageForNotAllowed.Replace("\r\n\r\n", "\r\n\t");
				Assert("Denied as the Local Client on the job is prohibited", previousMessages.ContainsMessageContainingThisText(expectedError));

				Consignor.CompanyData.OB_RateSecurityGroup = "XXX";
				glbSecurity1.GU_SecurityItemIsAllowed = true;
				Factory.Save();
				securityABC = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "ABC");
				Assert("Just to check security is found and working fine", securityABC.IsAllowed);
				Assert(Env.Security.RatesSecurity.IsAncestorOf(securityABC));

				AutorateAndAssert(expected, shipment, Consignor);
				(shipment.Job as Job).Charges.RemoveAndDeleteAll();

				Consignee.CompanyData.OB_RateSecurityGroup = "XYZ";
				Factory.Save();

				// Reloading shipment in new Factory because of security settings' caching.
				shipmentReloaded = Helper.LoadInNewFactory(shipment);

				AutorateAndAssert(Array.Empty<AssertionCharge>(), shipmentReloaded, Consignor);

				var securityCheckPointMessage = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "XYZ").ErrorMessageForNotAllowed;
				var expectedMessage = $@"Autorating Canceled: AutoRating has encountered XYZ rates for CNESYD.
	{securityCheckPointMessage.Replace("\r\n\r\n", "\r\n\t")}
Please refer to Notes->AutoRating Log for more information.";
				previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
				Assert("Denied due to prohibited rates encountered", previousMessages.ContainsMessageContainingThisText(expectedMessage));
			}
		}

		#endregion

		#region Operational Actions

		[TestDate(2015, 09, 01)]
		public void TestAutoRatingActionMethodApplicatorLog()
		{
			using (SetJR_DescDisplayAllText())
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org1.OH_IsDebtor = true;
				org2.OH_IsDebtor = true;

				Factory.Save();

				var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

				var clientRate = Helper.NewClientRate(org1);
				var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUBNE", "USLAX");
				entry1.TI_RC = gp20.PK;
				entry1.RateLines.RemoveAndDeleteAll();
				var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
				line1.GetCalculator<UnitCalculator>().PerUnit = 2000m;

				var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.SOR, RateMode.FCL, "AUBNE", "USLAX");
				entry2.TI_RC = gp20.PK;
				entry2.RateLines.RemoveAndDeleteAll();
				var line2 = entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN);
				line2.GetCalculator<UnitCalculator>().PerUnit = 200m;

				var bill1 = Factory.New<BillOfLading>();
				var bill2 = Factory.New<BillOfLading>();
				var bill3 = Factory.New<BillOfLading>();

				bill1.JS_UniqueConsignRef = "V00001011";
				bill2.JS_UniqueConsignRef = "V00001012";
				bill3.JS_UniqueConsignRef = "V00001013";

				bill1.JS_RL_NKOrigin = "AUBNE";
				bill2.JS_RL_NKOrigin = "AUSYD";
				bill3.JS_RL_NKOrigin = "AUBNE";

				bill1.JS_RL_NKDestination = "USLAX";
				bill2.JS_RL_NKDestination = "CNSHA";
				bill3.JS_RL_NKDestination = "USLAX";

				bill1.ConsignorPK = org1.PK;
				bill2.ConsignorPK = org1.PK;
				bill3.ConsignorPK = org1.PK;

				bill1.ConsigneePK = org2.PK;
				bill2.ConsigneePK = org2.PK;
				bill3.ConsigneePK = org2.PK;

				var container1 = bill1.ShippingContainers.AddNew();
				var container2 = bill2.ShippingContainers.AddNew();
				var container3 = bill3.ShippingContainers.AddNew();

				container1.JC_RC = gp20.PK;
				container2.JC_RC = gp20.PK;
				container3.JC_RC = gp20.PK;

				Factory.Save();

				using (var job1 = new Job.Loader(bill1).TryLoadOrCreateWithoutMutexForTestOnly())
				using (var job2 = new Job.Loader(bill2).TryLoadOrCreateWithoutMutexForTestOnly())
				using (var job3 = new Job.Loader(bill3).TryLoadOrCreateWithoutMutexForTestOnly())
				using (job1.GetValidationSuspender())
				using (job2.GetValidationSuspender())
				using (job3.GetValidationSuspender())
				{
					job1.AddExRate("USD", 1.05m);
					job2.AddExRate("USD", 1.05m);
					job3.AddExRate("USD", 1.05m);

					using (var plugin1 = new InvoicingPluginToFreight(bill1))
					using (var plugin2 = new InvoicingPluginToFreight(bill2))
					using (var plugin3 = new InvoicingPluginToFreight(bill3))
					{
						plugin1.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
						plugin2.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
						plugin3.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

						Factory.Save();
					}

					var emptyCharges = Array.Empty<AssertionCharge>();

					var expectedIntialCharges = new[]
						{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_Desc = "International Freight - 1 20GP Container(s) @ USD 2000.00/Container",
								JR_LocalSellAmt = 1904.76,
								JR_OSSellAmt = 2000,
								JR_RX_NKSellCurrency = "USD",
							},
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								JR_Desc = "Origin Documentation Fee - 1 20GP Container(s) @ AUD 200.00/Container",
								JR_LocalSellAmt = 200m,
								JR_OSSellAmt = 200m,
								JR_RX_NKSellCurrency = "AUD",
							},
					};

					AssertCharges(expectedIntialCharges, job1);
					AssertCharges("Expected no charges", emptyCharges, job2);
					AssertCharges("Expected the same two charges as job1 as job3 matches the same criteria", expectedIntialCharges, job3);

					var charge31 = job3.Charges.Cast<Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "FRT");
					var charge32 = job3.Charges.Cast<Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "ODOC");

					charge31.JR_OSSellAmt = 2100;
					charge32.JR_OSSellAmt = 199m;

					line1.GetCalculator<UnitCalculator>().PerUnit = 1900m;
					line2.GetCalculator<UnitCalculator>().PerUnit = 190m;

					Factory.Save();

					var actualLog = new DummyOperationalActionSectionLog();
					var applicator = new AutoRatingActionMethodApplicator(Factory);
					Assert("Currently AutoRatingApplicator doesn't support summary. If it starts doing so then probably the test will need to be revisited a bit.", !applicator.SupportsSummary);
					applicator.InitialiseBeforeIndividiualBatchRun();
					applicator.Apply(actualLog, new[] { bill1, bill2, bill3 });

					var expectedLog = new[]
						{ @"INFO: [HL Shipping Bill of Lading V00001011]: Shipping Bill of Lading V00001011 was auto-costed.
	No costs were found.",
						@"WARNING: [HL Shipping Bill of Lading V00001011]: Warnings Encountered:
Shipping Bill of Lading V00001011 was auto-costed.
	No costs were found.",
						@"INFO: [HL Shipping Bill of Lading V00001012]: Shipping Bill of Lading V00001012 was auto-costed.
	No costs were found.",
						@"WARNING: [HL Shipping Bill of Lading V00001012]: Warnings Encountered:
Shipping Bill of Lading V00001012 was auto-costed.
	No costs were found.",
						@"INFO: [HL Shipping Bill of Lading V00001013]: Shipping Bill of Lading V00001013 was auto-costed.
	No costs were found.",
						@"WARNING: [HL Shipping Bill of Lading V00001013]: Warnings Encountered:
Shipping Bill of Lading V00001013 was auto-costed.
	No costs were found.",
						@"WARNING: [HL Shipping Bill of Lading V00001012]: Warnings Encountered:
Shipping Bill of Lading V00001012 was auto-rated.
	No rates were found.",
						@"INFO: [HL Shipping Bill of Lading V00001012]: Shipping Bill of Lading V00001012 was auto-rated.
	No rates were found."
					};

					var actual = actualLog.messages.Select(x => x.Trim());
					AssertContainsExactElementsInAnyOrder("", expectedLog, actual);

					var expectedJob1Charges = new[]
						{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_Desc = "International Freight - 1 20GP Container(s) @ USD 1900.00/Container",
								JR_LocalSellAmt = 1809.52m,
								JR_OSSellAmt = 1900m,
								JR_RX_NKSellCurrency = "USD",
							},
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								JR_Desc = "Origin Documentation Fee - 1 20GP Container(s) @ AUD 190.00/Container",
								JR_LocalSellAmt = 190m,
								JR_RX_NKSellCurrency = "AUD",
							},
					};

					var expectedJob3Charges = new[]
						{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_Desc = "International Freight - 1 20GP Container(s) @ USD 2000.00/Container",
								JR_LocalSellAmt = 2000m,
								JR_OSSellAmt = 2100m,
								JR_RX_NKSellCurrency = "USD",
								JR_SellRatingOverride = true,
							},
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								JR_Desc = "Origin Documentation Fee - 1 20GP Container(s) @ AUD 200.00/Container",
								JR_LocalSellAmt = 199m,
								JR_OSSellAmt = 199m,
								JR_RX_NKSellCurrency = "AUD",
								JR_SellRatingOverride = true,
							},
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_Desc = "International Freight - 1 20GP Container(s) @ USD 1900.00/Container",
								JR_LocalSellAmt = 1809.52m,
								JR_OSSellAmt = 1900m,
								JR_RX_NKSellCurrency = "USD",
							},
						new AssertionCharge
							{
								ChargeCode = "ODOC",
								JR_Desc = "Origin Documentation Fee - 1 20GP Container(s) @ AUD 190.00/Container",
								JR_LocalSellAmt = 190m,
								JR_OSSellAmt = 190m,
								JR_RX_NKSellCurrency = "AUD",
							},
					};

					AssertCharges("reautorated charge", expectedJob1Charges, job1);
					AssertCharges("still should find no charges", emptyCharges, job2);
					AssertCharges("reautorate attempt failed because rating has been manualy overridden", expectedJob3Charges, job3);
				}
			}
		}

		public void TestAutoRateCostsRevenueActionMethod()
		{
			var shipment = GetShipmentWithMatchingCostAndClientRate(Consignee, Consignor, TransportProvider1, "AUSYD", "HKHKG", "FRT", 200m, 100m);
			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var log = new DummyOperationalActionSectionLog();
				var applicator = new AutoRatingActionMethodApplicator(Factory);
				applicator.InitialiseBeforeIndividiualBatchRun();
				applicator.Apply(log, new[] { shipment });

				var expectedJobCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 100m,
						JR_LocalSellAmt = 200m,
						JR_CostRated = true,
						JR_SellRated = true
					}
				};
				AssertCharges("Expected both costs and revenue to be autorated", expectedJobCharges, job);
			}
		}

		public void TestAutoRateCostsActionMethod()
		{
			var shipment = GetShipmentWithMatchingCostAndClientRate(Consignee, Consignor, TransportProvider1, "AUSYD", "HKHKG", "FRT", 200m, 100m);
			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var log = new DummyOperationalActionSectionLog();
				var applicator = new AutoRatingActionMethodApplicator(Factory, true, false);
				applicator.InitialiseBeforeIndividiualBatchRun();
				applicator.Apply(log, new[] { shipment });

				var expectedJobCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 100m,
						JR_LocalSellAmt = 100m,
						JR_CostRated = true,
						JR_SellRated = false
					}
				};

				AssertCharges("Expected only costs to be autorated", expectedJobCharges, job);
			}
		}

		public void TestAutoRateRevenueActionMethod()
		{
			var shipment = GetShipmentWithMatchingCostAndClientRate(Consignee, Consignor, TransportProvider1, "AUSYD", "HKHKG", "FRT", 200m, 100m);
			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var log = new DummyOperationalActionSectionLog();
				var applicator = new AutoRatingActionMethodApplicator(Factory, false);
				applicator.InitialiseBeforeIndividiualBatchRun();
				applicator.Apply(log, new[] { shipment });

				var expectedJobCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 200m,
						JR_LocalSellAmt = 200m,
						JR_CostRated = false,
						JR_SellRated = true
					}
				};
				AssertCharges("Expected only revenue to be autorated", expectedJobCharges, job);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateCostsRevenueActionMethod_HandlesConsolCostWarnings()
		{
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var costing = Helper.NewCosting(null);
				var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "GB");
				costEntry.RateLines.RemoveAndDeleteAll();
				var costLine = costEntry.AddRateLine("FRT", FlatCalculator.Code);
				costLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
				costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C0010022016";
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.LCL;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "GBSUN";
				consol.Transports[0].JW_IsLinked = false;
				consol.JK_PrepaidCollect = PaymentType.Prepaid;

				var shipment1 = consol.Shipments.AddNew();
				var shipment2 = consol.Shipments.AddNew();
				var job1 = new JobHeader.Loader(shipment1).TryCreate();
				var job2 = new JobHeader.Loader(shipment2).TryCreate();
				job1.JH_GE = ImportAirDepartment.PK;
				job2.JH_GE = ImportAirDepartment.PK;
				job1.LocalChargesPK = NewClient.PK;
				job2.LocalChargesPK = NewClient.PK;

				Factory.Save();

				var log = GetNewLogForConsol(consol);

				var expectedCosts = new List<AssertionCost>
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 100m
					}
				};

				AssertNotContains("Autorating should be completed successfully", "ERROR:", log.MessagesString());
				AssertCosts("Correct consol costs should have been autorated", consol, expectedCosts);
				consol.GetApportionments().CostsCollection[0].E6_RatingBehaviour = "NEW";

				log = GetNewLogForConsol(consol);

				expectedCosts.Add(new AssertionCost { ChargeCode = "FRT", E6_OSCostAmount = 100m });

				AssertContains("Autorating should have completed with the following warning", "Charge Code: There are more than one unposted charges with the same charge code, you are advised to review them before posting", log.MessagesString());
				AssertCosts("Correct consol costs should have been autorated", consol, expectedCosts);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoRateCostsRevenueActionMethod_HandlesConsolCostErrors()
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "GB");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine("FRT", FlatCalculator.Code);
			costLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0010022016";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBSUN";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var applicator = new AutoRatingActionMethodApplicator(Factory, true, false);
			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(log, new[] { consol });

			DeleteExistingCosts(consol.PK);

			var expectedMessage = @"ERROR: [HL Consol C0010022016]: Consol C0010022016 has encountered the following errors while AutoRating:
	•  Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.
	•  Exchange Rate: Please enter an Exchange Rate.
	•  Exchange Rate: Exchange Rate for Currency USD must be greater than 0.
	•  Local Cost Amount: Please enter a Local Cost Amount.";

			var actualMessage = log.MessagesString();

			AssertContains("Consol has no shipments so cannot be apportioned as well as being in USD with no currency conversion", expectedMessage, actualMessage);

			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job1.JH_GE = ImportAirDepartment.PK;
			job2.JH_GE = ImportAirDepartment.PK;
			job1.LocalChargesPK = NewClient.PK;
			job2.LocalChargesPK = NewClient.PK;

			Factory.Save();

			log = new DummyOperationalActionSectionLog();
			applicator = new AutoRatingActionMethodApplicator(Factory, true, false);
			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(log, new[] { consol });

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m
				}
			};

			AssertNotContains("Autorating should be completed successfully", "ERROR:", log.MessagesString());
			AssertCosts("Errors have been fixed so consol cost should be autorated", consol, expectedCosts);
		}

		[TestDate(2016, 12, 12)]
		public void TestAutoRatingLogForJobChargeHasError()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = org1.PK;
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUBNE", "USLAX", "", "20GP");
			entry1.RateLines.RemoveAndDeleteAll();

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			chargeCode.AC_DepartmentFilterList = "BRN";
			var line = entry1.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V00001011";
			bill1.JS_RL_NKOrigin = "AUBNE";
			bill1.JS_RL_NKDestination = "USLAX";
			bill1.ConsignorPK = org1.PK;
			bill1.ConsigneePK = org2.PK;

			var container1 = bill1.ShippingContainers.AddNew();
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.JC_RC = gp20.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(bill1).TryLoadOrCreate() as Job)
			{
				job1.AddExRate("USD", 1.05m);
				var actualLog = new DummyOperationalActionSectionLog();
				var applicator = new AutoRatingActionMethodApplicator(Factory);
				applicator.InitialiseBeforeIndividiualBatchRun();
				applicator.Apply(actualLog, new[] { bill1 });

				var expectedLogMessage = @"INFO: [HL Shipping Bill of Lading V00001011]: Shipping Bill of Lading V00001011 was auto-costed.
	No costs were found.
WARNING: [HL Shipping Bill of Lading V00001011]: Warnings Encountered:
Shipping Bill of Lading V00001011 was auto-costed.
	No costs were found.
ERROR: [HL Shipping Bill of Lading V00001011]: V00001011 has encountered the following errors while AutoRating:
	•  Department: Cannot issue job charges for a miscellaneous department.
	•  Invoice Type: Please enter an Invoice Type.";

				var actualLogMessage = string.Join(System.Environment.NewLine, actualLog.messages);
				AssertContains("Expected but did not find:" + expectedLogMessage, expectedLogMessage, actualLogMessage);
			}
		}

		[TestDate(2018, 01, 11)]
		public void TestCanAddCostWhenAutorating()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CCOST", "Consol Cost", "", ChargeCodeGroupList.Codes.Freight);
			var chargeCode2 = Helper.ChargeCodes["WAR"];
			var refContainerPK = Helper.Containers["20GP"].PK;

			var costing = Helper.NewCosting(null);
			var roadCostEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUMEL", "AUSYD");
			roadCostEntry.RateLines.RemoveAndDeleteAll();
			roadCostEntry.TI_RC = refContainerPK;
			var roadCostLine = roadCostEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			roadCostLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var seaCostEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "SGSIN");
			seaCostEntry.RateLines.RemoveAndDeleteAll();
			seaCostEntry.TI_ViaLRC = "NZAKL";
			seaCostEntry.TI_RC = refContainerPK;
			var seaCostLine = seaCostEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			seaCostLine.GetCalculator<UnitCalculator>().PerUnit = 250m;

			var seaCostLine2 = seaCostEntry.AddRateLine(chargeCode2, FlatCalculator.Code, "", CurrencyCodes.Australia);
			seaCostLine2.GetCalculator<FlatCalculator>().BaseRate = 80m;

			var seaCostLine3 = seaCostEntry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.HB, CurrencyCodes.Australia);
			seaCostLine3.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_UniqueConsignRef = "C00001213";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = refContainerPK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var t1 = consol.Transports[0];
			t1.JW_IsLinked = false;
			t1.JW_VoyageFlight = "VRRRRMM";
			t1.JW_RL_NKLoadPort = "AUMEL";
			t1.JW_RL_NKDiscPort = "AUSYD";
			t1.JW_ETD = ZDateTime.Now;
			t1.JW_ETA = ZDateTime.Now.AddDays(1);
			t1.JW_TransportMode = TransportModes.Road;
			t1.JW_TransportType = TransportPlanningType.PreCarriage;
			t1.JW_CarrierBookingReference = "Route1";

			var t2 = consol.Transports.AddNew("AUSYD", "NZAKL");
			t2.JW_IsLinked = false;
			t2.JW_VoyageFlight = "123S";
			t2.JW_ETD = ZDateTime.Now.AddDays(2);
			t2.JW_ETA = ZDateTime.Now.AddDays(4);
			t2.JW_TransportMode = TransportModes.Sea;
			t2.JW_TransportType = TransportPlanningType.MainVessel;

			var t3 = consol.Transports.AddNew("NZAKL", "SGSIN");
			t3.JW_IsLinked = false;
			t3.JW_VoyageFlight = "123S";
			t3.JW_ETD = ZDateTime.Now.AddDays(5);
			t3.JW_ETA = ZDateTime.Now.AddDays(7);
			t3.JW_TransportMode = TransportModes.Sea;
			t3.JW_TransportType = TransportPlanningType.Other;

			var shipment = consol.Shipments.AddNew();
			container.PackLines.Add(shipment.OuterPackLines.AddNew());
			consol.Shipments.AddNew();
			Factory.Save();

			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new AutoRatingActionMethodApplicator(Factory);

			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(actualLog, new[] { consol });

			var expectedLog = new[]
			{
				@"ERROR: [HL Consol C00001213]: Shipment S00001000 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Please enter a Department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep.",
				@"ERROR: [HL Consol C00001213]: Shipment S00001001 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Please enter a Department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep."
			};

			var actual = actualLog.messages.Select(x => x.Trim());
			AssertContainsExactElementsInAnyOrder("AutoRating via Operational actions should fail costs produced have errors and we should not save bizObjs with errors", expectedLog, actual);

			var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			AssertEquals("Should never save Consol Costs with red validation errors so operational actions autorating should fail", 0, costs.Length);

			var routeSetNumberDescription = DescriptionHelpers.FormatWithTab("Route Set Number:");
			var expected = new[]
			{
				new AssertionCost { ChargeCode = chargeCode.AC_Code, E6_OSCostAmount = 100m, CostCalculationDescription = $"{routeSetNumberDescription}1" },
				new AssertionCost { ChargeCode = chargeCode.AC_Code, E6_OSCostAmount = 250m, CostCalculationDescription = $"{routeSetNumberDescription}2" },
				new AssertionCost { ChargeCode = chargeCode2.AC_Code, E6_OSCostAmount = 280m }
			};

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("Costs should be added, even though we are creating costs with the same charge code on consolidation", null, expected, consol, false);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAutoratingConsolWithErrorsViaOperationalActions()
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "ZA", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = costEntry.AddRateLine("FRT", FlatCalculator.Code, currencyCode: CurrencyCodes.UnitedStates);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 1;

			var rateLine2 = costEntry.AddRateLine("DOF", FlatCalculator.Code, currencyCode: CurrencyCodes.SouthAfrica);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 2;

			var shipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "ZAJNB", "AUSYD", 100m);
			var shipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "ZAJNB", "AUSYD", 130m);
			var consol = CreateForwardingConsol(TransportModes.Air, "ZAJNB", "AUSYD", TransportProvider1, shipment1, PaymentType.Prepaid);
			consol.Shipments.Add(shipment2);

			consol.JK_UniqueConsignRef = "C00001213";
			shipment1.JS_UniqueConsignRef = "S00001000";
			shipment2.JS_UniqueConsignRef = "S00001001";

			var job1 = new JobHeader.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var job2 = new JobHeader.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = ImportAirDepartment.PK;
			job2.JH_GE = ImportAirDepartment.PK;
			job1.LocalChargesPK = Consignee.PK;
			job2.LocalChargesPK = Consignee.PK;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			var log = new DummyOperationalActionSectionLog();
			var applicator = new AutoRatingActionMethodApplicator(Factory, true, true);
			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(log, new BusinessObject[] { consol });

			var expected = @"ERROR: [HL Consol C00001213]: Consol C00001213 has encountered the following errors while AutoRating:
	•  Local Cost Amount: Local amount cannot be zero when Overseas Cost Amount is non zero.
	•  Cost Exchange Rate: Cost Exchange Rate for Currency USD must be greater than 0.
	•  Exchange Rate: Please enter an Exchange Rate.
	•  Exchange Rate: Exchange Rate for Currency USD must be greater than 0.
	•  Local Cost Amount: Please enter a Local Cost Amount.";
			var actualMessage = log.MessagesString();
			AssertContains(expected, actualMessage);

			var userNotificationText = UnitTestUserNotification.Instance.LastMessage.Text;
			Assert("Should not have any pop-up notificaitons for operational actions", userNotificationText.IsNullOrEmpty());
		}

		#endregion

		#region Equalization Calculator

		BusinessObject[] SetupEqualizationCostAndObjects_AIR(ZDecimal pivotBreak1, ZDecimal pivotBreak2, ZDecimal pivotBreak3)
		{
			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			#region Setup Equalization Rate July 1-15

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsShippingProvider = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			#region Cost for LD-8 Containers

			var costEntryLD8 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "", "LD-8");
			costEntryLD8.TI_RX_NKCurrency = "AUD";
			costEntryLD8.TI_ContractNumber = "010001";
			costEntryLD8.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntryLD8.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntryLD8.RateLines.RemoveAndDeleteAll();
			var rateLineLD8 = costEntryLD8.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG);
			var ld8plusRateLineItem = rateLineLD8.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			ld8plusRateLineItem.TM_Break = pivotBreak1;
			ld8plusRateLineItem.TM_FlatAmount = 15m;
			ld8plusRateLineItem.TM_RelevantValue = 20m;

			var rateLineLD82 = costEntryLD8.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG);
			var ld82plusRateLineItem = rateLineLD82.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			ld82plusRateLineItem.TM_Break = pivotBreak3;
			ld82plusRateLineItem.TM_FlatAmount = 10m;
			ld82plusRateLineItem.TM_RelevantValue = 10m;
			#endregion

			#region Cost for PA-5 Containers

			var costEntryPA5 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "", "PA-5");
			costEntryPA5.TI_RX_NKCurrency = "AUD";
			costEntryPA5.TI_ContractNumber = "010001";
			costEntryPA5.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntryPA5.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntryPA5.RateLines.RemoveAndDeleteAll();
			var rateLinePA5 = costEntryPA5.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG);
			var pa5plusRateLineItem = rateLinePA5.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			pa5plusRateLineItem.TM_Break = pivotBreak2;
			pa5plusRateLineItem.TM_FlatAmount = 10m;
			pa5plusRateLineItem.TM_RelevantValue = 10m;
			var pa5minusRateLineItem = rateLinePA5.RateLineItems.AddNew();
			pa5minusRateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			pa5minusRateLineItem.TM_Break = pivotBreak2;
			pa5minusRateLineItem.TM_FlatAmount = 30m;
			pa5minusRateLineItem.TM_RelevantValue = 30m;

			#endregion

			Factory.Save();

			#endregion

			var lD8_Container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-8").PK;
			var pA5_Container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "PA-5").PK;

			#region Consol Shipped July 2nd - 2600 x 2 x LD8 and 200 x PA5

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = TransportModes.Air;
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_AWBServiceLevel = "STD";
			consol1.JK_ConsolMode = ContainerModes.ULD;
			consol1.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol1.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";
			consol1.JK_PrepaidCollect = PaymentType.Prepaid;
			consol1.JK_UniqueConsignRef = "C00001";
			consol1.JK_CarrierContractNumber = "010001";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var contLD8c1_1 = consol1.Containers.AddNew();
			contLD8c1_1.JC_RC = lD8_Container;
			var contLD8c1_2 = consol1.Containers.AddNew();
			contLD8c1_2.JC_RC = lD8_Container;
			var contPA5c1_1 = consol1.Containers.AddNew();
			contPA5c1_1.JC_RC = pA5_Container;

			var s1c1 = consol1.Shipments.AddNew();
			s1c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c1.JS_TransportMode = TransportModes.Air;
			s1c1.JS_PackingMode = ContainerModes.ULD;
			s1c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c1.JS_RL_NKOrigin = "AUSYD";
			s1c1.JS_RL_NKDestination = "USLAX";
			s1c1.JS_INCO = "CFR";
			s1c1.JS_OH_DeliveryAgent = consignee.PK;
			s1c1.JS_ActualVolume = 1;
			s1c1.ConsignorPK = consignor.PK;
			s1c1.ConsigneePK = consignee.PK;
			s1c1.JS_E_DEP = new ZDate(2015, 7, 2);
			s1c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);

			var plc1s1_1 = s1c1.OuterPackLines.AddNew();
			plc1s1_1.JL_ActualWeight = 1000m;
			plc1s1_1.JL_JC = contLD8c1_1.PK;

			var plc1s1_2 = s1c1.OuterPackLines.AddNew();
			plc1s1_2.JL_ActualWeight = 800m;
			plc1s1_2.JL_JC = contLD8c1_2.PK;

			var plc1s1_3 = s1c1.OuterPackLines.AddNew();
			plc1s1_3.JL_ActualWeight = 200m;
			plc1s1_3.JL_JC = contPA5c1_1.PK;

			var s2c1 = consol1.Shipments.AddNew();
			s2c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c1.JS_TransportMode = TransportModes.Air;
			s2c1.JS_PackingMode = ContainerModes.ULD;
			s2c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c1.JS_RL_NKOrigin = "AUSYD";
			s2c1.JS_RL_NKDestination = "USLAX";
			s2c1.JS_INCO = "CFR";
			s2c1.JS_OH_DeliveryAgent = consignee.PK;
			s2c1.ConsignorPK = consignor.PK;
			s2c1.ConsigneePK = consignee.PK;
			s2c1.JS_E_DEP = new ZDate(2015, 7, 2);
			s2c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);
			s2c1.JS_ActualVolume = 2;

			var plc1s2_1 = s2c1.OuterPackLines.AddNew();
			plc1s2_1.JL_ActualWeight = 800m;
			plc1s2_1.JL_JC = contLD8c1_1.PK;

			#endregion

			#region Consol Shipped July 5th - 1800 x 2 x LD8 and 600 x PA5

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_AWBServiceLevel = "STD";
			consol2.JK_ConsolMode = ContainerModes.ULD;
			consol2.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol2.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;
			consol2.JK_UniqueConsignRef = "C00002";
			consol2.JK_CarrierContractNumber = "010001";

			var contLD8c2_1 = consol2.Containers.AddNew();
			contLD8c2_1.JC_RC = lD8_Container;
			var contLD8c2_2 = consol2.Containers.AddNew();
			contLD8c2_2.JC_RC = lD8_Container;
			var contPA5c2_1 = consol2.Containers.AddNew();
			contPA5c2_1.JC_RC = pA5_Container;

			var s1c2 = consol2.Shipments.AddNew();
			s1c2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c2.JS_TransportMode = TransportModes.Air;
			s1c2.JS_PackingMode = ContainerModes.ULD;
			s1c2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c2.JS_RL_NKOrigin = "AUSYD";
			s1c2.JS_RL_NKDestination = "USLAX";
			s1c2.JS_INCO = "CFR";
			s1c2.JS_OH_DeliveryAgent = consignee.PK;
			s1c2.JS_ActualVolume = 1;
			s1c2.ConsignorPK = consignor.PK;
			s1c2.ConsigneePK = consignee.PK;
			s1c2.JS_E_DEP = new ZDate(2015, 7, 5);
			s1c2.JS_E_ARV = new ZDateTime(2015, 7, 5, 6, 0, 0);

			var plc2s1_1 = s1c2.OuterPackLines.AddNew();
			plc2s1_1.JL_ActualWeight = 500m;
			plc2s1_1.JL_JC = contLD8c2_1.PK;

			var plc2s1_2 = s1c2.OuterPackLines.AddNew();
			plc2s1_2.JL_ActualWeight = 500m;
			plc2s1_2.JL_JC = contLD8c2_2.PK;

			var plc2s1_3 = s1c2.OuterPackLines.AddNew();
			plc2s1_3.JL_ActualWeight = 600m;
			plc2s1_3.JL_JC = contPA5c2_1.PK;

			var s2c2 = consol2.Shipments.AddNew();
			s2c2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c2.JS_TransportMode = TransportModes.Air;
			s2c2.JS_PackingMode = ContainerModes.ULD;
			s2c2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c2.JS_RL_NKOrigin = "AUSYD";
			s2c2.JS_RL_NKDestination = "USLAX";
			s2c2.JS_INCO = "CFR";
			s2c2.JS_OH_DeliveryAgent = consignee.PK;
			s2c2.ConsignorPK = consignor.PK;
			s2c2.ConsigneePK = consignee.PK;
			s2c2.JS_E_DEP = new ZDate(2015, 7, 5);
			s2c2.JS_E_ARV = new ZDateTime(2015, 7, 5, 6, 0, 0);
			s2c2.JS_ActualVolume = 2;

			var plc2s2_1 = s2c2.OuterPackLines.AddNew();
			plc2s2_1.JL_ActualWeight = 800m;
			plc2s2_1.JL_JC = contLD8c2_1.PK;

			#endregion

			#region Consol Shipped July 10th - 1400 x LD8 and 700 x PA5

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = TransportModes.Air;
			consol3.JK_AgentType = AgentType.Agent;
			consol3.JK_AWBServiceLevel = "STD";
			consol3.JK_ConsolMode = ContainerModes.ULD;
			consol3.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol3.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol3.JK_RL_NKLoadPort = "AUSYD";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_PrepaidCollect = PaymentType.Prepaid;
			consol3.JK_UniqueConsignRef = "C00003";
			consol3.JK_CarrierContractNumber = "010001";

			var contLD8c3 = consol3.Containers.AddNew();
			contLD8c3.JC_RC = lD8_Container;
			var contPA5c3 = consol3.Containers.AddNew();
			contPA5c3.JC_RC = pA5_Container;

			var s1c3 = consol3.Shipments.AddNew();
			s1c3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c3.JS_TransportMode = TransportModes.Air;
			s1c3.JS_PackingMode = ContainerModes.ULD;
			s1c3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c3.JS_RL_NKOrigin = "AUSYD";
			s1c3.JS_RL_NKDestination = "USLAX";
			s1c3.JS_INCO = "CFR";
			s1c3.JS_OH_DeliveryAgent = consignee.PK;
			s1c3.JS_ActualVolume = 1;
			s1c3.ConsignorPK = consignor.PK;
			s1c3.ConsigneePK = consignee.PK;
			s1c3.JS_E_DEP = new ZDate(2015, 7, 10);
			s1c3.JS_E_ARV = new ZDateTime(2015, 7, 10, 6, 0, 0);

			var plc3s1_1 = s1c3.OuterPackLines.AddNew();
			plc3s1_1.JL_ActualWeight = 300;
			plc3s1_1.JL_JC = contLD8c3.PK;

			var plc3s1_2 = s1c3.OuterPackLines.AddNew();
			plc3s1_2.JL_ActualWeight = 700m;
			plc3s1_2.JL_JC = contPA5c3.PK;

			var s2c3 = consol3.Shipments.AddNew();
			s2c3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c3.JS_TransportMode = TransportModes.Air;
			s2c3.JS_PackingMode = ContainerModes.ULD;
			s2c3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c3.JS_RL_NKOrigin = "AUSYD";
			s2c3.JS_RL_NKDestination = "USLAX";
			s2c3.JS_INCO = "CFR";
			s2c3.JS_OH_DeliveryAgent = consignee.PK;
			s2c3.ConsignorPK = consignor.PK;
			s2c3.ConsigneePK = consignee.PK;
			s2c3.JS_E_DEP = new ZDate(2015, 7, 10);
			s2c3.JS_E_ARV = new ZDateTime(2015, 7, 10, 6, 0, 0);
			s2c3.JS_ActualVolume = 3;

			var plc3s2_1 = s2c3.OuterPackLines.AddNew();
			plc3s2_1.JL_ActualWeight = 1100m;
			plc3s2_1.JL_JC = contLD8c3.PK;

			#endregion

			Factory.Save();

			return new BusinessObject[] { consol1, consol2, consol3 };
		}

		BusinessObject[] SetupEqualizationCostAndObjects_FCL(ZDecimal pivotBreak1, ZDecimal pivotBreak2, ZDecimal pivotBreak3, ZString quantityUnit)
		{
			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			#region Setup Equalization Rate July 1-15

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsShippingProvider = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			#region Cost for 20GP Containers

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.TI_ContractNumber = "010001";
			costEntry20GP.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntry20GP.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntry20GP.RateLines.RemoveAndDeleteAll();

			var costLine20GP = costEntry20GP.AddRateLine("FRT", EqualizationCalculator.Code, quantityUnit);
			costLine20GP.Calculator.UseInclusiveBreaks = true;

			var c20GPplusRateLineItem = costLine20GP.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			c20GPplusRateLineItem.TM_Break = pivotBreak1;
			c20GPplusRateLineItem.TM_FlatAmount = 15m;
			c20GPplusRateLineItem.TM_RelevantValue = 20m;

			var costLine20GP2 = costEntry20GP.AddRateLine("FRT", EqualizationCalculator.Code, quantityUnit);
			costLine20GP2.Calculator.UseInclusiveBreaks = true;
			var c20GP2plusRateLineItem = costLine20GP2.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			c20GP2plusRateLineItem.TM_Break = pivotBreak3;
			c20GP2plusRateLineItem.TM_FlatAmount = 10m;
			c20GP2plusRateLineItem.TM_RelevantValue = 10m;

			#endregion

			#region Cost for 40GP Containers

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.TI_ContractNumber = "010001";
			costEntry40GP.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntry40GP.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntry40GP.RateLines.RemoveAndDeleteAll();

			var costLine40GP = costEntry40GP.AddRateLine("FRT", EqualizationCalculator.Code, quantityUnit);

			var c20GP_plusRateLineItem = costLine40GP.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			c20GP_plusRateLineItem.TM_Break = pivotBreak2;
			c20GP_plusRateLineItem.TM_FlatAmount = 10m;
			c20GP_plusRateLineItem.TM_RelevantValue = 10m;

			var c20GP_minusRateLineItem = costLine40GP.RateLineItems.AddNew();
			c20GP_minusRateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			c20GP_minusRateLineItem.TM_Break = pivotBreak2;
			c20GP_minusRateLineItem.TM_FlatAmount = 30m;
			c20GP_minusRateLineItem.TM_RelevantValue = 30m;

			costLine40GP.Calculator.UseInclusiveBreaks = true;

			#endregion

			#region Non Equalizer Cost

			var costEntryNonEQU20RE = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20RE");
			costEntryNonEQU20RE.TI_RX_NKCurrency = "AUD";
			costEntryNonEQU20RE.TI_ContractNumber = "010001";
			costEntryNonEQU20RE.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntryNonEQU20RE.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntryNonEQU20RE.RateLines.RemoveAndDeleteAll();
			var rateLineNonEQU20RE = costEntryNonEQU20RE.AddRateLine("FRT", FlatCalculator.Code, "KG");
			rateLineNonEQU20RE.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)300m;

			#endregion

			Factory.Save();

			#endregion

			var cont20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var cont40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			#region Consol Shipped July 2nd - 2600 x 2 x 20GP and 200 x 40GP

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_AWBServiceLevel = "STD";
			consol1.JK_ConsolMode = ContainerModes.FCL;
			consol1.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol1.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";
			consol1.JK_PrepaidCollect = PaymentType.Prepaid;
			consol1.JK_UniqueConsignRef = "C00001";
			consol1.JK_CarrierContractNumber = "010001";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var cont20GPc1_1 = consol1.Containers.AddNew();
			cont20GPc1_1.JC_RC = cont20GP;
			var cont20GPc1_2 = consol1.Containers.AddNew();
			cont20GPc1_2.JC_RC = cont20GP;
			var cont40GPc1_1 = consol1.Containers.AddNew();
			cont40GPc1_1.JC_RC = cont40GP;

			var s1c1 = consol1.Shipments.AddNew();
			s1c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c1.JS_TransportMode = TransportModes.Sea;
			s1c1.JS_PackingMode = ContainerModes.FCL;
			s1c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c1.JS_RL_NKOrigin = "AUSYD";
			s1c1.JS_RL_NKDestination = "USLAX";
			s1c1.JS_INCO = "CFR";
			s1c1.JS_OH_DeliveryAgent = consignee.PK;
			s1c1.JS_ActualVolume = 1;
			s1c1.ConsignorPK = consignor.PK;
			s1c1.ConsigneePK = consignee.PK;
			s1c1.JS_E_DEP = new ZDate(2015, 7, 2);
			s1c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);

			var plc1s1_1 = s1c1.OuterPackLines.AddNew();
			plc1s1_1.JL_ActualWeight = 1000m;
			plc1s1_1.JL_ActualVolume = 10m;
			plc1s1_1.JL_JC = cont20GPc1_1.PK;

			var plc1s1_2 = s1c1.OuterPackLines.AddNew();
			plc1s1_2.JL_ActualWeight = 800m;
			plc1s1_2.JL_ActualVolume = 8m;
			plc1s1_2.JL_JC = cont20GPc1_2.PK;

			var plc1s1_3 = s1c1.OuterPackLines.AddNew();
			plc1s1_3.JL_ActualWeight = 200m;
			plc1s1_3.JL_ActualVolume = 2m;
			plc1s1_3.JL_JC = cont40GPc1_1.PK;

			var s2c1 = consol1.Shipments.AddNew();
			s2c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c1.JS_TransportMode = TransportModes.Sea;
			s2c1.JS_PackingMode = ContainerModes.FCL;
			s2c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c1.JS_RL_NKOrigin = "AUSYD";
			s2c1.JS_RL_NKDestination = "USLAX";
			s2c1.JS_INCO = "CFR";
			s2c1.JS_OH_DeliveryAgent = consignee.PK;
			s2c1.ConsignorPK = consignor.PK;
			s2c1.ConsigneePK = consignee.PK;
			s2c1.JS_E_DEP = new ZDate(2015, 7, 2);
			s2c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);
			s2c1.JS_ActualVolume = 2;

			var plc1s2_1 = s2c1.OuterPackLines.AddNew();
			plc1s2_1.JL_ActualWeight = 800m;
			plc1s2_1.JL_ActualVolume = 8m;
			plc1s2_1.JL_JC = cont20GPc1_1.PK;

			#endregion

			#region Consol Shipped July 5th - 1800 x 2 x 20GP and 600 x 40GP

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = TransportModes.Sea;
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_AWBServiceLevel = "STD";
			consol2.JK_ConsolMode = ContainerModes.FCL;
			consol2.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol2.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;
			consol2.JK_UniqueConsignRef = "C00002";
			consol2.JK_CarrierContractNumber = "010001";

			var cont20GPc2_1 = consol2.Containers.AddNew();
			cont20GPc2_1.JC_RC = cont20GP;
			var cont20GPc2_2 = consol2.Containers.AddNew();
			cont20GPc2_2.JC_RC = cont20GP;
			var cont40GPc2_1 = consol2.Containers.AddNew();
			cont40GPc2_1.JC_RC = cont40GP;

			var s1c2 = consol2.Shipments.AddNew();
			s1c2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c2.JS_TransportMode = TransportModes.Sea;
			s1c2.JS_PackingMode = ContainerModes.FCL;
			s1c2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c2.JS_RL_NKOrigin = "AUSYD";
			s1c2.JS_RL_NKDestination = "USLAX";
			s1c2.JS_INCO = "CFR";
			s1c2.JS_OH_DeliveryAgent = consignee.PK;
			s1c2.JS_ActualVolume = 1;
			s1c2.ConsignorPK = consignor.PK;
			s1c2.ConsigneePK = consignee.PK;
			s1c2.JS_E_DEP = new ZDate(2015, 7, 5);
			s1c2.JS_E_ARV = new ZDateTime(2015, 7, 5, 6, 0, 0);

			var plc2s1_1 = s1c2.OuterPackLines.AddNew();
			plc2s1_1.JL_ActualWeight = 500m;
			plc2s1_1.JL_ActualVolume = 5m;
			plc2s1_1.JL_JC = cont20GPc2_1.PK;

			var plc2s1_2 = s1c2.OuterPackLines.AddNew();
			plc2s1_2.JL_ActualWeight = 500m;
			plc2s1_2.JL_ActualVolume = 5m;
			plc2s1_2.JL_JC = cont20GPc2_2.PK;

			var plc2s1_3 = s1c2.OuterPackLines.AddNew();
			plc2s1_3.JL_ActualWeight = 600m;
			plc2s1_3.JL_ActualVolume = 6m;
			plc2s1_3.JL_JC = cont40GPc2_1.PK;

			var s2c2 = consol2.Shipments.AddNew();
			s2c2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c2.JS_TransportMode = TransportModes.Sea;
			s2c2.JS_PackingMode = ContainerModes.FCL;
			s2c2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c2.JS_RL_NKOrigin = "AUSYD";
			s2c2.JS_RL_NKDestination = "USLAX";
			s2c2.JS_INCO = "CFR";
			s2c2.JS_OH_DeliveryAgent = consignee.PK;
			s2c2.ConsignorPK = consignor.PK;
			s2c2.ConsigneePK = consignee.PK;
			s2c2.JS_E_DEP = new ZDate(2015, 7, 5);
			s2c2.JS_E_ARV = new ZDateTime(2015, 7, 5, 6, 0, 0);
			s2c2.JS_ActualVolume = 2;

			var plc2s2_1 = s2c2.OuterPackLines.AddNew();
			plc2s2_1.JL_ActualWeight = 800m;
			plc2s2_1.JL_ActualVolume = 8m;
			plc2s2_1.JL_JC = cont20GPc2_1.PK;

			#endregion

			#region Consol Shipped July 10th - 1400 x 20GP and 700 x 40GP

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.JK_TransportMode = TransportModes.Sea;
			consol3.JK_AgentType = AgentType.Agent;
			consol3.JK_AWBServiceLevel = "STD";
			consol3.JK_ConsolMode = ContainerModes.FCL;
			consol3.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol3.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol3.JK_RL_NKLoadPort = "AUSYD";
			consol3.JK_RL_NKDischargePort = "USLAX";
			consol3.JK_PrepaidCollect = PaymentType.Prepaid;
			consol3.JK_UniqueConsignRef = "C00003";
			consol3.JK_CarrierContractNumber = "010001";

			var cont20GPc3 = consol3.Containers.AddNew();
			cont20GPc3.JC_RC = cont20GP;
			var cont40GPc3 = consol3.Containers.AddNew();
			cont40GPc3.JC_RC = cont40GP;

			var s1c3 = consol3.Shipments.AddNew();
			s1c3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s1c3.JS_TransportMode = TransportModes.Sea;
			s1c3.JS_PackingMode = ContainerModes.FCL;
			s1c3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s1c3.JS_RL_NKOrigin = "AUSYD";
			s1c3.JS_RL_NKDestination = "USLAX";
			s1c3.JS_INCO = "CFR";
			s1c3.JS_OH_DeliveryAgent = consignee.PK;
			s1c3.JS_ActualVolume = 1;
			s1c3.ConsignorPK = consignor.PK;
			s1c3.ConsigneePK = consignee.PK;
			s1c3.JS_E_DEP = new ZDate(2015, 7, 10);
			s1c3.JS_E_ARV = new ZDateTime(2015, 7, 10, 6, 0, 0);

			var plc3s1_1 = s1c3.OuterPackLines.AddNew();
			plc3s1_1.JL_ActualWeight = 300m;
			plc3s1_1.JL_ActualVolume = 3m;
			plc3s1_1.JL_JC = cont20GPc3.PK;

			var plc3s1_2 = s1c3.OuterPackLines.AddNew();
			plc3s1_2.JL_ActualWeight = 700m;
			plc3s1_2.JL_ActualVolume = 7m;
			plc3s1_2.JL_JC = cont40GPc3.PK;

			var s2c3 = consol3.Shipments.AddNew();
			s2c3.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			s2c3.JS_TransportMode = TransportModes.Sea;
			s2c3.JS_PackingMode = ContainerModes.FCL;
			s2c3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			s2c3.JS_RL_NKOrigin = "AUSYD";
			s2c3.JS_RL_NKDestination = "USLAX";
			s2c3.JS_INCO = "CFR";
			s2c3.JS_OH_DeliveryAgent = consignee.PK;
			s2c3.ConsignorPK = consignor.PK;
			s2c3.ConsigneePK = consignee.PK;
			s2c3.JS_E_DEP = new ZDate(2015, 7, 10);
			s2c3.JS_E_ARV = new ZDateTime(2015, 7, 10, 6, 0, 0);
			s2c3.JS_ActualVolume = 3;

			var plc3s2_1 = s2c3.OuterPackLines.AddNew();
			plc3s2_1.JL_ActualWeight = 1100m;
			plc3s2_1.JL_ActualVolume = 11m;
			plc3s2_1.JL_JC = cont20GPc3.PK;

			#endregion

			Factory.Save();

			return new BusinessObject[] { consol1, consol2, consol3 };
		}

		[TestDate(2015, 07, 15)]
		public void TestAirConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_PivotNotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_AIR(3800, 2000, 1000);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Weight per LD-8 as per contract '010001' is 1000 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight achieved.
	Pivot Weight per PA-5 as per contract '010001' is 2000 KG
	C00001 - AUSYD - USLAX: 200 KG in 1xPA-5
	C00002 - AUSYD - USLAX: 600 KG in 1xPA-5
	C00003 - AUSYD - USLAX: 700 KG in 1xPA-5
	Average Weight: (200 + 600 + 700)/3 = 500.00 KG
	Pivot Weight not achieved.
	Pivot Weight per LD-8 as per contract '010001' is 3800 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		void DeleteUnDisposedJobHeader(BusinessObject[] equalizationSetupObjects)
		{
			foreach (var job in equalizationSetupObjects
				.OfType<ForwardingConsol>()
				.SelectMany(consol => consol.Shipments.OfType<ForwardingShipment>()))
			{
				var jobheader = new Job.Loader(job).Load(false);
				if (jobheader != null && !(jobheader.Charges.Count > 0) && !jobheader.IsInDatabase)
				{
					jobheader.Delete();
				}
			}
		}

		[TestDate(2015, 07, 15)]
		public void TestAirConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_PivotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_AIR(1100, 400, 900);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Weight per PA-5 as per contract '010001' is 400 KG
	C00001 - AUSYD - USLAX: 200 KG in 1xPA-5
	C00002 - AUSYD - USLAX: 600 KG in 1xPA-5
	C00003 - AUSYD - USLAX: 700 KG in 1xPA-5
	Average Weight: (200 + 600 + 700)/3 = 500.00 KG
	Pivot Weight achieved.
	Pivot Weight per LD-8 as per contract '010001' is 900 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight achieved.
	Pivot Weight per LD-8 as per contract '010001' is 1100 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			Factory.Save();
		}

		[TestDate(2015, 07, 15)]
		public void TestAirConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_AIR(1100, 550, 900);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Weight per PA-5 as per contract '010001' is 550 KG
	C00001 - AUSYD - USLAX: 200 KG in 1xPA-5
	C00002 - AUSYD - USLAX: 600 KG in 1xPA-5
	C00003 - AUSYD - USLAX: 700 KG in 1xPA-5
	Average Weight: (200 + 600 + 700)/3 = 500.00 KG
	Pivot Weight not achieved.
	Pivot Weight per LD-8 as per contract '010001' is 900 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight achieved.
	Pivot Weight per LD-8 as per contract '010001' is 1100 KG
	C00001 - AUSYD - USLAX: 2600 KG in 2xLD-8
	C00002 - AUSYD - USLAX: 1800 KG in 2xLD-8
	C00003 - AUSYD - USLAX: 1400 KG in 1xLD-8
	Average Weight: (2600 + 1800 + 1400)/5 = 1160.00 KG
	Pivot Weight achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);

				var consol1 = equalizationSetupObjects[0] as ForwardingConsol;
				var consol2 = equalizationSetupObjects[1] as ForwardingConsol;
				var consol3 = equalizationSetupObjects[2] as ForwardingConsol;

				var costsC1 = consol1.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c1_description_1 = costsC1[0].CostCalculationDescription.ToAscii();
				var c1_description_2 = costsC1[1].CostCalculationDescription.ToAscii();
				var costsC2 = consol2.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c2_description_1 = costsC2[0].CostCalculationDescription.ToAscii();
				var c2_description_2 = costsC2[1].CostCalculationDescription.ToAscii();
				var costsC3 = consol3.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c3_description_1 = costsC3[0].CostCalculationDescription.ToAscii();
				var c3_description_2 = costsC3[1].CostCalculationDescription.ToAscii();

				AssertContains("FRT: Base Rate AUD 10.00 + 2600 Kilogram(s) @ AUD 10.00/KG", c1_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 2600 Kilogram(s) @ AUD 20.00/KG", c1_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 200 Kilogram(s) @ AUD 30.00/KG", c1_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 1800 Kilogram(s) @ AUD 10.00/KG", c2_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 1800 Kilogram(s) @ AUD 20.00/KG", c2_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 600 Kilogram(s) @ AUD 30.00/KG", c2_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 1400 Kilogram(s) @ AUD 10.00/KG", c3_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 1400 Kilogram(s) @ AUD 20.00/KG", c3_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 700 Kilogram(s) @ AUD 30.00/KG", c3_description_2);

				AssertContains("Pivot Weight per LD-8 as per contract '010001' is 1100 KG", c1_description_1);
				AssertContains("Pivot Weight per LD-8 as per contract '010001' is 900 KG", c1_description_1);
				AssertContains("Pivot Weight per PA-5 as per contract '010001' is 550 KG", c1_description_2);

				AssertEquals(84055m, consol1.FreightCostsAmount);
				AssertEquals(72055m, consol2.FreightCostsAmount);
				AssertEquals(63055m, consol3.FreightCostsAmount);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_M3_PivotNotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(38, 20, 10, QuantityUnit.M3);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Volume per 20GP as per contract '010001' is 10 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume achieved.
	Pivot Volume per 40GP as per contract '010001' is 20 M3
	C00001 - AUSYD - USLAX: 5 M3 in 1x40GP
	C00002 - AUSYD - USLAX: 9 M3 in 1x40GP
	C00003 - AUSYD - USLAX: 11 M3 in 1x40GP
	Average Volume: (5 + 9 + 11)/3 = 8.33 M3
	Pivot Volume not achieved.
	Pivot Volume per 20GP as per contract '010001' is 38 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_M3_PivotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(11, 4, 9, QuantityUnit.M3);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Volume per 40GP as per contract '010001' is 4 M3
	C00001 - AUSYD - USLAX: 5 M3 in 1x40GP
	C00002 - AUSYD - USLAX: 9 M3 in 1x40GP
	C00003 - AUSYD - USLAX: 11 M3 in 1x40GP
	Average Volume: (5 + 9 + 11)/3 = 8.33 M3
	Pivot Volume achieved.
	Pivot Volume per 20GP as per contract '010001' is 9 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume achieved.
	Pivot Volume per 20GP as per contract '010001' is 11 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_M3()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(11, 9, 9, QuantityUnit.M3);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Volume per 20GP as per contract '010001' is 9 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume achieved.
	Pivot Volume per 40GP as per contract '010001' is 9 M3
	C00001 - AUSYD - USLAX: 5 M3 in 1x40GP
	C00002 - AUSYD - USLAX: 9 M3 in 1x40GP
	C00003 - AUSYD - USLAX: 11 M3 in 1x40GP
	Average Volume: (5 + 9 + 11)/3 = 8.33 M3
	Pivot Volume not achieved.
	Pivot Volume per 20GP as per contract '010001' is 11 M3
	C00001 - AUSYD - USLAX: 26 M3 in 2x20GP
	C00002 - AUSYD - USLAX: 18 M3 in 2x20GP
	C00003 - AUSYD - USLAX: 14 M3 in 1x20GP
	Average Volume: (26 + 18 + 14)/5 = 11.60 M3
	Pivot Volume achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);

				var consol1 = equalizationSetupObjects[0] as ForwardingConsol;
				var consol2 = equalizationSetupObjects[1] as ForwardingConsol;
				var consol3 = equalizationSetupObjects[2] as ForwardingConsol;

				var costsC1 = consol1.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c1_description_1 = costsC1[0].CostCalculationDescription.ToAscii();
				var c1_description_2 = costsC1[1].CostCalculationDescription.ToAscii();
				var costsC2 = consol2.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c2_description_1 = costsC2[0].CostCalculationDescription.ToAscii();
				var c2_description_2 = costsC2[1].CostCalculationDescription.ToAscii();
				var costsC3 = consol3.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c3_description_1 = costsC3[0].CostCalculationDescription.ToAscii();
				var c3_description_2 = costsC3[1].CostCalculationDescription.ToAscii();

				AssertContains("FRT: Base Rate AUD 10.00 + 26 Cubic Meter(s) @ AUD 10.00/M3", c1_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 26 Cubic Meter(s) @ AUD 20.00/M3", c1_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 5 Cubic Meter(s) @ AUD 30.00/M3", c1_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 18 Cubic Meter(s) @ AUD 10.00/M3", c2_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 18 Cubic Meter(s) @ AUD 20.00/M3", c2_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 9 Cubic Meter(s) @ AUD 30.00/M3", c2_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 14 Cubic Meter(s) @ AUD 10.00/M3", c3_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 14 Cubic Meter(s) @ AUD 20.00/M3", c3_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 11 Cubic Meter(s) @ AUD 30.00/M3", c3_description_2);

				AssertContains("Pivot Volume per 20GP as per contract '010001' is 11 M3", c1_description_1);
				AssertContains("Pivot Volume per 20GP as per contract '010001' is 9 M3", c1_description_1);
				AssertContains("Pivot Volume per 40GP as per contract '010001' is 9 M3", c1_description_2);

				AssertEquals(985m, consol1.FreightCostsAmount);
				AssertEquals(865m, consol2.FreightCostsAmount);
				AssertEquals(805m, consol3.FreightCostsAmount);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_TEU_PivotNotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(3, 3, 3, QuantityUnit.TU);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot TEUs per 20GP as per contract '010001' is 3 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs not achieved.
	Pivot TEUs per 20GP as per contract '010001' is 3 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs not achieved.
	Pivot TEUs per 40GP as per contract '010001' is 3 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00002 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00003 - AUSYD - USLAX: 2.00 TU in 1x40GP
	Average TEUs: (2.00 + 2.00 + 2.00)/3 = 2.00 TU
	Pivot TEUs not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_TEU_PivotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(1, 1, 1, QuantityUnit.TU);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot TEUs per 20GP as per contract '010001' is 1 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs achieved.
	Pivot TEUs per 20GP as per contract '010001' is 1 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs achieved.
	Pivot TEUs per 40GP as per contract '010001' is 1 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00002 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00003 - AUSYD - USLAX: 2.00 TU in 1x40GP
	Average TEUs: (2.00 + 2.00 + 2.00)/3 = 2.00 TU
	Pivot TEUs achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_TEU()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(1, 5, 1, QuantityUnit.TU);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot TEUs per 20GP as per contract '010001' is 1 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs achieved.
	Pivot TEUs per 20GP as per contract '010001' is 1 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00002 - AUSYD - USLAX: 2.00 TU in 2x20GP
	C00003 - AUSYD - USLAX: 1.00 TU in 1x20GP
	Average TEUs: (2.00 + 2.00 + 1.00)/5 = 1.00 TU
	Pivot TEUs achieved.
	Pivot TEUs per 40GP as per contract '010001' is 5 TU
	C00001 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00002 - AUSYD - USLAX: 2.00 TU in 1x40GP
	C00003 - AUSYD - USLAX: 2.00 TU in 1x40GP
	Average TEUs: (2.00 + 2.00 + 2.00)/3 = 2.00 TU
	Pivot TEUs not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);

				var consol1 = equalizationSetupObjects[0] as ForwardingConsol;
				var consol2 = equalizationSetupObjects[1] as ForwardingConsol;
				var consol3 = equalizationSetupObjects[2] as ForwardingConsol;

				var costsC1 = consol1.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c1_description_1 = costsC1[0].CostCalculationDescription.ToAscii();
				var c1_description_2 = costsC1[1].CostCalculationDescription.ToAscii();
				var costsC2 = consol2.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c2_description_1 = costsC2[0].CostCalculationDescription.ToAscii();
				var c2_description_2 = costsC2[1].CostCalculationDescription.ToAscii();
				var costsC3 = consol3.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c3_description_1 = costsC3[0].CostCalculationDescription.ToAscii();
				var c3_description_2 = costsC3[1].CostCalculationDescription.ToAscii();

				AssertContains("FRT: Base Rate AUD 10.00 + 2 Twenty foot equivalent unit(s) @ AUD 10.00/Twenty foot equivalent unit", c1_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 2 Twenty foot equivalent unit(s) @ AUD 20.00/Twenty foot equivalent unit", c1_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 2 Twenty foot equivalent unit(s) @ AUD 30.00/Twenty foot equivalent unit", c1_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 2 Twenty foot equivalent unit(s) @ AUD 10.00/Twenty foot equivalent unit", c2_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 2 Twenty foot equivalent unit(s) @ AUD 20.00/Twenty foot equivalent unit", c2_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 2 Twenty foot equivalent unit(s) @ AUD 30.00/Twenty foot equivalent unit", c2_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 1 Twenty foot equivalent unit(s) @ AUD 10.00/Twenty foot equivalent unit", c3_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 1 Twenty foot equivalent unit(s) @ AUD 20.00/Twenty foot equivalent unit", c3_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 2 Twenty foot equivalent unit(s) @ AUD 30.00/Twenty foot equivalent unit", c3_description_2);

				AssertContains("Pivot TEUs per 20GP as per contract '010001' is 1 TU", c1_description_1);
				AssertContains("Pivot TEUs per 20GP as per contract '010001' is 1 TU", c1_description_1);
				AssertContains("Pivot TEUs per 40GP as per contract '010001' is 5 TU", c1_description_2);

				AssertEquals(175m, consol1.FreightCostsAmount);
				AssertEquals(175m, consol2.FreightCostsAmount);
				AssertEquals(145m, consol3.FreightCostsAmount);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_CN_PivotNotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(10, 10, 10, QuantityUnit.CN);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Containers per 20GP as per contract '010001' is 10 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers not achieved.
	Pivot Containers per 20GP as per contract '010001' is 10 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers not achieved.
	Pivot Containers per 40GP as per contract '010001' is 10 CN
	C00001 - AUSYD - USLAX: 1x40GP Containers
	C00002 - AUSYD - USLAX: 1x40GP Containers
	C00003 - AUSYD - USLAX: 1x40GP Containers
	Total Containers: 3
	Pivot Containers not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_CN_PivotAchieved()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(1, 1, 1, QuantityUnit.CN);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Containers per 20GP as per contract '010001' is 1 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers achieved.
	Pivot Containers per 20GP as per contract '010001' is 1 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers achieved.
	Pivot Containers per 40GP as per contract '010001' is 1 CN
	C00001 - AUSYD - USLAX: 1x40GP Containers
	C00002 - AUSYD - USLAX: 1x40GP Containers
	C00003 - AUSYD - USLAX: 1x40GP Containers
	Total Containers: 3
	Pivot Containers achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestFCLConsolCalculateEqualizationPivotsAndAutoRateActionMethodApplicatorLog_CN()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_FCL(1, 5, 1, QuantityUnit.CN);
			applicator.InitialiseBeforeIndividiualBatchRun();

			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, equalizationSetupObjects);
				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Containers per 20GP as per contract '010001' is 1 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers achieved.
	Pivot Containers per 20GP as per contract '010001' is 1 CN
	C00001 - AUSYD - USLAX: 2x20GP Containers
	C00002 - AUSYD - USLAX: 2x20GP Containers
	C00003 - AUSYD - USLAX: 1x20GP Containers
	Total Containers: 5
	Pivot Containers achieved.
	Pivot Containers per 40GP as per contract '010001' is 5 CN
	C00001 - AUSYD - USLAX: 1x40GP Containers
	C00002 - AUSYD - USLAX: 1x40GP Containers
	C00003 - AUSYD - USLAX: 1x40GP Containers
	Total Containers: 3
	Pivot Containers not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);

				var consol1 = equalizationSetupObjects[0] as ForwardingConsol;
				var consol2 = equalizationSetupObjects[1] as ForwardingConsol;
				var consol3 = equalizationSetupObjects[2] as ForwardingConsol;

				var costsC1 = consol1.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c1_description_1 = costsC1[0].CostCalculationDescription.ToAscii();
				var c1_description_2 = costsC1[1].CostCalculationDescription.ToAscii();
				var costsC2 = consol2.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c2_description_1 = costsC2[0].CostCalculationDescription.ToAscii();
				var c2_description_2 = costsC2[1].CostCalculationDescription.ToAscii();
				var costsC3 = consol3.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var c3_description_1 = costsC3[0].CostCalculationDescription.ToAscii();
				var c3_description_2 = costsC3[1].CostCalculationDescription.ToAscii();

				AssertContains("FRT: Base Rate AUD 10.00 + 2 20GP Container(s) @ AUD 10.00/Container", c1_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 2 20GP Container(s) @ AUD 20.00/Container", c1_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 1 40GP Container(s) @ AUD 30.00/Container", c1_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 2 20GP Container(s) @ AUD 10.00/Container", c2_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 2 20GP Container(s) @ AUD 20.00/Container", c2_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 1 40GP Container(s) @ AUD 30.00/Container", c2_description_2);

				AssertContains("FRT: Base Rate AUD 10.00 + 1 20GP Container(s) @ AUD 10.00/Container", c3_description_1);
				AssertContains("FRT: Base Rate AUD 15.00 + 1 20GP Container(s) @ AUD 20.00/Container", c3_description_1);
				AssertContains("FRT: Base Rate AUD 30.00 + 1 40GP Container(s) @ AUD 30.00/Container", c3_description_2);

				AssertContains("Pivot Containers per 20GP as per contract '010001' is 1 CN", c1_description_1);
				AssertContains("Pivot Containers per 20GP as per contract '010001' is 1 CN", c1_description_1);
				AssertContains("Pivot Containers per 40GP as per contract '010001' is 5 CN", c1_description_2);

				AssertEquals(145m, consol1.FreightCostsAmount);
				AssertEquals(145m, consol2.FreightCostsAmount);
				AssertEquals(115m, consol3.FreightCostsAmount);
			}
			DeleteUnDisposedJobHeader(equalizationSetupObjects);
		}

		[TestDate(2015, 07, 15)]
		public void TestEqualizationSingleConsol_FCL_M3_PivotNotAchieved()
		{
			#region Setup Equalization Rate July 1-15

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsShippingProvider = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			#region Cost for 40GP Containers

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.TI_ContractNumber = "010001";
			costEntry40GP.TI_RateStartDate = new ZDate(2015, 7, 1);
			costEntry40GP.TI_RateEndDate = new ZDate(2015, 7, 15);
			costEntry40GP.RateLines.RemoveAndDeleteAll();

			var costLine40GP = costEntry40GP.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.M3);
			costLine40GP.Calculator.UseInclusiveBreaks = true;

			var c40GP_plusRateLineItem = costLine40GP.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			c40GP_plusRateLineItem.TM_Break = 30m;
			c40GP_plusRateLineItem.TM_FlatAmount = 10m;
			c40GP_plusRateLineItem.TM_RelevantValue = 0.5m;

			var c40GP_minusRateLineItem = costLine40GP.RateLineItems.AddNew();
			c40GP_minusRateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			c40GP_minusRateLineItem.TM_Break = 30m;
			c40GP_minusRateLineItem.TM_FlatAmount = 50m;
			c40GP_minusRateLineItem.TM_RelevantValue = 1m;

			#endregion

			Factory.Save();

			#endregion

			var cont40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			#region Consol Shipped July 2nd - 2 Shipments in 1 40GP container

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_UniqueConsignRef = "C00001";
			consol.JK_CarrierContractNumber = "010001";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = cont40GP_PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.JS_ActualVolume = 8m;
			shipment1.JS_ActualWeight = 5000m;

			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			shipment1.JS_E_DEP = new ZDate(2015, 7, 2);
			shipment1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);
			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var plc1s1_1 = shipment1.OuterPackLines.AddNew();
			plc1s1_1.JL_ActualWeight = 5000m;
			plc1s1_1.JL_ActualVolume = 8m;
			plc1s1_1.JL_JC = container40GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			shipment2.JS_E_DEP = new ZDate(2015, 7, 2);
			shipment2.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);
			shipment2.JS_ActualVolume = 15m;
			shipment2.JS_ActualWeight = 4000m;
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var plc1s2_1 = shipment2.OuterPackLines.AddNew();
			plc1s2_1.JL_ActualWeight = 4000m;
			plc1s2_1.JL_ActualVolume = 15m;
			plc1s2_1.JL_JC = container40GP.PK;

			#endregion

			Factory.Save();

			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			applicator.InitialiseBeforeIndividiualBatchRun();
			using (SetJR_DescDisplayAllText())
			{
				applicator.Apply(actualLog, new[] { consol });

				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Volume per 40GP as per contract '010001' is 30 M3
	C00001 - AUSYD - USLAX: 23 M3 in 1x40GP
	Average Volume: (23)/1 = 23.00 M3
	Pivot Volume not achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);

				var consolCosts = consol.GetConsolFreightCosts().Cast<JobConsolCost>().ToArray();
				var costDescription = consolCosts[0].CostCalculationDescription.ToAscii();

				AssertContains("FRT: Base Rate AUD 50.00 + 23 Cubic Meter(s) @ AUD 1.00/M3", costDescription);
				AssertEquals(73m, consol.FreightCostsAmount);
			}
			DeleteUnDisposedJobHeader(new BusinessObject[] { consol });
		}

		#endregion

		#region AutoratingStarter creates Jobs

		public void TestExecuteAutoratingCreatesNewJobs_CreateJobsAndAutoRates()
		{
			var consol = GetConsolWithRates();
			var shipment = AddShipmentToConsol(consol);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, ImportAirDepartment.PK.ToGuid()))
			using (var plugin = new InvoicingPluginToFreight(shipment))
			using (var form = new ZForm(plugin))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var interactor = new AutoRatingGUIInteractor(form);
				var starter = new AutoRatingStarter(shipment, interactor);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var result = GetJobFromParent(shipment.PK);
				AssertNotNull(result);
				AssertEquals("Expected the job to have one charge now it has been autorated", 1, result.Charges.Count);
				AssertEquals("Expected WAR charge on the job", "WAR", result.Charges[0].ChargeCode.AC_Code);
				result.Dispose();
			}
		}

		public void TestExecuteAutoratingCreatesNewJobs_CreateJobsAndAutoRatesForRelatedSupporters()
		{
			var consol = GetConsolWithRates(PaymentType.Prepaid);
			var shipment1 = AddShipmentToConsol(consol);
			var shipment2 = AddShipmentToConsol(consol);

			var shipment1Job = Factory.NewJobForTesting<Job>();
			shipment1Job.JH_ParentID = shipment1.PK;
			shipment1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			shipment1Job.JH_GE = ImportAirDepartment.PK;
			shipment1Job.LocalChargesPK = Consignee.PK;

			AssertNull("Pre-condition: Shipment 2 has no job", GetJobFromParent(shipment2.PK));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, ImportAirDepartment.PK.ToGuid()))
			using (var plugin = new InvoicingPluginToFreight(consol))
			using (var form = new ZForm(plugin))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var interactor = new AutoRatingGUIInteractor(form);
				var starter = new AutoRatingStarter(consol, interactor);

				shipment1Job = GetJobFromParent(shipment1.PK);
				AssertNotNull("Pre-existing Job", shipment1Job);
				AssertEquals("Job was created without charges attached", 0, shipment1Job.Charges.Count);

				var shipment2Job = GetJobFromParent(shipment2.PK);
				AssertNull("Job should not have been created when we intialised auto-rating starter", shipment2Job);

				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				Factory.Save();

				AssertEquals("Expected the job to have one charge now it has been autorated", 1, shipment1Job.Charges.Count);
				AssertEquals("Expected WAR charge on the job", "WAR", shipment1Job.Charges[0].ChargeCode.AC_Code);

				shipment2Job = GetJobFromParent(shipment2.PK);
				AssertNotNull("Job should have been created when we executed autorating", shipment2Job);
				AssertEquals("Expected the job to have one charge now it has been autorated", 1, shipment2Job.Charges.Count);
				AssertEquals("Expected WAR charge on the job", "WAR", shipment2Job.Charges[0].ChargeCode.AC_Code);

				shipment1Job.Dispose();
				shipment2Job.Dispose();
			}
		}

		public void TestExecuteAutoratingCreatesNewJobs_DoesNotCreateJobsForRelatedSupporters()
		{
			var consol = GetConsolWithRates();
			var shipment = AddShipmentToConsol(consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, ImportAirDepartment.PK.ToGuid()))
			using (var plugin = new InvoicingPluginToFreight(consol))
			using (var form = new ZForm(plugin))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var interactor = new AutoRatingGUIInteractor(form);
				var starter = new AutoRatingStarter(consol, interactor, additionalJobsAction: AdditionalJobsAction.NoAction);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var resultJob = GetJobFromParent(shipment.PK);
				AssertNull("No job should have been created as AutoRating starter is overriden not to use strategies from supporters", resultJob);
			}
		}

		public void TestExecuteAutoratingCreatesNewJobs_DoesNotCreateJobsWhenDialogSelectedIsNo()
		{
			var consol = GetConsolWithRates();
			var shipment = AddShipmentToConsol(consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, ImportAirDepartment.PK.ToGuid()))
			using (var plugin = new InvoicingPluginToFreight(consol))
			using (var form = new ZForm(plugin))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var interactor = new AutoRatingGUIInteractor(form);
				var starter = new AutoRatingStarter(consol, interactor);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var resultJob = GetJobFromParent(shipment.PK);
				AssertNull("No job should have been created for shipment as dialog result is no", resultJob);
			}
		}

		#endregion

		#region AutoRating Explorer Form

		public void TestAutoRatingExplorerFormThrowsNoException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var message = "Should not throw exception opening autorating explorer on a new shipment";
			AssertNoExceptionThrown(message, () => new AutoRatingExplorerForm(shipment));
		}

		#endregion

		#region TestDeferErrorPopup

		public void TestDeferErrorPopup_AutoRatingGUIInteractor()
		{
			var interactor = new AutoRatingGUIInteractor(null);
			using (interactor.StartRatingSession())
			using (interactor.DeferErrorPopup())
			{
				interactor.Log(LogType.Error, "Error Message 1");
				interactor.Log(LogType.Error, "Error Message 2");
				interactor.Log(LogType.Warning, "Warning Message");
				interactor.Log(LogType.Error, "Error Message 3");
				interactor.Log(LogType.Error, "Error Message 4");
			}

			var expectedMessage = @"Error Message 1
Error Message 2
Error Message 3
Error Message 4";

			AssertEquals("There should be 1 aggregated error message for all individual error messages", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeferErrorPopup_WhenAutoRating_ErrorsOnShipments()
		{
			var consol = GetConsolWithRates(PaymentType.Prepaid);
			var shipment1 = AddShipmentToConsol(consol);
			shipment1.JS_UniqueConsignRef = "S00000001";
			var shipment2 = AddShipmentToConsol(consol);
			shipment2.JS_UniqueConsignRef = "S00000002";

			var shipment1Job = Factory.NewJobForTesting<Job>();
			shipment1Job.JH_ParentID = shipment1.PK;
			shipment1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment1Job.LocalChargesPK = Consignee.PK;

			var shipment2Job = Factory.NewJobForTesting<Job>();
			shipment2Job.JH_ParentID = shipment2.PK;
			shipment2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.LocalChargesPK = Consignee.PK;

			using (var plugin = new InvoicingPluginToFreight(consol))
			using (var form = new ZForm(plugin))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var interactor = new AutoRatingGUIInteractor(form);
				var starter = new AutoRatingStarter(consol, interactor);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				shipment1Job.Dispose();
				shipment2Job.Dispose();

				var expectedMessage = @"Shipment S00000001 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Cannot issue job charges for a miscellaneous department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep.
Shipment S00000002 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Cannot issue job charges for a miscellaneous department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep.";
				AssertEquals("There should be 1 aggregated error message for both shipments", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
		#region Set up

		DummyOperationalActionSectionLog GetNewLogForConsol(ForwardingConsol consol)
		{
			var log = new DummyOperationalActionSectionLog();
			var applicator = new AutoRatingActionMethodApplicator(Factory, true, false);
			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(log, new[] { consol });
			return log;
		}

		Job GetJobFromParent(ZGuid parentGuid)
		{
			return Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, parentGuid).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
		}

		DtbBooking GetBooking(BusinessObject parent, bool isDelivery)
		{
			var consolidationBooking = Factory.New<DtbBookingConsolidation>();
			consolidationBooking.KB_ParentID = parent.PK;
			consolidationBooking.KB_ParentTableCode = parent.TablePrefix;
			consolidationBooking.KB_JobDirection = isDelivery ? InstructionTypes.Codes.Delivery : InstructionTypes.Codes.PickUp;
			consolidationBooking.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			if (parent != null)
			{
				consolidationBooking.KB_ParentID = parent.PK;
				consolidationBooking.KB_ParentTableCode = parent.TablePrefix;
			}

			var booking = consolidationBooking.Bookings.AddNew();
			booking.Address.OrganisationPK = TransportProvider1.PK;
			booking.KM_KT_NKBookingTemplate = isDelivery ? "EFPU" : "ILDV";
			booking.KM_RatingFreightMode = ContainerModes.Loose;

			var package = CreatePackage(booking, 2, PkgUnit.Package, null, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = Consignor.PK;
			CreateInstructionPkgDivots(fromInstruction, package);

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = Consignee.PK;
			CreateInstructionPkgDivots(toInstruction, package);

			return booking;
		}

		ForwardingConsol GetConsolWithRates(string prepaidCollect = "")
		{
			var rate = Helper.NewClientRate(Consignee);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "NZAKL", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_OH_Consignee = Consignee.PK;

			var rateLine = rateEntry.AddRateLine("WAR", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 60;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_CreditorAddress = Consignee.MainAddress.PK;
			consol.Transports[0].CarrierPK = Consignee.PK;
			consol.JK_PrepaidCollect = prepaidCollect;

			var consolDocAddress = consol.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			consolDocAddress.E2_OA_Address = Consignee.MainAddress.PK;

			Factory.Save();

			return consol;
		}

		ForwardingShipment AddShipmentToConsol(ForwardingConsol parentConsol)
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "NZAKL", "AUSYD", 1m, 3m);
			parentConsol.Shipments.Add(shipment);

			return shipment;
		}

		GlbDepartment ImportAirDepartment
		{
			get { return importAirDepartment ?? (importAirDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA"))); }
		}

		GlbDepartment importAirDepartment;

		//This registry setting ensure that JR_Desc displays not only the the charge code description but all information in the following format;
		//International Freight - 1 20GP Container(s) @ USD 2000.00/Container
		IDisposable SetJR_DescDisplayAllText()
		{
			var defaultValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			defaultValue.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			var collection = new InvoiceRollupOrGroupCollection();
			collection.Add(defaultValue);

			return OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion
	}
}