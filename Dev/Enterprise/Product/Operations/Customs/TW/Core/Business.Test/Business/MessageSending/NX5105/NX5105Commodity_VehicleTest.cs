using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Commodity_VehicleTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodity_Vehicle()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			IVehicle iVehicle = new NX5105Commodity_Vehicle(entryLine);
			NUnit.Framework.Assert.That(((NX5105Commodity_Vehicle)iVehicle).Empty, NUnit.Framework.Is.EqualTo(ZBool.True), "Vehicle.Empty should be");
			invoiceLine.JI_HasCatalystConverter = "Y";
			NUnit.Framework.Assert.That(iVehicle.Catalyst, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "Vehicle.Catalyst should be");
			invoiceLine.JI_CarType = ZString.Empty;
			invoiceLine.JI_Transmission = ZString.Empty;
			invoiceLine.JI_Gears = ZShort.Zero;
			NUnit.Framework.Assert.That(iVehicle.ClassificationCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.ClassificationCode should be");
			invoiceLine.JI_CarType = "A1";
			invoiceLine.JI_Transmission = "A";
			NUnit.Framework.Assert.That(iVehicle.ClassificationCode, NUnit.Framework.Is.EqualTo("A1A").Using(CustomComparers.TypeComparison), "Vehicle.ClassificationCode should be");
			invoiceLine.JI_Gears = 2;
			NUnit.Framework.Assert.That(iVehicle.ClassificationCode, NUnit.Framework.Is.EqualTo("A1A2").Using(CustomComparers.TypeComparison), "Vehicle.ClassificationCode should be");
			invoiceLine.JI_Transmission = "";
			NUnit.Framework.Assert.That(iVehicle.ClassificationCode, NUnit.Framework.Is.EqualTo("A12").Using(CustomComparers.TypeComparison), "Vehicle.ClassificationCode should be");
			invoiceLine.JI_Cylinders = ZShort.Zero;
			NUnit.Framework.Assert.That(iVehicle.CylinderQuantity, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Vehicle.CylinderQuantity should be");
			invoiceLine.JI_Cylinders = 3;
			NUnit.Framework.Assert.That(iVehicle.CylinderQuantity, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison), "Vehicle.CylinderQuantity should be");
			invoiceLine.JI_Displacement = "";
			NUnit.Framework.Assert.That(iVehicle.Displace, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.Displace should be");
			invoiceLine.JI_Displacement = "500";
			NUnit.Framework.Assert.That(iVehicle.Displace, NUnit.Framework.Is.EqualTo("500").Using(CustomComparers.TypeComparison), "Vehicle.Displace should be");
			invoiceLine.JI_NumberOfDoor = ZShort.Zero;
			NUnit.Framework.Assert.That(iVehicle.DoorQuantity, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Vehicle.DoorQuantity should be");
			invoiceLine.JI_NumberOfDoor = 5;
			NUnit.Framework.Assert.That(iVehicle.DoorQuantity, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "Vehicle.DoorQuantity should be");
			invoiceLine.JI_LHD = "";
			NUnit.Framework.Assert.That(iVehicle.DrivingSide, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.DrivingSide should be");
			invoiceLine.JI_LHD = "Y";
			NUnit.Framework.Assert.That(iVehicle.DrivingSide, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "Vehicle.DrivingSide should be");
			invoiceLine.JI_EngineType = "";
			NUnit.Framework.Assert.That(iVehicle.FuelTypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.FuelTypeCode should be");
			invoiceLine.JI_EngineType = "CG";
			NUnit.Framework.Assert.That(iVehicle.FuelTypeCode, NUnit.Framework.Is.EqualTo("CG").Using(CustomComparers.TypeComparison), "Vehicle.FuelTypeCode should be");
			invoiceLine.JI_ModelYear = ZShort.Zero;
			NUnit.Framework.Assert.That(iVehicle.ModelYearNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Vehicle.ModelYearNumeric should be");
			invoiceLine.JI_ModelYear = 2000;
			NUnit.Framework.Assert.That(iVehicle.ModelYearNumeric, NUnit.Framework.Is.EqualTo(2000).Using(CustomComparers.TypeComparison), "Vehicle.ModelYearNumeric should be");
			invoiceLine.JI_Seats = ZShort.Zero;
			NUnit.Framework.Assert.That(iVehicle.SeatQuantity, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Vehicle.SeatQuantity should be");
			invoiceLine.JI_Seats = 5;
			NUnit.Framework.Assert.That(iVehicle.SeatQuantity, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "Vehicle.SeatQuantity should be");
			invoiceLine.JI_CarCondition = "";
			NUnit.Framework.Assert.That(iVehicle.StatusCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.StatusCode should be");
			invoiceLine.JI_CarCondition = "1";
			NUnit.Framework.Assert.That(iVehicle.StatusCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Vehicle.StatusCode should be");
			invoiceLine.JI_Transmission = "";
			NUnit.Framework.Assert.That(iVehicle.TransmissionTypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Vehicle.TransmissionTypeCode should be");
			invoiceLine.JI_Transmission = "A";
			NUnit.Framework.Assert.That(iVehicle.TransmissionTypeCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "Vehicle.TransmissionTypeCode should be");
			var chassis1 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			var chassis2 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis2.JG_ReferenceNumber = "1234567";
			var chassis3 = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis3.JG_ReferenceNumber = "1234567";
			NUnit.Framework.Assert.That(iVehicle.VehicleIDs.Count(), NUnit.Framework.Is.EqualTo(2), "Vehicle.VehicleIDs.Count() should be");
			NUnit.Framework.Assert.That(iVehicle.VehicleIDs.ElementAt(0), NUnit.Framework.Is.EqualTo("123456").Using(CustomComparers.TypeComparison), "Vehicle.VehicleIDs[0] should be");
			NUnit.Framework.Assert.That(iVehicle.VehicleIDs.ElementAt(1), NUnit.Framework.Is.EqualTo("1234567").Using(CustomComparers.TypeComparison), "Vehicle.VehicleIDs[1] should be");
			NUnit.Framework.Assert.That(((NX5105Commodity_Vehicle)iVehicle).Empty, NUnit.Framework.Is.EqualTo(ZBool.False), "Vehicle.Empty should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				IVehicle vehicle = new NX5105Commodity_Vehicle(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				IVehicle vehicle = new NX5105Commodity_Vehicle(entryLine);
			}

			);
		}
	}
}
