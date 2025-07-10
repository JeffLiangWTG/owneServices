using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAgentRelationshipCollection : BusinessObjectCollection<OrgAgentRelationship>
	{
		public OrgAgentRelationshipCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgAgentRelationshipCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		#region ReadOnly

		/// <summary>
		/// For all agent relationships in this collection, sets the relevant field readonly
		/// if it is a certain org.
		/// </summary>
		/// <param name="org">The org to search for.</param>
		public void SetOrganisationReadOnly(OrgHeader org)
		{
			foreach (OrgAgentRelationship relationship in this)
			{
				if (relationship.O3_OH_ReceivingAgent == org.PK)
				{
					relationship.O3_OH_ReceivingAgent_ReadOnly = true;
				}

				if (relationship.O3_OH_SendingAgent == org.PK)
				{
					relationship.O3_OH_SendingAgent_ReadOnly = true;
				}

				relationship.OrgBeingViewedFrom = org;
			}
		}

		#endregion
	}
}
