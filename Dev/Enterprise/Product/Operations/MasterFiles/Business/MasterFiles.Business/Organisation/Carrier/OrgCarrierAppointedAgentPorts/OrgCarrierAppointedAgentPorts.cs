using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgentPorts : OrgAppointedAgentPorts
	{
		public OrgCarrierAppointedAgentPorts(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Schema

		public static class CarrierAppPortsSchema
		{
			public const string OrganisationPK = "OrganisationPK";
			public const string OrganisationName = "OrganisationName";
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O5_SeaAirCarrierOrForwarderType = "";
		}

		#endregion

		#region Validation

		protected override OrgAppointedAgentPortsValidation GetNewValidation()
		{
			switch (O5_SeaAirCarrierOrForwarderType)
			{
				case CarrierOrForwarderType.Codes.AirCTO:
					return new OrgCarrierAppointedAirCTOValidation(this);
				case CarrierOrForwarderType.Codes.Stevedore:
					return new OrgCarrierAppointedSeaCTOValidation(this);
				case CarrierOrForwarderType.Codes.RoadDepotShed:
					return new OrgCarrierAppointedRoadCTOValidation(this);
				case CarrierOrForwarderType.Codes.RailHeadDepot:
					return new OrgCarrierAppointedRailCTOValidation(this);
				case CarrierOrForwarderType.Codes.ContainerYard:
					return new OrgCarrierAppointedCYValidation(this);
				case CarrierOrForwarderType.Codes.Agency:
					return new OrgCarrierAppointedAgencyValidation(this);
				default:
					return new OrgCarrierAppointedAgentPortsValidation(this);
			}
		}

		public new OrgCarrierAppointedAgentPortsValidation Validation
		{
			get { return (OrgCarrierAppointedAgentPortsValidation)base.Validation; }
		}

		#endregion

		#region Lookups

		public new OrgCarrierAppointedAgentPortsLookups Lookups
		{
			get { return (OrgCarrierAppointedAgentPortsLookups)base.Lookups; }
		}

		protected override OrgAppointedAgentPortsLookups GetNewLookups()
		{
			if (O5_SeaAirCarrierOrForwarderType == CarrierOrForwarderType.Codes.Agency)
			{
				return new OrgCarrierAppointedAgentPortsLookups(this);
			}

			return new OrgCarrierAppointedAgentPortsUNLOCOLookups(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ContainerTypes.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Properties

		#region OrganisationPK

		[List("Lookups.OrganisationList")]
		public ZGuid OrganisationPK
		{
			get
			{
				if (AgentOfficeAddress != null)
				{
					organisationPK = AgentOfficeAddress.OA_OH;
				}

				return organisationPK;
			}
			set
			{
				if (organisationPK != value)
				{
					organisationPK = value;

					var newOrganisation = Factory.Load<OrgHeader>(value);
					if (newOrganisation != null && newOrganisation.MainAddress != null)
					{
						O5_OA_AgentOfficeAddress = newOrganisation.MainAddress.PK;
					}
					else
					{
						O5_OA_AgentOfficeAddress = ZGuid.Empty;
					}

					Validation.ValidateOrganisationPK();
					OrganisationPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(CarrierAppPortsSchema.OrganisationPK); }
		}

		ZGuid organisationPK;

		#endregion

		#region OrganisationName

		public ZString OrganisationName
		{
			get { return Organisation != null ? Organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(CarrierAppPortsSchema.OrganisationName); }
		}

		#endregion

		#region O5_PortOrCountry

		[List("Lookups.Locations")]
		public override ZString O5_PortOrCountry
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.O5_PortOrCountry; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.O5_PortOrCountry = value; }
		}

		#endregion

		#region O5_TerminalType

		[List("Lookups.StevedoreTypeList")]
		public override ZString O5_TerminalType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.O5_TerminalType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.O5_TerminalType = value; }
		}

		#endregion

		#region O5_AgentDirection

		[ResourceStringData("OrgCarrierAppointedAgentPorts|O5_AgentDirection", Caption = "Direction")]
		public override ZString O5_AgentDirection
		{
			get { return base.O5_AgentDirection; }
			set { base.O5_AgentDirection = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Organisation

		public OrgHeader Organisation
		{
			get { return Factory.Load<OrgHeader>(OrganisationPK); }
		}

		#endregion

		#region Container Types

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgParkContainerTypeCollection ContainerTypes
		{
			get
			{
				if (fContainerTypes == null)
				{
					fContainerTypes = new OrgParkContainerTypeCollection(this);
					RegisterEditableChildObject(fContainerTypes);
				}
				return fContainerTypes;
			}
		}

		OrgParkContainerTypeCollection fContainerTypes;

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected override bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			OrgHeader header = Header;
			return header == null || !header.SecurityProvider.HasModifyCarrierSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
