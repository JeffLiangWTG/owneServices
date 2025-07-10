using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.ZQueryHelpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentNotesChecker : NotesChecker
	{
		public ShipmentNotesChecker(EnterpriseBusinessObject parent)
			: base(parent)
		{
			shipment = (ForwardingShipment)parent;
		}

		#region Fetch Hints

		public override void AddBusinessObjectsWithRelatedNotesFetchHints()
		{
			foreach (var declaration in shipment.Declarations)
			{
				Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			}

			Factory.AddFetchHint(JobOrderHeaderSchema.JD_JS, shipment.PK);
			Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, shipment.PK);
			Factory.AddFetchHint(JobDocumentDataSchema.Instance, JobDocumentDataZQueryFilters.GetIndexedQuery(shipment.PK, shipment.TablePrefix));

			foreach (var packLine in shipment.OuterPackLines)
			{
				Factory.AddFetchHint(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL, packLine.PK);
			}
		}

		#endregion

		readonly ForwardingShipment shipment;
	}
}
