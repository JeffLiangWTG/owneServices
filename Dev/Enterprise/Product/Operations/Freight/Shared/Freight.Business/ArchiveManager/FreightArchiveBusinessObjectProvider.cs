using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.ArchiveManager
{
	public class FreightArchiveBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			switch (item.PKColumn.TableName)
			{
				case JobShipmentSchema.Constants.TableName:
					CommonShipment shipment = factory.Load<CommonShipment>(item.PK);

					if (shipment.JS_IsForwardRegistered && shipment.JS_IsCFSRegistered)
					{
						CommonShipment alterEgoShipment = null;

						if (shipment is Integration.CFS.ICFSShipment)
						{
							alterEgoShipment = (CommonShipment)factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(item.PK);
						}
						else if (shipment is Enterprise.Integration.Forwarding.IForwardingShipment)
						{
							alterEgoShipment = (CommonShipment)factory.Load<Integration.CFS.ICFSShipment>(item.PK);
						}

						if (alterEgoShipment != null)
						{
							return new IArchiveableBusinessObject[] { new ArchiveCommonShipment(shipment), new ArchiveCommonShipment(alterEgoShipment) };
						}
					}

					return new IArchiveableBusinessObject[] { new ArchiveCommonShipment(shipment) };

				case JobConsolSchema.Constants.TableName:

					CommonConsol consol = factory.Load<CommonConsol>(item.PK);
					if (consol.JK_IsForwarding && consol.JK_IsCFS)
					{
						CommonConsol alterEgoConsol = null;

						if (consol is Integration.CFS.ICFSLoadListConsol)
						{
							alterEgoConsol = (CommonConsol)factory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(item.PK);
						}
						else if (consol is Enterprise.Integration.Forwarding.IForwardingConsol)
						{
							alterEgoConsol = (CommonConsol)factory.Load<Integration.CFS.ICFSLoadListConsol>(item.PK);
						}

						if (alterEgoConsol != null)
						{
							return new IArchiveableBusinessObject[] { new ArchiveCommonConsol(consol), new ArchiveCommonConsol(alterEgoConsol) };
						}
					}

					return new IArchiveableBusinessObject[] { new ArchiveCommonConsol(factory.Load<CommonConsol>(item.PK)) };
			}

			return null;
		}

		public IEnumerable<string> TableNamesSupported
		{
			get
			{
				yield return JobShipmentSchema.Constants.TableName;
				yield return JobConsolSchema.Constants.TableName;
			}
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get
			{
				return new[]
				{
					ArchiveReferenceKey.CommonTypes.JobNo,
					ArchiveReferenceKey.CommonTypes.ShipmentNo,
					ArchiveReferenceKey.CommonTypes.DeclarationNo,
					FreightArchiveKeyTypes.Consol,
					ArchiveReferenceKey.CommonTypes.Housebill,
					ArchiveReferenceKey.CommonTypes.Masterbill,
					FreightArchiveKeyTypes.Order,
					ArchiveReferenceKey.CommonTypes.Consignee,
					ArchiveReferenceKey.CommonTypes.Consignor
				};
			}
		}

		#endregion
	}

	public static class FreightArchiveKeyTypes
	{
		public readonly static ReferenceKeyType Consol = new ReferenceKeyType("CON", ResString.GetMultilingualString("8c0467b5-1a63-4170-b436-872e8c893520", "Consol #"));
		public readonly static ReferenceKeyType Order = new ReferenceKeyType("ORD", ResString.GetMultilingualString("596f58ae-2650-48d9-85d3-a7f2fcf83f2e", "Order #"));
		public readonly static ReferenceKeyType BookingRef = new ReferenceKeyType("BOO", ResString.GetMultilingualString("b83a3a11-0502-4fb1-a7a8-3b6bb902b60d", "Booking Ref #"));
		public readonly static ReferenceKeyType CFSRef = new ReferenceKeyType("CFS", ResString.GetMultilingualString("b83a3a11-0502-4fb1-a7a8-3b6bb902b61e", "CFS Ref #"));
		public readonly static ReferenceKeyType AgentRef = new ReferenceKeyType("AGT", ResString.GetMultilingualString("1756d9d8-d8d8-4d9f-a9ab-a1ba323d1560", "Agent Ref #"));
	}
}
