using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAgentRelationshipValidation : AutoOrgAgentRelationshipValidation
	{
		public OrgAgentRelationshipValidation(AutoOrgAgentRelationship parent) : base(parent)
		{
		}

		#region Receiving Agent

		protected override void CheckO3_OH_ReceivingAgent()
		{
			base.CheckO3_OH_ReceivingAgent();

			if (!Parent.O3_OH_ReceivingAgent.IsEmpty && Parent.O3_OH_ReceivingAgent == Parent.O3_OH_SendingAgent)
			{
				Parent.O3_OH_ReceivingAgentInfo.AddError(Res.GetString("6cd40d0e-acc9-44ad-b7f9-8c4510a6b5d3", "Sending and Receiving Agent must be different organizations."));
			}

			if (!Parent.O3_OH_ReceivingAgent.IsEmpty && !Parent.O3_OH_SendingAgent.IsEmpty && !RelationshipIsUnique)
			{
				Parent.O3_OH_ReceivingAgentInfo.AddError(Res.GetString("1817a495-280d-4cf6-8847-e3a4dd1ac236", "An agent relationship already exists for this Sending and Receiving Agent pair."));
			}
		}

		#endregion

		#region Sending Agent

		protected override void CheckO3_OH_SendingAgent()
		{
			base.CheckO3_OH_SendingAgent();

			if (!Parent.O3_OH_SendingAgent.IsEmpty && Parent.O3_OH_SendingAgent == Parent.O3_OH_ReceivingAgent)
			{
				Parent.O3_OH_SendingAgentInfo.AddError(Res.GetString("6cd40d0e-acc9-44ad-b7f9-8c4510a6b5d3", "Sending and Receiving Agent must be different organizations."));
			}

			if (!Parent.O3_OH_ReceivingAgent.IsEmpty && !Parent.O3_OH_SendingAgent.IsEmpty && !RelationshipIsUnique)
			{
				Parent.O3_OH_SendingAgentInfo.AddError(Res.GetString("1817a495-280d-4cf6-8847-e3a4dd1ac236", "An agent relationship already exists for this Sending and Receiving Agent pair."));
			}
		}

		#endregion

		#region Unique Relationship

		bool RelationshipIsUnique
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgAgentRelationshipSchema.O3_OH_SendingAgent, Parent.O3_OH_SendingAgent);
				filter.AddToFilter(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, Parent.O3_OH_ReceivingAgent);

				BusinessObject[] results = Parent.Factory.Load(typeof(OrgAgentRelationship), filter);

				return results.Length == 0 || (results.Length == 1 && results[0].PK == Parent.PK);
			}
		}

		#endregion

		#region Profit Share Type

		protected override void CheckO3_ProfitShareType()
		{
			base.CheckO3_ProfitShareType();
			MandatoryValidation.CheckEntered(Parent.O3_ProfitShareTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.O3_ProfitShareTypeInfo);
		}

		#endregion

		#region O3_OH_GroupNetworkOrFranchise

		protected override void CheckO3_OH_GroupNetworkOrFranchise()
		{
			base.CheckO3_OH_GroupNetworkOrFranchise();
			ValidateChildrenPartyDetailsType();
		}

		void ValidateChildrenPartyDetailsType()
		{
			foreach (OrgProfitShareDetails details in Parent.ProfitShareDetails)
			{
				foreach (OrgProfitShareParty party in details.PartyDetails)
				{
					party.Validation.ValidatePS_PartyType();
				}
			}
		}

		#endregion

		#region Implementation

		public new OrgAgentRelationship Parent
		{
			get { return (OrgAgentRelationship)base.Parent; }
		}

		#endregion
	}
}
