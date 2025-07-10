using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<DtbBooking, DtbBookingFlattened>
	{
		public DtbBookingFlattenedDataTransferProcessor(ImportCollectionInfoImplForDtbBookingFlattened importCollectionInfo)
			: base(new DtbBookingCollection(((IImportCollectionInfo)importCollectionInfo).Collection.Factory, new AdhocCollectionRelationship(typeof(DtbBooking))), importCollectionInfo)
		{
		}

		protected override DtbBooking CreateHeader(IBusinessObjectCollection headerCollection, DtbBookingFlattened flattenedRecord)
		{
			var booking = GetHeader(flattenedRecord.KM_TransportReference, x => x.KM_TransportReference);

			SetGoodsDescription(flattenedRecord, booking);
			CopyIdenticallyNamedProperties(booking, flattenedRecord, DtbBookingSchema.Constants.Prefix);
			SetClientRequestedBillingParty(flattenedRecord, booking);
			var instructions = SetInstructions(flattenedRecord, booking);
			SetPackage(flattenedRecord, booking, instructions);

			return booking;
		}

		protected override void SetupNewHeader(DtbBooking header)
		{
			base.SetupNewHeader(header);

			var consolidation = Factory.New<DtbBookingConsolidation>();
			consolidation.Bookings.Add(header);
		}

		void SetGoodsDescription(DtbBookingFlattened flattenedRecord, DtbBooking booking)
		{
			booking.ConsolidationSingleJob.KB_GoodsDescription = flattenedRecord.Consolidation_KB_GoodsDescription;
		}

		void SetPackage(DtbBookingFlattened flattenedRecord, DtbBooking booking, IEnumerable<DtbBookingInstruction> instructions)
		{
			if (flattenedRecord.Pack_KP_PackageQty > 0)
			{
				PkgPackage package = null;
				if (!flattenedRecord.Pack_KP_PackageID.IsEmpty)
				{
					package = booking.PackageJob.Packages.FirstOrDefault(x => x.KP_PackageID == flattenedRecord.Pack_KP_PackageID);
				}

				if (package == null)
				{
					package = booking.PackageJob.Packages.AddNew();
				}

				CopyIdenticallyNamedProperties(package, flattenedRecord, (NoResString)"Pack", (NoResString)"Pack");

				if (package.HasErrors)
				{
					var errorMessage = package.GetErrors().ToUniqueMessageListString();
					Globals.Message.ShowError(Res.GetString("f7106687-e1fe-446a-ad4e-69157b7de216", @"Package has errors. Nothing was saved. Please refer to the following messages: 
{0}", errorMessage));
					IsCanceled = true;
				}
				else
				{
					foreach (var instruction in instructions)
					{
						SetPackageInstructionDivot(instruction, package);
					}
				}
			}
		}

		void SetPackageInstructionDivot(DtbBookingInstruction instruction, PkgPackage package)
		{
			instruction.DivotsWithPackages.AddPackage(package);
			var divot = instruction.PackageDivots.Single(x => x.KD_KP_Package == package.PK);
			divot.KD_Quantity = package.KP_PackageQty;
		}

		IEnumerable<DtbBookingInstruction> SetInstructions(DtbBookingFlattened flattenedRecord, DtbBooking booking)
		{
			var result = new List<DtbBookingInstruction>();
			result.Add(FindOrCreateAndSetInstruction(booking, flattenedRecord, new PickupInstructionDetail(), flattenedRecord.Pickup_Sequence, (NoResString)"Pickup"));
			result.Add(FindOrCreateAndSetInstruction(booking, flattenedRecord, new DeliveryInstructionDetail(), flattenedRecord.Delivery_Sequence, (NoResString)"Delivery"));

			return result;
		}

		DtbBookingInstruction FindOrCreateAndSetInstruction(DtbBooking booking, DtbBookingFlattened flattenedRecord, InstructionDetail instructionDetail, int sequence, string prefix)
		{
			DtbBookingInstruction instruction = null;

			if (sequence == 0)
			{
				instruction = booking.Instructions.FirstOrDefault(x => x.KN_InstructionType == instructionDetail.InstructionType);
			}
			else
			{
				instruction = booking.Instructions.FirstOrDefault(x => x.KN_InstructionType == instructionDetail.InstructionType && x.KN_Sequence == sequence);
			}

			if (instruction == null)
			{
				instruction = booking.Instructions.AddNew(instructionDetail.InstructionType);
			}

			SetInstruction(prefix, flattenedRecord, instruction, instructionDetail);

			return instruction;
		}

		void SetClientRequestedBillingParty(DtbBookingFlattened flattenedRecord, DtbBooking booking)
		{
			var localClientAddress = GetAddress(flattenedRecord, "LocalClient", true);
			if (localClientAddress != null)
			{
				var address = booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ClientRequestedBillingParty);
				address.E2_OA_Address = localClientAddress.PK;
			}
		}

		void SetInstruction(string prefix, DtbBookingFlattened flattenedRecord, DtbBookingInstruction instruction, InstructionDetail instructionDetails)
		{
			CopyPropertiesFromFlattenedToDestination(flattenedRecord, instruction, prefix + "_");

			var confirmation = instruction.Confirmations.FirstOrDefault(x => x.KK_ConfirmationType == instructionDetails.ConfirmationType)
				?? instruction.Confirmations.AddNew(instructionDetails.ConfirmationType);

			CopyPropertiesFromFlattenedToDestination(flattenedRecord, confirmation, prefix + "Confirmation_");

			instruction.OrganisationType = instructionDetails.OrgType;
			SetJobDocAddress(prefix, flattenedRecord, instruction.Address);
		}

		void CopyPropertiesFromFlattenedToDestination(DtbBookingFlattened flattenedRecord, object destination, string prefix)
		{
			foreach (var propertyInfo in FlattenedProperties.Where(x => x.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			{
				var value = (IZType)propertyInfo.GetValue(flattenedRecord);
				if (!value.IsEmpty)
				{
					var propertyToSet = destination.GetType().GetProperty(propertyInfo.Name.Replace(prefix, string.Empty));
					if (propertyToSet != null)
					{
						var readonlyAttributes = (ReadOnlyAttribute[])propertyToSet.GetCustomAttributes(typeof(ReadOnlyAttribute), true);
						if (!readonlyAttributes.Any(x => x.IsReadOnly))
						{
							propertyToSet.SetValue(destination, value);
						}
					}
				}
			}
		}

		abstract class InstructionDetail
		{
			public abstract string InstructionType { get; }
			public abstract string OrgType { get; }
			public abstract string ConfirmationType { get; }
		}

		class PickupInstructionDetail : InstructionDetail
		{
			public override string InstructionType
			{
				get { return InstructionTypes.Codes.PickUp; }
			}

			public override string OrgType
			{
				get { return OrganisationTypesList.Codes.CNR; }
			}

			public override string ConfirmationType
			{
				get { return ConfirmationTypes.Codes.PickUp; }
			}
		}

		class DeliveryInstructionDetail : InstructionDetail
		{
			public override string InstructionType
			{
				get { return InstructionTypes.Codes.Delivery; }
			}

			public override string OrgType
			{
				get { return OrganisationTypesList.Codes.CNE; }
			}

			public override string ConfirmationType
			{
				get { return ConfirmationTypes.Codes.Delivery; }
			}
		}
	}
}
