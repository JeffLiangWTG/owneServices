using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ForwardingPackLineWithPortMessagingCollection : ForwardingPackLineCollection
	{
		public ForwardingPackLineWithPortMessagingCollection(ForwardingShipment master)
			: base(master)
		{
			master.OuterPackLines.CountChanged += (s, e) =>
				{
					if (!IsLoading)
					{
						Load();
					}
				};
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public new ForwardingPackLineWithPortMessaging AddNew()
		{
			return (ForwardingPackLineWithPortMessaging)base.AddNew();
		}

		public new ForwardingPackLineWithPortMessaging this[int index]
		{
			get { return (ForwardingPackLineWithPortMessaging)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(ForwardingPackLineWithPortMessaging);
		}

		#endregion
	}
}
