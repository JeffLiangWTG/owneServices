using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public static class PreAdviceConversionHelper
	{
		public static Func<ForwardingConsol, bool> ShowFormWithNewlyCreatedConsol()
		{
			return consol =>
				{
					if (consol != null)
					{
						var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
						controller.ShowChildrenAsDialog = true;
						controller.ShowFormForNewEntity(consol);

						return !consol.HasChanges && consol.IsInDatabase;
					}

					return false;
				};
		}

		public static Func<ForwardingConsol, bool> ShowFormWithEditedConsol()
		{
			return consol =>
				{
					if (consol != null)
					{
						var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
						controller.ShowChildrenAsDialog = true;
						controller.ShowEditForm(consol);

						return !consol.HasChanges && consol.IsInDatabase;
					}

					return false;
				};
		}
	}
}
