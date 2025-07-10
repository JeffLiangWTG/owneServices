using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class DeliveryNotificationPartyLookups : ZLookups
	{
		public DeliveryNotificationPartyLookups(DeliveryNotificationParty deliveryNotificationParty) : base(deliveryNotificationParty)
		{
			this.deliveryNotificationParty = deliveryNotificationParty;
		}

		readonly DeliveryNotificationParty deliveryNotificationParty;

		public OrgHeaderCollection NotifyParty_List
		{
			get
			{
				if (fNotifyParty_List == null)
				{
					fNotifyParty_List = new OrgHeaderCollection(deliveryNotificationParty.Factory);
				}
				return fNotifyParty_List;
			}
		}
		OrgHeaderCollection fNotifyParty_List;

		public RefUNLOCOCollection DeliveryNotificationPartyPorts
		{
			get
			{
				return Factory.GetCachedValue("DeliveryNotificationPartyPorts", () =>
				{
					ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand);
					return new RefUNLOCOCollection(deliveryNotificationParty.Parent.Factory, filter);
				});
			}
		}
	}
}
