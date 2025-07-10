using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly), CodeProperty(OrgAgentRelationshipSchema.Constants.O3_ProfitShareType)]
	public class OrgAgentRelationship : AutoOrgAgentRelationship, ITemplateCopyable, IDocManagerSupport
	{
		public OrgAgentRelationship(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class ProfitShareTypes
		{
			public const string Standard = "STD";
			public const string AgencyProfile = "AGY";
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			/// <summary>
			/// Loads the Agent Relationship that either has:
			///		1. matching pickup agent, sending agent, and receiving agent.
			///		2. or pickup matching, sending matching, and receiving blank.
			///		3. or pickup matching, sending blank, and receiving matching.
			///		4. or pickup blank, sending matching, and receiving matching.
			///		5. or pickup blank, sending matching and receiving blank.
			///		6. or pickup blank, sending blank and receiving matching.
			///		7. or all blank
			/// </summary>
			public OrgAgentRelationship Load(OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader pickupAgent = null)
			{
				return pickupAgent == null
					? LoadBestMatchWithoutPickupAgent(sendingAgent, receivingAgent)
					: LoadBestMatchWithPickupAgent(sendingAgent, receivingAgent, pickupAgent);
			}

			OrgAgentRelationship LoadBestMatchWithoutPickupAgent(OrgHeader sendingAgent, OrgHeader receivingAgent)
			{
				ColumnValueRanker ranker = new ColumnValueRanker();
				ranker.Add(OrgAgentRelationshipSchema.O3_OH_SendingAgent, sendingAgent?.PK, null);
				ranker.Add(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, receivingAgent?.PK, null);

				ZQuery query = new ZQuery(OrgAgentRelationshipSchema.O3_ProfitShareType, ProfitShareTypes.Standard);
				return ranker.GetBestMatch<OrgAgentRelationship>(Factory, query);
			}

			/// <summary>
			/// The pickup agent is part of ProfitShareDetails (child), not OrgAgentRelationship (parent).
			/// Therefore, ColumnValueRanker cannot directly rank it as it does in LoadBestMatchWithoutPickupAgent.
			/// Instead, retrieve all relationships matching the sending and receiving agents,
			/// then prioritize by assigning the highest rank to the matching pickup agent.
			/// </summary>
			OrgAgentRelationship LoadBestMatchWithPickupAgent(OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader pickupAgent)
			{
				ZQuery relationshipQuery = new ZQuery(OrgAgentRelationshipSchema.O3_ProfitShareType, ProfitShareTypes.Standard);
				var rankerRelationship = new ColumnValueRanker();
				rankerRelationship.Add(OrgAgentRelationshipSchema.O3_OH_SendingAgent, sendingAgent?.PK, null);
				rankerRelationship.Add(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, receivingAgent?.PK, null);

				// Sorted matched relationships without PickupAgent
				var rankedRelationships = rankerRelationship.GetBestMatches<OrgAgentRelationship>(Factory, relationshipQuery, useInMemoryFiltering: false, "", 0);
				if (rankedRelationships.Length == 0)
				{
					// Even PickupAgent has the priority but if there is no parent relationship at all, there will be no agreement.
					return null;
				}

				foreach (var relationship in rankedRelationships)
				{
					var bestSetup = relationship.ProfitShareDetails
						.OfType<OrgProfitShareDetails>()
						.FirstOrDefault(x =>
							x.O4_OH_OrgOverride == pickupAgent.PK &&
							x.O4_OrgOverrideType == OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code);

					if (bestSetup != null)
					{
						// Now that the relationship becomes the best match as PickupAgent has the highest rank, return it.
						return relationship;
					}
				}

				// If no specific setup for the pickup agent is found, return the first relationship, which has the highest rank.
				return rankedRelationships[0];
			}

			public OrgAgentRelationship LoadAgencyProfile(OrgHeader agency)
			{
				ZQuery query = new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agency.PK);
				query.AddToFilter(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, null);
				query.AddToFilter(OrgAgentRelationshipSchema.O3_ProfitShareType, ProfitShareTypes.AgencyProfile);

				return Factory.LoadTop1<OrgAgentRelationship>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(OrgAgentRelationship);
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = (NoResString)"Receiving Agent: " + ReceivingAgentName;
				logReference += (NoResString)" Sending Agent: " + SendingAgentName;

				return logReference;
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O3_OH_SendingAgent = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		}

		#endregion

		#region Related Business Objects

		#region All Profit Share Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgProfitShareDetailsDependentCollection ProfitShareDetails
		{
			get
			{
				if (fProfitShareDetails == null)
				{
					fProfitShareDetails = new OrgProfitShareDetailsDependentCollection(this);
					fProfitShareDetails.Load();
					RegisterEditableChildObject(fProfitShareDetails);
					if (OrgBeingViewedFrom != null)
					{
						fProfitShareDetails.SetReadOnlyIncludingChildren(!OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity);
					}
				}

				return fProfitShareDetails;
			}
		}
		OrgProfitShareDetailsDependentCollection fProfitShareDetails;

		#endregion

		#region Generic Profit Share Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgProfitShareDetailsGenericCollection GenericProfitShareDetails
		{
			get
			{
				if (fGenericProfitShareDetails == null)
				{
					fGenericProfitShareDetails = new OrgProfitShareDetailsGenericCollection(this);
					fGenericProfitShareDetails.Load();
					RegisterEditableChildObject(fGenericProfitShareDetails);
				}

				return fGenericProfitShareDetails;
			}
		}
		OrgProfitShareDetailsGenericCollection fGenericProfitShareDetails;

		#endregion

		#region Client-Specific Profit Share Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgProfitShareDetailsClientSpecificCollection ClientSpecificProfitShareDetails
		{
			get
			{
				if (fClientSpecificProfitShareDetails == null)
				{
					fClientSpecificProfitShareDetails = new OrgProfitShareDetailsClientSpecificCollection(this);
					fClientSpecificProfitShareDetails.Load();
					RegisterEditableChildObject(fClientSpecificProfitShareDetails);
				}

				return fClientSpecificProfitShareDetails;
			}
		}
		OrgProfitShareDetailsClientSpecificCollection fClientSpecificProfitShareDetails;

		#endregion

		#region UI Helpers

		/// <summary>
		/// This list holds selected items from ZGrid of OrgProfitShareDetails to pass the data through UI actions.
		/// </summary>
		public List<OrgProfitShareDetails> SelectedProfitShareDetailsList = new List<OrgProfitShareDetails>();

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			GenericProfitShareDetails.RemoveAndDeleteAll();
			ClientSpecificProfitShareDetails.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Properties

		public ZBool AgentAgreement
		{
			get { return O3_ProfitShareType == OrgAgentRelationship.ProfitShareTypes.Standard; }
		}

		public ZPropertyInfo AgentAgreementInfo { get { return GetZPropertyInfo(nameof(AgentAgreement)); } }

		#region Description

		public ZString Description
		{
			get
			{
				ZString result = ZString.Empty;
				if (SendingAgent != null)
				{
					result += SendingAgent.OH_Code;
				}
				else
				{
					result = Res.GetString("MasterFiles|OrgAgentRelationship|AllAgents", "All Agents");
				}

				result += " - ";

				if (ReceivingAgent != null)
				{
					result += ReceivingAgent.OH_Code;
				}
				else
				{
					result += Res.GetString("MasterFiles|OrgAgentRelationship|AllAgents", "All Agents");
				}

				return result;
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region O3_OH_GroupNetworkOrFranchise
		[List("Lookups.GroupNetworkOrFranchises")]
		public override ZGuid O3_OH_GroupNetworkOrFranchise
		{
			get
			{
				return base.O3_OH_GroupNetworkOrFranchise;
			}
			set
			{
				base.O3_OH_GroupNetworkOrFranchise = value;
			}
		}

		#endregion

		#region Sending Agent

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString SendingAgentName
		{
			get { return SendingAgent != null ? SendingAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo SendingAgentNameInfo
		{
			get { return GetZPropertyInfo(nameof(SendingAgentName)); }
		}

		[List("Lookups.SendingAgents")]
		public override ZGuid O3_OH_SendingAgent
		{
			get { return base.O3_OH_SendingAgent; }
			set
			{
				base.O3_OH_SendingAgent = value;
				PropagateProfitShareDetails();
			}
		}

		public bool O3_OH_SendingAgent_ReadOnly
		{
			get { return o3_OH_SendingAgent_ReadOnly; }
			set
			{
				if (o3_OH_SendingAgent_ReadOnly != value)
				{
					o3_OH_SendingAgent_ReadOnly = value;
					O3_OH_SendingAgentInfo.RefreshBinding();
				}
			}
		}

		bool o3_OH_SendingAgent_ReadOnly;
		internal OrgHeader OrgBeingViewedFrom;

		#endregion

		#region Receiving Agent

		[MaxLength(OrgHeader.Schema.OH_FullNameMaxLength)]
		public ZString ReceivingAgentName
		{
			get { return ReceivingAgent != null ? ReceivingAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo ReceivingAgentNameInfo
		{
			get { return GetZPropertyInfo(nameof(ReceivingAgentName)); }
		}

		[List("Lookups.ReceivingAgents")]
		public override ZGuid O3_OH_ReceivingAgent
		{
			get { return base.O3_OH_ReceivingAgent; }
			set
			{
				base.O3_OH_ReceivingAgent = value;
				PropagateProfitShareDetails();
			}
		}

		public bool O3_OH_ReceivingAgent_ReadOnly
		{
			get { return o3_OH_ReceivingAgent_ReadOnly; }
			set
			{
				if (o3_OH_ReceivingAgent_ReadOnly != value)
				{
					o3_OH_ReceivingAgent_ReadOnly = value;
					O3_OH_ReceivingAgentInfo.RefreshBinding();
				}
			}
		}

		bool o3_OH_ReceivingAgent_ReadOnly;

		#endregion

		[List("Lookups.ProfitShareTypeList")]
		public override ZString O3_ProfitShareType
		{
			get { return base.O3_ProfitShareType; }
			set
			{
				base.O3_ProfitShareType = value;
				if (base.O3_ProfitShareType == ProfitShareTypes.AgencyProfile)
				{
					O3_OH_ReceivingAgent = ZGuid.Empty;
				}
				else
				{
					foreach (OrgProfitShareDetails profitShare in ProfitShareDetails)
					{
						profitShare.O4_GatewayProfitApportionmentMethod = ZString.Empty;
					}
				}
			}
		}

		protected override bool CreateAutoLogIfOnlyChildrenHaveChanges => true;
		protected override bool UpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		void PropagateProfitShareDetails()
		{
			if (fProfitShareDetails != null)
			{
				foreach (OrgProfitShareDetails profitShare in ProfitShareDetails)
				{
					foreach (OrgProfitShareParty party in profitShare.PartyDetails)
					{
						party.PropagateValuesToRelatedParty();
					}
				}
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (OrgBeingViewedFrom != null)
			{
				shouldBeReadOnly = !OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			OrgAgentRelationship result = (OrgAgentRelationship)Clone();
			foreach (OrgProfitShareDetails detail in ProfitShareDetails)
			{
				result.ProfitShareDetails.Add(detail.Clone());
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.OrgAgentRelationShip));

		DocManagerInfo docManagerInfo;

		#endregion
	}
}
