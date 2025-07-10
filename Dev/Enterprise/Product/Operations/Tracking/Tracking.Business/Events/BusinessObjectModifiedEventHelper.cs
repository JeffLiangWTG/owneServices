using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class BusinessObjectModifiedEventHelper
	{
		readonly BusinessObject BizO;
		bool wasChanged;

		public BusinessObjectModifiedEventHelper(BusinessObject bizO)
		{
			if (bizO == null)
			{
				throw new ArgumentNullException(nameof(bizO));
			}

			BizO = bizO;
			HookEvents();
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			wasChanged = BizO.HasChanges;
		}

		void BizO_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully && wasChanged)
			{
				wasChanged = false;
				UnHookEvents();

				var eventReference = ((IEventReferenceProvider)BizO).EventReference;
				new EventLogHelper().CreateWMREvent(BizO, eventReference);

				HookEvents();
			}
		}

		void HookEvents()
		{
			BizO.Factory.Saved += BizO_Saved;
			BizO.Factory.Saving += Factory_Saving;
		}

		void UnHookEvents()
		{
			BizO.Factory.Saved -= BizO_Saved;
			BizO.Factory.Saving -= Factory_Saving;
		}
	}
}
