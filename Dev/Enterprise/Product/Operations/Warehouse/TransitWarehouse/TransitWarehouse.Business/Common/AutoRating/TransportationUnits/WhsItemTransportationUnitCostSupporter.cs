using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransportationUnitCostSupporter<T> : GenericJobCostSupporter
		where T : BusinessObject, ITransportationUnitForApportioning
	{
		public WhsItemTransportationUnitCostSupporter(T parent)
		{
			Parent = parent;
		}

		public T Parent { get; private set; }

		public override DocumentSupporter DocumentSupporter => Parent is IDocumentSupportable docSupportable ? docSupportable.DocumentSupporter : null;

		public override ZDateTime ETA => Parent.ETA;

		public override ZDateTime ETD => Parent.ETD;

		public override bool HasChanges => Parent.HasChanges;

		public override bool IsInDatabase => Parent.IsInDatabase;

		public override ZString MasterBillNum => Parent.MasterBillNum;

		public override ZGuid GetCreditorPK(ZString chargeCode, ZGuid rateProviderOrgPK) => Parent.DefaultChargeGroups.Contains(chargeCode) ? Parent.CreditorPK : ZGuid.Empty;

		public override ZGuid PK => Parent.PK;

		public override IJobInvoicingPlugIn[] ShipmentsList => shipmentList ?? (shipmentList = Parent.Consignments.ToArray());

		IJobInvoicingPlugIn[] shipmentList;

		public override ZGuid[] ShipmentsListPKs => ((IGenericJobCostSupporter)this).ShipmentsList.Select(b => b.PK).ToArray();

		public override ZString Type => Parent.TablePrefix;

		public override IEnumerable<ZString> ExcludedApportionmentMethods
		{
			get
			{
				yield return AllocationMethod.Revenue;
				yield return AllocationMethod.ChargeableUnits;
				yield return AllocationMethod.ContainerCount;
				yield return AllocationMethod.TwentyFootEquivalentUnit;
				yield return AllocationMethod.CapacityPerContainer;
				yield return AllocationMethod.FreeSpaceContribution;
			}
		}
	}
}
