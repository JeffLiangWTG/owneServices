using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class RateOneOffShipmentRatingAdapterTest : RatingTestCase
	{
		public void TestInvoicingSupporter()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(QuotedBookingState.QuoteOnly, quotedBooking.ObjectState);
			AssertType<QuoteInvoicingSupporter>(quote.CurrentOneOffQuote.RatingAdapter.InvoicingSupporter);
		}

		public void TestAdapterTypeAndID()
		{
			var oneOffQuote = Helper.NewQuote(Helper.NewOrgHeader());
			oneOffQuote.TH_OneTimeQuote = true;
			var oneOffShipment = oneOffQuote.CurrentOneOffQuote;
			AssertEquals(AdapterType.OneOffQuote, oneOffShipment.RatingAdapter.AdapterType);
			AssertEquals(oneOffQuote.TH_QuoteNumber, oneOffShipment.RatingAdapter.OperationalJobCode);
			AssertEquals(oneOffQuote.TH_QuoteNumber, oneOffShipment.RatingAdapter.JobID);
		}

		public void TestImportBroker()
		{
			var oneOffShipment = Factory.New<RateOneOffShipment>();
			AssertNull(oneOffShipment.RatingAdapter.ImportBroker);
		}

		public void TestExportBroker()
		{
			var oneOffShipment = Factory.New<RateOneOffShipment>();
			AssertNull(oneOffShipment.RatingAdapter.ExportBroker);
		}

		public void TestCarrier()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsAirLine = true;
			Quote rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			AssertEquals(carrier.PK, rate.CurrentOneOffQuote.RatingAdapter.Carrier.PK);
			rate.CurrentOneOffQuote.TT_OH_Carrier = ZGuid.Empty;
			AssertNull(rate.CurrentOneOffQuote.RatingAdapter.Carrier);
			Assert(rate.CurrentOneOffQuote.RatingAdapter.Creditors[""].Count == 0);
		}

		public void TestCreditor()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsAirLine = true;
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			rate.CurrentOneOffQuote.TT_OH_Creditor = creditor.PK;

			Assert(rate.CurrentOneOffQuote.RatingAdapter.Creditors.AllOrgs.Contains(carrier));
			Assert(rate.CurrentOneOffQuote.RatingAdapter.Creditors.AllOrgs.Contains(creditor));
		}

		public void TestPossibleCarriers()
		{
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_IsAirLine = true;
			var carrier2 = Factory.New<OrgHeader>();
			carrier1.OH_IsAirLine = true;
			var carrier3 = Factory.New<OrgHeader>();
			carrier3.OH_IsAirLine = true;

			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			var oneOffQuote = rate.CurrentOneOffQuote;
			Assert(!rate.CurrentOneOffQuote.RatingAdapter.PossibleCarriers.Any());

			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			oneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 },
				rate.CurrentOneOffQuote.RatingAdapter.PossibleCarriers);
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 },
				rate.CurrentOneOffQuote.RatingAdapter.Creditors.AllOrgs);

			// Same org in both Carrier and PossibleCarriers...
			rate.CurrentOneOffQuote.TT_OH_Carrier = carrier1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 },
				rate.CurrentOneOffQuote.RatingAdapter.PossibleCarriers);
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 },
				rate.CurrentOneOffQuote.RatingAdapter.Creditors.AllOrgs);

			rate.CurrentOneOffQuote.TT_OH_Carrier = carrier3.PK;
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 },
				rate.CurrentOneOffQuote.RatingAdapter.PossibleCarriers);
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2, carrier3 },
				rate.CurrentOneOffQuote.RatingAdapter.Creditors.AllOrgs);
		}

		public void TestFreightModeDefaults()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("Local Currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			AssertEquals("Should have a oneoff quote", 1, rate.OneOffQuote.Count);
			AssertEquals("TransportMode", "", rate.CurrentOneOffQuote.TT_TransportMode);
			AssertEquals("ContainerMode", "", rate.CurrentOneOffQuote.TT_ContainerMode);
			AssertEquals("FreightMode", FreightMode.UKN, rate.CurrentOneOffQuote.RatingAdapter.FreightMode);
		}

		public void TestMeasures_Weight_ULD_ShouldUseWeightFromPackLines()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_RH_NKCommodity = "AUTO";
			oneOff.TT_UnitOfWeight = "KG";
			oneOff.TT_ContainerMode = "ULD";

			var packLine1 = oneOff.LooseCargo.AddNew();
			packLine1.TPL_Weight = 100;
			packLine1.TPL_WeightUQ = "KG";
			packLine1.TPL_RC_RefContainer = Helper.Containers["20GP"].PK;

			var packLine2 = oneOff.LooseCargo.AddNew();
			packLine2.TPL_Weight = 0.5;
			packLine2.TPL_WeightUQ = "T";
			packLine2.TPL_RC_RefContainer = Helper.Containers["40GP"].PK;

			var adapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var weight = (RateablePartList)measures.GetPartList(MeasureType.Weight);
			AssertEquals("KG", weight.WeightUnit);
			RatingTestUtils.AssertArraysAreEquivalent(new[]
			{
				new { Weight = 100m, ContainerTypePk = Helper.Containers["20GP"].PK.ToGuid(), CommodityCode = "AUTO" },
				new { Weight = 500m, ContainerTypePk = Helper.Containers["40GP"].PK.ToGuid(), CommodityCode = "AUTO" }
			}, weight.ToArray());
		}

		public void TestMeasures_Weight_NonULD_ShouldUseGoodsWeight()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_RH_NKCommodity = "AUTO";
			oneOff.TT_ContainerMode = "LSE";

			var packLine = oneOff.LooseCargo.AddNew();
			packLine.TPL_Weight = 100;
			packLine.TPL_WeightUQ = "KG";
			packLine.TPL_RC_RefContainer = Helper.Containers["20GP"].PK;

			oneOff.TT_ActualWeight = 200;
			oneOff.TT_UnitOfWeight = "KG";

			var adapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var measure = (JobLevelPart)measures.GetPartList(MeasureType.Weight);
			AssertEquals("KG", measure.WeightUnit);
			AssertEquals("for nun ULD jobs it should come from goods, even if there are packlines", 200m, measure.Weight);
		}

		public void TestMeasures_Volume_ULD_ShouldUseVolumeFromPackLines()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_RH_NKCommodity = "AUTO";
			oneOff.TT_UnitOfVolume = "M3";
			oneOff.TT_ContainerMode = "ULD";

			var packLine1 = oneOff.LooseCargo.AddNew();
			packLine1.TPL_Volume = 2;
			packLine1.TPL_VolumeUQ = "M3";
			packLine1.TPL_RC_RefContainer = Helper.Containers["20GP"].PK;

			var packLine2 = oneOff.LooseCargo.AddNew();
			packLine2.TPL_Volume = 6000000;
			packLine2.TPL_VolumeUQ = "CC";
			packLine2.TPL_RC_RefContainer = Helper.Containers["40GP"].PK;

			var adapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var volume = (RateablePartList)measures.GetPartList(MeasureType.Volume);
			AssertEquals("M3", volume.VolumeUnit);
			RatingTestUtils.AssertArraysAreEquivalent(new[]
			{
				new { Volume = 2m, ContainerTypePk = Helper.Containers["20GP"].PK.ToGuid(), CommodityCode = "AUTO" },
				new { Volume = 6m, ContainerTypePk = Helper.Containers["40GP"].PK.ToGuid(), CommodityCode = "AUTO" }
			}, volume.ToArray());
		}

		public void TestMeasures_Volume_NonULD_ShouldUseGoodsVolume()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_RH_NKCommodity = "AUTO";
			oneOff.TT_ContainerMode = "LSE";

			var packLine = oneOff.LooseCargo.AddNew();
			packLine.TPL_Volume = 2;
			packLine.TPL_VolumeUQ = "M3";
			packLine.TPL_RC_RefContainer = Helper.Containers["20GP"].PK;

			oneOff.TT_ActualVolume = 5;
			oneOff.TT_UnitOfVolume = "M3";

			var adapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var measure = (JobLevelPart)measures.GetPartList(MeasureType.Volume);
			AssertEquals("M3", measure.VolumeUnit);
			AssertEquals("for nun ULD jobs it should come from goods, even if there are packlines", 5m, measure.Volume);
		}

		public void TestMeasures_PackageAndUnit()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_RH_NKCommodity = "AUTO";

			var packLine1 = oneOff.LooseCargo.AddNew();
			packLine1.TPL_PackLineCount = 2;
			packLine1.TPL_F3_NKPackType = "BOX";

			var packLine2 = oneOff.LooseCargo.AddNew();
			packLine2.TPL_PackLineCount = 5;
			packLine2.TPL_F3_NKPackType = "CTN";

			var adapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			var packages = (RateablePartList)measures.GetPartList(MeasureType.Package);
			RatingTestUtils.AssertArraysAreEquivalent(new[]
			{
				new { PackageCount = 2m, CommodityCode = "AUTO" },
				new { PackageCount = 5m, CommodityCode = "AUTO" }
			}, packages.ToArray());

			var units = (RateablePartList)measures.GetPartList(MeasureType.Unit);
			RatingTestUtils.AssertArraysAreEquivalent(new[]
			{
				new { UnitCount = 2m, PackageType = "BOX", CommodityCode = "AUTO" },
				new { UnitCount = 5m, PackageType = "CTN", CommodityCode = "AUTO" }
			}, units.ToArray());
		}

		public void TestMeasures_Chargeable()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			oneOff.TT_UnitOfWeight = Constants.Weight.Kilograms;
			oneOff.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			oneOff.TT_Chargeable = 333.33m;

			var measures = (RateableMeasureSet)oneOff.RatingAdapter.RateableMeasures;
			AssertEquals(333.33m, measures.GetActual(MeasureType.Chargeable));
		}

		public void TestAutoRatingMeasures_Shipment()
		{
			var oneOff = Factory.New<RateOneOffShipment>();

			var measures = (RateableMeasureSet)oneOff.RatingAdapter.RateableMeasures;
			AssertEquals(1m, measures.GetActual(MeasureType.Shipment));
		}

		public void TestAutoRatingPaymentTerms()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			var spotQuote = Factory.NewWithValidTestData<RateOneOffShipment>();
			spotQuote.TT_TransportMode = Constants.TransportModes.Sea;
			spotQuote.TT_ContainerMode = Constants.ContainerModes.LCL;
			spotQuote.TT_RL_NKReceivalLocation = "AUSYD";
			spotQuote.TT_RL_NKDeliveryLocation = "CNSHA";
			spotQuote.TT_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			var adapterPaymentTerms = spotQuote.RatingAdapter.PaymentTerm.PaymentTermInfoCollection;
			AssertEquals(2, adapterPaymentTerms.Count);
			AssertEquals(Constants.IncoTerms.FreeOnBoard, adapterPaymentTerms[0].Value);
			AssertEquals(PaymentTermType.Incoterm, adapterPaymentTerms[0].InfoType);
			spotQuote.TT_RL_NKDeliveryLocation = "AUMEL";
			AssertEquals("Pre-condition to using domestic payment terms instead of inco terms on a spot quote", true, spotQuote.IsDomesticFreight);
			spotQuote.TT_IncoTerm = Constants.DomesticPaymentTerms.Collect;
			adapterPaymentTerms = spotQuote.RatingAdapter.PaymentTerm.PaymentTermInfoCollection;
			AssertEquals("Expected inco payment term to be replaced by domestic payment term", 2, adapterPaymentTerms.Count);
			AssertEquals(Constants.DomesticPaymentTerms.Collect, adapterPaymentTerms[0].Value);
			AssertEquals(PaymentTermType.DomesticPaymentTerm, adapterPaymentTerms[0].InfoType);
		}

		public void TestAutoRatingServiceLevel()
		{
			Quote rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_RS_NKServiceLevel = "";
			var freightInfo = (IAutoRatingFreightInfo)rate.CurrentOneOffQuote.RatingAdapter;
			AssertEquals("STD", freightInfo.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("STD", freightInfo.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));
			rate.CurrentOneOffQuote.TT_RS_NKServiceLevel = "EXP";
			AssertEquals("EXP", freightInfo.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("STD", freightInfo.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));
		}

		public void TestMonetaryValues()
		{
			Quote rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.TT_ValueOfGoods = 55m;
			rate.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rate.CurrentOneOffQuote.TT_InsureVal = 65m;
			rate.CurrentOneOffQuote.TT_RX_NKInsureValCurr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			MoneyType monetaryValues = rate.CurrentOneOffQuote.RatingAdapter.MonetaryValues;
			var converter = rate.CurrentOneOffQuote.RatingAdapter.CurrencyConverter;
			AssertEquals(55m, monetaryValues.GetMoney(MoneyType.ValueType.GoodsValue, GlbCompany.CurrentCompany.LocalCurrency, converter as CurrencyConverter).Amount);
			AssertEquals(65m, monetaryValues.GetMoney(MoneyType.ValueType.InsuranceValue, GlbCompany.CurrentCompany.LocalCurrency, converter as CurrencyConverter).Amount);
		}

		public void TestWithDifferentGlbCompany()
		{
			Quote testQuote;
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "NEW";
			company1.GC_RN_NKCountryCode = Constants.CountryCodes.Ukraine;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company1.Branches.Add(branch1);
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			}

			testQuote.TH_OneTimeQuote = true;
			AssertEquals(testQuote.TH_GC, ((IAutoRatingGlbCompany)testQuote.CurrentOneOffQuote.RatingAdapter).Company.PK);
		}

		public void TestRateableMeasures_Containers()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.Mode = "ULD";

			// Just to make sure that the job has other than default units (KG/M3), that it doesn't affect calculation
			quote.CurrentOneOffQuote.TT_UnitOfWeight = "T";
			quote.CurrentOneOffQuote.TT_UnitOfVolume = "CC";

			#region 20GP

			var cnt20GP = quote.CurrentOneOffQuote.Containers.AddNew();
			cnt20GP.TC_ContainerCount = 2;
			cnt20GP.TC_RC = Helper.Containers["20GP"].PK;

			var cnt20GP_2 = quote.CurrentOneOffQuote.Containers.AddNew();
			cnt20GP_2.TC_ContainerCount = 4;
			cnt20GP_2.TC_RC = Helper.Containers["20GP"].PK;

			var pkg20GP = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pkg20GP.TPL_F3_NKPackType = "PLT";
			pkg20GP.TPL_PackLineCount = 10;
			pkg20GP.TPL_Weight = 1.0;
			pkg20GP.TPL_WeightUQ = "T";
			pkg20GP.TPL_Volume = 5;
			pkg20GP.TPL_VolumeUQ = "M3";
			pkg20GP.TPL_RC_RefContainer = Helper.Containers["20GP"].PK;

			#endregion

			#region 40GP

			var cnt40GP = quote.CurrentOneOffQuote.Containers.AddNew();
			cnt40GP.TC_ContainerCount = 3;
			cnt40GP.TC_RC = Helper.Containers["40GP"].PK;

			var pkg40GP = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pkg40GP.TPL_F3_NKPackType = "BOX";
			pkg40GP.TPL_PackLineCount = 1;
			pkg40GP.TPL_Weight = 200.0;
			pkg40GP.TPL_WeightUQ = "KG";
			pkg40GP.TPL_Volume = 2;
			pkg40GP.TPL_VolumeUQ = "M3";
			pkg40GP.TPL_RC_RefContainer = Helper.Containers["40GP"].PK;

			var pkg40GP_2 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pkg40GP_2.TPL_F3_NKPackType = "BOX";
			pkg40GP_2.TPL_PackLineCount = 1;
			pkg40GP_2.TPL_Weight = 300.0;
			pkg40GP_2.TPL_WeightUQ = "KG";
			pkg40GP_2.TPL_Volume = 1000000;
			pkg40GP_2.TPL_VolumeUQ = "CC";
			pkg40GP_2.TPL_RC_RefContainer = Helper.Containers["40GP"].PK;

			#endregion

			#region 20FR

			var cnt20FR = quote.CurrentOneOffQuote.Containers.AddNew();
			cnt20FR.TC_ContainerCount = 5;
			cnt20FR.TC_RC = Helper.Containers["20FR"].PK;

			#endregion

			var quotedBookingAdapter = new RateOneOffShipmentRatingAdapter(quote.CurrentOneOffQuote);
			var measures = (RateableMeasureSet)quotedBookingAdapter.RateableMeasures;

			var containers = measures.GetAllContainers();
			var actualContainers = containers.Select(c => new
			{
				c.ContainerTypePk,
				c.ContainerCount,
				c.ContainerWeightInKG,
				c.ContainerVolumeInM3
			});

			RatingTestUtils.AssertArraysAreEquivalent(new[]
				{
					new
					{
						ContainerTypePk = Helper.Containers["20GP"].PK.ToGuid(),
						ContainerCount = 6,
						ContainerWeightInKG = 1000m,
						ContainerVolumeInM3 = 5m
					},
					new
					{
						ContainerTypePk = Helper.Containers["40GP"].PK.ToGuid(),
						ContainerCount = 3,
						ContainerWeightInKG = 500m,
						ContainerVolumeInM3 = 3m
					},
					new
					{
						ContainerTypePk = Helper.Containers["20FR"].PK.ToGuid(),
						ContainerCount = 5,
						ContainerWeightInKG = 0m,
						ContainerVolumeInM3 = 0m
					}
				},
				actualContainers.ToArray());
		}
	}
}
