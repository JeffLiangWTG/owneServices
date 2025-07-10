using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI.Testing;
using Enterprise.Customs.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TariffFindBoxPopupTest : FindBoxWrapperWithBorderWiseIntegrationTestCase<TariffFindBoxPopup>
	{
		[TestDate(2010, 2, 23)]
		public void TestSetTariffFilter()
		{
			using (var findBox = new ZCodeFindBox())
			{
				var popup = new TariffFindBoxPopup();
				AssertExceptionThrown(typeof(ArgumentException), () => popup.CreateTariffFilter(findBox));
			}

			using (var findBox = new TariffFindBox())
			{
				var popup = new TariffFindBoxPopup();

				//Test with default filter
				var filters = popup.CreateTariffFilter(findBox);
				AssertEquals("popup.tariffToSend", "", popup.tariffToSend);
				AssertBorderWiseFilters(filters, "E", popup.tariffToSend, "20100223");

				((IFindBox)findBox).Code = "2222";
				findBox.TariffInfo.DateForDutyRate = ZDateTime.Empty;
				filters = popup.CreateTariffFilter(findBox);

				AssertEquals("popup.tariffToSend", "2222", popup.tariffToSend); // Tariff is gotten from CodeBox when filter empty
				AssertBorderWiseFilters(filters, "E", popup.tariffToSend, "20100223"); //Date is Today if DateForDutyRate filter is empty

				findBox.TariffInfo.TariffType = TariffType.Import;
				findBox.TariffInfo.TariffCode = "44";
				findBox.TariffInfo.DateForDutyRate = new ZDateTime(2010, 2, 20);
				filters = popup.CreateTariffFilter(findBox);
				AssertEquals("popup.tariffToSend", "44", popup.tariffToSend); // CodeBox value is ignored when doesn't match filter
				AssertBorderWiseFilters(filters, "I", popup.tariffToSend, "20100220");

				((IFindBox)findBox).Code = "4422";
				filters = popup.CreateTariffFilter(findBox);
				AssertBorderWiseFilters(filters, "I", "4422", "20100220"); // Tariff is gotten from CodeBox when matches filter

				findBox.SetDataBinding(new TariffObjectForTesting(), "TS_TariffCode");
				findBox.BindToTariffPropertyInfo = "TS_TariffCodeTariffInfo";
				filters = popup.CreateTariffFilter(findBox);
				AssertBorderWiseFilters(filters, "E", "10", "20001010"); // TariffInfo is gotten from bound object if BindToTariffPropertyInfo specified
			}
		}

		[TestDate(2010, 2, 23)]
		public void TestSetTariffFilter_Should_ReturnImport_WhenCountryIsCanadaAndTariffTypeIsExport()
		{
			AssertSetTariffFilterCananda(TariffType.Export);
		}

		[TestDate(2010, 2, 23)]
		public void TestSetTariffFilter_Should_ReturnImport_WhenCountryIsCanadaAndTariffTypeIsImport()
		{
			AssertSetTariffFilterCananda(TariffType.Import);
		}

		[TestDate(2010, 2, 23)]
		public void TestGetBoundValue()
		{
			using (var findBox = new TariffFindBox())
			{
				var popup = new TariffFindBoxPopup();
				var objectForTesting = new TariffObjectForTesting();
				findBox.SetDataBinding(objectForTesting, "TS_TariffCode");
				findBox.BindToTariffPropertyInfo = "TS_TariffCodeTariffInfo";
				TariffPropertyInfoTest.AssertTariffInfo(popup.GetBoundValue(findBox), TariffType.Export, new ZDateTime(2000, 10, 10), "10");

				findBox.DataBindings.Clear();
				findBox.SetDataBinding(objectForTesting, "Collection.TS_TariffCode");
				findBox.BindToTariffPropertyInfo = "Collection.TS_TariffCodeOtherTariffInfo";
				TariffPropertyInfoTest.AssertTariffInfo(popup.GetBoundValue(findBox), TariffType.Import, new ZDateTime(2010, 2, 23), "2222");

				findBox.DataBindings.Clear();
				findBox.SetDataBinding(objectForTesting, "TS_TariffCode");
				findBox.BindToTariffPropertyInfo = "TS_NotExistingTariffInfo";
				AssertExceptionThrown(typeof(InvalidOperationException), () => popup.GetBoundValue(findBox));

				findBox.DataBindings.Clear();
				findBox.SetDataBinding(objectForTesting, "TS_TariffCode");
				findBox.BindToTariffPropertyInfo = "TS_TariffCodeTariffInfoWithWrongType";
				AssertExceptionThrown(typeof(InvalidOperationException), () => popup.GetBoundValue(findBox));
			}
		}

		public void TestSetReturnedData()
		{
			using (var findBox = new TariffFindBox())
			{
				var additionalData = new AdditionalDataForBorderWise("E", ZDateTime.Today, x => x.Replace(".", ""));
				var filters = new BorderWiseFilters { AdditionalData = additionalData };
				var popup = new TariffFindBoxPopup();

				AssertEquals("Pre-condition: FindBox code", string.Empty, ((IFindBox)findBox).Code);

				popup.tariffToSend = "9910";
				popup.SetReturnedData(filters, new BorderWiseInvoiceLine("9910", null), findBox);
				AssertEquals("FindBox code not changed when returned data match sent data", string.Empty, ((IFindBox)findBox).Code);

				popup.SetReturnedData(filters, new BorderWiseInvoiceLine("9910.00.00", null), findBox);
				AssertEquals("FindBox code when returned tariff doesn't match sent one", "99100000", ((IFindBox)findBox).Code);

				popup.SetReturnedData(filters, new BorderWiseInvoiceLine("9910.00.00", "10"), findBox);
				AssertEquals("FindBox code when tariff and stat codes are returned", "99100000 10", ((IFindBox)findBox).Code);
			}
		}

		protected override TariffFindBoxPopup CreateFindBoxPopup(AdditionalDataForBorderWise filterData) => (TariffFindBoxPopup)TariffFindBoxPopup.GetNewPopup();

		protected override IFindBox CreateFindBox(string initialFindBoxCode)
		{
			var findBox = new TariffFindBox { CurrentCode = initialFindBoxCode };
			findBox.TariffInfo.TariffType = TariffType.Import;

			return findBox;
		}

		protected override void AssertShowModalDoesNothingWhenNoExternalToolChosen(TariffFindBoxPopup findBoxWrapper, IFindBox findBox)
		{
			AssertNull($"{nameof(TariffFindBoxPopup.GetNewPopup)} returns a null wrapper when the registry isn't enabled.", findBoxWrapper);
		}

		static void AssertBorderWiseFilters(BorderWiseFilters filters, string expectedImpExp, string expectedTariff, string expectedDateForDutyRate)
		{
			AssertEquals("TariffType", expectedImpExp, filters.ImpExp);
			AssertEquals("TariffCode", expectedTariff, filters.Tariff);
			AssertEquals("DateForDutyRate", expectedDateForDutyRate, filters.DateForDutyRate);
		}

		static void AssertSetTariffFilterCananda(TariffType tariffType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			using (var findBox = new TariffFindBox())
			{
				var popup = new TariffFindBoxPopup();
				findBox.TariffInfo.TariffType = tariffType;

				var filters = popup.CreateTariffFilter(findBox);
				AssertEquals("popup.tariffToSend", "", popup.tariffToSend);
				AssertBorderWiseFilters(filters, "I", popup.tariffToSend, "20100223");
			}
		}
	}

	sealed class TariffObjectForTesting
	{
		public ZString TS_TariffCode { get; set; }

		public TariffPropertyInfo TS_TariffCodeTariffInfo => new TariffPropertyInfo(TariffType.Export, new ZDateTime(2000, 10, 10), "10");

		public TariffPropertyInfo TS_TariffCodeOtherTariffInfo => new TariffPropertyInfo(TariffType.Import, ZDateTime.Today, "2222");

		public ZString TS_TariffCodeTariffInfoWithWrongType { get; set; }

		public List<TariffObjectForTesting> Collection => new List<TariffObjectForTesting>(new[] { new TariffObjectForTesting() });
	}
}
