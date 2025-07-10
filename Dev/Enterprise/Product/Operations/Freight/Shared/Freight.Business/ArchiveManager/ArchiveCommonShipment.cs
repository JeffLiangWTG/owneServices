using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.ArchiveManager
{
	internal class ArchiveCommonShipment : IArchiveableBusinessObject
	{
		public ArchiveCommonShipment(CommonShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly CommonShipment shipment;

		#region IArchiveableBusinessObject Members

		IEnumerable<ArchiveReferenceKey> IArchiveableBusinessObject.AdditionalKeys
		{
			get
			{
				if (!shipment.JS_HouseBill.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, shipment.JS_HouseBill);
				}

				if (!shipment.JS_CFSReference.IsEmpty)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.CFSRef, shipment.JS_CFSReference);
				}

				if (!shipment.JS_BookingReference.IsEmpty)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.BookingRef, shipment.JS_BookingReference);
				}

				if (shipment.Job != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.JobNo, shipment.Job.JH_JobNum);
				}

				if (!shipment.JS_OrderReferences.IsEmpty)
				{
					if (shipment.JS_OrderReferences.Length > StorageReferenceSchema.SR_Reference.MaxLength)
					{
						var orderReferences = shipment.JS_OrderReferences.Split(',');
						if (orderReferences != null)
						{
							foreach (var orderReference in orderReferences)
							{
								yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.Order, orderReference);
							}
						}
					}
					else
					{
						yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.Order, shipment.JS_OrderReferences);
					}
				}

				if (shipment.Declarations.Length > 0)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.DeclarationNo, shipment.Declarations[0].JE_DeclarationReference);
				}

				if (shipment.Consignee != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Consignee, shipment.Consignee.OH_Code);
				}

				if (shipment.Consignor != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Consignor, shipment.Consignor.OH_Code);
				}
			}
		}

		IEnumerable<ArchiveDocumentDescriptor> IArchiveableBusinessObject.ArchiveDocuments
		{
			get { return null; }
		}

		BusinessObject IArchiveableBusinessObject.ArchiveableBusinessObject
		{
			get { return shipment; }
		}

		ArchiveReferenceKey IArchiveableBusinessObject.NaturalKey
		{
			get { return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.ShipmentNo, shipment.JS_UniqueConsignRef); }
		}

		Guid IArchiveableBusinessObject.BranchPK
		{
			get { return Guid.Empty; }
		}

		#endregion
	}
}
