using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public delegate ZQuery GetRelatedPartiesQuery(ZQuery orgHeaderFilter);

	public class OrgRelatedPartiesModuleFilter : ModuleTextFilter
	{
		public OrgRelatedPartiesModuleFilter(ZString description)
			: base(description, EmptyQuery, Modes)
		{
			this.relatedPartiesQueryDelegate = (q) => q;
		}

		public OrgRelatedPartiesModuleFilter(ZString description, GetRelatedPartiesQuery queryDelegate)
			: base(description, EmptyQuery, Modes)
		{
			if (queryDelegate == null)
			{
				throw new ArgumentNullException(nameof(queryDelegate));
			}

			this.relatedPartiesQueryDelegate = queryDelegate;
		}

		#region Properties

		#region PartyType

		[BusinessObjectTestExclude]
		[List("PartyTypeList")]
		public ZString PartyType
		{
			get { return fPartyType; }
			set
			{
				if (SetNonPersistentPropertyValue(PartyTypeInfo, ref fPartyType, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidatePartyType();
					}

					PartyTypeInfo.RefreshBinding();
					TransportModeInfo.RefreshBinding();

					if (ShouldCalculateDirection)
					{
						Direction = ZString.Empty;
					}

					if (!ShouldHaveMode)
					{
						TransportMode = ZString.Empty;
						ContainerMode = ZString.Empty;
					}
				}
			}
		}
		ZString fPartyType;

		public ZPropertyInfo PartyTypeInfo
		{
			get { return GetZPropertyInfo(nameof(PartyType)); }
		}

		public CodeDescriptionPairList PartyTypeList
		{
			get { return partyTypeList ?? (partyTypeList = CreatePartyList()); }
		}
		RelatedPartyTypeList partyTypeList;

		public virtual RelatedPartyTypeList CreatePartyList()
		{
			return new RelatedPartyTypeList();
		}

		#endregion

		#region Direction

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldCalculateDirection))]
		[List("DirectionList")]
		public ZString Direction
		{
			get { return fDirection; }
			set
			{
				if (SetNonPersistentPropertyValue(DirectionInfo, ref fDirection, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateDirection();
					}
					DirectionInfo.RefreshBinding();
				}
			}
		}

		ZString fDirection;

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(nameof(Direction)); }
		}

		public virtual bool ShouldCalculateDirection
		{
			get
			{
				switch (PartyType)
				{
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.LocalTransportBillTo:
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
					case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
					case RelatedPartyTypeList.Codes.ReportRevenueTo:
					case RelatedPartyTypeList.Codes.ControllingCustomer:
					case RelatedPartyTypeList.Codes.ClientCFS:
						return false;

					default:
						return true;
				}
			}
		}

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (directionList == null)
				{
					directionList = new CodeDescriptionPairList();
					directionList.AddPair(RelatedPartyDirectionList.Codes.Delivery, RelatedPartyDirectionList.Descriptions.Delivery);
					directionList.AddPair(RelatedPartyDirectionList.Codes.Pickup, RelatedPartyDirectionList.Descriptions.Pickup);
					directionList.AddPair(RelatedPartyDirectionList.Codes.PickupAndDelivery, RelatedPartyDirectionList.Descriptions.PickupAndDelivery);
				}
				return directionList;
			}
		}
		CodeDescriptionPairList directionList;

		#endregion

		#region Transport Mode

		[BusinessObjectTestExclude]
		[List("TransportModeList")]
		public ZString TransportMode
		{
			get { return fTransportMode; }
			set
			{
				if (SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateTransportMode();
					}
					TransportModeInfo.RefreshBinding();
				}
			}
		}
		ZString fTransportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(nameof(TransportMode)); }
		}

		protected bool TransportMode_ReadOnly
		{
			get { return !ShouldHaveMode; }
		}

		public bool ShouldHaveMode
		{
			get
			{
				return PartyType == RelatedPartyTypeList.Codes.LocalTransport
					|| PartyType == RelatedPartyTypeList.Codes.LocalTransportBillTo
					|| PartyType == RelatedPartyTypeList.Codes.CustomsAgentBroker
					|| PartyType == RelatedPartyTypeList.Codes.ForwarderCFS
					|| PartyType == RelatedPartyTypeList.Codes.ForwarderLocalTransport
					|| PartyType == RelatedPartyTypeList.Codes.ReceivingAgent
					|| PartyType == RelatedPartyTypeList.Codes.SendingAgent
					|| PartyType == RelatedPartyTypeList.Codes.ClientCFS;
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (fTransportModeList == null)
				{
					fTransportModeList = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);
				}
				return fTransportModeList;
			}
		}
		CodeDescriptionPairList fTransportModeList;

		static ReadOnlyCodeDescriptionPairList Modes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode); }
		}

		#endregion

		#region Container Mode

		[BusinessObjectTestExclude]
		[List("ContainerModeList")]
		public ZString ContainerMode
		{
			get { return fContainerMode; }
			set
			{
				if (SetNonPersistentPropertyValue(ContainerModeInfo, ref fContainerMode, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateContainerMode();
					}
					ContainerModeInfo.RefreshBinding();
				}
			}
		}
		ZString fContainerMode;

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerMode)); }
		}

		protected bool ContainerMode_ReadOnly
		{
			get { return TransportMode_ReadOnly; }
		}

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				if (fContainerModeList == null)
				{
					fContainerModeList = new CodeDescriptionPairList(OLookUpEditType.ContainerMode);
				}
				return fContainerModeList;
			}
		}
		CodeDescriptionPairList fContainerModeList;

		#endregion

		#region Related Party

		[List("RelatedParties")]
		public ZGuid RelatedParty
		{
			get { return fRelatedParty; }
			set
			{
				if (SetNonPersistentPropertyValue(RelatedPartyInfo, ref fRelatedParty, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateRelatedParty();
					}
					RelatedPartyInfo.RefreshBinding();
				}
			}
		}
		ZGuid fRelatedParty;

		public ZPropertyInfo RelatedPartyInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedParty)); }
		}

		public OrgHeaderCollection RelatedParties
		{
			get
			{
				if (fRelatedParties == null)
				{
					fRelatedParties = new OrgHeaderCollection(new BusinessObjectFactory());
				}
				return fRelatedParties;
			}
		}

		OrgHeaderCollection fRelatedParties;

		#endregion

		#endregion

		#region Validation

		public new OrgRelatedPartiesModuleFilterValidation Validation
		{
			get { return (OrgRelatedPartiesModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgRelatedPartiesModuleFilterValidation(this);
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : relatedPartiesQueryDelegate(GetRelatedPartyFilter());
		}

		ZQuery GetRelatedPartyFilter()
		{
			ZDBOnlySubQuery relatedPartyQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			if (!PartyType.IsEmpty)
			{
				relatedPartyQuery.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_PartyType, PartyType);
			}

			if (!TransportMode.IsEmpty)
			{
				relatedPartyQuery.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_FreightTransportMode, TransportMode);
			}

			if (!ContainerMode.IsEmpty)
			{
				relatedPartyQuery.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_FreightContainerMode, ContainerMode);
			}

			if (!Direction.IsEmpty)
			{
				relatedPartyQuery.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_FreightDirection, Direction);
			}

			if (!RelatedParty.IsEmpty)
			{
				relatedPartyQuery.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_OH_RelatedParty, RelatedParty);
			}

			ZQuery companyQuery = new ZQuery(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);

			relatedPartyQuery.AddToFilter(companyQuery);

			ZDBOnlyQuery orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgHeaderQuery.AddSubQuery(relatedPartyQuery, JoinCondition.And);

			return orgHeaderQuery;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("PartyType", PartyType);
			writer.WriteElementString("Direction", Direction);
			writer.WriteElementString("TransportMode", TransportMode);
			writer.WriteElementString("ContainerMode", ContainerMode);
			writer.WriteElementString("RelatedParty", RelatedParty.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "PartyType")
			{
				PartyType = reader.ReadElementString("PartyType");
			}

			if (reader.Name == "Direction")
			{
				Direction = reader.ReadElementString("Direction");
			}

			if (reader.Name == "TransportMode")
			{
				TransportMode = reader.ReadElementString("TransportMode");
			}

			if (reader.Name == "ContainerMode")
			{
				ContainerMode = reader.ReadElementString("ContainerMode");
			}

			if (reader.Name == "RelatedParty")
			{
				RelatedParty = new Guid(reader.ReadElementString("RelatedParty"));
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEmptyCore => base.IsEmptyCore && PartyType.IsEmpty && TransportMode.IsEmpty && ContainerMode.IsEmpty && Direction.IsEmpty && RelatedParty.IsEmpty;

		protected override void ClearCore()
		{
			base.ClearCore();
			PartyType = ZString.Empty;
			TransportMode = ZString.Empty;
			ContainerMode = ZString.Empty;
			Direction = ZString.Empty;
			RelatedParty = ZGuid.Empty;
		}

		readonly GetRelatedPartiesQuery relatedPartiesQueryDelegate;

		#endregion
	}
}
