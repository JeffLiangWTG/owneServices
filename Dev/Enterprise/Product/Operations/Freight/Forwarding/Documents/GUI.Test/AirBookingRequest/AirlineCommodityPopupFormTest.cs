using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	[TestedType(typeof(AirlineCommodityPopupForm))]
	sealed class AirlineCommodityPopupFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			using (TempEnableCarrierConfiguration())
			using (TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;
				var etihad = airlinesDetails["607"];

				var commodityCollection = new AirlineConfigCommodityBusinessObjectCollection();

				foreach (var airlineConfigCommodity in etihad.Commodities.OrderBy(c => c.Description))
				{
					var commodityBizObj = new AirlineConfigCommodityBusinessObject(airlineConfigCommodity);
					commodityCollection.Add(commodityBizObj);
				}

				var findBox = new DummyFindBox
				{
					ListProvider = commodityCollection
				};

				using (var form = new AirlineCommodityPopupFormForTesting(findBox))
				{
					form.Show();
					form.FilterControl.Grid.Select(0);
					form.OkButton.PerformClick();

					AssertEquals("ACCESRY", findBox.Code);
				}
			}
		}

		public void TestOKButton_NoRowSelected()
		{
			using (TempEnableCarrierConfiguration())
			using (TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;
				var etihad = airlinesDetails["607"];

				var commodityCollection = new AirlineConfigCommodityBusinessObjectCollection();

				foreach (var airlineConfigCommodity in etihad.Commodities.OrderBy(c => c.Description))
				{
					var commodityBizObj = new AirlineConfigCommodityBusinessObject(airlineConfigCommodity);
					commodityCollection.Add(commodityBizObj);
				}

				var findBox = new DummyFindBox
				{
					ListProvider = commodityCollection
				};

				using (var form = new AirlineCommodityPopupFormForTesting(findBox))
				{
					form.Show();
					form.OkButton.PerformClick();

					AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please select one item from the grid.");
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestCancelButton()
		{
			using (TempEnableCarrierConfiguration())
			using (TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;
				var etihad = airlinesDetails["607"];

				var commodityCollection = new AirlineConfigCommodityBusinessObjectCollection();

				foreach (var airlineConfigCommodity in etihad.Commodities.OrderBy(c => c.Description))
				{
					var commodityBizObj = new AirlineConfigCommodityBusinessObject(airlineConfigCommodity);
					commodityCollection.Add(commodityBizObj);
				}

				var findBox = new DummyFindBox
				{
					ListProvider = commodityCollection
				};

				using (var form = new AirlineCommodityPopupFormForTesting(findBox))
				{
					form.Show();
					form.FilterControl.FilteredGrid.Select(0);
					form.CancelButton.PerformClick();

					AssertNull(findBox.Code);
				}
			}
		}

		public void TestFilterControl_PerformSearch()
		{
			using (TempEnableCarrierConfiguration())
			using (TempSetSupportedCarriers())
			{
				var airlinesDetails = AirBookingCarrierConfigurationManager.SupportedAirlines;
				var etihad = airlinesDetails["607"];

				var commodityCollection = new AirlineConfigCommodityBusinessObjectCollection();

				foreach (var airlineConfigCommodity in etihad.Commodities.OrderBy(c => c.Description))
				{
					var commodityBizObj = new AirlineConfigCommodityBusinessObject(airlineConfigCommodity);
					commodityCollection.Add(commodityBizObj);
				}

				var findBox = new DummyFindBox
				{
					ListProvider = commodityCollection
				};

				using (var form = new AirlineCommodityPopupFormForTesting(findBox))
				{
					form.Show();
					var filterControl = form.FilterControl;
					var filter = filterControl.FilterBusinessObject as AirlineCommodityFilterStripBusinessObject;

					AssertEquals(7, filterControl.GridCollection.Count);

					filter.CodeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					filter.CodeFilter.Property = "A";
					filterControl.FirePerformSearch();
					AssertEquals(5, filterControl.GridCollection.Count);

					filter.CodeFilter.Property = "B";
					filterControl.FirePerformSearch();
					AssertEquals(0, filterControl.GridCollection.Count);
					AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "There are no records that match your search.");
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		#region Implement

		protected override Form GetFormToBashCore()
		{
			return new AirlineCommodityPopupForm(new DummyFindBox());
		}

		#endregion

		class AirlineCommodityPopupFormForTesting : AirlineCommodityPopupForm
		{
			public AirlineCommodityPopupFormForTesting(IFindBox findBox)
				: base(findBox)
			{
			}

			public Button OkButton => OkBtn;
			public new Button CancelButton => CancelBtn;

			public new ZFilterStripControl FilterControl => base.FilterControl;
		}

		IDisposable TempEnableCarrierConfiguration() => FreightDataRegistry.Instance.EnableAirBookingCarrierConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		IDisposable TempSetSupportedCarriers()
		{
			const string supportedCarriersJson = @"[
			{
				""prefix"": ""057"",
				""code"": ""AF"",
				""name"": ""Air France"",
				""options"": {
					""uldBooking"": ""NotSupported"",
					""looseBooking"": ""Required"",
					""allotmentBooking"": ""NotSupported"",
					""commodityCode"": ""NotSupported"",
					""requiredTermsAgreement"": false
				}
			},
			{
				""prefix"": ""020"",
				""code"": ""LH"",
				""name"": ""Lufthansa"",
				""options"": {
					""uldBooking"": ""NotSupported"",
					""looseBooking"": ""NotSupported"",
					""allotmentBooking"": ""NotSupported"",
					""commodityCode"": ""NotSupported"",
					""requiredTermsAgreement"": true
				}
			},
			{
				""prefix"": ""074"",
				""code"": ""KL"",
				""name"": ""KLM Royal Dutch Airlines"",
				""options"": {
					""uldBooking"": ""NotSupported"",
					""looseBooking"": ""Required"",
					""allotmentBooking"": ""NotSupported"",
					""commodityCode"": ""NotSupported"",
					""requiredTermsAgreement"": false
				},
			},
			{
				""prefix"": ""618"",
				""code"": ""SQ"",
				""name"": ""Singapore Airline"",
				""options"": {
					""uldBooking"": ""Optional"",
					""looseBooking"": ""Optional"",
					""allotmentBooking"": ""Optional"",
					""commodityCode"": ""Required"",
					""requiredTermsAgreement"": false
				},
				""commodities"": [],
				""products"": []
			},
			{
				""prefix"": ""607"",
				""code"": ""EY"",
				""name"": ""Etihad Airways"",
				""options"": {
					""uldBooking"": ""Optional"",
					""looseBooking"": ""Optional"",
					""allotmentBooking"": ""Optional"",
					""commodityCode"": ""Optional"",
					""requiredTermsAgreement"": false
				},
				""commodities"": [
					{
						""code"": ""GENERAL"",
						""description"": ""GENERAL CARGO"",
						""specialHandlingCodes"": ""GEN""
					},
					{
						""Code"": ""ACCESRY"",
						""Description"": ""ACCESSORIES"",
						""SpecialHandlingCodes"": ""GEN, HVY, HUM""
					},
					{
						""Code"": ""ACE"",
						""Description"": ""AIRCRAFT ENGINES"",
						""SpecialHandlingCodes"": ""GEN""
					},
					{
						""Code"": ""ACPTS"",
						""Description"": ""AIRCRAFT PARTS"",
						""SpecialHandlingCodes"": ""GEN""
					},
					{
						""Code"": ""ADHE"",
						""Description"": ""ADHESIVE"",
						""SpecialHandlingCodes"": ""GEN""
					},
					{
						""Code"": ""AERO"",
						""Description"": ""AEROSOL"",
						""SpecialHandlingCodes"": ""DGR""
					},
					{
						""code"": ""OTH"",
						""description"": ""OTHER""
					}
				],
				""products"": [
					{
						""Code"": ""AOG"",
						""Description"": ""Emirates AOG""
					},
					{
						""Code"": ""AWA"",
						""Description"": ""SkyWheels Premium Door-to-Door""
					},
					{
						""Code"": ""ACE"",
						""Description"": ""AIRCRAFT ENGINES""
					}
				]
			}]";

			var settings = new EBookingCarrierConfiguration
			{
				LastUpdatedTime = ZDateTime.Now,
				LastResponse = supportedCarriersJson
			};

			return FreightDataRegistry.Instance.EBookingCarrierConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}
	}
}
