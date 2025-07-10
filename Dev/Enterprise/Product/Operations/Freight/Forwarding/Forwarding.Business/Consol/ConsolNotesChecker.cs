using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.ZQueryHelpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolNotesChecker : NotesChecker
	{
		public ConsolNotesChecker(EnterpriseBusinessObject parent)
			: base(parent)
		{
			consol = (ForwardingConsol)parent;
		}

		#region Fetch Hints

		public override void AddBusinessObjectsWithRelatedNotesFetchHints()
		{
			consol.Factory.AddFetchHint(JobContainerSchema.JC_JK, consol.PK);

			consol.Factory.AddFetchHint(OrgAddressSchema.PK, consol.JK_OA_SendingForwarderAddress);
			consol.Factory.AddFetchHint(OrgAddressSchema.PK, consol.JK_OA_ReceivingForwarderAddress);
			consol.Factory.AddFetchHint(OrgAddressSchema.PK, consol.JK_OA_CreditorAddress);
			consol.Factory.AddFetchHint(OrgAddressSchema.PK, consol.JK_OA_ShippingLineAddress);
			consol.Factory.AddFetchHint(JobDocumentDataSchema.Instance, JobDocumentDataZQueryFilters.GetIndexedQuery(consol.PK, consol.TablePrefix));

			foreach (var jobConShipLink in consol.Factory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol.PK)))
			{
				consol.Factory.AddFetchHint(JobShipmentSchema.PK, jobConShipLink.JN_JS);
				consol.Factory.AddFetchHint(JobDeclarationSchema.JE_JS, jobConShipLink.JN_JS);
			}
		}

		#endregion

		readonly ForwardingConsol consol;
	}
}
