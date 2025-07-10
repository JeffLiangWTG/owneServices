using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageDataValidation : AutoInBondMenuItemMessageDataValidation
	{
		public InBondMenuItemMessageDataValidation(AutoInBondMenuItemMessageData inBondMenuItemMessageData)
			: base(inBondMenuItemMessageData)
		{
		}

		protected new InBondMenuItemMessageData Parent => (InBondMenuItemMessageData)base.Parent;

		protected override void CheckArrivalDate()
		{
			base.CheckArrivalDate();

			var parent = Parent;
			if (parent.IsArrival && parent.InBondMenuItemMessageSendingObjects
				.Cast<InBondMenuItemMessageSendingObject>()
				.Any(x => x.MovementHeader?.BM_ArrivalDate.IsEmpty ?? true))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ArrivalDateInfo);
			}
		}

		protected override void CheckUSDestinationPortCode()
		{
			base.CheckUSDestinationPortCode();

			var parent = Parent;
			if (parent.IsArrival || parent.IsExport)
			{
				if (parent.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().Any(x => x.USDestinationPortCode.IsEmpty))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.USDestinationPortCodeInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.USDestinationPortCodeInfo, Parent.Lookups.RegionDistrictPorts);
			}
		}

		protected override void CheckFIRMSCode()
		{
			base.CheckFIRMSCode();

			var parent = Parent;
			if (parent.IsArrival)
			{
				if (parent.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().Any(x => !(x.Header?.IsAir ?? false) && (x.MovementHeader?.BM_FIRMS.IsEmpty ?? true)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.FIRMSCodeInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(parent.FIRMSCodeInfo, parent.Lookups.FIRMSCollection);
			}
		}

		protected override void CheckExportDate()
		{
			base.CheckExportDate();

			var parent = Parent;
			if (parent.IsExport && parent.InBondMenuItemMessageSendingObjects
				.Cast<InBondMenuItemMessageSendingObject>()
				.Any(x => x.MovementHeader?.BM_ExportDate.IsEmpty ?? true))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ExportDateInfo);
			}
		}

		protected override void CheckExportMOT()
		{
			base.CheckExportMOT();

			var parent = Parent;
			if (parent.IsExport)
			{
				if (parent.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().
				Any(x => !((x.MovementHeader?.BM_ExportLadenOn.IsEmpty ?? true) && parent.ExportConveyance.IsEmpty) &&
				(x.MovementHeader?.BM_ExportTransportMode.IsEmpty ?? true)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ExportMOTInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.ExportMOTInfo, parent.Lookups.TransportModeCodes);
			}
		}

		protected override void CheckExportConveyance()
		{
			base.CheckExportConveyance();

			var parent = Parent;
			if (parent.IsExport)
			{
				if (parent.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().
				Any(x => !((x.MovementHeader?.BM_ExportTransportMode.IsEmpty ?? true) && parent.ExportMOT.IsEmpty) &&
				(x.MovementHeader?.BM_ExportLadenOn.IsEmpty ?? true)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ExportConveyanceInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(parent.ExportConveyanceInfo, parent.Lookups.ConveyanceList);
			}
		}
	}
}
