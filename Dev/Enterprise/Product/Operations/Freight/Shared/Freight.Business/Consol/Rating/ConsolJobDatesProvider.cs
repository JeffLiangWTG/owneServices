using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ConsolJobDatesProvider : JobDatesProvider<CommonConsol>
	{
		public ConsolJobDatesProvider(CommonConsol consol)
			: base(consol) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			if (Parent.JK_JX_JB_A_ARV.IsValid)
			{
				return Parent.JK_JX_JB_A_ARV;
			}

			if (Parent.JK_JX_JB_E_ARV.IsValid)
			{
				return Parent.JK_JX_JB_E_ARV;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			if (Parent.JK_JX_JA_A_DEP.IsValid)
			{
				return Parent.JK_JX_JA_A_DEP;
			}

			if (Parent.JK_JX_JA_E_DEP.IsValid)
			{
				return Parent.JK_JX_JA_E_DEP;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetEstimatedArrivalDateCore()
		{
			if (Parent.JK_JX_JB_E_ARV.IsValid)
			{
				return Parent.JK_JX_JB_E_ARV;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetEstimatedDepartureDateCore()
		{
			if (Parent.JK_JX_JA_E_DEP.IsValid)
			{
				return Parent.JK_JX_JA_E_DEP;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetAWBIssueDateCore()
		{
			return Parent.JK_MasterBillIssueDate;
		}

		protected override ZDateTime GetFirstContainerGateInDateCore()
		{
			return GetEarliestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetLastContainerGateInDateCore()
		{
			return GetLatestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetCFSReceivalStartDateCore()
		{
			var result = ZDateTime.Empty;
			var routeSets = ((IRoutingSupport)Parent)?.TransportsIncludingRelated?.RouteSets;
			if (routeSets.Any())
			{
				result = routeSets.Select(x => x.ReferenceLeg).First().JW_DepotReceivalCommences;
			}

			return result;
		}

		protected override ZDateTime InterimReceiptDateCore()
		{
			if (!Parent.Shipments.Any())
			{
				return ZDateTime.Empty;
			}

			if (Parent.Shipments.Count == 1)
			{
				var interimDate = Parent.Shipments[0].JS_A_RCV;
				return interimDate.IsValid
					? interimDate
					: ZDateTime.Empty;
			}

			if (Parent.IsDomestic() || Parent.IsCrossTrade())
			{
				return ZDateTime.Empty;
			}

			if (Parent.IsExport())
			{
				return Parent.Shipments.Min(x => (x as CommonShipment)?.JS_A_RCV) ?? ZDateTime.Empty;
			}

			if (Parent.IsImport())
			{
				return Parent.Shipments.Max(x => (x as CommonShipment)?.JS_A_RCV) ?? ZDateTime.Empty;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetCostingAutoratingDateOverrideCore()
		{
			return Parent.AutoratingDate;
		}
	}
}
