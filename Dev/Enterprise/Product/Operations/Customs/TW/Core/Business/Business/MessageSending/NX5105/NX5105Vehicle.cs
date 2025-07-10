using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Commodity_Vehicle : IVehicle
	{
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine invoiceLine;

		public NX5105Commodity_Vehicle(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, "entryLine");
			this.invoiceLine = Argument.NotNull(entryLine.RandomLine, "randomLine");
		}

		ZString IVehicle.Catalyst => invoiceLine.JI_HasCatalystConverter;

		ZString IVehicle.ClassificationCode => FormattableString.Invariant($"{invoiceLine.JI_CarType}{invoiceLine.JI_Transmission}{invoiceLine.JI_GearsStringFormatting}");

		ZInt IVehicle.CylinderQuantity => invoiceLine.JI_Cylinders;

		ZString IVehicle.Displace => invoiceLine.JI_Displacement;

		ZInt IVehicle.DoorQuantity => invoiceLine.JI_NumberOfDoor;

		ZString IVehicle.DrivingSide => invoiceLine.JI_LHD;

		ZString IVehicle.FuelTypeCode => invoiceLine.JI_EngineType;

		ZInt IVehicle.ModelYearNumeric => invoiceLine.JI_ModelYear;

		ZInt IVehicle.SeatQuantity => invoiceLine.JI_Seats;

		ZString IVehicle.StatusCode => invoiceLine.JI_CarCondition;

		ZString IVehicle.TransmissionTypeCode => invoiceLine.JI_Transmission;

		IEnumerable<ZString> IVehicle.VehicleIDs
		{
			get
			{
				var chassis = entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
					.SelectMany(x => x.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Select(y => y.JG_ReferenceNumber)).Distinct();

				foreach (var item in chassis)
				{
					yield return item;
				}
			}
		}

		public ZBool Empty
		{
			get
			{
				IVehicle iVehicle = this;
				return iVehicle.Catalyst.IsEmpty
						&& iVehicle.ClassificationCode.IsEmpty
						&& iVehicle.CylinderQuantity.IsEmpty
						&& iVehicle.Displace.IsEmpty
						&& iVehicle.DoorQuantity.IsEmpty
						&& iVehicle.DrivingSide.IsEmpty
						&& iVehicle.FuelTypeCode.IsEmpty
						&& iVehicle.ModelYearNumeric.IsEmpty
						&& iVehicle.SeatQuantity.IsEmpty
						&& iVehicle.StatusCode.IsEmpty
						&& iVehicle.TransmissionTypeCode.IsEmpty
						&& !iVehicle.VehicleIDs.Any();
			}
		}
	}
}
