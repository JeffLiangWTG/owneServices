using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test.GUI
{
	public class MNRRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region MNR Rating
		public void TestAutoRating_MNRWorkOrderHeader_HasNoRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);

			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient, autorateCosts: false);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasNoMatchingRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");
			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");

			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "CM"
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);

			var refRepair1 = Helper.CreateRefRepairCode("456", "456");
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair1, 10, 10);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient, autorateCosts: false);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasPerimeterRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");
			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "CM",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 5, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 7, BreakHourRate = 10m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "CM",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10, 2);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 10, 3);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 140m
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 120m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 270m
				}
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasAreaRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");
			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "CM2",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 50m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 50m, Value = 20m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "CM2",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 22m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 22m, Value = 40m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10, 3);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 2, 10, 5);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 60m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 270m
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 450m
				}
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasQuantityRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");
			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10, 3);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 10, 5);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 240m
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 400m
				}
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasLinearLengthRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = SetupClient("AAA");
			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				LabourChargeCode = "LBRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				],
				CBICalculatorParametersForLabour =
				[
					new() { ItemType = "-", BreakAmount = 10m, BreakHour = 8, BreakHourRate = 10m },
					new() { ItemType = "+", BreakAmount = 10m, BreakHour = 9, BreakHourRate = 10m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 240m
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "LBRCH1",
					JR_OSSellAmt = 400m
				}
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasDebtorCodeLessee()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var lessee = Helper.CreateDebtor("LESSEE");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				]
			});
			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");

			receiveAdvice.Lessee.OrganisationPK = lessee.PK;
			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);

			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			var workOrderLine2 = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Lessee;
			workOrderLine2.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Lessee;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = lessee.OH_Code,
				},

				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m,
					SellAccountCode = lessee.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasDebtorCodeInsurer()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var insurer = Helper.CreateDebtor("INSURER");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.Insurer.OrganisationPK = insurer.PK;

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10, 3);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = insurer.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_HasDebtorCodeThirdParty()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var thirdParty = Helper.CreateDebtor("THIRDPARTY");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.ThirdParty.OrganisationPK = thirdParty.PK;

			var mnrWorkOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 10, 3);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.ThirdParty;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = thirdParty.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_DPPCoveredFirstJobPartly()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var lessee = Helper.CreateDebtor("LESSEE");
			var insurer = Helper.CreateDebtor("INSURER");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.Insurer.OrganisationPK = insurer.PK;
			yardUnit.UnitLineItem.YLI_DPPAmount = 10m;

			receiveAdvice.Lessee.OrganisationPK = lessee.PK;
			var workOrderHeaderPK = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK).PK;
			Factory.Save();

			var mnrWorkOrderHeader = (new BusinessObjectFactory()).Load<MNRWorkOrderHeader>(workOrderHeaderPK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			var workOrderLine2 = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			workOrderLine2.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 20m,
					SellAccountCode = lessee.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 10m,
					SellAccountCode = insurer.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m,
					SellAccountCode = lessee.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_DPPCoveredFirstJob()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var lessee = Helper.CreateDebtor("LESSEE");
			var insurer = Helper.CreateDebtor("INSURER");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.Insurer.OrganisationPK = insurer.PK;
			yardUnit.UnitLineItem.YLI_DPPAmount = 30m;

			receiveAdvice.Lessee.OrganisationPK = lessee.PK;
			var workOrderHeaderPK = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK).PK;
			Factory.Save();

			var mnrWorkOrderHeader = (new BusinessObjectFactory()).Load<MNRWorkOrderHeader>(workOrderHeaderPK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			var workOrderLine2 = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			workOrderLine2.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = insurer.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m,
					SellAccountCode = lessee.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_DPPCoveredSecondJobPartly()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var lessee = Helper.CreateDebtor("LESSEE");
			var insurer = Helper.CreateDebtor("INSURER");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.Insurer.OrganisationPK = insurer.PK;
			yardUnit.UnitLineItem.YLI_DPPAmount = 50m;

			receiveAdvice.Lessee.OrganisationPK = lessee.PK;
			var workOrderHeaderPK = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK).PK;
			Factory.Save();

			var mnrWorkOrderHeader = (new BusinessObjectFactory()).Load<MNRWorkOrderHeader>(workOrderHeaderPK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			var workOrderLine2 = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			workOrderLine2.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = insurer.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 20m,
					SellAccountCode = insurer.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 80m,
					SellAccountCode = lessee.OH_Code,
				},
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		public void TestAutoRating_MNRWorkOrderHeader_DPPFullCoverage()
		{
			var yard = Helper.CreateCYDWarehouse();
			Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);

			var localClient = Helper.CreateClient("AAA");
			var lessee = Helper.CreateDebtor("LESSEE");
			var insurer = Helper.CreateDebtor("INSURER");

			var clientRate = Helper.CreateClientRate(localClient);

			var refUnitSection = Helper.CreateRefUnitSection("BL001", "BL001");
			var refMaterial = Helper.CreateRefMaterial("123", "123");
			var refComponent = Helper.CreateRefMRComponentCode("123", "123");
			var refRepair = Helper.CreateRefRepairCode("123", "123");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 10m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 20m },
				]
			});

			var refComponent1 = Helper.CreateRefMRComponentCode("456", "456");
			SetupRating(clientRate, new CYMRateEntryParameters()
			{
				YardPk = yard.PK,
				MaterialChargeCode = "MNRCH1",
				ComponentCodePK = refComponent1.PK.ToGuid(),
				MaterialPK = refMaterial.PK.ToGuid(),
				RepairPK = refRepair.PK.ToGuid(),
				UnitSection = "BL",
				MeasurementUnit = "UNT",
				CBICalculatorParametersForMaterial =
				[
					new() { ItemType = "-", BreakAmount = 10m, Value = 20m },
					new() { ItemType = "+", BreakAmount = 10m, Value = 40m },
				]
			});

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			yardUnit.Insurer.OrganisationPK = insurer.PK;
			yardUnit.UnitLineItem.YLI_DPPAmount = 130m;

			receiveAdvice.Lessee.OrganisationPK = lessee.PK;
			var workOrderHeaderPK = Helper.CreateMNRWorkOrderHeader(yard, yardUnit.PK).PK;
			Factory.Save();

			var mnrWorkOrderHeader = (new BusinessObjectFactory()).Load<MNRWorkOrderHeader>(workOrderHeaderPK);
			Helper.CreateMNRSurvey(mnrWorkOrderHeader, yardUnit.PK);
			var workOrderLine = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent, refMaterial, refRepair, 10, 0, 3);
			var workOrderLine2 = Helper.CreateMNRWorkOrderLine(mnrWorkOrderHeader, refUnitSection, refComponent1, refMaterial, refRepair, 5, 0, 5);
			workOrderLine.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			workOrderLine2.MWL_ResponsibleParty = ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 30m,
					SellAccountCode = insurer.OH_Code,
				},
				new AssertionCharge
				{
					ChargeCode = "MNRCH1",
					JR_OSSellAmt = 100m,
					SellAccountCode = insurer.OH_Code,
				}
			};
			AutorateAndAssert(expected, mnrWorkOrderHeader, localClient);
		}

		#endregion

		#region Implementation

		new CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory)); }
		}

		CYDYardTestHelper helper;

		OrgHeader SetupClient(string clientCode)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;

			return client;
		}

		RateEntry SetupRating(ClientRate clientRate, CYMRateEntryParameters rateEntryParams)
		{
			var rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);
			var today = ZDateTime.Today;
			rateEntry.TI_RateEndDate = new ZDate(today.AddDays(2));
			rateEntry.TI_RateStartDate = new ZDate(today.AddDays(-1));
			rateEntry.TI_RCC_ComponentCode = rateEntryParams.ComponentCodePK;
			rateEntry.TI_RRC_RepairCode = rateEntryParams.RepairPK;
			rateEntry.TI_ContainerUnitSection = rateEntryParams.UnitSection;
			rateEntry.TI_RMC_Material = rateEntryParams.MaterialPK;

			return rateEntry;
		}

		#endregion
	}
}
