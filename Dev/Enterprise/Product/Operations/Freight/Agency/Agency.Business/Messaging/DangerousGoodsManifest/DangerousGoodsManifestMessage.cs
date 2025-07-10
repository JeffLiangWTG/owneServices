using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestMessage : AutoDangerousGoodsManifestMessage
	{
		public DangerousGoodsManifestMessage(JobVoyage voyage)
			: base(voyage.Factory)
		{
			Voyage = voyage;
		}

		public JobVoyage Voyage { get; private set; }

		#region Port

		[List("Lookups.Port_List")]
		public override ZString Port
		{
			get
			{
				return base.Port;
			}
			set
			{
				if (base.Port != value)
				{
					base.Port = value;

					var directionList = Lookups.Direction_List;
					if (directionList.Count == 0)
					{
						Direction = ZString.Empty;
					}
					else if (directionList.Count == 1)
					{
						Direction = directionList[0].Code;
					}

					UpdatePrincipal();
				}
			}
		}

		#endregion

		#region Principal

		[List("Lookups.Principal_List")]
		public override ZGuid PrincipalPK { get => base.PrincipalPK; set => base.PrincipalPK = value; }

		void UpdatePrincipal()
		{
			var principalList = Lookups.Principal_List;
			if (principalList.Count == 1)
			{
				PrincipalPK = principalList[0].PK;
			}
			else if (PrincipalPK != ZGuid.Empty && !principalList.OfType<OrgHeader>().Any(x => x.PK == PrincipalPK))
			{
				PrincipalPK = ZGuid.Empty;
			}
		}

		#endregion

		#region Direction

		[List("Lookups.Direction_List")]
		public override ZString Direction
		{
			get
			{
				return base.Direction;
			}
			set
			{
				if (base.Direction != value)
				{
					base.Direction = value;

					UpdatePrincipal();
				}
			}
		}

		#endregion

		#region MessageType

		[List("Lookups.MessageType_List")]
		public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

		#endregion

		#region Issues

		public PortMessageIssueCollection Issues
		{
			get
			{
				if (issues == null)
				{
					issues = new PortMessageIssueCollection();
				}

				return issues;
			}
		}

		PortMessageIssueCollection issues;

		#endregion

		#region CommunicationModes

		public IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes
		{
			get
			{
				if (communicationModes == null)
				{
					communicationModes = new[]
					{
						new NonPersistentEDICommunicationMode
						{
							EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
							EK_Destination = "SHIPPING_PORT_MESSAGING"
						}
					};
				}

				return communicationModes;
			}
		}
		IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		#endregion

		#region RelatedBillOfLadings

		public IEnumerable<BillOfLading> GetRelatedBillOfLadings()
		{
			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, Voyage, Port, Direction);
			return Factory.Load<BillOfLading>(filter).Where
				(x => x.Principal != null
				 && x.Principal.PK == PrincipalPK
				 && x.OuterPackLines.OfType<PackLine>().Any(p => p.UNDGs != null && p.UNDGs.OfType<UNDGDataItem>().Any(u => u.UNDGSubstance != null))
				);
		}

		#endregion

		#region SenderID

		public ZString GetSenderID()
		{
			var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration(Port, PrincipalPK);
			return portConfig == null || !portConfig.Enabled ? ZString.Empty : portConfig.SenderID;
		}

		#endregion

		#region Lookups

		public DangerousGoodsManifestMessageLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new DangerousGoodsManifestMessageLookups(this);
				}

				return lookups;
			}
		}

		DangerousGoodsManifestMessageLookups lookups;

		#endregion
	}
}
