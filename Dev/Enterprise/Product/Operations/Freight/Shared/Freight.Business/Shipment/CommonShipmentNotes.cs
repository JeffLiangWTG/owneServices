using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentNotes : Notes
	{
		public CommonShipmentNotes(CommonShipment parent) : base(parent) { }

		protected override StmNoteContexts GetNoteContextsForRelatedBizObject(BusinessObject relatedBizObject)
		{
			CommonShipment shipment = (CommonShipment)Parent;

			if (relatedBizObject == null)
			{
				throw new ArgumentNullException(nameof(relatedBizObject));
			}

			StmNoteContexts parentContexts = base.GetNoteContextsForRelatedBizObject(relatedBizObject);
			StmNoteContexts contexts =
				new StmNoteContexts
				{
					Module = parentContexts.Module,
					Direction = parentContexts.Direction,
					FreightMode = parentContexts.FreightMode
				};

			if (relatedBizObject == shipment.Consignor && relatedBizObject != shipment.Consignee)
			{
				contexts.Direction &= ~StmNoteContextDirection.I;
				if (shipment.IsImport())
				{
					contexts.Direction |= StmNoteContextDirection.E;
				}
			}
			else if (relatedBizObject == shipment.Consignee)
			{
				contexts.Direction &= ~StmNoteContextDirection.E;
				if (shipment.IsExport())
				{
					contexts.Direction |= StmNoteContextDirection.I;
				}
			}

			return contexts;
		}
	}
}
