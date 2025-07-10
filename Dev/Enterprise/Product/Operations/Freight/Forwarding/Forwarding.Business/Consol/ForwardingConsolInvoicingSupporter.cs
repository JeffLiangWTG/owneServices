using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolInvoicingSupporter : JobInvoicingSupporter
	{
		public ForwardingConsolInvoicingSupporter(ForwardingConsol parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			Consol = parent;
		}

		readonly ForwardingConsol Consol;

		public override bool CanCreateInvoicingJob
		{
			get { return Consol.IsGateway(); }
		}

		public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			return null;
		}

		IGateway GatewayConsol => Consol;

		public override void ValidateJobProperty(ZPropertyInfo info)
		{
			if (info.Name == JobHeaderSchema.Constants.JH_GB && Consol.IsGatewayConsol)
			{
				var job = info.BizObj as JobHeader;
				if (job != null)
				{
					var gatewayAgent = GatewayConsol.GatewayBillingSupporter.GatewayAgent();
					if (gatewayAgent == (null, null))
					{
						info.AddError(Res.GetString("1af5fb88-dc86-44a0-832b-2ae9ce2515ee", "Gateway Job Header must have a Gateway Agent. Please check either the Sending or Receiving Agent is a valid Gateway Agent for this company."));
					}
					else
					{
						var isMatchingSendingAgentOrgProxy = gatewayAgent.sendingAgent != null && job.Branch != null && job.Branch.GB_OH_OrgProxy == gatewayAgent.sendingAgent.PK;
						var isMatchingReceivingAgentOrgProxy = gatewayAgent.receivingAgent != null && job.Branch != null && job.Branch.GB_OH_OrgProxy == gatewayAgent.receivingAgent.PK;
						if (job.Branch == null || (!isMatchingSendingAgentOrgProxy && !isMatchingReceivingAgentOrgProxy))
						{
							info.AddError(Res.GetString("101dc924-b2b3-45b3-aca5-f608c68cb604", "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy."));
						}
					}
				}
			}
		}

		public override OrgHeader GetOrganisationByBranchDefaultingRule(ZString defaultingRule)
		{
			var result = base.GetOrganisationByBranchDefaultingRule(defaultingRule);

			if (result == null)
			{
				switch (defaultingRule)
				{
					case Constants.ChargeCodeBranchDefaultingRule.SendingAgent:
						result = SendingAgent;
						break;

					case Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent:
						result = ReceivingAgent;
						break;
				}
			}

			return result;
		}

		public override ZString ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid localChargesPK)
		{
			var result = ZString.Empty;
			if (Consol.SendingForwarder != null
				&& Consol.IsGateway()
				&& GatewayConsol.GatewayBillingSupporter.GatewayAgent().sendingAgent?.PK == Consol.SendingForwarder.PK)
			{
				if (!(GlbCompany.CurrentCompany.GC_OH_OrgProxy == localChargesPK
					|| GlbCompany.CurrentCompany.Branches.Any(x => x.GB_IsActive && x.GB_OH_OrgProxy == localChargesPK)))
				{
					result = Res.GetString("584aa6d0-a496-45f7-861c-2c539fa16c0d", "Prepaid Agent has to be a proxy of the Gateway Agents Company.");
				}
			}
			return result;
		}

		public override ZString ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid agentCollectPK)
		{
			var result = ZString.Empty;
			if (Consol.ReceivingForwarder != null
				&& Consol.IsGateway()
				&& GatewayConsol.GatewayBillingSupporter.GatewayAgent().receivingAgent?.PK == Consol.ReceivingForwarder.PK)
			{
				if (!(GlbCompany.CurrentCompany.GC_OH_OrgProxy == agentCollectPK
					|| GlbCompany.CurrentCompany.Branches.Any(x => x.GB_IsActive && x.GB_OH_OrgProxy == agentCollectPK)))
				{
					result = Res.GetString("80029d7f-b221-4d07-870f-f5f33d8d704d", "Collect Agent has to be a proxy of the Gateway Agents Company.");
				}
			}
			return result;
		}

		public override ZDecimal ActualChargeable
		{
			get { return Consol.JK_TotalShipmentChargeable; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return Consol.JK_TotalShipmentChargeableUnit; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Consol.JK_TotalShipmentVolume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Consol.JK_TotalShipmentVolumeUnit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Consol.JK_TotalShipmentWeight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Consol.JK_TotalShipmentWeightUnit; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.MaintainConsolAuditBilling;
		}

		public override OrgHeader Consignee
		{
			get { return Consol.ReceivingForwarder; }
		}

		public override OrgHeader Consignor
		{
			get { return Consol.SendingForwarder; }
		}

		public override ZString ConsolType
		{
			get { return Consol.JK_AgentType; }
		}

		public override ZString ConsolNumber
		{
			get { return Consol.JK_UniqueConsignRef; }
		}

		public override ZDecimal ConsolExchangeRate
		{
			get { return Consol.FreightCostsExchangeRate; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return Consol.FreightCostsCurrency; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get
			{
				return Consol.IsGateway()
					? JobInvoicingConsumerTypes.GatewayConsol
					: JobInvoicingConsumerTypes.ForwardingConsol;
			}
		}

		public override ZString ContainerMode
		{
			get { return Consol.JK_ConsolMode; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return Consol.IsGateway(); }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			if (Consol.IsGateway())
			{
				var creditorPK = Consol.GetCreditorPKForConsolCost(ZGuid.Empty);
				if (!creditorPK.IsEmpty)
				{
					return Consol.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, creditorPK));
				}
			}

			return Consol.Creditor;
		}

		public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber)
		{
			if (ConsumerType.Code == JobInvoicingConsumerTypes.GatewayConsol.Code && (chargeCode != null || Consol.HasContext(BusinessContext.GetDefaultDebtorWithoutChargeCode)))
			{
				OrgHeader debtor = null;

				var ranker = new StringColumnValueRanker();
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.ConsolDirection, GetConsolDirectionFallBackCode());
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.ConsolTransportMode, GetConsolTransportModeFallBackCode());
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.ChargeGroup, GetChargeGroupFallBackCode(chargeCode?.AC_ChargeGroup ?? ZString.Empty));
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.ConsolPaymentTerm, GetConsolPaymentTermFallBackCode(PaymentType));
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.RelatedJob, GetRelatedJobFallBackCode(relatedJobNumber));
				ranker.Add(GatewayChargeDefaultDebtorConfiguration.Schema.PreviousSendingAgent, GetPreviousSendingAgentFallBackCode(relatedJobNumber));

				var configurations = new List<GatewayChargeDefaultDebtorConfiguration>();
				AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.Value.ForEach(x => configurations.Add(x as GatewayChargeDefaultDebtorConfiguration));

				var matchedConfigurations = (ranker.GetBestMatch(configurations) ?? Enumerable.Empty<GatewayChargeDefaultDebtorConfiguration>()).ToArray();
				var matchedConfiguration = matchedConfigurations.FirstOrDefault();
				if (matchedConfiguration != null)
				{
					ForwardingShipment shipment;

					switch (matchedConfiguration.Debtor)
					{
						case Constants.GatewayDebtor.Codes.SendingAgent:
							debtor = SendingAgent;
							break;
						case Constants.GatewayDebtor.Codes.ReceivingAgent:
							debtor = ReceivingAgent;
							break;

						case Constants.GatewayDebtor.Codes.ShipmentPickupAgent:
							shipment = GetShipment(relatedJobNumber);
							debtor = shipment?.PickupAgent;
							break;

						case Constants.GatewayDebtor.Codes.ShipmentDeliveryAgent:
							shipment = GetShipment(relatedJobNumber);
							debtor = shipment?.DeliveryAgent;
							break;

						case Constants.GatewayDebtor.Codes.PreviousSendingAgent:
							debtor = GetPreviousConsol(relatedJobNumber)?.InvoicingSupporter?.SendingAgent;
							break;
					}
				}

				return debtor;
			}

			return base.GetDefaultDebtor(chargeCode, job, relatedJobNumber);
		}

		public override IJobInvoicingPlugIn GetPreviousConsol(ZString relatedJobNumber)
		{
			var shipment = GetShipment(relatedJobNumber);
			var supporter = shipment?.InvoicingSupporter as IGatewayJobInvoicingSupporter;
			var allConsols = supporter?.OrderedInvoiceTargets;
			return allConsols?.ElementInFrontOf(Consol);
		}

		public override string GetReasonNotToAllowAutoRate(AutoRateOptions options = default)
		{
			string result = base.GetReasonNotToAllowAutoRate(options);

			if (string.IsNullOrEmpty(result))
			{
				if (Consol.JK_PrepaidCollect.IsEmpty)
				{
					result = Res.GetString("F5B1CF6D-A30A-42B4-A18A-0DBC446F182E", "Payment Term is mandatory for Autorating Consol.");
				}
				var useGlowRateSelector = RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection();
				if (useGlowRateSelector)
				{
					result = TrySetReasonToNotAllowAutoRateForC3();
				}
			}

			return result;
		}

		string TrySetReasonToNotAllowAutoRateForC3()
		{
			switch (Consol.JK_ConsolMode)
			{
				case Constants.ContainerModes.FCL:
				case Constants.ContainerModes.ULD:
					if (Consol.Containers.Count == 0)
					{
						return Res.GetString("920959D1-D01A-4703-A3BA-6710908730D3", "Autorating unavailable for an empty Consol. Please add container into Consol to continue.");
					}
					break;
				default:
					if (Consol.ShipmentCount == 0)
					{
						return Res.GetString("929A4ADE-9620-4F9D-BC14-530CFD3F5571", "Autorating unavailable for an empty Consol. Please add cargo into Consol to continue.");
					}
					break;
			}

			return null;
		}

		ForwardingShipment GetShipment(ZString relatedJobNumber)
		{
			if (!relatedJobNumber.IsEmpty)
			{
				return Consol.CostSupporter.ShipmentsList.SingleOrDefault(x => x.JobNumber == relatedJobNumber) as ForwardingShipment;
			}

			return null;
		}

		IZType[] GetChargeGroupFallBackCode(ZString chargeGroup)
		{
			var all = GatewayChargeDefaultDebtorConfigurationLookups.All;
			var result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(chargeGroup) && chargeGroup != all)
			{
				result.Add(chargeGroup);
			}
			result.Add((ZString)all);
			return result.ToArray();
		}

		IZType[] GetConsolPaymentTermFallBackCode(ZString consolPaymentTerm)
		{
			var all = GatewayChargeDefaultDebtorConfigurationLookups.All;
			var result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(consolPaymentTerm) && consolPaymentTerm != all)
			{
				result.Add(consolPaymentTerm);
			}
			result.Add((ZString)all);
			return result.ToArray();
		}

		public override IZType[] GetConsolDirectionFallBackCode()
		{
			var all = GatewayChargeDefaultDebtorConfigurationLookups.All;
			var result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(Direction) && Direction != all)
			{
				result.Add(Direction);
			}
			result.Add((ZString)all);
			return result.ToArray();
		}

		public override IZType[] GetConsolTransportModeFallBackCode()
		{
			var all = GatewayChargeDefaultDebtorConfigurationLookups.All;
			var result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(TransportMode) && TransportMode != all)
			{
				result.Add(TransportMode);
			}
			result.Add((ZString)all);
			return result.ToArray();
		}

		IZType[] GetRelatedJobFallBackCode(ZString relatedJobNumber)
		{
			return new IZType[]
			{
				(ZString)(relatedJobNumber.IsEmpty ? Constants.GatewayRelatedJob.Codes.NotRelatedToJob : Constants.GatewayRelatedJob.Codes.RelatedToShipment),
				(ZString)Constants.GatewayRelatedJob.Codes.All
			};
		}

		public override IZType[] GetPreviousSendingAgentFallBackCode(ZString relatedJobNumber)
		{
			var result = new List<ZString>();
			var previousConsol = GetPreviousConsol(relatedJobNumber);

			if (previousConsol != null && previousConsol is ForwardingConsol prevForwConsol && prevForwConsol.SendingForwarder != null)
			{
				if (prevForwConsol.JK_SendingForwarderHandlingType == AgentStatusList.Codes.GatewayAgent)
				{
					result.Add(Constants.GatewayPreviousSendingAgent.Codes.GatewayAgent);
				}
				else if (prevForwConsol.JK_SendingForwarderHandlingType == AgentStatusList.Codes.GatewayAgentWithTariff)
				{
					result.Add(Constants.GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT);
				}
				else
				{
					result.Add(Constants.GatewayPreviousSendingAgent.Codes.SendingAgent);
				}
			}
			else
			{
				result.Add(Constants.GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent);
			}

			result.Add(Constants.GatewayPreviousSendingAgent.Codes.All);

			return result.Cast<IZType>().ToArray();
		}

		public override RefUNLOCO Destination
		{
			get { return Consol.DischargePort; }
		}

		public override ZDateTime ATA
		{
			get
			{
				return Consol.JK_JX_JB_A_ARV.IsValid ? Consol.JK_JX_JB_A_ARV : Consol.JK_JX_JB_E_ARV;
			}
		}

		public override ZDateTime ATD
		{
			get
			{
				return Consol.JK_JX_JA_A_DEP.IsValid ? Consol.JK_JX_JA_A_DEP : Consol.JK_JX_JA_E_DEP;
			}
		}

		public override ZDateTime ETA
		{
			get { return Consol.JK_JX_JB_E_ARV; }
		}

		public override ZDateTime ETD
		{
			get { return Consol.JK_JX_JA_E_DEP; }
		}

		public override ZDateTime ArrivalAtLoadPort
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				var transport = Consol.Transports.MostInterestingTransport;
				if (transport != null && transport.JW_IsLinked && transport.Sailing != null)
				{
					if (transport.Sailing.Origin.JA_A_ARV.IsValid)
					{
						result = transport.Sailing.Origin.JA_A_ARV;
					}
					else if (transport.Sailing.Origin.JA_E_ARV.IsValid)
					{
						result = transport.Sailing.Origin.JA_E_ARV;
					}
				}

				return result;
			}
		}

		public override ZDateTime EstimatedArrivalAtLoadPort
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				var transport = Consol.Transports.MostInterestingTransport;
				if (transport != null && transport.JW_IsLinked && transport.Sailing != null && transport.Sailing.Origin.JA_E_ARV.IsValid)
				{
					result = transport.Sailing.Origin.JA_E_ARV;
				}

				return result;
			}
		}

		public override bool EditSecurityLock
		{
			get { return false; }
		}

		public override bool IsDomestic
		{
			get { return Consol.IsDomesticFreight; }
		}

		public override bool IsExport
		{
			get { return Consol.IsExport(); }
		}

		public override bool IsImport
		{
			get { return Consol.IsImport(); }
		}

		public ZString Direction => IsDomestic ? Constants.FreightShipmentDirection.Code.Domestic :
									IsImport ? Constants.FreightShipmentDirection.Code.Import :
									IsExport ? Constants.FreightShipmentDirection.Code.Export :
									Constants.FreightShipmentDirection.Code.Other;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Consol.IsGateway()
			? Env.Security.GatewayConsolJobInvoicing
			: Env.Security.MaintainConsolJobInvoicing;
		}

		public override ZString MasterBillNumber
		{
			get { return Consol.JK_MasterBillNum; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			return Consol.RatingAdapter.JobDatesProvider.GetJobDateByType(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override RefUNLOCO Origin
		{
			get { return Consol.LoadPort; }
		}

		public override ZGuid OverriddenDepartmentPK => Consol.IsGateway() ? ZGuid.Empty : GlbDepartment.CurrentDepartment.PK.ToGuid();

		public override OrgHeader ReceivingAgent
		{
			get { return Consol.ReceivingForwarder; }
		}

		public override OrgHeader SendingAgent
		{
			get { return Consol.SendingForwarder; }
		}

		public override ZString TransportMode
		{
			get { return Consol.JK_TransportMode; }
		}

		public override int ContainerCount
		{
			get { return Consol.JK_Calc_ContainerCount; }
		}

		public override ZDecimal TEUCount
		{
			get { return Consol.Containers.TEUCount; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return Consol.RatingAdapter.PaymentTerm; }
		}

		public ZString PaymentType
		{
			get { return Consol.JK_PrepaidCollect; }
		}

		public override ZString VoyageVesselOrFlightDate
		{
			get
			{
				var result = ZString.Empty;
				if (Consol != null)
				{
					var departureDate = !Consol.JK_JX_JA_A_DEP.IsEmpty ? Consol.JK_JX_JA_A_DEP : Consol.JK_JX_JA_E_DEP;
					result = GetVoyageVesselOrFlightDatesCore(Consol.JK_TransportMode, departureDate, Consol.JK_JX_JV_NKVessel, Consol.JK_JX_JV_VoyageFlight);
				}
				return result;
			}
		}
	}
}
