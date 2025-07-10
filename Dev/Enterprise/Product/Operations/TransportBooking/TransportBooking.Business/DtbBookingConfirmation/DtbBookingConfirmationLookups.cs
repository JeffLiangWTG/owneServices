using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConfirmationLookups : Common.DtbBookingConfirmationLookups
	{
		public DtbBookingConfirmationLookups(DtbBookingConfirmation parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ConfirmationTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var booking = Confirmation.Booking;
				var instruction = Confirmation.Instruction;

				if (booking != null && instruction != null)
				{
					result = BindToLists.GetConfirmationTypes(booking.KM_Direction, instruction.KN_InstructionType, Confirmation.OrganisationType);
				}
				return result;
			}
		}

		public CodeDescriptionPairList ConfirmationDescriptions
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var booking = Confirmation.Booking;
				var instruction = Confirmation.Instruction;

				if (booking != null && instruction != null)
				{
					result = BindToLists.GetConfirmationDescriptions(booking.KM_Direction, instruction.KN_InstructionType, Confirmation.OrganisationType);
				}
				return result;
			}
		}

		public CodeDescriptionPairList InstructionOrPackageDivots
		{
			get
			{
				// do not cache - will change when packages are assigned and un-assigned

				var result = new CodeDescriptionPairList();
				var instruction = Confirmation.Instruction;

				if (instruction != null)
				{
					result.Add(new InstructionOrPackageDivotCodeDescriptionPair(instruction.PK, ResString.GetMultilingualString("d9b6360c-7644-4a7b-b2fd-e8b73a5fefa9", "ALL"), ResString.GetMultilingualString("ea9101cd-1242-4566-a15a-c6743452fb28", "Applies to All Packages on Instruction")));

					foreach (var divot in instruction.PackageDivots)
					{
						var packageDescription = (string)divot.PackageDescriptionWithIDAndQty;
						result.Add(new InstructionOrPackageDivotCodeDescriptionPair(divot.PK, packageDescription, ResString.GetMultilingualString("0cfa4efe-c915-4ad3-99eb-515ad49079e3", "Applies to '{0}' Only", packageDescription)));
					}
				}

				return result;
			}
		}

		public override OrgContactCollection Drivers
		{
			get { return GetDrivers(); }
		}

		OrgContactCollection GetDrivers()
		{
			var booking = Confirmation.Booking;
			OrgContactCollection result;

			if (booking != null && booking.Address.Organisation != null)
			{
				var query = new ZQuery(OrgContactSchema.OC_OH, booking.Address.Organisation.PK);
				result = new OrgContactCollection(Factory, query);
			}
			else
			{
				result = new OrgContactCollection(Factory, ZQuery.NoResultQuery);
			}

			result.Load();

			return result;
		}

		BindToLists BindToLists
		{
			get { return Factory.GetCachedValue("TransportBookings|BindToLists", () => new BindToLists(Factory)); }
		}

		DtbBookingConfirmation Confirmation
		{
			get { return (DtbBookingConfirmation)Parent; }
		}
	}
}
