using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents
{
	sealed class UpdateJobCommand : CustomCommand
	{
		public override bool Invoke()
		{
			var res = false;

			if (documentInfo?.Document?.Data is IDynamicData data
				&& data.Value is BookingConfirmation bookingConfirmation)
			{
				res = UpdateConsol(bookingConfirmation);
			}

			return res;
		}

		bool UpdateConsol(BookingConfirmation bookingConfirmation)
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.Load<ForwardingConsol>(bookingConfirmation.ConsolPK);

			if (consol == null)
			{
				return false;
			}

			// To do in a later WI

			return true;
		}

		public override string Id => CommandIds.SaveOverriddenData;

		public override string Caption => CommandResources.Captions.SaveOverriddenData;

		public override bool IsEnabled => isEnabled ?? documentInfo?.Document?.Data?.HasChanges ?? false;
		readonly bool? isEnabled = false;

		public override bool IsVisible => true;
	}
}
