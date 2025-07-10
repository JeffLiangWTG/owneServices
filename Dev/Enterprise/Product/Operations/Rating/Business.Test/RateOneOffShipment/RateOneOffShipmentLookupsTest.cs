using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateOneOffShipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompanyTariffLevelOverrideList()
		{
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();
			var compTariff = Factory.New<CompanyTariff>();
			var compTariff2 = Factory.New<CompanyTariff>();
			var compTariff3 = Factory.New<CompanyTariff>();
			Factory.Save();
			var list = new CodeDescriptionPairList();
			list.AddPair("1", "Global or Company Tariff Level 1");
			list.AddPair("2", "Global or Company Tariff Level 2");
			list.AddPair("3", "Company Tariff Level 3");
			list.Sort();
			var oneOffQuote = Factory.New<RateOneOffShipment>();
			AssertNotNull("OneOffQuoteSourceList list should exist", oneOffQuote.Lookups.CompanyTariffLevelList);
			AssertContainsExactElementsInAnyOrder(
				"CompanyTariffLevelList should match the expected list",
				list,
				oneOffQuote.Lookups.CompanyTariffLevelList
			);
		}

		public void TestEquipments()
		{
			var shipment = Factory.New<RateOneOffShipment>();
			var shipmentLookups = new RateOneOffShipmentLookups(shipment);
			CodeDescriptionPairList fclEquipmentTypes = new FCLEquipmentNeededList(false);
			CodeDescriptionPairList lclEquipmentTypes = new LCLAIREquipmentNeededList(false);

			shipment.TT_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Core.Constants.RateMode.SEA;
			AssertEquals("Equipment should be FCL", fclEquipmentTypes, shipmentLookups.Equipments);
			shipment.TT_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Equipment should be FCL", fclEquipmentTypes, shipmentLookups.Equipments);
			shipment.TT_TransportMode = Core.Constants.TransportModes.Road;
			shipment.TT_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Equipment should be FCL", fclEquipmentTypes, shipmentLookups.Equipments);
			shipment.TT_TransportMode = Core.Constants.TransportModes.Rail;
			shipment.TT_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Equipment should be FCL", fclEquipmentTypes, shipmentLookups.Equipments);
			shipment.TT_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.TT_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Equipment should be LCL", lclEquipmentTypes, shipmentLookups.Equipments);
		}

		public void TestOneOffQuoteKPIList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "KPI EEE");
			list.AddPair("FFF", "KPI FFF");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var oneOffQuote = Factory.New<RateOneOffShipment>();
				AssertNotNull("OneOffQuoteKPIList list should exist", oneOffQuote.Lookups.OneOffQuoteKPIList);

				AssertContainsExactElementsInAnyOrder(
					"OneOffQuoteKPIList should be equivalent to the expected list",
					list,
					oneOffQuote.Lookups.OneOffQuoteKPIList
				);
			}
		}

		public void TestOneOffQuoteSourceList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("AAA", "Source AAA");
			list.AddPair("BBB", "Source BBB");
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var oneOffQuote = Factory.New<RateOneOffShipment>();
				AssertNotNull("OneOffQuoteSourceList list should exist", oneOffQuote.Lookups.OneOffQuoteSourceList);

				var expectedList = list.Cast<CodeDescriptionPair>().Select(pair => $"{pair.Code}|{pair.Description}").ToArray();
				var actualList = oneOffQuote.Lookups.OneOffQuoteSourceList
					.Cast<CodeDescriptionPair>()
					.Select(pair => $"{pair.Code}|{pair.Description}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder("OneOffQuoteSourceList should match the expected list", expectedList, actualList);
			}
		}

		public void TestOneOffQuoteRevisionReasonList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CCC", "Revision CCC");
			list.AddPair("DDD", "Revision DDD");
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				var oneOffQuote = Factory.New<RateOneOffShipment>();
				AssertNotNull("OneOffQuoteRevisionReasonList list should exist", oneOffQuote.Lookups.OneOffQuoteRevisionReasonList);

				var expectedList = list.Cast<CodeDescriptionPair>().Select(item => $"{item.Code}|{item.Description}").ToArray();
				var actualList = oneOffQuote.Lookups.OneOffQuoteRevisionReasonList.Cast<CodeDescriptionPair>().Select(item => $"{item.Code}|{item.Description}").ToArray();

				AssertContainsExactElementsInAnyOrder(
					"OneOffQuoteRevisionReasonList should match the expected list",
					expectedList,
					actualList
				);
			}
		}
	}
}
