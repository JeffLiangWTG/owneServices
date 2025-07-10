using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public sealed class ProfitShareRedistributionRule : AutoProfitShareRedistributionRule
	{
		public ProfitShareRedistributionRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ProfitShareRedistributionRule FromProfitShareDetails(OrgProfitShareDetails orgProfitShareDetails)
		{
			PRR_StartDate = orgProfitShareDetails.O4_StartDate;
			PRR_EndDate = orgProfitShareDetails.O4_EndDate;
			PRR_SendingPortOrCountry = orgProfitShareDetails.O4_SendingPortOrCountry;
			PRR_ReceivingPortOrCountry = orgProfitShareDetails.O4_ReceivingPortOrCountry;
			PRR_JobType = orgProfitShareDetails.O4_JobType;
			PRR_GatewayAgentType = orgProfitShareDetails.O4_GatewayAgentType;
			PRR_GatewayProfitApportionmentMethod = orgProfitShareDetails.O4_GatewayProfitApportionmentMethod;
			PRR_FreightMode = orgProfitShareDetails.O4_FreightMode;
			PRR_AgreementType = orgProfitShareDetails.O4_AgreementType;
			PRR_ShareLosses = orgProfitShareDetails.O4_ShareLosses;
			PRR_OH_ControllingAgent = orgProfitShareDetails.O4_OH_ControllingAgent;
			PRR_OrgOverrideType = orgProfitShareDetails.O4_OrgOverrideType;
			PRR_OH_OrgOverride = orgProfitShareDetails.O4_OH_OrgOverride;
			PRR_ShipmentPickupAgentProfitSharePercent = orgProfitShareDetails.ShipmentPickupAgentProfitShare;
			PRR_ShipmentDeliveryAgentProfitSharePercent = orgProfitShareDetails.ShipmentDeliveryAgentProfitShare;

			ProfitShareDetails = orgProfitShareDetails;
			AgreementTypeDescription = orgProfitShareDetails.AgreementTypeDescription;

			return this;
		}

		public ProfitShareRedistribution Parent
		{
			get => Factory.Load<ProfitShareRedistribution>(PRR_PSR_ProfitShareRedistribution);
		}

		[RelatedBusinessObject("Parent")]
		public override ZGuid PRR_PSR_ProfitShareRedistribution
		{
			get => base.PRR_PSR_ProfitShareRedistribution;
			set => base.PRR_PSR_ProfitShareRedistribution = value;
		}

		public OrgProfitShareDetails ProfitShareDetails { get; private set; }

		[BusinessObjectTestExclude]
		public ZString AgreementTypeDescription { get; private set; }
	}
}
