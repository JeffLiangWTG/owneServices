using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.CarrierConnect.Test;

public class AutoratedConsolCreatorTest : TestCaseWithFactory
{
	public void TestCreateFCLConsol()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, chargeableOverride: 5, chargeableUnit: Core.Constants.Volume.CubicMetres);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, chargeableOverride: 35.315m, chargeableUnit: Core.Constants.Volume.CubicFeet);
		var charge1 = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var charge2 = JobTestHelpers.CreateChargeDto("ODOC", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.FCL,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "567",
				NamedAccounts = new string[] { "234" },
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				PerContainerCommodity = [
					new() {	Container = "20GP", ContainerQuality = "NOR", Commodity = "GEN" },
					new() { Container = "40GP", ContainerQuality = "NOR", Commodity = "GEN" },
				],
				Charges = [charge1, charge2],
			},
			ChargesToApply = [charge1.ChargeID]
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		CombineAssertions(() =>
		{
			AssertEquals(consol.JK_TransportMode, Core.Constants.TransportModes.Sea);
			AssertEquals(consol.JK_ConsolMode, Core.Constants.ContainerModes.FCL);
			AssertEquals(consol.JK_RL_NKLoadPort, "AUSYD");
			AssertEquals(consol.JK_RL_NKDischargePort, "USLAX");
			AssertEquals(consol.AutoratingDate, effectiveOn);
			AssertEquals(consol.JK_CarrierContractNumber, "123");
			AssertEquals("should use STD as default value for Service Level", consol.JK_AWBServiceLevel, "STD");
			AssertEquals(consol.JK_PrepaidCollect, Core.Constants.PaymentType.Prepaid);
			AssertEquals(consol.JK_OA_ShippingLineAddress, abcOrg.MainAddress.PK);
			AssertEquals(consol.JK_RH_NKConsolCommodity, "");

			AssertHasExactNumbers(consol,
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN, "567"),
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "234"));

			AssertHasExactContainers(consol, "NOR", container1Dto, container2Dto);

			AssertHasSingleShipment(consol, 16000, Core.Constants.Weight.Kilograms, 0, Core.Constants.Volume.CubicMetres, 13);

			AssertHasExactCharges(consol, charge1);
		});

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	public void TestCreateULDConsol()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, volume: 20, chargeableOverride: 5, chargeableUnit: Core.Constants.Weight.Kilograms);
		// 2.205 Lb ~= 1 Kg
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, volume: 30, chargeableOverride: 2.2045m, chargeableUnit: Core.Constants.Weight.Pounds);
		var charge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Air,
				ContainerMode = Core.Constants.ContainerModes.ULD,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "788",
				NamedAccounts = ["234"],
				CarrierServiceLevel = "EXP",
				PerContainerCommodity = [],
				Charges = [charge],
			},
			ChargesToApply = [charge.ChargeID]
		};
		Env.Registry.ConsolPaymentTerm = Core.Constants.PaymentType.Collect;

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		CombineAssertions(() =>
		{
			AssertEquals(consol.JK_TransportMode, Core.Constants.TransportModes.Air);
			AssertEquals(consol.JK_ConsolMode, Core.Constants.ContainerModes.ULD);
			AssertEquals(consol.JK_RL_NKLoadPort, "AUSYD");
			AssertEquals(consol.JK_RL_NKDischargePort, "USLAX");
			AssertEquals(consol.AutoratingDate, effectiveOn);
			AssertEquals(consol.JK_CarrierContractNumber, "123");
			AssertEquals(consol.JK_AWBServiceLevel, "EXP");
			AssertEquals("should use default PaymentTerm value from registry when one is not supplied", consol.JK_PrepaidCollect, Core.Constants.PaymentType.Collect);
			AssertEquals(consol.JK_OA_ShippingLineAddress, abcOrg.MainAddress.PK);
			AssertEquals(consol.JK_RH_NKConsolCommodity, "");

			AssertHasExactNumbers(consol,
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN, "788"),
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "234"));

			AssertHasExactContainers(consol, "", container1Dto, container2Dto);

			// Weight = 5000 * 2 + 2000 * 3 = 16000. Volume = 20 * 2 + 30 * 3 = 130. Chargeable = 0.999... * 3 + 5 * 2
			AssertHasSingleShipment(consol, 16000, Core.Constants.Weight.Kilograms, 130, Core.Constants.Volume.CubicMetres, 13m);

			AssertHasExactCharges(consol, charge);
		});

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	public void TestCreateLSEConsol()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, volume: 50);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, volume: 35.315m, volumeUnit: Core.Constants.Volume.CubicFeet);
		var charge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Air,
				ContainerMode = Core.Constants.ContainerModes.Loose,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "111",
				NamedAccounts = new string[] { "234" },
				CarrierServiceLevel = "STD",
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge],
			},
			ChargesToApply = [charge.ChargeID]
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		CombineAssertions(() =>
		{
			AssertEquals(consol.JK_TransportMode, Core.Constants.TransportModes.Air);
			AssertEquals(consol.JK_ConsolMode, Core.Constants.ContainerModes.Loose);
			AssertEquals(consol.JK_RL_NKLoadPort, "AUSYD");
			AssertEquals(consol.JK_RL_NKDischargePort, "USLAX");
			AssertEquals(consol.AutoratingDate, effectiveOn);
			AssertEquals(consol.JK_CarrierContractNumber, "123");
			AssertEquals(consol.JK_AWBServiceLevel, "STD");
			AssertEquals(consol.JK_PrepaidCollect, Core.Constants.PaymentType.Prepaid);
			AssertEquals(consol.JK_OA_ShippingLineAddress, abcOrg.MainAddress.PK);
			AssertEquals(consol.JK_RH_NKConsolCommodity, "GEN");
			Assert(consol.Containers.IsNullOrEmpty());

			AssertHasExactNumbers(consol,
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN, "111"),
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "234"));

			AssertHasSingleShipment(consol, 16000, Core.Constants.Weight.Kilograms, 103, Core.Constants.Volume.CubicMetres, 17166.667m);

			AssertHasExactCharges(consol, charge);
		});

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	public void TestCreateLCLConsol()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, volume: 50);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, volume: 35.315m, volumeUnit: Core.Constants.Volume.CubicFeet);
		var charge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.LCL,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "222",
				NamedAccounts = new string[] { "234" },
				CarrierServiceLevel = "STD",
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge],
			},
			ChargesToApply = [charge.ChargeID]
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		CombineAssertions(() =>
		{
			AssertEquals(consol.JK_TransportMode, Core.Constants.TransportModes.Sea);
			AssertEquals(consol.JK_ConsolMode, Core.Constants.ContainerModes.LCL);
			AssertEquals(consol.JK_RL_NKLoadPort, "AUSYD");
			AssertEquals(consol.JK_RL_NKDischargePort, "USLAX");
			AssertEquals(consol.AutoratingDate, effectiveOn);
			AssertEquals(consol.JK_CarrierContractNumber, "123");
			AssertEquals(consol.JK_AWBServiceLevel, "STD");
			AssertEquals(consol.JK_PrepaidCollect, Core.Constants.PaymentType.Prepaid);
			AssertEquals(consol.JK_OA_ShippingLineAddress, abcOrg.MainAddress.PK);
			AssertEquals(consol.JK_RH_NKConsolCommodity, "GEN");
			Assert(consol.Containers.IsNullOrEmpty());

			AssertHasExactNumbers(consol,
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN, "222"),
				(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "234"));

			AssertHasSingleShipment(consol, 16000, Core.Constants.Weight.Kilograms, 103m, Core.Constants.Volume.CubicMetres, 103m);

			AssertHasExactCharges(consol, charge);
		});

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
		Assert(true);
	}

	public void TestCreateConsol_ApplyZeroCharges_False()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, chargeableOverride: 5, chargeableUnit: Core.Constants.Volume.CubicMetres);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, chargeableOverride: 35.315m, chargeableUnit: Core.Constants.Volume.CubicFeet);
		var charge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var zeroCharge = JobTestHelpers.CreateChargeDto("ODOC", 0, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.FCL,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "567",
				NamedAccounts = ["234"],
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge, zeroCharge],
			},
			ChargesToApply = [charge.ChargeID, zeroCharge.ChargeID],
			ApplyZeroCharges = false,
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		AssertHasExactCharges(consol, charge);

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	public void TestCreateConsol_ApplyZeroCharges_True()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, chargeableOverride: 5, chargeableUnit: Core.Constants.Volume.CubicMetres);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, chargeableOverride: 35.315m, chargeableUnit: Core.Constants.Volume.CubicFeet);
		var charge = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var zeroCharge = JobTestHelpers.CreateChargeDto("ODOC", 0, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.FCL,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "567",
				NamedAccounts = ["234"],
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge, zeroCharge],
			},
			ChargesToApply = [charge.ChargeID, zeroCharge.ChargeID],
			ApplyZeroCharges = true,
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		AssertHasExactCharges(consol, charge, zeroCharge);

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	public void TestCreateConsol_DoesNotApplyInclusiveCharges()
	{
		// Arrange
		var abcOrg = JobTestHelpers.CreateOrgHeader("ABC", "Alpha Beta");
		var effectiveOn = new ZDate(2020, 1, 1);
		var container1Dto = JobTestHelpers.CreateContainerDto("20GP", count: 2, weight: 5000, chargeableOverride: 5, chargeableUnit: Core.Constants.Volume.CubicMetres);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = JobTestHelpers.CreateContainerDto("40GP", count: 3, weight: 2000, chargeableOverride: 35.315m, chargeableUnit: Core.Constants.Volume.CubicFeet);
		var charge1 = JobTestHelpers.CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var inclusiveCharge = JobTestHelpers.CreateChargeDto("INCL", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		inclusiveCharge.IsInclusiveCalculator = true;

		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.FCL,
				ServiceProvider = new(abcOrg),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "567",
				NamedAccounts = ["234"],
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge1, inclusiveCharge],
			},
			ChargesToApply = [charge1.ChargeID, inclusiveCharge.ChargeID]
		};

		// Act
		var consol = new AutoratedConsolCreator(dto).CreateJob() as ForwardingConsol;

		// Assert
		AssertHasExactCharges(consol, charge1);

		// Required to release and dispose mutexes on Consol Shipment.
		consol.GetApportionments().ReleaseMutexes();
	}

	#region Implementation

	void AssertHasExactNumbers(ForwardingConsol consol, params (string entryType, string reference)[] numbers) =>
		AssertCollectionsExactEquivalance(
			consol.Numbers.Cast<CusEntryNumber>(),
			numbers,
			(number, expected) =>
				number.CE_EntryNum == expected.reference
				&& number.CE_EntryType == expected.entryType);

	void AssertHasExactContainers(ForwardingConsol consol, string containerQuality, params JobContainerDto[] dtos) =>
		AssertCollectionsExactEquivalance(
			consol.Containers.Cast<CommonContainer>(),
			dtos,
			(container, dto) =>
				container.ContainerCommodityCode.RH_Code == dto.Commodity
				&& container.JC_ContainerCount == dto.Count
				&& container.RefContainer.RC_Code == dto.ContainerType
				&& container.JC_ContainerQuality == containerQuality);

	void AssertHasExactCharges(ForwardingConsol consol, params RateChargeDto[] dtos) =>
		AssertCollectionsExactEquivalance(
			consol.GetApportionments().CostsCollection.Cast<JobConsolCost>(),
			dtos,
			(cost, dto) =>
				cost.ChargeCode.AC_Code == dto.ChargeCode.ChargeCode
				&& cost.E6_RX_NKCurrency == dto.RateCurrency
				&& cost.E6_OSCostAmount == dto.RateAmount
				&& cost.E6_RatingBehaviour == RatingBehaviours.ReAutorateCharge);

	void AssertHasSingleShipment(
		ForwardingConsol consol,
		decimal weight,
		ZString weightUnit,
		decimal volume,
		ZString volumeUnit,
		decimal chargeable)
	{
		Assert(consol.Shipments.Count == 1);
		var shipment = consol.Shipments[0];
		AssertEquals(shipment.JS_ActualWeight, weight);
		AssertEquals(shipment.JS_UnitOfWeight, weightUnit);
		AssertEquals(shipment.JS_ActualVolume, volume);
		AssertEquals(shipment.JS_UnitOfVolume, volumeUnit);
		AssertEquals(shipment.JS_ActualChargeable, chargeable);
	}

	public static void AssertCollectionsExactEquivalance<T, V>(IEnumerable<T> actual, IEnumerable<V> expected, Func<T, V, bool> predicate)
	{
		var notInExpected = expected.Where((v) => actual.All((t) => !predicate(t, v)));
		var notInActual = actual.Where((t) => expected.All((v) => !predicate(t, v)));

		AssertEquals(expected.Count(), actual.Count());
		Assert(notInExpected.IsNullOrEmpty());
		Assert(notInActual.IsNullOrEmpty());
	}

	#endregion
}
