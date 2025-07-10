using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingProfitShareRedistribution : ProfitShareRedistribution
	{
		public ForwardingProfitShareRedistribution(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		/// <summary>
		/// Add Forwarding Consols to Consols collection and update Shipments collection. Only add ones that have not been in the collections.
		/// </summary>
		public void AddConsols(IEnumerable<ForwardingConsol> consolsToAdd)
		{
			var newConsols = consolsToAdd.Select(x => new ProfitShareForwardingConsolWrapper(x)).ToArray();
			Consols.AddRange(newConsols);
			AddConsolShipments(newConsols, Shipments);
		}

		void AddConsolShipments(IEnumerable<ProfitShareForwardingConsolWrapper> newConsols, ProfitShareForwardingShipmentWrapperCollection collection)
		{
			var readOnlyMode = ConsolProfitShares.Any(i => i.IsInDatabase);

			var profitShareShipments =
				ConsolProfitShares
				.Cast<ConsolidationProfitShare>()
				.SelectMany(x => x.ShipmentProfitShares.Cast<ShipmentProfitShares>())
				.ToDictionary(s => s.PSS_JS);

			if (readOnlyMode)
			{
				foreach (var shipmentProfitShare in profitShareShipments.Values)
				{
					collection.Add(new ProfitShareForwardingShipmentWrapper((ForwardingShipment)shipmentProfitShare.Shipment, shipmentProfitShare));
				}
			}
			else
			{
				var uniqueShipments = newConsols
					.SelectMany(x => x.Consol.Shipments)
					.DistinctBy(s => s.PK)
					.ToArray();

				foreach (var shipment in uniqueShipments)
				{
					if (profitShareShipments.ContainsKey(shipment.PK))
					{
						collection.Add(new ProfitShareForwardingShipmentWrapper((ForwardingShipment)shipment, profitShareShipments[shipment.PK]));
					}
					else
					{
						collection.Add(new ProfitShareForwardingShipmentWrapper((ForwardingShipment)shipment));
					}
				}
			}
		}

		/// <summary>
		/// Remove Forwarding Consols from Consols collection and update Shipments collection. Only remove ones that have no references.
		/// Keep everything separately to be more readable
		/// </summary>
		public void RemoveConsols(IEnumerable<ProfitShareForwardingConsolWrapper> consolsToRemove)
		{
			Consols.RemoveRange(consolsToRemove);

			var shipmentsToKeep = Consols
						.SelectMany(x => ((ProfitShareForwardingConsolWrapper)x).Consol.Shipments.Select(s => s.PK));

			var shipmentPksToRemove = consolsToRemove
						.SelectMany(x => x.Consol.Shipments.Select(s => s.PK))
						.Except(shipmentsToKeep).ToArray();

			var shipmentsToRemove = Shipments.Where(s => shipmentPksToRemove.Contains(s.PK)).ToArray();
			Shipments.RemoveRange(shipmentsToRemove);
		}

		/// <summary>
		/// Shipments to be redistributed gateway consol profit shares. For UI bindings.
		/// </summary>
		public ProfitShareForwardingShipmentWrapperCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new ProfitShareForwardingShipmentWrapperCollection(Factory);
					AddConsolShipments(Consols.Cast<ProfitShareForwardingConsolWrapper>(), shipments);
				}

				return shipments;
			}
		}
		ProfitShareForwardingShipmentWrapperCollection shipments;

		/// <summary>
		/// User selected Gateway consolidations for profit share redistributions. For UI bindings.
		/// </summary>
		public ProfitShareForwardingConsolWrapperCollection Consols
		{
			get
			{
				if (consols == null)
				{
					consols = new ProfitShareForwardingConsolWrapperCollection(Factory);
					consols.Load(ConsolProfitShares);
				}

				return consols;
			}
		}
		ProfitShareForwardingConsolWrapperCollection consols;

		/// <summary>
		/// All consolidations listed in Job Consol Module after a search. For UI binding with Job Consol Module only.
		/// </summary>
		public ProfitShareForwardingModuleConsolCollection Consols_List
		{
			get
			{
				if (consols_List == null)
				{
					consols_List = new ProfitShareForwardingModuleConsolCollection(Factory);
				}

				return consols_List;
			}
		}
		ProfitShareForwardingModuleConsolCollection consols_List;

		public ForwardingModuleOrgAgentRelationshipCollection OrgAgentRelationship_List
		{
			get
			{
				if (orgAgentRelationship_List == null)
				{
					orgAgentRelationship_List = new ForwardingModuleOrgAgentRelationshipCollection(Factory);
				}
				return orgAgentRelationship_List;
			}
		}
		ForwardingModuleOrgAgentRelationshipCollection orgAgentRelationship_List;

		public OrgAgentRelationship ProfitShareRulesParent { get; set; }

		/// <summary>
		/// Add ProfitShare Rules to ProfitShareRules collection from a single selected Profit Share Agreement.
		/// </summary>
		public void AddProfitShareRules(IEnumerable<OrgAgentRelationship> profitShareAgreementsToAdd)
		{
			if (profitShareAgreementsToAdd.Count() > 1)
			{
				Globals.Message.ShowError(Res.GetString("6D24634C-2662-4497-AB6A-CB194F728BAF", "You can select only one Profit Share Agreement."));
				return;
			}

			if (!profitShareAgreementsToAdd.Any())
			{
				return;
			}

			var newAgreementToAdd = profitShareAgreementsToAdd.First();

			if (ProfitShareRulesParent != null && ProfitShareRulesParent.PK != newAgreementToAdd.PK)
			{
				var message = ResString.GetMultilingualString(
				"9276256A-6444-4DE5-A0D5-21D5DD05CB6E",
				"The new selected Profit Share Agreement is different from what has already been selected.\r\nThis action will override the previous one. Would you like to proceed?");

				var dialogResult = Globals.Message.Show(message, (NoResString)"Profit Share Agreement Selection", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Information, ZDialogResult.No);
				if (dialogResult == ZDialogResult.No)
				{
					return;
				}
			}

			// Only eligible selected rules are accepted
			var filteredList = newAgreementToAdd.SelectedProfitShareDetailsList
				.Where(x => x.O4_JobType == JobTypesList.Codes.GCN
					&& !x.O4_GatewayProfitApportionmentMethod.IsEmpty)
				.Select(x => Factory.New<ProfitShareRedistributionRule>().FromProfitShareDetails(x));

			ProfitShareRulesParent = newAgreementToAdd;
			ProfitShareRules.RemoveAndDeleteAll();
			ProfitShareRules.AddRange(filteredList);
		}

		/// <summary>
		/// Remove ProfitShare Rules from ProfitShareRules collection.
		/// </summary>
		public void RemoveProfitShareRules(IEnumerable<OrgProfitShareDetails> profitShareRules)
		{
			ProfitShareRules.RemoveRange(ProfitShareRules.Where(r => profitShareRules.Any(psd => r.ProfitShareDetails == psd)).ToList());
		}
	}
}
