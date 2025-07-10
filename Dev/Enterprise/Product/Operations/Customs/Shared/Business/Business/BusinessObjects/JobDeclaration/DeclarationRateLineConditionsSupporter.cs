using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class DeclarationRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public DeclarationRateLineConditionsSupporter(BaseJobDeclaration objectToWrap)
			: base(objectToWrap)
		{
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = ObjectToWrap as BaseJobDeclaration); }
		}

		protected override OrgHeader GetExportBroker()
		{
			return GlbBranch.CurrentBranch.OrgProxy;
		}

		protected override OrgHeader GetImportBroker()
		{
			return GlbBranch.CurrentBranch.OrgProxy;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return Declaration.Forwarder ?? RetrieveFromConsol(x => x.SendingAgent);
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return Declaration.Forwarder ?? RetrieveFromConsol(x => x.ReceivingAgent);
		}

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		protected override OrgHeader GetDepartureCFS()
		{
			if (Declaration.DepotDocAddress != null && Declaration.DepotDocAddress.Organisation != null)
			{
				return Declaration.DepotDocAddress.Organisation;
			}

			return RetrieveFromConsol(x => x.DepartureCFS);
		}

		protected override OrgHeader GetArrivalCFS()
		{
			if (Declaration.DepotDocAddress != null && Declaration.DepotDocAddress.Organisation != null)
			{
				return Declaration.DepotDocAddress.Organisation;
			}

			return RetrieveFromConsol(x => x.ArrivalCFS);
		}

		protected override bool GetHasDangerousGoods()
		{
			return Declaration.Packages.Cast<BasePackage>().Any(package => package.UNDGs.Any())
				|| Declaration.Shipment != null && Declaration.Shipment.OuterPackLines.Cast<PackLine>().Any(line => line.UNDGs.Any());
		}

		OrgHeader RetrieveFromConsol(Func<RateLineConditionsSupporter, OrgHeader> retriever)
		{
			if (Declaration.RelevantConsol != null)
			{
				var supportable = Declaration.RelevantConsol.RatingAdapter as IAutoRatingFreightConditionsSupportable;

				if (supportable != null)
				{
					return retriever(supportable.ConditionsSupporter);
				}
			}

			return null;
		}
	}
}
