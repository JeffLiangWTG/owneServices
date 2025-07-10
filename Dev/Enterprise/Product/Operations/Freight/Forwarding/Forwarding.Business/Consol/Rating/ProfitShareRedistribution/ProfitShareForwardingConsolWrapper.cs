using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ProfitShareForwardingConsolWrapper : NonPersistentBusinessObject
	{
		public ProfitShareForwardingConsolWrapper(ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));
			Consol = consol;
		}

		public ProfitShareForwardingConsolWrapper(ForwardingConsol consol, ConsolidationProfitShare profitShare) : this(consol)
		{
			JK_Calc_TotalProfitAmount = profitShare.CPS_TotalConsolProfitShare;
			JK_Calc_RedistributedProfitAmount = profitShare.CPS_RedistributedConsolProfitShare;
		}

		public ForwardingConsol Consol { get; }

		protected override ZGuid GetPK() => Consol.PK;

		public ZDecimal JK_Calc_TotalProfitAmount
		{
			get => calcTotalProfitAmount;
			set
			{
				if (calcTotalProfitAmount != value)
				{
					calcTotalProfitAmount = value;
					JK_Calc_TotalProfitAmountInfo.RefreshBinding();
				}
			}
		}
		ZDecimal calcTotalProfitAmount = 0m;

		public ZPropertyInfo JK_Calc_TotalProfitAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_TotalProfitAmount)); }
		}

		public ZDecimal JK_Calc_RedistributedProfitAmount
		{
			get => calcRedistributedProfitAmount;
			set
			{
				if (calcRedistributedProfitAmount != value)
				{
					calcRedistributedProfitAmount = value;
					JK_Calc_RedistributedProfitAmountInfo.RefreshBinding();
				}
			}
		}
		ZDecimal calcRedistributedProfitAmount = 0m;

		public ZPropertyInfo JK_Calc_RedistributedProfitAmountInfo
		{
			get { return GetZPropertyInfo(nameof(JK_Calc_RedistributedProfitAmount)); }
		}

		public ZString JK_UniqueConsignRef => Consol.JK_UniqueConsignRef;
		public ZString JK_TransportMode => Consol.JK_TransportMode;
		public ZString JK_ConsolMode => Consol.JK_ConsolMode;
		public ZString JK_MasterBillNum => Consol.JK_MasterBillNum;
		public ZString JK_RL_NKLoadPort => Consol.JK_RL_NKLoadPort;
		public ZString JK_RL_NKDischargePort => Consol.JK_RL_NKDischargePort;
		public ZDateTime JK_JX_JA_E_DEP => Consol.JK_JX_JA_E_DEP;
		public ZDateTime JK_JX_JB_E_ARV => Consol.JK_JX_JB_E_ARV;
		public ZString JK_JX_JV_VoyageFlight => Consol.JK_JX_JV_VoyageFlight;
		public ZDecimal JK_ConsolChargeableRate => Consol.JK_ConsolChargeableRate;
		public ZDecimal JK_TotalShipmentWeight => Consol.JK_TotalShipmentWeight;
		public ZString JK_TotalShipmentWeightUnit => Consol.JK_TotalShipmentWeightUnit;
		public ZDecimal JK_TotalShipmentVolume => Consol.JK_TotalShipmentVolume;
		public ZString JK_TotalShipmentVolumeUnit => Consol.JK_TotalShipmentVolumeUnit;
		public ZDecimal JK_TotalShipmentChargeable => Consol.JK_TotalShipmentChargeable;
		public ZString JK_CorrectedConsolVolumeUnit => Consol.JK_CorrectedConsolVolumeUnit;
		public ZString JK_Calc_TotalShipmentChargeableUnit => Consol.JK_Calc_TotalShipmentChargeableUnit;
		public ZDecimal JK_CorrectedConsolWeight => Consol.JK_CorrectedConsolWeight;
		public ZString JK_CorrectedConsolWeightUnit => Consol.JK_CorrectedConsolWeightUnit;
		public ZDecimal JK_CorrectedConsolVolume => Consol.JK_CorrectedConsolVolume;
		public ZString JK_SendingForwarderHandlingType => Consol.JK_SendingForwarderHandlingType;
		public ZString JK_Calc_SendingAgentCode => Consol.JK_Calc_SendingAgentCode;
		public ZString JK_ReceivingForwarderHandlingType => Consol.JK_ReceivingForwarderHandlingType;
		public ZString JK_Calc_ReceivingAgentCode => Consol.JK_Calc_ReceivingAgentCode;
	}

	public class ProfitShareForwardingConsolWrapperCollection : NonPersistentBusinessObjectCollection<ProfitShareForwardingConsolWrapper>
	{
		public ProfitShareForwardingConsolWrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

		protected override bool AllowNewCore => false;

		public void Load(ConsolidationProfitShareCollection consoleProfitShareCollection)
		{
			var profitShares = consoleProfitShareCollection.OfType<ConsolidationProfitShare>();
			var query = new ZQuery(JobConsolSchema.PK, profitShares.Select(x => x.CPS_JK));
			var consols = Factory.Load<ForwardingConsol>(query).ToDictionary(c => c.PK);

			foreach (var profitShare in profitShares)
			{
				Add(new ProfitShareForwardingConsolWrapper(consols[profitShare.CPS_JK], profitShare));
			}
		}
	}
}
