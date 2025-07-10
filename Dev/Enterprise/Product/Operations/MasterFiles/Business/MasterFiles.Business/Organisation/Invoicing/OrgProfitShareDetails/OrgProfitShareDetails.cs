using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgProfitShareDetails : AutoOrgProfitShareDetails
	{
		public OrgProfitShareDetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O4_StartDate = ZDateTime.Today;
			O4_EndDate = O4_StartDate.AddYears(1);
			O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreight;
		}

		/// <summary>
		/// Is the charge code group passed in relevant to the current Profit Share Agreement.
		/// </summary>
		public bool IsChargeGroupProfitShared(AccChargeCode chargeCode, PaymentTermInfos paymentTerm)
		{
			var helper = new OrgProfitShareDetailsHelper(Factory, O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined ? GetUserDefinedChargeCodes() : null);

			return helper.IsChargeGroupProfitShared(O4_AgreementType, chargeCode, paymentTerm);
		}

		#region Properties

		#region Controlling Agent

		[List("Lookups.ControllingAgents")]
		public override ZGuid O4_OH_ControllingAgent
		{
			get { return base.O4_OH_ControllingAgent; }
			set
			{
				base.O4_OH_ControllingAgent = value;
				foreach (OrgProfitShareParty party in PartyDetails)
				{
					party.PropagateValuesToRelatedParty();
				}
			}
		}

		public bool ControllingAgentIsDifferentToSendingAndReceiving
		{
			get { return !ControllingAgentIsSendingAgent && !ControllingAgentIsReceivingAgent; }
		}

		public bool ControllingAgentIsSendingAgent
		{
			get { return OrgProfitShareHeader != null && !O4_OH_ControllingAgent.IsEmpty && O4_OH_ControllingAgent == OrgProfitShareHeader.O3_OH_SendingAgent; }
		}

		public bool ControllingAgentIsReceivingAgent
		{
			get { return OrgProfitShareHeader != null && !O4_OH_ControllingAgent.IsEmpty && O4_OH_ControllingAgent == OrgProfitShareHeader.O3_OH_ReceivingAgent; }
		}

		#endregion

		#region O4_AgreementType

		[BusinessObjectTestExclude]
		[List("Lookups.CurrentCompanyChargeCodes")]
		[ReadOnlyMember(nameof(AgreementTypeDescriptionIsReadOnly))]
		public ZString AgreementTypeDescription
		{
			get
			{
				return this.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined
												? GetUserChargeCodes()
												: Lookups.AgreementTypes.GetDescriptionFromCode(O4_AgreementType);
			}
			set
			{
				this.userChargeCodes = value;
				ValidateUserChargeCodes();
				if (!AgreementTypeDescriptionInfo.HasErrors() || string.IsNullOrEmpty(userChargeCodes))
				{
					this.UpdatePivotsToRelatedObjects<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges, UserChargeCodesDictionary.Keys.ToArray());
				}
				AgreementTypeDescriptionInfo.RefreshBinding();
			}
		}

		string userChargeCodes;

		bool AgreementTypeDescriptionIsReadOnly
		{
			get { return this.O4_AgreementType != OrgProfitShareDetailsLookups.AgreementTypeUserDefined; }
		}

		public ZPropertyInfo AgreementTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AgreementTypeDescription)); }
		}

		#endregion

		[List("Lookups.Locations")]
		public override ZString O4_SendingPortOrCountry
		{
			get
			{
				return base.O4_SendingPortOrCountry;
			}
			set
			{
				base.O4_SendingPortOrCountry = value;
			}
		}

		[List("Lookups.Locations")]
		public override ZString O4_ReceivingPortOrCountry
		{
			get
			{
				return base.O4_ReceivingPortOrCountry;
			}
			set
			{
				base.O4_ReceivingPortOrCountry = value;
			}
		}

		[List("Lookups.FreightModes")]
		public override ZString O4_FreightMode
		{
			get
			{
				return base.O4_FreightMode;
			}
			set
			{
				base.O4_FreightMode = value;
			}
		}

		[List("Lookups.AgreementTypes")]
		public override ZString O4_AgreementType
		{
			get
			{
				return base.O4_AgreementType;
			}
			set
			{
				base.O4_AgreementType = value;
				if (this.O4_AgreementType != OrgProfitShareDetailsLookups.AgreementTypeUserDefined)
				{
					AgreementTypeDescription = string.Empty;
				}
			}
		}

		[List("Lookups.OrgOverrideTypes")]
		public override ZString O4_OrgOverrideType
		{
			get
			{
				return base.O4_OrgOverrideType;
			}
			set
			{
				base.O4_OrgOverrideType = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(OrgProfitShareDetailsLookups.JobTypes))]
		public override ZString O4_JobType
		{
			get => base.O4_JobType;
			set
			{
				base.O4_JobType = value;
				if (value != JobTypesList.Codes.GCN)
				{
					O4_GatewayAgentType = ZString.Empty;
					O4_GatewayProfitApportionmentMethod = ZString.Empty;
				}
			}
		}

		/// <summary>
		/// To be mapped on UI as "Condition"
		/// </summary>
		[List(nameof(Lookups) + "." + nameof(OrgProfitShareDetailsLookups.GatewayAgentTypes))]
		[ReadOnlyMember(nameof(IsNotGatewayConsolJobType))]
		public override ZString O4_GatewayAgentType
		{
			get => base.O4_GatewayAgentType;
			set => base.O4_GatewayAgentType = value;
		}

		bool IsNotGatewayConsolJobType => O4_JobType != JobTypesList.Codes.GCN;

		[List(nameof(Lookups) + "." + nameof(OrgProfitShareDetailsLookups.GatewayProfitApportionmentMethods))]
		[ReadOnlyMember(nameof(IsNotGatewayConsolJobType))]
		public override ZString O4_GatewayProfitApportionmentMethod
		{
			get => base.O4_GatewayProfitApportionmentMethod;
			set
			{
				base.O4_GatewayProfitApportionmentMethod = value;
				if (value.IsEmpty)
				{
					PartyDetailsForGatewayProfitShareRedistribution.RemoveAndDeleteAll();
				}
			}
		}

		#region AgentProfitShares

		public ZDecimal ShipmentPickupAgentProfitShare
		{
			get
			{
				return GetAgentProfitShare(OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent);
			}
		}

		public ZDecimal ShipmentDeliveryAgentProfitShare
		{
			get
			{
				return GetAgentProfitShare(OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent);
			}
		}

		ZDecimal GetAgentProfitShare(ZString partyType)
		{
			var result = PartyDetailsForGatewayProfitShareRedistribution.Cast<OrgProfitShareParty>().FirstOrDefault(x => x.PS_PartyType == partyType)?.PS_PartyProfitSharePercent;
			return result == null ? ZDecimal.Zero : (ZDecimal)result;
		}

		#endregion

		#endregion

		public override void Delete()
		{
			PartyDetails.RemoveAndDeleteAll();
			PartyDetailsForGatewayProfitShareRedistribution.RemoveAndDeleteAll();
			if (this.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined)
			{
				AgreementTypeDescription = string.Empty;
			}
			base.Delete();
		}

		#region Logging

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> objects = new List<BusinessObject>();
				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(PartyDetails);
				objects.AddRange(PartyDetailsForGatewayProfitShareRedistribution);
				return objects.ToArray();
			}
		}

		#endregion

		#region Related Objects

		/// <summary>
		/// Collection of Profit Share Party Details in general.
		/// </summary>
		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgProfitSharePartyGeneralCollection PartyDetails
		{
			get
			{
				if (fPartyDetailsGeneral != null)
				{
					return fPartyDetailsGeneral;
				}

				fPartyDetailsGeneral = new OrgProfitSharePartyGeneralCollection(this);
				fPartyDetailsGeneral.Load();
				RegisterEditableChildObject(fPartyDetailsGeneral);
				if (OrgProfitShareHeader != null && OrgProfitShareHeader.OrgBeingViewedFrom != null)
				{
					fPartyDetailsGeneral.SetReadOnlyIncludingChildren(!OrgProfitShareHeader.OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity);
				}
				return fPartyDetailsGeneral;
			}
		}
		OrgProfitSharePartyGeneralCollection fPartyDetailsGeneral;

		/// <summary>
		/// Collection of Profit Share Party Details for Gateway Consol Profit Share Redistribution.
		/// </summary>
		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgProfitSharePartyRedistributionCollection PartyDetailsForGatewayProfitShareRedistribution
		{
			get
			{
				if (fPartyDetailsRedistribution != null)
				{
					return fPartyDetailsRedistribution;
				}

				fPartyDetailsRedistribution = new OrgProfitSharePartyRedistributionCollection(this);
				fPartyDetailsRedistribution.Load();
				RegisterEditableChildObject(fPartyDetailsRedistribution);
				if (OrgProfitShareHeader != null && OrgProfitShareHeader.OrgBeingViewedFrom != null)
				{
					fPartyDetailsRedistribution.SetReadOnlyIncludingChildren(!OrgProfitShareHeader.OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity);
				}
				return fPartyDetailsRedistribution;
			}
		}
		OrgProfitSharePartyRedistributionCollection fPartyDetailsRedistribution;

		#region User Charge Codes

		string GetUserChargeCodes()
		{
			if (userChargeCodes == null)
			{
				var chargeCodes = GetUserDefinedChargeCodes();
				if (chargeCodes.Any())
				{
					userChargeCodes = JoinChargeCodesList(chargeCodes.Select(x => x.AC_Code));
				}
			}
			return userChargeCodes;
		}

		public AccChargeCode[] GetUserDefinedChargeCodes() => this.GetRelatedObjectsViaPivot<AccChargeCode>(Core.Constants.GenPivotTypes.ProfitShareUserCharges);

		public Dictionary<ZGuid, ZString> UserChargeCodesDictionary
		{
			get
			{
				var userCompanyCodes = Factory.GetCachedValue(userChargeCodes ?? "UserChargeCodes", () => Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK.ToGuid())));
				var result = new Dictionary<ZGuid, ZString>();

				if (!string.IsNullOrEmpty(userChargeCodes))
				{
					foreach (var chargeCode in SplitChargeCodesString(userChargeCodes))
					{
						var chargeCodePK = userCompanyCodes.Where(x => x.AC_Code == chargeCode).Select(x => x.PK).FirstOrDefault();
						if (!chargeCodePK.IsEmpty && !result.ContainsKey(chargeCodePK))
						{
							result.Add(chargeCodePK, chargeCode);
						}
					}
				}
				return result;
			}
		}

		void ValidateUserChargeCodes()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateAgreementTypeDescription();
			}
		}

		public static ZString JoinChargeCodesList(IEnumerable<ZString> list)
		{
			return list != null ? string.Join(", ", list.ToArray()) : string.Empty;
		}

		public static List<ZString> SplitChargeCodesString(ZString stringToSplit)
		{
			return stringToSplit.Trim()
					.Split(',')
					.Where(x => !string.IsNullOrEmpty(x))
					.Select(x => x.Trim())
					.ToList();
		}

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (OrgProfitShareHeader != null && OrgProfitShareHeader.OrgBeingViewedFrom != null)
			{
				shouldBeReadOnly = !OrgProfitShareHeader.OrgBeingViewedFrom.SecurityProvider.HasModifyForwarderProfitShareSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			OrgProfitShareDetails result = (OrgProfitShareDetails)base.CloneInternal(args);
			foreach (OrgProfitShareParty party in PartyDetails)
			{
				result.PartyDetails.Add(party.Clone());
			}
			foreach (OrgProfitShareParty party in PartyDetailsForGatewayProfitShareRedistribution)
			{
				result.PartyDetailsForGatewayProfitShareRedistribution.Add(party.Clone());
			}
			if (result.O4_AgreementType == OrgProfitShareDetailsLookups.AgreementTypeUserDefined)
			{
				result.AgreementTypeDescription = this.AgreementTypeDescription;
			}
			return result;
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"Agreement Type = {AgreementTypeDescription}");
			builder.AppendLine($"Start Date = {O4_StartDate.ToString()}");
			builder.AppendLine($"End Date = {O4_EndDate.ToString()}");
			builder.AppendLine($"Sending Location = {O4_SendingPortOrCountry}");
			builder.AppendLine($"Receiving Location = {O4_ReceivingPortOrCountry}");
			builder.AppendLine($"Freight Mode = {O4_FreightMode}");
			builder.AppendLine($"Job Type = {O4_JobType}");
			builder.AppendLine($"Gateway Agent Type = {O4_GatewayAgentType}");
			builder.AppendLine($"Controlling Agent = {ControllingAgent?.OH_Code}");
			builder.AppendLine($"Org. Override Type = {O4_OrgOverrideType}");
			builder.AppendLine($"Org. Override = {OrgOverride?.OH_Code}");

			return builder.ToString();
		}

		#endregion
	}
}
