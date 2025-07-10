using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Business.ArchiveManager;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.ArchiveManager
{
	internal class ArchiveCommonOrder : IArchiveableBusinessObject
	{
		public ArchiveCommonOrder(Order order)
		{
			this.order = order;
		}

		readonly Order order;

		#region IArchiveableBusinessObject Members

		IEnumerable<ArchiveReferenceKey> IArchiveableBusinessObject.AdditionalKeys
		{
			get
			{
				if (order.Declaration != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.ShipmentNo, (ZString)order.Declaration[JobDeclarationSchema.JE_DeclarationReference]);
					if (!((ZString)order.Declaration[JobDeclarationSchema.JE_HouseBill]).IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, (ZString)order.Declaration[JobDeclarationSchema.JE_HouseBill]);
					}
				}

				if (order.Shipment != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.ShipmentNo, order.Shipment.JS_UniqueConsignRef);
					if (!order.Shipment.JS_HouseBill.IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, order.Shipment.JS_HouseBill);
					}
				}

				if (order.PreAdvice != null && !order.PreAdvice.EF_HouseBill.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, order.PreAdvice.EF_HouseBill);
				}

				if (order.PreAdvice != null && !order.PreAdvice.EF_MasterBill.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Masterbill, order.PreAdvice.EF_MasterBill);
				}
			}
		}

		IEnumerable<ArchiveDocumentDescriptor> IArchiveableBusinessObject.ArchiveDocuments
		{
			get { return null; }
		}

		BusinessObject IArchiveableBusinessObject.ArchiveableBusinessObject
		{
			get { return order; }
		}

		ArchiveReferenceKey IArchiveableBusinessObject.NaturalKey
		{
			get { return new ArchiveReferenceKey(FreightArchiveKeyTypes.Order, order.JD_OrderNumber); }
		}

		Guid IArchiveableBusinessObject.BranchPK
		{
			get { return Guid.Empty; }
		}

		#endregion
	}
}
