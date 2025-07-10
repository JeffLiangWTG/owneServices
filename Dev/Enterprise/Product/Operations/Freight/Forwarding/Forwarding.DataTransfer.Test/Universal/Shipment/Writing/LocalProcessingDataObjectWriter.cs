

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Extensions;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.ZArchitecture.Schema;

	public partial class ShipmentDataObjectWriterTest
	{
		public static void AssertContents(LocalProcessing localProcessingData, ICodeDescription fCLPickupEquipmentNeeded, ICodeDescription printOptionForPackagesOnAWB, ICodeDescription fCLDeliveryEquipmentNeeded, ICodeDescription exportStatement)
		{
			AssertContents(
				localProcessingData,
				fCLPickupEquipmentNeeded,
				new ZDateTime(2011, 1, 1),
				new ZDateTime(2011, 1, 2),
				new ZDateTime(2011, 1, 3),
				new ZDateTime(2011, 1, 4),
				"ARRCARREF1",
				new ZDateTime(2011, 1, 5),
				new ZDateTime(2011, 1, 6),
				125.65m,
				new ZDateTime(2011, 1, 7),
				895.45m,
				printOptionForPackagesOnAWB,
				fCLDeliveryEquipmentNeeded,
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				12,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				new ZDateTime(2011, 1, 17),
				89.65m,
				new ZDateTime(2011, 1, 18),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				exportStatement,
				4,
				5,
				43.21m,
				6,
				7,
				76.32m);
		}

		public static void AssertContents(
			LocalProcessing localProcessingData,
			ICodeDescription fCLPickupEquipmentNeeded,
			ZDateTime? estimatedPickup,
			ZDateTime? pickupRequiredBy,
			ZDateTime? pickupRequiredFrom,
			ZDateTime? pickupCartageAdvised,
			ZString? arrivalCartageRef,
			ZDateTime? pickupCartageCompleted,
			ZDateTime? pickupLabourTime,
			ZDecimal? pickupLabourCharge,
			ZDateTime? demurrageOnPickupTime,
			ZDecimal? demurrageOnPickupCharge,
			ICodeDescription printOptionForPackagesOnAWB,
			ICodeDescription fCLDeliveryEquipmentNeeded,
			ZDateTime? fCLAvailable,
			ZDateTime? fCLStorageCommences,
			ZDateTime? lCLAvailable,
			ZDateTime? lCLStorageCommences,
			ZByte? lCLAirStorageDaysOrHours,
			ZDecimal? lCLAirStorageCharge,
			ZDateTime? estimatedDelivery,
			ZDateTime? deliveryRequiredBy,
			ZDateTime? deliveryRequiredFrom,
			ZDateTime? deliveryCartageAdvised,
			ZDateTime? deliveryCartageCompleted,
			ZDateTime? deliveryLabourTime,
			ZDecimal? deliveryLabourCharge,
			ZDateTime? demurrageOnDeliveryTime,
			ZDecimal? demurrageOnDeliveryCharge,
			ZBool? hasProhibitedPackaging,
			ZBool? insuranceRequired,
			ZBool? isContingencyRelease,
			ZBool? lCLDatesOverrideConsol,
			ICodeDescription exportStatement,
			ZByte? fclPickupDetentionFreeDays,
			ZByte? fclPickupDetentionDays,
			ZDecimal? fclPickupDetentionCharge,
			ZByte? fclDeliveryDetentionFreeDays,
			ZByte? fclDeliveryDetentionDays,
			ZDecimal? fclDeliveryDetentionCharge)
		{
			AssertNotNull("Precondition: localProcessingData", localProcessingData);

			AssertNotNull("localProcessingData.FCLPickupEquipmentNeeded", localProcessingData.FCLPickupEquipmentNeeded);
			AssertEquals("localProcessingData.FCLPickupEquipmentNeeded.Code", fCLPickupEquipmentNeeded.Code, localProcessingData.FCLPickupEquipmentNeeded.Code);
			AssertEquals("localProcessingData.FCLPickupEquipmentNeeded.Description", fCLPickupEquipmentNeeded.Description, localProcessingData.FCLPickupEquipmentNeeded.Description);
			AssertEquals("localProcessingData.EstimatedPickup", estimatedPickup, localProcessingData.EstimatedPickup);
			AssertEquals("localProcessingData.PickupRequiredBy", pickupRequiredBy, localProcessingData.PickupRequiredBy);
			AssertEquals("localProcessingData.PickupRequiredFrom", pickupRequiredFrom, localProcessingData.PickupRequiredFrom);
			AssertEquals("localProcessingData.PickupCartageAdvised", pickupCartageAdvised, localProcessingData.PickupCartageAdvised);
			AssertEquals("localProcessingData.ArrivalCartageRef", arrivalCartageRef, localProcessingData.ArrivalCartageRef);
			AssertEquals("localProcessingData.PickupCartageCompleted", pickupCartageCompleted, localProcessingData.PickupCartageCompleted);
			AssertEquals("localProcessingData.PickupLabourTime", pickupLabourTime, localProcessingData.PickupLabourTime);
			AssertEquals("localProcessingData.PickupLabourCharge", pickupLabourCharge, localProcessingData.PickupLabourCharge);
			AssertEquals("localProcessingData.DemurrageOnPickupTime", demurrageOnPickupTime, localProcessingData.DemurrageOnPickupTime);
			AssertEquals("localProcessingData.PickupTruckWaitTime", demurrageOnPickupTime, localProcessingData.PickupTruckWaitTime);
			AssertEquals("localProcessingData.DemurrageOnPickupCharge", demurrageOnPickupCharge, localProcessingData.DemurrageOnPickupCharge);
			AssertEquals("localProcessingData.PickupTruckWaitCharge", demurrageOnPickupCharge, localProcessingData.PickupTruckWaitCharge);
			AssertNotNull("localProcessingData.PrintOptionForPackagesOnAWB", localProcessingData.PrintOptionForPackagesOnAWB);
			AssertEquals("localProcessingData.PrintOptionForPackagesOnAWB.Code", printOptionForPackagesOnAWB.Code, localProcessingData.PrintOptionForPackagesOnAWB.Code);
			AssertEquals("localProcessingData.PrintOptionForPackagesOnAWB.Description", printOptionForPackagesOnAWB.Description, localProcessingData.PrintOptionForPackagesOnAWB.Description);
			AssertNotNull("localProcessingData.FCLDeliveryEquipmentNeeded", localProcessingData.FCLDeliveryEquipmentNeeded);
			AssertEquals("localProcessingData.FCLDeliveryEquipmentNeeded.Code", fCLDeliveryEquipmentNeeded.Code, localProcessingData.FCLDeliveryEquipmentNeeded.Code);
			AssertEquals("localProcessingData.FCLDeliveryEquipmentNeeded.Description", fCLDeliveryEquipmentNeeded.Description, localProcessingData.FCLDeliveryEquipmentNeeded.Description);
			AssertEquals("localProcessingData.FCLAvailable", fCLAvailable, localProcessingData.FCLAvailable);
			AssertEquals("localProcessingData.FCLStorageCommences", fCLStorageCommences, localProcessingData.FCLStorageCommences);
			AssertEquals("localProcessingData.LCLAvailable", lCLAvailable, localProcessingData.LCLAvailable);
			AssertEquals("localProcessingData.LCLStorageCommences", lCLStorageCommences, localProcessingData.LCLStorageCommences);
			AssertEquals("localProcessingData.LCLAirStorageDaysOrHours", lCLAirStorageDaysOrHours, localProcessingData.LCLAirStorageDaysOrHours);
			AssertEquals("localProcessingData.LCLAirStorageCharge", lCLAirStorageCharge, localProcessingData.LCLAirStorageCharge);
			AssertEquals("localProcessingData.EstimatedDelivery", estimatedDelivery, localProcessingData.EstimatedDelivery);
			AssertEquals("localProcessingData.DeliveryRequiredBy", deliveryRequiredBy, localProcessingData.DeliveryRequiredBy);
			AssertEquals("localProcessingData.DeliveryRequiredFrom", deliveryRequiredFrom, localProcessingData.DeliveryRequiredFrom);
			AssertEquals("localProcessingData.DeliveryCartageAdvised", deliveryCartageAdvised, localProcessingData.DeliveryCartageAdvised);
			AssertEquals("localProcessingData.DeliveryCartageCompleted", deliveryCartageCompleted, localProcessingData.DeliveryCartageCompleted);
			AssertEquals("localProcessingData.DeliveryLabourTime", deliveryLabourTime, localProcessingData.DeliveryLabourTime);
			AssertEquals("localProcessingData.DeliveryLabourCharge", deliveryLabourCharge, localProcessingData.DeliveryLabourCharge);
			AssertEquals("localProcessingData.DemurrageOnDeliveryTime", demurrageOnDeliveryTime, localProcessingData.DemurrageOnDeliveryTime);
			AssertEquals("localProcessingData.DeliveryTruckWaitTime", demurrageOnDeliveryTime, localProcessingData.DeliveryTruckWaitTime);
			AssertEquals("localProcessingData.DemurrageOnDeliveryCharge", demurrageOnDeliveryCharge, localProcessingData.DemurrageOnDeliveryCharge);
			AssertEquals("localProcessingData.DeliveryTruckWaitCharge", demurrageOnDeliveryCharge, localProcessingData.DeliveryTruckWaitCharge);
			AssertEquals("localProcessingData.HasProhibitedPackaging", hasProhibitedPackaging, localProcessingData.HasProhibitedPackaging);
			AssertEquals("localProcessingData.InsuranceRequired", insuranceRequired, localProcessingData.InsuranceRequired);
			AssertEquals("localProcessingData.IsContingencyRelease", isContingencyRelease, localProcessingData.IsContingencyRelease);
			AssertEquals("localProcessingData.LCLDatesOverrideConsol", lCLDatesOverrideConsol, localProcessingData.LCLDatesOverrideConsol);
			AssertNotNull("localProcessingData.ExportStatement", localProcessingData.ExportStatement);
			AssertEquals("localProcessingData.ExportStatement.Code", exportStatement.Code, localProcessingData.ExportStatement.Code);
			AssertEquals("localProcessingData.ExportStatement.Description", exportStatement.Description, localProcessingData.ExportStatement.Description);
			AssertEquals("localProcessingData.FCLPickupDetentionFreeDays", fclPickupDetentionFreeDays, localProcessingData.FCLPickupDetentionFreeDays);
			AssertEquals("localProcessingData.FCLPickupDetentionDays", fclPickupDetentionDays, localProcessingData.FCLPickupDetentionDays);
			AssertEquals("localProcessingData.FCLPickupDetentionCharge", fclPickupDetentionCharge, localProcessingData.FCLPickupDetentionCharge);
			AssertEquals("localProcessingData.FCLDeliveryDetentionFreeDays", fclDeliveryDetentionFreeDays, localProcessingData.FCLDeliveryDetentionFreeDays);
			AssertEquals("localProcessingData.FCLDeliveryDetentionDays", fclDeliveryDetentionDays, localProcessingData.FCLDeliveryDetentionDays);
			AssertEquals("localProcessingData.FCLDeliveryDetentionCharge", fclDeliveryDetentionCharge, localProcessingData.FCLDeliveryDetentionCharge);
		}

		public static JobDocsAndCartage SetupJobDocsAndCartage(JobDocsAndCartage jobDocsAndCartage, ZString fCLPickupEquipmentNeeded, ZString printOptionForPackagesOnAWB, ZString fCLDeliveryEquipmentNeeded, ZString exportStatement)
		{
			return SetupJobDocsAndCartage(
				jobDocsAndCartage,
				fCLPickupEquipmentNeeded,
				new ZDateTime(2011, 1, 1),
				new ZDateTime(2011, 1, 2),
				new ZDateTime(2011, 1, 3),
				new ZDateTime(2011, 1, 4),
				"ARRCARREF1",
				new ZDateTime(2011, 1, 5),
				new ZDateTime(2011, 1, 6),
				125.65m,
				new ZDateTime(2011, 1, 7),
				895.45m,
				printOptionForPackagesOnAWB,
				fCLDeliveryEquipmentNeeded,
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				12,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				new ZDateTime(2011, 1, 17),
				89.65m,
				new ZDateTime(2011, 1, 18),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				exportStatement,
				4,
				5,
				43.21m,
				6,
				7,
				76.32m);
		}

		public static JobDocsAndCartage SetupJobDocsAndCartage(
			JobDocsAndCartage jobDocsAndCartage,
			ZString fCLPickupEquipmentNeeded,
			ZDateTime estimatedPickup,
			ZDateTime pickupRequiredBy,
			ZDateTime pickupRequiredFrom,
			ZDateTime pickupCartageAdvised,
			ZString arrivalCartageRef,
			ZDateTime pickupCartageCompleted,
			ZDateTime pickupLabourTime,
			ZDecimal pickupLabourCharge,
			ZDateTime demurrageOnPickupTime,
			ZDecimal demurrageOnPickupCharge,
			ZString printOptionForPackagesOnAWB,
			ZString fCLDeliveryEquipmentNeeded,
			ZDateTime fCLAvailable,
			ZDateTime fCLStorageCommences,
			ZDateTime lCLAvailable,
			ZDateTime lCLStorageCommences,
			ZByte lCLAirStorageDaysOrHours,
			ZDecimal lCLAirStorageCharge,
			ZDateTime estimatedDelivery,
			ZDateTime deliveryRequiredBy,
			ZDateTime deliveryRequiredFrom,
			ZDateTime deliveryCartageAdvised,
			ZDateTime deliveryCartageCompleted,
			ZDateTime deliveryLabourTime,
			ZDecimal deliveryLabourCharge,
			ZDateTime demurrageOnDeliveryTime,
			ZDecimal demurrageOnDeliveryCharge,
			ZBool hasProhibitedPackaging,
			ZBool insuranceRequired,
			ZBool isContingencyRelease,
			ZBool lCLDatesOverrideConsol,
			ZString exportStatement,
			ZByte fclPickupDetentionFreeDays,
			ZByte fclPickupDetentionDays,
			ZDecimal fclPickupDetentionCharge,
			ZByte fclDeliveryDetentionFreeDays,
			ZByte fclDeliveryDetentionDays,
			ZDecimal fclDeliveryDetentionCharge)
		{
			// Should set via Row Data to avoid business object logic
			var row = (IColumnIndexer)((IBusinessObjectInternals)jobDocsAndCartage).Row;
			row.SetValue(JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, fCLPickupEquipmentNeeded);
			row.SetValue(JobDocsAndCartageSchema.JP_EstimatedPickup, estimatedPickup);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupRequiredBy, pickupRequiredBy);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupRequiredFrom, pickupRequiredFrom);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupCartageAdvised, pickupCartageAdvised);
			row.SetValue(JobDocsAndCartageSchema.JP_ArrivalCartageRef, arrivalCartageRef);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupCartageCompleted, pickupCartageCompleted);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupLabourTime, pickupLabourTime);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupLabourCharge, pickupLabourCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupTruckWaitTime, demurrageOnPickupTime);
			row.SetValue(JobDocsAndCartageSchema.JP_PickupTruckWaitCharge, demurrageOnPickupCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_PrintOptionForPackagesOnAWB, printOptionForPackagesOnAWB);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, fCLDeliveryEquipmentNeeded);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLAvailable, fCLAvailable);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLStorageCommences, fCLStorageCommences);
			row.SetValue(JobDocsAndCartageSchema.JP_LCLAvailable, lCLAvailable);
			row.SetValue(JobDocsAndCartageSchema.JP_LCLStorageCommences, lCLStorageCommences);
			row.SetValue(JobDocsAndCartageSchema.JP_LCLAirStorageDaysOrHours, lCLAirStorageDaysOrHours);
			row.SetValue(JobDocsAndCartageSchema.JP_LCLAirStorageCharge, lCLAirStorageCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_EstimatedDelivery, estimatedDelivery);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryRequiredBy, deliveryRequiredBy);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryRequiredFrom, deliveryRequiredFrom);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryCartageAdvised, deliveryCartageAdvised);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, deliveryCartageCompleted);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryLabourTime, deliveryLabourTime);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryLabourCharge, deliveryLabourCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryTruckWaitTime, demurrageOnDeliveryTime);
			row.SetValue(JobDocsAndCartageSchema.JP_DeliveryTruckWaitCharge, demurrageOnDeliveryCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_HasProhibitedPackaging, hasProhibitedPackaging);
			row.SetValue(JobDocsAndCartageSchema.JP_InsuranceRequired, insuranceRequired);
			row.SetValue(JobDocsAndCartageSchema.JP_IsContingencyRelease, isContingencyRelease);
			row.SetValue(JobDocsAndCartageSchema.JP_LCLDatesOverrideConsol, lCLDatesOverrideConsol);
			row.SetValue(JobDocsAndCartageSchema.JP_ExportStatement, exportStatement);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLPickupDetentionFreeDays, fclPickupDetentionFreeDays);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLPickupDetentionDays, fclPickupDetentionDays);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLPickupDetentionCharge, fclPickupDetentionCharge);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLDeliveryDetentionFreeDays, fclDeliveryDetentionFreeDays);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLDeliveryDetentionDays, fclDeliveryDetentionDays);
			row.SetValue(JobDocsAndCartageSchema.JP_FCLDeliveryDetentionCharge, fclDeliveryDetentionCharge);
			return jobDocsAndCartage;
		}
	}
}
