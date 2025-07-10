using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Builders.Testing
{
	sealed class ContainerBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateWeightAndVolume()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();
			containerBO.JC_GrossVolume = 26.801m;
			containerBO.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			var container = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("GrossVolumeValue", 26.801m, container.GrossVolume.Value);
			AssertEquals("GrossVolumeUnit", Core.Constants.Volume.CubicMetres, container.GrossVolume.Unit.Code);

			container = new ContainerBuilder().Build(containerBO, context, unitOfVolume: Core.Constants.Volume.CubicFeet);
			Assert("GrossVolumeValue", Core.Constants.Volume.Convert(26.801m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicFeet) == container.GrossVolume.Value);
			AssertEquals("GrossVolumeUnit", Core.Constants.Volume.CubicFeet, container.GrossVolume.Unit.Code);
		}

		public void TestPopulateContainerMode()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();
			containerBO.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, container.ContainerMode.Code);
		}

		public void TestRenameDemurrageAndStorage()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();

			containerBO.DepartureTruckWaitCost = 200.12m;
			containerBO.DepartureTruckWaitTime = new ZDateTime(2019, 10, 1);
			containerBO.ArrivalTruckWaitCost = 220.34m;
			containerBO.ArrivalTruckWaitTime = new ZDateTime(2019, 10, 3);
			containerBO.ArrivalCTOStorageCost = 230.56m;
			containerBO.JC_ArrivalCTOStorageStartDate = new ZDateTime(2019, 10, 5);
			containerBO.ArrivalCTOStorageDays = 3;
			containerBO.ArrivalCarrierDetentionCost = 240.78m;
			containerBO.ArrivalCarrierDetentionDays = 9;

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("DepartureTruckWaitCost", 200.12m, container.DepartureTruckWaitCost);
			AssertEquals("DepartureTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 1), container.DepartureTruckWaitTime);
			AssertEquals("ArrivalTruckWaitCost", 220.34m, container.ArrivalTruckWaitCost);
			AssertEquals("ArrivalTruckWaitTime", new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 3), container.ArrivalTruckWaitTime);
			AssertEquals("ArrivalCTOStorageCost", 230.56m, container.ArrivalCTOStorageCost);
			AssertEquals("ArrivalCTOStorageDays", (ZByte)3, container.ArrivalCTOStorageDays);
			AssertEquals("ArrivalCTOStorageStartDate", new ZDateTime(2019, 10, 5), container.ArrivalCTOStorageStartDate);
			AssertEquals("ArrivalCarrierDetentionCost", 240.78m, container.ArrivalCarrierDetentionCost);
			AssertEquals("ArrivalCarrierDetentionDays", (ZByte)9, container.ArrivalCarrierDetentionDays);
		}

		public void TestPopulateVolumeFromPackLines()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var containerBO = Factory.New<ForwardingContainer>();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 3m;
			packline1.JL_ActualVolumeUQ = "CF";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5m;
			packline2.JL_ActualVolumeUQ = "CF";

			containerBO.PackLines.Add(packline1);
			containerBO.PackLines.Add(packline2);

			var packLineBuilder = new PackingLineBuilder();
			var container = new ContainerBuilder().Build(
				containerBO,
				context,
				shipment.OuterPackLines.Cast<PackLine>().Select(p => packLineBuilder.Build(p)).ToArray(), "KG", "CF");

			AssertEquals(8m, container.Volume.Value);
		}

		public void TestPopulateMeasures()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();
			var container = new ContainerBuilder().Build(containerBO, context);

			Assert("OverhangLengthValue", container.OverhangLength.Value.IsEmpty);
			Assert("OverhangLengthValue", container.OverhangLength.Unit.Code == Core.Constants.Length.Feet);
			Assert("OverhangWidthUnit", container.OverhangWidth.Value.IsEmpty);
			Assert("OverhangWidthUnit", container.OverhangWidth.Unit.Code == Core.Constants.Length.Feet);

			containerBO.JC_TotalWidth = 4m;
			containerBO.JC_TotalLength = 10m;
			container = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("OverhangLength", 10m, container.OverhangLength.Value);
			AssertEquals("OverhangLength", Core.Constants.Length.Feet, container.OverhangLength.Unit.Code);
			AssertEquals("OverhangWidth", 4m, container.OverhangWidth.Value);
			AssertEquals("OverhangWidth", Core.Constants.Length.Feet, container.OverhangWidth.Unit.Code);
		}

		public void TestPopulateDepotCustomsReferences()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();
			containerBO.JC_ExportDepotCustomsReference = "EXREF";
			containerBO.JC_ImportDepotCustomsReference = "IMREF";

			var container = new ContainerBuilder().Build(containerBO, context);

			AssertEquals("EXREF", container.ExportDepotCustomsReference);
			AssertEquals("IMREF", container.ImportDepotCustomsReference);
		}

		public void TestContainerID()
		{
			var context = new CommonContext(Factory);
			var expectedIdentifier = "ContainerID";
			var containerBO = Factory.New<ForwardingContainer>();

			var container1 = new ContainerBuilder().Build(containerBO, context, containerID: expectedIdentifier);
			AssertEquals("Should use the passed in identifier", expectedIdentifier, container1.Identifier);

			var container2 = new ContainerBuilder().Build(containerBO, context);
			AssertEquals("Should use the container PK if no identifier passed", containerBO.PK, container2.Identifier);
		}

		public void TestPopulateCustomerLoadReference()
		{
			var context = new CommonContext(Factory);
			var containerBO = Factory.New<ForwardingContainer>();

			AssertNullOrEmpty(new ContainerBuilder().Build(containerBO, context).CustomerLoadReference);

			AddAdditionalReferenceNumber("NUM1_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
			AddAdditionalReferenceNumber("NUM2_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC);
			AddAdditionalReferenceNumber("NUM3_1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UBR);
			AddAdditionalReferenceNumber("NUM1_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference);
			AddAdditionalReferenceNumber("NUM2_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoAdvice);
			AddAdditionalReferenceNumber("NUM3_1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference);

			AddAdditionalReferenceNumber("NUM1", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM2", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM3", Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM1", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM2", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM3", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM4", Core.Constants.CountryCodes.UnitedStates, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM5", ZString.Empty, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);
			AddAdditionalReferenceNumber("NUM1", ZString.Empty, Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);

			AssertEquals("NUM1, NUM2, NUM3, NUM4, NUM5", new ContainerBuilder().Build(containerBO, context).CustomerLoadReference);

			void AddAdditionalReferenceNumber(ZString entryNum, ZString countryCode, ZString entryType)
			{
				var additionalReferenceNumber = containerBO.AdditionalReferenceNumbers.AddNew();
				additionalReferenceNumber.CE_EntryNum = entryNum;
				additionalReferenceNumber.CE_RN_NKCountryCode = countryCode;
				additionalReferenceNumber.CE_EntryType = entryType;
			}
		}
	}
}
