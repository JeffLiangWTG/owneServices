using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgProfitShareParty : AutoOrgProfitShareParty
	{
		public OrgProfitShareParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Calculation

		public (ZDecimal result, ZString calculationDescription) CalculateProfitShare(ZDecimal totalProfit, ZDecimal grossRevenue, ZDecimal totalChargeable, int containerCount)
		{
			var additionalCalculationDescription = ZString.Empty;
			var rateBasisDescription = Lookups.FeeBasis.GetDescriptionFromCode(PS_PartyRateBasis);

			var isGrossRevenue = PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;
			var amountToUse = isGrossRevenue ? grossRevenue : totalProfit;

			// core share calculated from percentages
			ZDecimal result = amountToUse * (PS_PartyProfitSharePercent / 100);
			var calculationDescription = isGrossRevenue
				? Res.GetString("dd294761-c61a-4b4e-a304-95b430547b19", "{0}% of gross revenue of {1}", PS_PartyProfitSharePercent.ToString(2), amountToUse.ToString(2))
				: Res.GetString("a522a233-d67b-4bf6-bc72-babfa62a568c", "{0}% of profit of {1}", PS_PartyProfitSharePercent.ToString(2), amountToUse.ToString(2));

			// additional share calculated from rate
			if (PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit)
			{
				additionalCalculationDescription = $" + ({PS_PartyRate.ToString(2)} * {totalChargeable} {rateBasisDescription})";
				result += PS_PartyRate * totalChargeable;
			}
			else if (PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee)
			{
				additionalCalculationDescription = $" + {PS_PartyRate.ToString(2)} {rateBasisDescription}";
				result += PS_PartyRate;
			}
			else if (PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer)
			{
				additionalCalculationDescription = $" + ({PS_PartyRate.ToString(2)} * {containerCount} {rateBasisDescription})";
				result += PS_PartyRate * containerCount;
			}

			if (!additionalCalculationDescription.IsEmpty)
			{
				calculationDescription += additionalCalculationDescription;
			}

			// minimum share can override the above calculations
			if (PS_PartyMinimum > 0m && PS_PartyMinimum > result)
			{
				calculationDescription = Res.GetString("bc713aeb-c519-42c3-a002-4c62536b3bb2", "Minimum Profit Share overrides") + " " + calculationDescription;
				result = PS_PartyMinimum;
			}

			return (result, calculationDescription);
		}

		public (ZDecimal result, ZString calculationDescription) CalculateProfitShareForSingleCharge(ZDecimal totalProfit, ZDecimal grossRevenue, ZDecimal totalChargeable, int containerCount)
		{
			var additionalCalculationDescription = ZString.Empty;
			var rateBasisDescription = Lookups.FeeBasis.GetDescriptionFromCode(PS_PartyRateBasis);

			var isGrossRevenue = PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;
			var amountToUse = isGrossRevenue ? grossRevenue : totalProfit;

			ZDecimal result = amountToUse * (PS_PartyProfitSharePercent / 100);
			var calculationDescription = isGrossRevenue
				? Res.GetString("dd294761-c61a-4b4e-a304-95b430547b19", "{0}% of gross revenue of {1}", PS_PartyProfitSharePercent.ToString(2), amountToUse.ToString(2))
				: Res.GetString("a522a233-d67b-4bf6-bc72-babfa62a568c", "{0}% of profit of {1}", PS_PartyProfitSharePercent.ToString(2), amountToUse.ToString(2));

			return (result, calculationDescription);
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return (NoResString)"Party - " + PS_PartyType; }
		}

		#endregion

		#region Properties

		#region PS_PartyType

		[List(nameof(Lookups) + "." + nameof(OrgProfitSharePartyLookups.PartyTypes))]
		public override ZString PS_PartyType
		{
			get { return base.PS_PartyType; }
			set
			{
				base.PS_PartyType = value;
				if (PS_PartyType != OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent)
				{
					PropagateValuesToRelatedParty();
				}
			}
		}

		#endregion

		#region Party Type Description

		public ZString PartyTypeDescription => Lookups.PartyTypes.GetDescriptionFromCode(PS_PartyType);

		public ZPropertyInfo PartyTypeDescriptionInfo => GetZPropertyInfo(nameof(PartyTypeDescription));

		#endregion

		#region PS_PartyRate

		[DecimalPlaces(nameof(RateDecimals))]
		public override ZDecimal PS_PartyRate
		{
			get { return base.PS_PartyRate; }
			set
			{
				base.PS_PartyRate = value;
				PropagateValuesToRelatedParty();
			}
		}

		#endregion

		#region PS_PartyRateBasis

		[List("Lookups.FeeBasis")]
		public override ZString PS_PartyRateBasis
		{
			get { return base.PS_PartyRateBasis; }
			set
			{
				base.PS_PartyRateBasis = value;
				PropagateValuesToRelatedParty();
			}
		}

		#endregion

		#region PS_PartyMinimum

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal PS_PartyMinimum
		{
			get { return base.PS_PartyMinimum; }
			set
			{
				base.PS_PartyMinimum = value;
				PropagateValuesToRelatedParty();
			}
		}

		#endregion

		#region PS_PartyProfitSharePercent

		[DecimalPlaces(nameof(PercentDecimals))]
		public override ZDecimal PS_PartyProfitSharePercent
		{
			get { return base.PS_PartyProfitSharePercent; }
			set
			{
				base.PS_PartyProfitSharePercent = value;
				PropagateValuesToRelatedParty();
			}
		}

		#endregion

		internal void PropagateValuesToRelatedParty()
		{
			OrgProfitShareParty relatedParty = GetRelatedParty();
			if (relatedParty != null)
			{
				if (relatedParty.PS_PartyRate != PS_PartyRate)
				{
					relatedParty.PS_PartyRate = PS_PartyRate;
				}

				if (relatedParty.PS_PartyRateBasis != PS_PartyRateBasis)
				{
					relatedParty.PS_PartyRateBasis = PS_PartyRateBasis;
				}

				if (relatedParty.PS_PartyProfitSharePercent != PS_PartyProfitSharePercent)
				{
					relatedParty.PS_PartyProfitSharePercent = PS_PartyProfitSharePercent;
				}

				if (relatedParty.PS_PartyMinimum != PS_PartyMinimum)
				{
					relatedParty.PS_PartyMinimum = PS_PartyMinimum;
				}
			}
			RefreshBinding();
		}

		OrgProfitShareParty GetRelatedParty()
		{
			OrgProfitShareParty relatedParty = null;
			if ((PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent &&
					ProfitShareDetails.ControllingAgentIsSendingAgent) ||
					(PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent &&
					ProfitShareDetails.ControllingAgentIsReceivingAgent))
			{
				relatedParty = ProfitShareDetails.PartyDetails.GetParty(OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent);
			}

			return relatedParty;
		}

		#endregion

		#region Decimals

		public int CurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int PercentDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		public int RateDecimals => 2;

		#endregion

		#region Readonly

		public override bool ReadOnly
		{
			get
			{
				bool controlAgentIsSendRcv = PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent &&
					ProfitShareDetails != null &&
					(ProfitShareDetails.ControllingAgentIsReceivingAgent || ProfitShareDetails.ControllingAgentIsSendingAgent);

				return base.ReadOnly || controlAgentIsSendRcv;
			}
			set { base.ReadOnly = value; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (ProfitShareDetails != null && ProfitShareDetails.OrgProfitShareHeader != null && ProfitShareDetails.OrgProfitShareHeader.OrgBeingViewedFrom != null)
			{
				shouldBeReadOnly = !ProfitShareDetails.OrgProfitShareHeader.OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		internal string OrgProfitShareType
		{
			get
			{
				var parentCollection = ((IBusinessObjectInternals)this).ParentCollections
					.OfType<OrgProfitSharePartyCollection>()
					.FirstOrDefault();
				return parentCollection?.OrgProfitShareType;
			}
		}
	}
}
