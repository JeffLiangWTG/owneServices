using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ShowMessageOnGUIEventArgs : EventArgs
	{
		public ShowMessageOnGUIEventArgs(ZString title, ZString message)
		{
			Title = title;
			Message = message;
		}

		public readonly ZString Title;
		public readonly ZString Message;
	}

	public class JobDeclarationCreationEventArgs : EventArgs
	{
		public JobDeclarationCreationEventArgs(Enterprise.Integration.Customs.IBaseJobDeclaration declaration)
		{
			Declaration = declaration;
		}

		public readonly Enterprise.Integration.Customs.IBaseJobDeclaration Declaration;
	}

	public class ReasonForChangingSecurityInspectionStatusEventArgs : EventArgs
	{
		public ZString Reason { get; set; }
	}

	public class ShipmentBookingStatusEventArgs : EventArgs
	{
		public ShipmentBookingStatusEventArgs(ZString currentStatus, ZString newStatus)
		{
			CurrentStatus = currentStatus;
			NewStatus = newStatus;
		}

		public ZString CurrentStatus { get; }

		public ZString NewStatus { get; }

		public ZString StatusUpdatedReason { get; set; }
	}

	public class ReasonForChangingDeliveryDueDateEventArgs : EventArgs
	{
		public ZString Reason { get; set; }
	}
}
