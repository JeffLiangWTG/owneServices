using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ZArchitecture.Modules.ModuleId.UNDGCountryReference)]
	public class UNDGCountryReferenceCollection : ActiveBusinessObjectCollection<UNDGCountryReference>, IFilterModuleExtraNotificationProvider
	{
		public UNDGCountryReferenceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UNDGCountryReferenceCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		// In Transit Warehouse, we allow user to manage the UNDG limit by Country Reference, thus user can attach all of Country References.
		public UNDGCountryReferenceCollection(BusinessObjectFactory factory, IWhsUNDGLimit undgLimit)
			: base(factory)
		{
			IsAttachToWhsUNDGLimit = undgLimit != null;
		}

		bool IsAttachToWhsUNDGLimit { get; }

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			if (!IsAttachToWhsUNDGLimit)
			{
				if (businessObject is UNDGCountryReference countryReference
					&& countryReference.IsSystemCountryReference)
				{
					return new Notification(NotificationType.Error, Res.GetString("455c9900-4292-04b7-42cc-20d73f9ab842", "System maintained Country/Region Reference/s (SG, PSA) cannot be attached."));
				}
				else if (!Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed)
				{
					return new Notification(NotificationType.Error, Res.GetString("61f7b306-c216-7599-47aa-d2efcea85ee9", "You do not have the security rights to attach Country/Region Regulations."));
				}
			}

			return null;
		}
	}
}
