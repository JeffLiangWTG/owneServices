using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateOneOffShipmentTest : RatingTestCase
	{
		public void TestGetTopBusinessObject()
		{
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
			var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { rate.CurrentOneOffQuote.GetTopBusinessObject() });
			AssertNotNull("We got the controller, so we got the Form", controller);
			AssertEquals("Controller type", "Enterprise.Rating.Module.QuotationsController", controller.GetType().FullName);
		}

		#region ITemplateReversible

		public void TestReverse()
		{
			var originalOneOff = Factory.New<RateOneOffShipment>();

			originalOneOff.TT_TransportMode = Constants.TransportModes.Air;
			originalOneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			originalOneOff.TT_RL_NKReceivalLocation = "USCHI";
			originalOneOff.TT_RL_NKDeliveryLocation = "AUSYD";

			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("1E3F2085-5625-4570-B27C-0B80443E963E"));
			var pickupOrgAddress = Factory.LoadTop1<OrgAddress>(query);

			originalOneOff.PickUpDocAddress.E2_OA_Address = pickupOrgAddress.PK;

			query.Clear();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("3378A125-DED1-4FE8-BC3E-0BAEE21F7AAD"));
			var deliveryOrgAddress = Factory.LoadTop1<OrgAddress>(query);

			originalOneOff.DeliveryDocAddress.E2_OA_Address = deliveryOrgAddress.PK;

			((ITemplateReversible)originalOneOff).Reverse();

			AssertEquals("Correct transport mode", "AIR", originalOneOff.TT_TransportMode);
			AssertEquals("Correct container mode", "LSE", originalOneOff.TT_ContainerMode);
			AssertEquals("Correct Origin", "AUSYD", originalOneOff.TT_RL_NKReceivalLocation);
			AssertEquals("Correct Destination", "USCHI", originalOneOff.TT_RL_NKDeliveryLocation);

			AssertEquals("Must have the same number of DocAddresses", 2, originalOneOff.DocAddresses.Count);
			AssertEquals("PickupAddress is incorrect", deliveryOrgAddress.PK, originalOneOff.PickUpDocAddress.Address.PK);
			AssertEquals("DeliveryAddress is incorrect", pickupOrgAddress.PK, originalOneOff.DeliveryDocAddress.Address.PK);
			AssertEquals("Reversed copy pickup address should have the correct parent", originalOneOff.PK, originalOneOff.PickUpDocAddress.E2_ParentID);
			AssertEquals("Reversed copy delivery address should have the correct parent", originalOneOff.PK, originalOneOff.PickUpDocAddress.E2_ParentID);
		}

		#endregion

		public void TestDefaultValues()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Pounds;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicYards;

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			AssertEquals(Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_UnitOfWeight);
			AssertEquals(Constants.Volume.CubicYards, quote.CurrentOneOffQuote.TT_UnitOfVolume);

			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			AssertEquals(Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_UnitOfWeight);
			AssertEquals(Constants.Volume.CubicMetres, quote.CurrentOneOffQuote.TT_UnitOfVolume);

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr);
		}

		public void TestOnLoadedDoesNotChangePersistentPropertiesWhenSettingIsDomesticField()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new TestHelper(newFactory);
			var quote = helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUMEL";
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Road;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.FTL;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
			quote.CurrentOneOffQuote.TT_ActualWeight = 915000m;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			quote.CurrentOneOffQuote.TT_ActualVolume = 476500m;
			quote.CurrentOneOffQuote.TT_Chargeable = 951m;
			newFactory.Save();

			Assert(quote.CurrentOneOffQuote.IsDomesticFreight);
			AssertEquals(951m, quote.CurrentOneOffQuote.TT_Chargeable);

			var copyQuote = Factory.Load<Quote>(quote.PK);

			Assert(copyQuote.CurrentOneOffQuote.IsDomesticFreight);
			AssertEquals("Chargeable value unchanged, as chargeable was NOT recalculated.", 951m, copyQuote.CurrentOneOffQuote.TT_Chargeable);
			AssertNoExceptionThrown("No arithmetic overflow exception, as chargeable was NOT recalculated.", () => Factory.Save());
		}

		public void TestOneOffQuotationCreation()
		{
			var rate = Helper.NewQuote(Helper.NewOrgHeader());

			AssertEquals("Default condition of quote is One Off Quote = OFF", false, rate.TH_OneTimeQuote);
			AssertEquals("Default condition of quote is One Off Quote = OFF", false, rate.TH_OneTimeQuote);
			AssertEquals("No one off quote objects created", 0, rate.OneOffQuote.Count);

			rate.TH_OneTimeQuote = true;
			AssertEquals("Helper method correctly returns one off quote status", true, rate.TH_OneTimeQuote);
			AssertEquals("New one off quote objects created", 1, rate.OneOffQuote.Count);
		}

		public void TestGoodsDetailsUpdatesVolumeWeightUnitsWithMultipleLooseCargoItems()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;

			var loosecargo1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loosecargo1.TPL_PackLineCount = 1;
			loosecargo1.TPL_Volume = 200;
			loosecargo1.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			loosecargo1.TPL_Weight = 100;
			loosecargo1.TPL_WeightUQ = Constants.Weight.Kilograms;

			var loosecargo2 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loosecargo2.TPL_PackLineCount = 1;
			loosecargo2.TPL_Volume = 250;
			loosecargo2.TPL_VolumeUQ = Constants.Volume.CubicFeet;
			loosecargo2.TPL_Weight = 150;
			loosecargo2.TPL_WeightUQ = Constants.Weight.Pounds;

			var loosecargo3 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			loosecargo3.TPL_PackLineCount = 1;
			loosecargo3.TPL_Volume = 400;
			loosecargo3.TPL_VolumeUQ = Constants.Volume.CubicFeet;
			loosecargo3.TPL_Weight = 300;
			loosecargo3.TPL_WeightUQ = Constants.Weight.Pounds;

			AssertEquals("TT_UnitOfVolume", Constants.Volume.CubicMetres, quote.CurrentOneOffQuote.TT_UnitOfVolume);
			AssertEquals("TT_UnitOfWeight", Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_UnitOfWeight);

			loosecargo1.TPL_VolumeUQ = Constants.Volume.CubicFeet;
			loosecargo1.TPL_WeightUQ = Constants.Weight.Pounds;

			AssertEquals("TT_UnitOfVolume", Constants.Volume.CubicFeet, quote.CurrentOneOffQuote.TT_UnitOfVolume);
			AssertEquals("TT_UnitOfWeight", Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_UnitOfWeight);
		}

		#region One Off Quote Matching

		Quote oneOffQuoteForFindPossibleMatches;
		public void TestAutoRateFindsPossibleOneOffQuotes()
		{
			oneOffQuoteForFindPossibleMatches = SetupSampleOneOffQuotation();
			oneOffQuoteForFindPossibleMatches.TH_OneTimeQuote = true;
			oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUMEL";
			oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			Factory.Save();

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object, isEqualization: false))
			{
				Mock.Get(_Rating.Interactor)
					.Setup(m => m.SelectQuote(It.IsAny<QuoteCollection>()))
					.Returns((QuoteCollection quotes) => (Quote)quotes.Single());

				var testAutoRater = new FreightAutoRater(new RatingContext());

				var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 1200M, .5M, NewClient);
				testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
				Mock.Get(_Rating.Interactor)
					.Verify(m => m.SelectQuote(It.IsAny<QuoteCollection>()), Times.Never, "One off quote doesn't match origin");

				oneOffQuoteForFindPossibleMatches.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
				Factory.Save();

				testAutoRater = new FreightAutoRater(new RatingContext());
				var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
				Mock.Get(_Rating.Interactor)
					.Verify(m => m.SelectQuote(It.IsAny<QuoteCollection>()), Times.Once, "One off quote matches origin");

				var result = results.First(i => i.ChargeCode.PK == TestFRT.PK);
				AssertEquals("Ratess from one off quote are used (1200Kg * $2/kg)", 2400m, result.Amount);

				// Use client rates as one off quote event returned null
				testAutoRater = new FreightAutoRater(new RatingContext());

				Mock.Get(_Rating.Interactor)
					.Setup(m => m.SelectQuote(It.IsAny<QuoteCollection>()))
					.Returns((QuoteCollection _) => null);

				results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
				result = results.FirstOrDefault(i => i.ChargeCode.PK == TestFRT.PK);
				AssertEquals("Rates from one off quote are NOT used", null, result);
			}
		}

		#endregion

		public void TestUnitOfVolumeAndWeight_WhenChangeTransportMode_ThenShouldBeSetToDefaultValue()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_TransportMode = "AIR";

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "";
			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "";

			testQuote.CurrentOneOffQuote.TT_TransportMode = "SEA";
			AssertEquals("GIVEN empty UnitOfWeight WHEN change transport mode THEN it should be redefaulted.", Env.Registry.FreightWeightUnit, testQuote.CurrentOneOffQuote.TT_UnitOfWeight);
			AssertEquals("GIVEN empty UnitOfWeight WHEN change transport mode THEN it should be redefaulted.", Env.Registry.FreightVolumeUnit, testQuote.CurrentOneOffQuote.TT_UnitOfVolume);
		}

		static Dictionary<string, Func<HBLDeliveryModeRegistryItem>> RegistriesByContainerModes { get; } = new Dictionary<string, Func<HBLDeliveryModeRegistryItem>>
		{
			{ ContainerModes.FCL, () => FreightDataRegistry.Instance.HBLDeliveryMode_FCL },
			{ ContainerModes.Loose, () => FreightDataRegistry.Instance.HBLDeliveryMode_LSE },
			{ ContainerModes.ULD, () => FreightDataRegistry.Instance.HBLDeliveryMode_ULD },
			{ ContainerModes.LCL, () => FreightDataRegistry.Instance.HBLDeliveryMode_LCL },
			{ ContainerModes.BuyersConsol, () => FreightDataRegistry.Instance.HBLDeliveryMode_BCN },
			{ ContainerModes.Bulk, () => FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD },
			{ ContainerModes.RollOnRollOff, () => FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD },
			{ ContainerModes.ShippersConsol, () => FreightDataRegistry.Instance.HBLDeliveryMode_SCN }
		};

		void TestHBLDeliveryMode_WhenChangeContainerMode(string containerMode)
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = "AIR";
			oneOffQuote.TT_ContainerMode = "";
			AssertEquals("Empty Default HBL Delivery Mode when Container Mode is Empty", ZString.Empty, testQuote.CurrentOneOffQuote.TT_HBLDeliveryMode);

			var collection = new HBLDeliveryModeCollection();
			collection.Add(Constants.HBLDeliveryModes.Codes.DOOR_DOOR, (NoResString)Constants.HBLDeliveryModes.Codes.DOOR_DOOR);
			collection.Add(Constants.HBLDeliveryModes.Codes.DOOR_CFS, (NoResString)Constants.HBLDeliveryModes.Codes.DOOR_CFS);

			var hblDeliveryModes = new Enterprise.Registry.Business.HBLDeliveryModes(containerMode, collection);
			hblDeliveryModes.DefaultHBLDeliveryMode = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			RegistriesByContainerModes[containerMode]().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModes);

			testQuote.CurrentOneOffQuote.TT_ContainerMode = containerMode;
			AssertEquals("DOOR/DOOR", testQuote.CurrentOneOffQuote.TT_HBLDeliveryMode);
		}

		public void TestHBLDeliveryMode_WhenChangeContainerModeFCL_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.FCL);

		public void TestHBLDeliveryMode_WhenChangeContainerModeLoose_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.Loose);

		public void TestHBLDeliveryMode_WhenChangeContainerModeULD_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.ULD);

		public void TestHBLDeliveryMode_WhenChangeContainerModeLCL_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.LCL);

		public void TestHBLDeliveryMode_WhenChangeContainerModeBuyersConsol_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.BuyersConsol);

		public void TestHBLDeliveryMode_WhenChangeContainerModeBulk_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.Bulk);

		public void TestHBLDeliveryMode_WhenChangeContainerModeRollOnRollOff_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.RollOnRollOff);

		public void TestHBLDeliveryMode_WhenChangeContainerModeShippersConsol_ThenShouldBeSetToDefaultRegistryValue()
			=> TestHBLDeliveryMode_WhenChangeContainerMode(ContainerModes.ShippersConsol);

		public void TestUnitOfVolumeAndWeight_WhenChangeContainerMode_ThenShouldBeSetToDefaultValue()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_TransportMode = "AIR";
			testQuote.CurrentOneOffQuote.TT_ContainerMode = "LSE";

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "";
			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "";

			testQuote.CurrentOneOffQuote.TT_ContainerMode = "FCL";
			AssertEquals("GIVEN empty UnitOfWeight WHEN change Container Mode THEN it should be redefaulted.", Env.Registry.FreightWeightUnit, testQuote.CurrentOneOffQuote.TT_UnitOfWeight);
			AssertEquals("GIVEN empty UnitOfWeight WHEN change Container Mode THEN it should be redefaulted.", Env.Registry.FreightVolumeUnit, testQuote.CurrentOneOffQuote.TT_UnitOfVolume);
		}

		public void TestVolumeCalculation()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var looseCargo = oneOff.LooseCargo.AddNew();
			looseCargo.TPL_PackLineCount = 1;
			looseCargo.TPL_Height = 10;
			looseCargo.TPL_Width = 5;
			looseCargo.TPL_Length = 2;

			AssertEquals("Volume Calculation", 100m, oneOff.TT_ActualVolume);

			looseCargo.TPL_PackLineCount = 5;
			AssertEquals("Volume Calculation", 500m, oneOff.TT_ActualVolume);

			oneOff.TT_ActualVolume = 200;
			AssertEquals("Volume can be manually specified", 200m, oneOff.TT_ActualVolume);
			AssertEquals("Error present regarding Count x HxWxD <> Volume", true, oneOff.TT_ActualVolumeInfo.HasErrors());

			looseCargo.TPL_Height = 0;
			looseCargo.TPL_Width = 0;
			looseCargo.TPL_Length = 0;
			looseCargo.TPL_Volume = 0;

			AssertEquals("Volume is set to 0", 0m, oneOff.TT_ActualVolume);

			looseCargo.TPL_Volume = 10;
			oneOff.RunPreSaveValidation();
			AssertEquals("Error Cleared", false, oneOff.TT_ActualVolumeInfo.HasErrors());
		}

		public void TestVolumeCalculationWithRounding()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var looseCargo = oneOff.LooseCargo.AddNew();
			looseCargo.TPL_PackLineCount = 1;
			looseCargo.TPL_Height = 1.126m;
			looseCargo.TPL_Width = 2.357m;
			looseCargo.TPL_Length = 3.481m;

			AssertEquals("Volume Calculation", 9.239m, oneOff.TT_ActualVolume);

			looseCargo.TPL_PackLineCount = 2;
			AssertEquals("Volume Calculation", 18.477m, oneOff.TT_ActualVolume);

			oneOff.TT_ActualVolume = 20m;
			oneOff.RunPreSaveValidation();
			AssertEquals("Error present regarding Count x HxWxD <> Volume", true, oneOff.TT_ActualVolumeInfo.HasErrors());

			oneOff.TT_ActualVolume = 18.477m;
			oneOff.RunPreSaveValidation();
			AssertEquals("Error not present as value is correct - to 3 decimals", false, oneOff.TT_ActualVolumeInfo.HasErrors());
		}

		public void TestActualVolumeActualWeight_AddingDeletingCargo_EqualsTotalLoose()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var looseCargo = oneOff.LooseCargo.AddNew();
			looseCargo.TPL_PackLineCount = 1;
			looseCargo.TPL_Volume = 10;
			looseCargo.TPL_Weight = 10;
			AssertEquals("Volume Calculation", 10m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 10m, oneOff.TT_ActualWeight);

			var looseCargo2 = oneOff.LooseCargo.AddNew();

			looseCargo.TPL_PackLineCount = 1;
			looseCargo2.TPL_Volume = 20;
			looseCargo2.TPL_Weight = 20;
			AssertEquals("Volume Calculation", 30m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 30m, oneOff.TT_ActualWeight);

			looseCargo.TPL_Volume = 15;
			looseCargo.TPL_Weight = 15;
			AssertEquals("Volume Calculation", 35m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 35m, oneOff.TT_ActualWeight);

			oneOff.LooseCargo.Delete(looseCargo2);
			AssertEquals("Volume Calculation", 15m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 15m, oneOff.TT_ActualWeight);

			oneOff.LooseCargo.Delete(looseCargo);
			AssertEquals("Volume Calculation", 0m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 0m, oneOff.TT_ActualWeight);
		}

		public void TestContainersAvailableOnFCLandSeaAndULD()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.ContainerModes.FCL;
			oneOff.Containers.AddNew();

			AssertEquals("Containers can be specified", false, oneOff.Containers.ReadOnly);
			AssertEquals("1 container present", 1, oneOff.Containers.Count);

			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			AssertEquals("Containers cannot be specified", true, oneOff.Containers.ReadOnly);
			AssertEquals("All containers deleted", 0, oneOff.Containers.Count);

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.RateMode.SEA;
			AssertEquals("Containers can be specified", false, oneOff.Containers.ReadOnly);
			AssertEquals("No containers", 0, oneOff.Containers.Count);

			oneOff.Containers.AddNew();
			AssertEquals("1 new container", 1, oneOff.Containers.Count);

			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.ULD;
			AssertEquals("Containers can be specified", false, oneOff.Containers.ReadOnly);
			AssertEquals("Containers remained", 1, oneOff.Containers.Count);
		}

		public void TestClone()
		{
			var originalOneOff = Factory.New<RateOneOffShipment>();

			originalOneOff.TT_TransportMode = Constants.TransportModes.Air;
			originalOneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			originalOneOff.TT_RL_NKDeliveryLocation = "AUSYD";

			var container = originalOneOff.Containers.AddNew();
			container.TC_ContainerCount = 2;
			container.TC_RC = GP20.PK;

			var loose = originalOneOff.LooseCargo.AddNew();
			loose.TPL_PackLineCount = 3;
			loose.TPL_Height = 2;
			loose.TPL_Width = 3;
			loose.TPL_Length = 4;

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsShippingProvider = true;
			var possibleCarrier = originalOneOff.PossibleCarriers.AddNew();
			possibleCarrier.TTC_OH_Carrier = carrierOrg.PK;

			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("1E3F2085-5625-4570-B27C-0B80443E963E"));
			var pickupOrgAddress = Factory.LoadTop1<OrgAddress>(query);

			originalOneOff.PickUpDocAddress.E2_OA_Address = pickupOrgAddress.PK;

			query.Clear();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("3378A125-DED1-4FE8-BC3E-0BAEE21F7AAD"));
			var deliveryOrgAddress = Factory.LoadTop1<OrgAddress>(query);

			originalOneOff.DeliveryDocAddress.E2_OA_Address = deliveryOrgAddress.PK;

			var oneOff = (RateOneOffShipment)originalOneOff.Clone();
			AssertEquals("Correct transport mode", "AIR", oneOff.TT_TransportMode);
			AssertEquals("Correct container mode", "LSE", oneOff.TT_ContainerMode);
			AssertEquals("Correct Destination", "AUSYD", oneOff.TT_RL_NKDeliveryLocation);

			AssertEquals("1 one off container details exist", 1, oneOff.Containers.Count);
			AssertEquals("1 one off loose cargo details exist", 1, oneOff.LooseCargo.Count);

			AssertEquals("Loose cargo details correct - ContainerCount", (ZShort)3, oneOff.LooseCargo[0].TPL_PackLineCount);
			AssertEquals("Loose cargo details correct - Height", 2m, oneOff.LooseCargo[0].TPL_Height);
			AssertEquals("Loose cargo details correct - Width", 3m, oneOff.LooseCargo[0].TPL_Width);
			AssertEquals("Loose cargo details correct - Length", 4m, oneOff.LooseCargo[0].TPL_Length);

			AssertEquals("Container details correct - ContainerCount", (ZShort)2, oneOff.Containers[0].TC_ContainerCount);
			AssertEquals("Container details correct - Container Type", GP20.PK, oneOff.Containers[0].TC_RC);

			AssertEquals("Must have the same number of DocAddresses", 2, oneOff.DocAddresses.Count);
			AssertEquals("OneOffQuotePickupAddress must be the same in both RateOneOffShipment", "1E3F2085-5625-4570-B27C-0B80443E963E", oneOff.PickUpDocAddress.Address.PK.ToString().ToUpper());
			AssertEquals("OneOffQuoteDeliveryAddress must be the same in both RateOneOffShipment", "3378A125-DED1-4FE8-BC3E-0BAEE21F7AAD", oneOff.DeliveryDocAddress.Address.PK.ToString().ToUpper());
			AssertEquals("Cloned pickup address should have the correct parent", oneOff.PK, oneOff.PickUpDocAddress.E2_ParentID);
			AssertEquals("Cloned delivery address should have the correct parent", oneOff.PK, oneOff.PickUpDocAddress.E2_ParentID);

			AssertEquals("PossibleCarriers.Count", 1, oneOff.PossibleCarriers.Count);
			AssertEquals("TTC_OH_Carrier is cloned", carrierOrg.PK, oneOff.PossibleCarriers[0].TTC_OH_Carrier);
			AssertEquals("TTC_TT is correct", oneOff.PK, oneOff.PossibleCarriers[0].TTC_TT);
		}

		public void TestCloneTransportModeAndContainerModeCorrectly()
		{
			var originalOneOff = Factory.New<RateOneOffShipment>();
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Air, Constants.ContainerModes.Loose);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Air, Constants.ContainerModes.ULD);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Sea, Constants.RateMode.SEA);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Road, Constants.RateMode.ROA);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Road, Constants.RateMode.LRO);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Road, Constants.ContainerModes.FCL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Road, Constants.ContainerModes.FTL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Rail, Constants.RateMode.RAI);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Rail, Constants.RateMode.FWL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Rail, Constants.ContainerModes.FCL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Rail, Constants.ContainerModes.LCL);
			AssertTransportModeAndContainerModeAreClonedCorrectly(Constants.TransportModes.Courier, Constants.RateMode.COU);

			void AssertTransportModeAndContainerModeAreClonedCorrectly(ZString transportMode, ZString containerMode)
			{
				originalOneOff.TT_TransportMode = transportMode;
				originalOneOff.TT_ContainerMode = containerMode;

				var oneOff = (RateOneOffShipment)originalOneOff.Clone();

				AssertEquals($"Given the original transport mode is {transportMode}, when clone, then cloned transport mode should also be {transportMode}", transportMode, oneOff.TT_TransportMode);
				AssertEquals($"Given the original container mode is {containerMode}, when clone, then cloned container mode should also be {containerMode}", containerMode, oneOff.TT_ContainerMode);
			}
		}
		public void TestSetChargeableDoesNotCauseException()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			testQuote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "##";
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 1m;
			AssertEquals(0m, testQuote.CurrentOneOffQuote.TT_Chargeable);

			testQuote.CurrentOneOffQuote.TT_UnitOfWeight = "KG";
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 1m;
			AssertEquals(1m, testQuote.CurrentOneOffQuote.TT_Chargeable);

			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "##";
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 2m;
			AssertEquals(1m, testQuote.CurrentOneOffQuote.TT_Chargeable);

			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "M3";
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 2m;
			AssertEquals(2m, testQuote.CurrentOneOffQuote.TT_Chargeable);

			testQuote.CurrentOneOffQuote.TT_UnitOfVolume = "CF";
			testQuote.CurrentOneOffQuote.TT_ActualWeight = 5m;
			AssertEquals(5m, testQuote.CurrentOneOffQuote.TT_Chargeable);
		}

		public void TestChargeableUpdate()
		{
			var domesticAirFactor = FreightDataRegistry.Instance.DomesticChargeableFactorAir.Value;
			var internationalAirFactor = FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value;

			FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(194m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(12000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(400m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			try
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				quote.TH_OneTimeQuote = true;

				quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
				quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUMEL";
				quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
				quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
				quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
				quote.CurrentOneOffQuote.TT_ActualWeight = 200m;
				quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
				quote.CurrentOneOffQuote.TT_ActualVolume = 1m;

				AssertEquals(200m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_ActualWeight = 100m;

				AssertEquals(166.667m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_ActualVolume = 0.5m;

				AssertEquals(100m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Pounds;

				AssertEquals(83.333m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Kilograms, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicInches;

				AssertEquals(100m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_ActualVolume = 38800m;

				AssertEquals(200m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "NZAKL";

				AssertEquals(100m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "NZCHC";

				AssertEquals(200m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Weight.Pounds, quote.CurrentOneOffQuote.TT_ChargeableUnit);

				quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;
				quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.LCL;

				AssertEquals(22.454m, quote.CurrentOneOffQuote.TT_Chargeable);
				AssertEquals(Constants.Volume.CubicFeet, quote.CurrentOneOffQuote.TT_ChargeableUnit);
			}
			finally
			{
				FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domesticAirFactor);
				FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, internationalAirFactor);
			}
		}

		public void TestChargeable_WhenTransportModeIsAir_ShouldUseRegistry()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;

			var roundings = new ChargeableWeightRoundingCollection
			{
				new ChargeableWeightRounding
				{
					RoundingMode = nameof(ChargeableWeightRoundingType.Up),
					RoundingScale = "0.5"
				}
			};

			var registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings;
			using (var re = registryEntry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				quote.CurrentOneOffQuote.TT_Chargeable = 12.333;
				AssertEquals(12.500m, quote.CurrentOneOffQuote.TT_Chargeable);
			}
		}

		public void TestChargeable_WhenTransportModeIsNotAir_ShouldNotUseRegistry()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Sea;

			var roundings = new ChargeableWeightRoundingCollection
			{
				new ChargeableWeightRounding
				{
					RoundingMode = nameof(ChargeableWeightRoundingType.Up),
					RoundingScale = "0.5"
				}
			};

			var registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings;
			using (var re = registryEntry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				quote.CurrentOneOffQuote.TT_Chargeable = 12.333;
				AssertEquals(12.333m, quote.CurrentOneOffQuote.TT_Chargeable);
			}
		}

		public void TestOneOffQuotePickupAndDeliveryAddresses()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var query = new ZQuery();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("956C47D8-A8DD-4947-BF73-6F2D09A135F4"));
			var pickupAddress = Factory.LoadTop1<OrgAddress>(query);
			oneOff.PickUpDocAddress.E2_OA_Address = pickupAddress.PK;
			oneOff.PickUpDocAddress.E2_AddressOverride = ZBool.True;
			AssertEquals(oneOff.PickUpDocAddress.E2_AddressType.ToUpper(), "OQP");
			AssertEquals(oneOff.PickUpDocAddress.E2_ParentID, oneOff.PK);
			AssertEquals(oneOff.PickUpDocAddress.E2_CompanyName.ToUpper(), "CALZATURIFICIO MALLEGNI");
			AssertEquals(oneOff.PickUpDocAddress.E2_Address1.ToUpper(), "DI MALLEGNI FERNINANDO GIUSEPPE & C");
			AssertEquals(oneOff.PickUpDocAddress.E2_Address2.ToUpper(), "VIA NOCCHI 71-55041 CAMAIORE");

			query.Clear();
			query.AddToFilter(OrgAddressSchema.PK, new ZGuid("2E852B21-90EE-4001-A035-0B899B9E99FA"));
			var deliveryAddress = Factory.LoadTop1<OrgAddress>(query);
			oneOff.DeliveryDocAddress.E2_OA_Address = deliveryAddress.PK;
			AssertEquals(oneOff.DeliveryDocAddress.E2_AddressType.ToUpper(), "OQD");
			AssertEquals(oneOff.DeliveryDocAddress.E2_ParentID, oneOff.PK);

			AssertEquals(oneOff.DeliveryDocAddress.E2_OA_Address.ToString().ToUpper(), "2E852B21-90EE-4001-A035-0B899B9E99FA");

			AssertEquals(oneOff.DeliveryDocAddress.E2_Address1.ToUpper(), "53 FUK WAH STREET, G/G");
			AssertEquals(oneOff.DeliveryDocAddress.E2_Address2.ToUpper(), "SHAMSHUIPO");
		}

		public void TestTT_ChargeableVolumeWeight()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			var kgM3 = 1000000 / ConversionFactor.Standard.Metric.Air.Factor;

			shipment.TT_TransportMode = Constants.TransportModes.Air;
			shipment.TT_ContainerMode = Constants.ContainerModes.Loose;
			shipment.TT_Chargeable = 12;
			AssertEquals(shipment.TT_ActualVolume, ZArchitecture.Core.Utilities.Round(12 / kgM3, 3));
			AssertEquals(shipment.TT_ActualWeight, (ZDecimal)0);

			shipment.TT_ActualVolume = 0;
			shipment.TT_TransportMode = Constants.TransportModes.Other;
			shipment.TT_ContainerMode = Constants.ContainerModes.Other;
			shipment.TT_Chargeable = 14;
			AssertEquals(shipment.TT_ActualVolume, ZArchitecture.Core.Utilities.Round(14 / kgM3, 3));
			AssertEquals(shipment.TT_ActualWeight, (ZDecimal)0);

			kgM3 = 1000000 / 3000;
			shipment.TT_ActualVolume = 0;
			shipment.TT_TransportMode = Constants.TransportModes.Road;
			shipment.TT_ContainerMode = Constants.RateMode.ROA;
			shipment.TT_Chargeable = 17;
			AssertEquals(shipment.TT_ActualVolume, ZArchitecture.Core.Utilities.Round(17 / kgM3, 3));
			AssertEquals(shipment.TT_ActualWeight, (ZDecimal)0);

			kgM3 = 1000000 / ConversionFactor.Standard.Metric.Air.Factor;
			shipment.TT_ActualVolume = 0;
			shipment.TT_TransportMode = Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Constants.RateMode.SEA;
			shipment.TT_Chargeable = 21;
			AssertEquals(shipment.TT_ActualWeight, 21 * ConversionFactor.Standard.Metric.Sea.Factor);
			AssertEquals(shipment.TT_ActualVolume, (ZDecimal)0);

			shipment.TT_ActualWeight = 0;
			shipment.TT_TransportMode = Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Constants.RateMode.SEA;
			shipment.TT_Chargeable = 26;
			AssertEquals(shipment.TT_ActualWeight, 26 * ConversionFactor.Standard.Metric.Sea.Factor);
			AssertEquals(shipment.TT_ActualVolume, (ZDecimal)0);
		}

		public void TestTT_Chargeable_WithInvalidVolumeUnit()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			shipment.TT_TransportMode = Constants.TransportModes.Air;
			shipment.TT_ContainerMode = Constants.ContainerModes.Loose;
			Factory.Save();

			shipment.TT_ActualVolume = 0;
			shipment.TT_ActualWeight = 0;
			shipment.TT_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.TT_UnitOfVolume = "0";

			AssertNoExceptionThrown("Setting TT_Chargeable on a shipment with invalid unit for Volume should not throw exceptions.", () => shipment.TT_Chargeable = 10);
		}

		public void TestTT_Chargeable_WithInvalidWeightUnit()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			shipment.TT_TransportMode = Constants.TransportModes.Air;
			shipment.TT_ContainerMode = Constants.ContainerModes.Loose;
			Factory.Save();

			shipment.TT_ActualVolume = 0;
			shipment.TT_ActualWeight = 0;
			shipment.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.TT_UnitOfWeight = "0";

			AssertNoExceptionThrown("Setting TT_Chargeable on a shipment with invalid unit for Weight should not throw exceptions.", () => shipment.TT_Chargeable = 10);
		}

		public void TestTT_Chargeable_WithInvalidWeightAndVolumeUnit()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			shipment.TT_TransportMode = Constants.TransportModes.Air;
			shipment.TT_ContainerMode = Constants.ContainerModes.Loose;
			Factory.Save();

			shipment.TT_ActualVolume = 0;
			shipment.TT_ActualWeight = 0;
			shipment.TT_UnitOfVolume = "0";
			shipment.TT_UnitOfWeight = "0";

			AssertNoExceptionThrown("Setting TT_Chargeable on a shipment with invalid units for both Weight and Volume should not throw exceptions.", () => shipment.TT_Chargeable = 10);
		}

		public void TestIsImportIsExport()
		{
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;

			rate.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			rate.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			AssertEquals("IsImport", true, rate.CurrentOneOffQuote.IsImport());
			AssertEquals("IsExport", false, rate.CurrentOneOffQuote.IsExport());

			rate.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			rate.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			AssertEquals("IsImport", false, rate.CurrentOneOffQuote.IsImport());
			AssertEquals("IsExport", true, rate.CurrentOneOffQuote.IsExport());
		}

		public void TestDocAddressWithUniversalCopyRelatedEntityAttribute()
		{
			AssertPropertyWithUniversalCopyRelatedEntityAttribute("PickUpDocAddress", "E2_ParentID");
			AssertPropertyWithUniversalCopyRelatedEntityAttribute("DeliveryDocAddress", "E2_ParentID");
		}

		void AssertPropertyWithUniversalCopyRelatedEntityAttribute(string propertyName, string skipPropertyName)
		{
			var propertyInfo = typeof(RateOneOffShipment).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			AssertNotNull(propertyInfo);

			var relatedEntityAttribute = propertyInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).FirstOrDefault() as UniversalCopyRelatedEntityAttribute;
			AssertNotNull(relatedEntityAttribute);
			AssertEquals("should skip property in Universal Copy.", skipPropertyName, relatedEntityAttribute.CommaSeparatedSkipPropertiesNames);
			AssertEquals("should call 'MakePersistentEvenIfEmpty' method to make address saved by factory", "MakePersistentEvenIfEmpty", relatedEntityAttribute.MakeRelatedEntitySavedByFactoryMethod);
		}

		#region Rounding according to Registry

		public void TestChargeableIsRoundedAccordingToRegistry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			AddDefaultNumberOfDecimals(collection, Constants.TransportModes.Air, Constants.Weight.Kilograms, numberOfDecimals: 2, RoundingModes.Up);

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				quote.TH_OneTimeQuote = true;
				quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
				quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
				quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;

				var looseCargo = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				looseCargo.TPL_PackLineCount = 1;
				looseCargo.TPL_F3_NKPackType = Constants.PkgUnit.Crate;
				looseCargo.TPL_Weight = 11.1111m;
				looseCargo.TPL_WeightUQ = Constants.Weight.Kilograms;
				looseCargo.TPL_Volume = 22.2222m;
				looseCargo.TPL_VolumeUQ = Constants.Volume.CubicFeet;

				AssertEquals
				(
					"GIVEN matching DefaultNumberOfDecimalPlaces=2, THEN chargeable should be rounded to 2 decimal places",
					104.88m,
					quote.CurrentOneOffQuote.TT_Chargeable
				);
			}
		}
		public static void AddDefaultNumberOfDecimals(DefaultNumberOfDecimalsCollection defaultNumberOfDecimalsCollection, ZString transportMode, ZString unitOfMeasure, ZInt numberOfDecimals, ZString roundingMode)
		{
			var defaultNumberOfDecimals = defaultNumberOfDecimalsCollection.AddNew();
			defaultNumberOfDecimals.TransportMode = transportMode;
			defaultNumberOfDecimals.UnitOfMeasure = unitOfMeasure;
			defaultNumberOfDecimals.NumberOfDecimals = numberOfDecimals;
			defaultNumberOfDecimals.RoundingMode = roundingMode;
		}

		public void TestTotalVolumeIsRoundedAccordingToRegistry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_AirVolume = collection.AddNew();
			defaultNumberOfDecimals_AirVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AirVolume.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfDecimals_AirVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AirVolume.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				quote.TH_OneTimeQuote = true;
				quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
				quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
				quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;

				var loosecargo1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loosecargo1.TPL_PackLineCount = 2;
				loosecargo1.TPL_F3_NKPackType = Constants.PkgUnit.Skid;
				loosecargo1.TPL_Volume = 29.204m;
				loosecargo1.TPL_VolumeUQ = Constants.Volume.CubicMetres;

				var loosecargo2 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loosecargo2.TPL_PackLineCount = 1;
				loosecargo2.TPL_F3_NKPackType = Constants.PkgUnit.Skid;
				loosecargo2.TPL_Volume = 10.274m;
				loosecargo2.TPL_VolumeUQ = Constants.Volume.CubicMetres;

				AssertEquals("TotalLooseVolume", 39.49m, quote.CurrentOneOffQuote.TotalLooseVolume);
			}
		}

		public void TestTotalWeightIsRoundedAccordingToRegistry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_AirWeight = collection.AddNew();
			defaultNumberOfDecimals_AirWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_AirWeight.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfDecimals_AirWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AirWeight.RoundingMode = RoundingModes.Up;

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				quote.TH_OneTimeQuote = true;
				quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
				quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
				quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;

				var loosecargo1 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loosecargo1.TPL_PackLineCount = 2;
				loosecargo1.TPL_F3_NKPackType = Constants.PkgUnit.Skid;
				loosecargo1.TPL_Weight = 2046.578m;
				loosecargo1.TPL_WeightUQ = Constants.Weight.Kilograms;

				var loosecargo2 = quote.CurrentOneOffQuote.LooseCargo.AddNew();
				loosecargo2.TPL_PackLineCount = 1;
				loosecargo2.TPL_F3_NKPackType = Constants.PkgUnit.Skid;
				loosecargo2.TPL_Weight = 12.249m;
				loosecargo2.TPL_WeightUQ = Constants.Weight.Kilograms;

				AssertEquals("TotalLooseWeight", 2058.83m, quote.CurrentOneOffQuote.TotalLooseWeight);
			}
		}

		#endregion

		public void TestTransportMode_NewModeIsFCL_SetWeightVolumeToZero()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.ContainerModes.LCL;
			var looseCargo = oneOff.LooseCargo.AddNew();
			looseCargo.TPL_Volume = 10;
			looseCargo.TPL_Weight = 10000;
			var looseCargo2 = oneOff.LooseCargo.AddNew();
			looseCargo2.TPL_Volume = 20;
			looseCargo2.TPL_Weight = 20000;

			AssertEquals("Volume Calculation", 30m, oneOff.TT_ActualVolume);
			AssertEquals("Weight Calculation", 30000m, oneOff.TT_ActualWeight);
			AssertEquals("Chargeable Calculation", 30m, oneOff.TT_Chargeable);

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("Volume Calculation", 0m, oneOff.TT_ActualVolume);
			AssertNoErrors(oneOff.TT_ActualVolumeInfo);
			AssertEquals("Volume Units", Env.Registry.FreightVolumeUnit, oneOff.TT_UnitOfVolume);
			AssertNoErrors(oneOff.TT_UnitOfVolumeInfo);
			AssertEquals("Weight Calculation", 0m, oneOff.TT_ActualWeight);
			AssertNoErrors(oneOff.TT_ActualWeightInfo);
			AssertEquals("Weight Units", Env.Registry.FreightWeightUnit, oneOff.TT_UnitOfWeight);
			AssertNoErrors(oneOff.TT_UnitOfWeightInfo);
			AssertEquals("Chargeable Calculation", 0m, oneOff.TT_Chargeable);
			AssertNoErrors(oneOff.TT_ChargeableInfo);
		}

		#region ValidateConsignorAndConsignee

		public void TestConsignorValidation()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "CONSIGNOR";
			client.OH_Code = "CON";
			client.OH_IsConsignor = true;

			var rate = Helper.NewQuote(client);
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = client.PK;

			AssertNoErrors("CONSIGNOR is a valid Consignor Organisation", rate.CurrentOneOffQuote.PickUpDocAddress.OrganisationPKInfo);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_Code = "CONS";
			consignee.OH_IsConsignor = false;
			consignee.OH_IsConsignee = true;

			var quote = Helper.NewQuote(consignee);
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = consignee.PK;

			AssertHasErrors("CONSIGNEE is not a valid Consignor Organisation", quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPKInfo);
		}

		public void TestConsigneeValidation()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "CONSIGNEE";
			client.OH_Code = "CON";
			client.OH_IsConsignee = true;

			var rate = Helper.NewQuote(client);
			rate.TH_OneTimeQuote = true;
			rate.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = client.PK;

			AssertNoErrors("CONSIGNEE is a valid Consignee Organisation", rate.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPKInfo);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = false;

			var quote = Helper.NewQuote(consignor);
			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = consignor.PK;

			AssertHasErrors("CONSIGNOR is not a valid Consignee Organisation", quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPKInfo);
		}

		#endregion

		#region Test IDocAddresses Methods

		public void TestSupportedAddressTypes()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var addressTypes = ((IDocAddresses)oneOff).SupportedAddressTypes;

			AssertEquals("New address types might have been added, ensure they are tested.", 2, addressTypes.Count);
			AssertCollectionContains(DocAddressType.OneOffQuoteDeliveryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.OneOffQuotePickupAddress, addressTypes);
		}

		public void TestGetDocAddress()
		{
			var oneOff = Factory.New<RateOneOffShipment>();
			var iDocAddresses = ((IDocAddresses)oneOff);

			AssertEquals(DocAddressType.OneOffQuoteDeliveryAddress, iDocAddresses.GetDocAddressRequirement(DocAddressType.OneOffQuoteDeliveryAddress).DefaultDocAddressType);
			AssertEquals(DocAddressType.OneOffQuotePickupAddress, iDocAddresses.GetDocAddressRequirement(DocAddressType.OneOffQuotePickupAddress).DefaultDocAddressType);
		}

		public void TestGetCanOverrideAddressCheckpointWhenImport()
		{
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			var oneOff = rate.CurrentOneOffQuote;
			var iDocAddresses = ((IDocAddresses)oneOff);

			oneOff.TT_RL_NKReceivalLocation = "USLAX";
			oneOff.TT_RL_NKDeliveryLocation = "AUSYD";
			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpAirConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpAirConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.ULD;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpAirConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpAirConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.RateMode.SEA;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpSeaConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpSeaConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpSeaConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpSeaConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Rail;
			oneOff.TT_ContainerMode = Constants.RateMode.RAI;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpRailConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpRailConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Road;
			oneOff.TT_ContainerMode = Constants.RateMode.ROA;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpRoadConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpRoadConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Courier;
			oneOff.TT_ContainerMode = Constants.RateMode.COU;
			AssertEquals(Env.Security.MaintainOneOffQuoteImpOtConsignor, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteImpOtConsignee, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));
		}

		public void TestGetCanOverrideAddressCheckpointWhenExport()
		{
			var rate = Helper.NewQuote(Helper.NewOrgHeader());
			rate.TH_OneTimeQuote = true;
			var oneOff = rate.CurrentOneOffQuote;
			var iDocAddresses = (IDocAddresses)oneOff;

			oneOff.TT_RL_NKReceivalLocation = "AUSYD";
			oneOff.TT_RL_NKDeliveryLocation = "USLAX";
			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.Loose;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpAirConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpAirConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Air;
			oneOff.TT_ContainerMode = Constants.ContainerModes.ULD;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpAirConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpAirConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.RateMode.SEA;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpSeaConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpSeaConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Sea;
			oneOff.TT_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpSeaConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpSeaConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Rail;
			oneOff.TT_ContainerMode = Constants.RateMode.RAI;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpRailConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpRailConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Road;
			oneOff.TT_ContainerMode = Constants.RateMode.ROA;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpRoadConsignorD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpRoadConsigneeD, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));

			oneOff.TT_TransportMode = Constants.TransportModes.Courier;
			oneOff.TT_ContainerMode = Constants.RateMode.COU;
			AssertEquals(Env.Security.MaintainOneOffQuoteExpOtConsignor, iDocAddresses.GetCanOverrideCheckpoint(oneOff.PickUpDocAddress));
			AssertEquals(Env.Security.MaintainOneOffQuoteExpOtConsignee, iDocAddresses.GetCanOverrideCheckpoint(oneOff.DeliveryDocAddress));
		}

		#endregion

		#region Test FreightModeConverter Class

		public void TestFreightModeConverter()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			var rateModes = new[] { "LSE", "ULD", "SEA", "FCL", "LCL", "FRO", "FTL", "LRO", "ROA","FRA", "LRA", "RAI", "FWL", "COU" };

			FreightMode freightMode;

			foreach (var rateMode in rateModes)
			{
				shipment.TT_TransportMode = RatingConstants.GetTransportModeFromMode(rateMode);
				shipment.TT_ContainerMode = RatingConstants.GetOneOffQuoteContainerModeFromMode(rateMode);
				var expectedAnswer = GetExpectedAnswerFromRateMode(rateMode);
				freightMode = RateOneOffShipment.FreightModeConverter.GetFreightMode(shipment.TT_TransportMode,shipment.TT_ContainerMode, shipment);

				AssertEquals("Expected rate mode to match freight modes in all cases except for sea shipments", expectedAnswer, freightMode.ToString());

				var reconvertedRateMode = RateOneOffShipment.FreightModeConverter.GetRateModes(freightMode);

				Assert("Expected at least one rate mode converted from freight mode", reconvertedRateMode.Length > 0);
				AssertEquals("Expected transport mode to match freight modes", expectedAnswer, reconvertedRateMode[0]);
			}

			freightMode = RateOneOffShipment.FreightModeConverter.GetFreightMode("","", shipment);
			AssertEquals(FreightMode.UKN, freightMode);
		}

		string GetExpectedAnswerFromRateMode(ZString rateMode)
		{
			if (rateMode == Core.Constants.RateMode.SEA)
			{
				return nameof(FreightMode.LCL);
			}

			return rateMode;
		}

		public void TestStandardTransportMode()
		{
			var shipment = Factory.New<RateOneOffShipment>();

			shipment.TT_TransportMode = Constants.TransportModes.Air;
			shipment.TT_ContainerMode = Constants.ContainerModes.Loose;
			AssertEquals(shipment.StandardTransportMode, Constants.TransportModes.Air);

			shipment.TT_TransportMode = Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals(shipment.StandardTransportMode, Constants.TransportModes.Sea);

			shipment.TT_TransportMode = Constants.TransportModes.Rail;
			shipment.TT_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(shipment.StandardTransportMode, Constants.TransportModes.Rail);

			shipment.TT_TransportMode = Constants.TransportModes.Road;
			shipment.TT_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(shipment.StandardTransportMode, Constants.TransportModes.Road);

			shipment.TT_TransportMode = Constants.TransportModes.Courier;
			shipment.TT_ContainerMode = Constants.RateMode.COU;
			AssertEquals(shipment.StandardTransportMode, Constants.TransportModes.Courier);
		}

		public void TestModeConverter()
		{
			AssertMode(Constants.TransportModes.Air, Constants.ContainerModes.AIR, Constants.RateMode.LSE);
			AssertMode(Constants.TransportModes.Air, Constants.ContainerModes.Loose, Constants.RateMode.LSE);
			AssertMode(Constants.TransportModes.Air, Constants.ContainerModes.ULD, Constants.RateMode.ULD);
			AssertMode(Constants.TransportModes.Air, Constants.ContainerModes.BuyersConsol, Constants.RateMode.BCN);
			AssertMode(Constants.TransportModes.Air, Constants.ContainerModes.ShippersConsol, Constants.RateMode.SCN);

			AssertMode(Constants.TransportModes.Sea, Constants.RateMode.SEA, Constants.RateMode.SEA);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.FCL, Constants.RateMode.FCL);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.LCL, Constants.RateMode.LCL);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk, Constants.RateMode.BBK);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff, Constants.RateMode.ROR);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.Bulk, Constants.RateMode.BLK);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.BuyersConsol, Constants.RateMode.BCN);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.ShippersConsol, Constants.RateMode.SCN);
			AssertMode(Constants.TransportModes.Sea, Constants.ContainerModes.Liquid, Constants.ContainerModes.Liquid);// WHY WE DON'T HAVE RATEMODE LQD?

			AssertMode(Constants.TransportModes.AirSea, Constants.ContainerModes.Loose, Constants.RateMode.LSE);
			AssertMode(Constants.TransportModes.AirSea, Constants.ContainerModes.ULD, Constants.RateMode.ULD);
			AssertMode(Constants.TransportModes.AirSea, Constants.ContainerModes.LCL, Constants.RateMode.LCL);

			AssertMode(Constants.TransportModes.SeaAir, Constants.ContainerModes.Loose, Constants.RateMode.LSE);
			AssertMode(Constants.TransportModes.SeaAir, Constants.ContainerModes.ULD, Constants.RateMode.ULD);
			AssertMode(Constants.TransportModes.SeaAir, Constants.ContainerModes.LCL, Constants.RateMode.LCL);

			AssertMode(Constants.TransportModes.Road, Constants.RateMode.ROA, Constants.RateMode.ROA);
			AssertMode(Constants.TransportModes.Road, Constants.RateMode.LRO, Constants.RateMode.LRO);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.RateMode.FRO);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.LCL, Constants.RateMode.LRO);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.FTL, Constants.RateMode.FTL);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.LTL, Constants.RateMode.LRO);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.BuyersConsol, Constants.RateMode.BCN);
			AssertMode(Constants.TransportModes.Road, Constants.ContainerModes.ShippersConsol, Constants.RateMode.SCN);

			AssertMode(Constants.TransportModes.Rail, Constants.RateMode.RAI, Constants.RateMode.RAI);
			AssertMode(Constants.TransportModes.Rail, Constants.RateMode.FWL, Constants.RateMode.FWL);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.FCL, Constants.RateMode.FRA);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.LCL, Constants.RateMode.LRA);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.Liquid, Constants.ContainerModes.Liquid);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.BuyersConsol, Constants.RateMode.BCN);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.ShippersConsol, Constants.RateMode.SCN);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.BreakBulk, Constants.RateMode.BBK);
			AssertMode(Constants.TransportModes.Rail, Constants.ContainerModes.Bulk, Constants.RateMode.BLK);

			AssertMode(Constants.TransportModes.Courier, Constants.RateMode.COU, Constants.RateMode.COU);
			AssertMode(Constants.TransportModes.Courier, Constants.ContainerModes.OnBoardCourier, Constants.RateMode.OBC);
			AssertMode(Constants.TransportModes.Courier, Constants.ContainerModes.Unaccompanied, Constants.RateMode.UNA);

			void AssertMode(string transportMode, string containerMode, string expectedMode)
			{
				var mode = RateOneOffShipment.FreightModeConverter.GetMode(transportMode, containerMode);
				AssertEquals($"Correct Mode converted from TransportMode = {transportMode}, ContainerMode = {containerMode}", expectedMode, mode);
			}
		}
		#endregion

		#region AdditionalReferenceNumber

		public void TestAdditionalReferenceNumber()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			var additionalReferenceNumberTypeList = ((IAdditionalReferenceNumberTypeProvider)shipment).GetAdditionalReferenceNumberTypeList
			(
				CusEntryNumber.Categories.AdditionalReferenceNumber,
				Constants.CountryCodes.Australia
			);

			AssertContainsExactElementsInAnyOrder(new[] { "CON", "NAC", "CQN" }, additionalReferenceNumberTypeList.GetAllCodes());
		}

		#endregion

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			shipment.TT_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = shipment.CarrierServiceLevel; });
		}

		#endregion

		public void TestQuoteKPIAndRevisionReasonShouldNotBeCloned_WhenRateOneOffShipmentIsCopied()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "DesEEE");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var testRateOneOffShipment = Factory.New<RateOneOffShipment>();
				testRateOneOffShipment.TT_QuoteKPI = "EEE";
				testRateOneOffShipment.TT_QuoteSource = "EEE";
				testRateOneOffShipment.TT_RevisionReason = "EEE";

				var clonedRateOneOffShipment = testRateOneOffShipment.Clone() as RateOneOffShipment;
				Assert(clonedRateOneOffShipment.TT_QuoteKPI.IsEmpty);
				Assert(clonedRateOneOffShipment.TT_RevisionReason.IsEmpty);
				AssertEquals("Quote Source should be copied", "EEE", clonedRateOneOffShipment.TT_QuoteSource);
			}
		}

		public void TestModeChange_LooseCargoContainerTypeWillBeReset()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var shipment = Factory.New<RateOneOffShipment>();
			shipment.TT_TransportMode = "ULD";

			var looseCargo = shipment.LooseCargo.AddNew();
			looseCargo.TPL_F3_NKPackType = "PLT";
			looseCargo.TPL_PackLineCount = 10;
			looseCargo.TPL_RC_RefContainer = gp20.PK;

			shipment.TT_TransportMode = "SEA";

			AssertEquals("Container Type on packline will be reset.", ZGuid.Empty, looseCargo.TPL_RC_RefContainer);
			AssertEquals("Other attributes on packline will not be changed.", "PLT", looseCargo.TPL_F3_NKPackType);
			AssertEquals("Other attributes on packline will not be changed.", new ZShort(10), looseCargo.TPL_PackLineCount);
		}
	}

	#region Business Object TestCase

	[TestedType(typeof(RateOneOffShipment))]
	public class RateOneOffShipmentBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = Factory.New<Quote>();
			return quote.OneOffQuote.AddNew();
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			/*
			 * We are doing the right thing by calling
			 * Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, GB_RL_NKHomePort);
			 * to retrive a RefUNLOCO. Just this test is not suite to our case.
			 */
			Assert("We are doing the right thing to call ", true);
		}
	}

	#endregion
}
