using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public abstract class PortMessagingData : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected PortMessagingData(ForwardingConsol consol)
			: base(consol.Factory)
		{
			this.consol = consol;
		}

		#region Schema

		internal class Schema
		{
			public const string Operator = "Operator";
			public const string OperatorEmail = "OperatorEmail";
			public const string OperatorPhone = "OperatorPhone";
			public const string OperatorFax = "OperatorFax";
			public const string Date = "Date";
			public const string Berth = "Berth";
			public const string ShippingLine = "ShippingLine";
			public const string SenderCode = "SenderCode";
			public const string SenderName = "SenderName";
			public const string AccountNo = "AccountNo";
			public const string VesselName = "VesselName";
			public const string BillNo = "BillNo";
			public const string BookingReference = "BookingReference";
			public const string Departure = "Departure";
			public const string DepartureReference = "DepartureReference";
			public const string VoyageNo = "VoyageNo";
			public const string Destination = "Destination";
			public const string Warehouse = "Warehouse";
			public const string ShipperCode = "ShipperCode";
			public const string ShipperName = "ShipperName";
			public const string ShipperPortAccount = "ShipperPortAccount";
			public const string ShipperEORI = "ShipperEORI";
			public const string AgentEORI = "AgentEORI";

			public const int OperatorMaxLength = 100;
			public const int OperatorEmailMaxLength = 128;
			public const int OperatorPhoneMaxLength = 20;
			public const int OperatorFaxMaxLength = 20;
			public const int BerthMaxLength = 10;
			public const int ShippingLineMaxLength = 100;
			public const int SenderCodeMaxLength = 70;
			public const int SenderNameMaxLength = 100;
			public const int AccountNoMaxLength = 70;
			public const int VesselNameMaxLength = 35;
			public const int BillNoMaxLength = 35;
			public const int VoyageNoMaxLength = 35;
			public const int DestinationMaxLength = 5;
			public const int WarehouseMaxLength = 70;
			public const int ShipperCodeMaxLength = 70;
			public const int ShipperNameMaxLength = 100;
			public const int ShipperPortAccountMaxLength = 70;
			public const int DepartureReferenceMaxLength = 20;
		}

		#endregion

		#region Properties

		#region Operator

		[MaxLength(Schema.OperatorMaxLength)]
		public ZString Operator
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		public ZPropertyInfo OperatorInfo
		{
			get { return GetZPropertyInfo(Schema.Operator); }
		}

		#endregion

		#region Operator Email

		[MaxLength(Schema.OperatorEmailMaxLength)]
		public ZString OperatorEmail
		{
			get { return GlbStaff.CurrentUser.GS_EmailAddress; }
		}

		public ZPropertyInfo OperatorEmailInfo
		{
			get { return GetZPropertyInfo(Schema.OperatorEmail); }
		}

		#endregion

		#region Operator Phone

		[MaxLength(Schema.OperatorPhoneMaxLength)]
		public ZString OperatorPhone
		{
			get { return GlbStaff.CurrentUser.GS_WorkPhone; }
		}

		public ZPropertyInfo OperatorPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.OperatorPhone); }
		}

		#endregion

		#region Operator Fax

		[MaxLength(Schema.OperatorFaxMaxLength)]
		public ZString OperatorFax
		{
			get { return GlbStaff.CurrentUser.GS_FaxNum; }
		}

		public ZPropertyInfo OperatorFaxInfo
		{
			get { return GetZPropertyInfo(Schema.OperatorFax); }
		}

		#endregion

		#region Date

		public ZDateTime Date
		{
			get { return ZDateTime.Now; }
		}

		public ZPropertyInfo DateInfo
		{
			get { return GetZPropertyInfo(Schema.Date); }
		}

		#endregion

		#region Berth

		[MaxLength(Schema.BerthMaxLength)]
		public ZString Berth
		{
			get
			{
				var result = ZString.Empty;

				if (Consol.Voyage != null)
				{
					var originVoyage = Consol.Voyage.Origins.OfType<VoyageOrigin>().FirstOrDefault(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
					if (originVoyage != null)
					{
						if (!originVoyage.JA_Berth.IsEmpty)
						{
							result = originVoyage.JA_Berth;
						}
						else
						{
							if (originVoyage.DepartureCTOAddress != null && originVoyage.DepartureCTOAddress.Header != null)
							{
								result = originVoyage.DepartureCTOAddress.Header.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, Constants.CountryCodes.Germany, originVoyage.DepartureCTOAddress.PK);
							}
						}
					}
				}

				if (result.IsEmpty && Consol.DepartureCTOAddress != null && Consol.DepartureCTOAddress.Header != null)
				{
					result = Consol.DepartureCTOAddress.Header.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, Constants.CountryCodes.Germany, Consol.DepartureCTOAddress.PK);
				}

				return result;
			}
		}

		public ZPropertyInfo BerthInfo
		{
			get { return GetZPropertyInfo(Schema.Berth); }
		}

		#endregion

		#region Shipping Line

		[MaxLength(Schema.ShippingLineMaxLength)]
		public ZString ShippingLine
		{
			get
			{
				return Consol.IsCoLoad
					? Consol.Creditor?.OH_FullNameTruncated ?? ZString.Empty
					: Consol.ShippingLine?.OH_FullNameTruncated ?? ZString.Empty;
			}
		}

		public ZPropertyInfo ShippingLineInfo
		{
			get { return GetZPropertyInfo(Schema.ShippingLine); }
		}

		#endregion

		#region Sender Code

		[MaxLength(Schema.SenderCodeMaxLength)]
		public ZString SenderCode
		{
			get { return GetOrgCusCode(Consol.SendingForwarder, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode); }
		}

		public ZPropertyInfo SenderCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SenderCode); }
		}

		#endregion

		#region Sender Name

		[MaxLength(Schema.SenderNameMaxLength)]
		public ZString SenderName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		public ZPropertyInfo SenderNameInfo
		{
			get { return GetZPropertyInfo(Schema.SenderName); }
		}

		#endregion

		#region Account Number

		[MaxLength(Schema.AccountNoMaxLength)]
		public ZString AccountNo
		{
			get { return GetOrgCusCode(Consol.SendingForwarder, GermanyOrgCusCodeInfo.OrgCusCodes.ZAP); }
		}

		public ZPropertyInfo AccountNoInfo
		{
			get { return GetZPropertyInfo(Schema.AccountNo); }
		}

		#endregion

		#region Vessel Name

		[MaxLength(Schema.VesselNameMaxLength)]
		public ZString VesselName
		{
			get { return Consol.Vessel != null ? Consol.Vessel.RV_Name : ZString.Empty; }
		}

		public ZPropertyInfo VesselNameInfo
		{
			get { return GetZPropertyInfo(Schema.VesselName); }
		}

		#endregion

		#region Bill Number

		[MaxLength(Schema.BillNoMaxLength)]
		public ZString BillNo
		{
			get { return Consol.IsCoLoad ? consol.JK_CoLoadMasterBill.Substring(0, 10) : Consol.JK_MasterBillNum.Substring(0, 10); }
		}

		public ZPropertyInfo BillNoInfo
		{
			get { return GetZPropertyInfo(Schema.BillNo); }
		}

		#endregion

		#region Booking Reference

		public ZString BookingReference => BookingReferenceCore;

		public ZPropertyInfo BookingReferenceInfo => GetZPropertyInfo(Schema.BookingReference);

		protected virtual ZString BookingReferenceCore => Consol.IsCoLoad ? Consol.JK_CoLoadBookingReference : Consol.JK_BookingReference;

		#endregion

		#region Departure

		public ZDateTime Departure
		{
			get { return Consol.MostInterestingTransportForBinding.Any() ? Consol.MostInterestingTransportForBinding.OfType<Transport>().First().JW_ETD : ZDateTime.Empty; }
		}

		public ZPropertyInfo DepartureInfo
		{
			get { return GetZPropertyInfo(Schema.Departure); }
		}

		#endregion

		#region Departure Reference

		[MaxLength(Schema.DepartureReferenceMaxLength)]
		public ZString DepartureReference
		{
			get
			{
				if (Consol.Voyage != null)
				{
					var originVoyage = Consol.Voyage.Origins.OfType<VoyageOrigin>().FirstOrDefault(o => o.JA_RL_NKPortOfLoading.Equals("DEHAM"));
					if (originVoyage != null && !originVoyage.JA_DepartReference.IsEmpty)
					{
						return originVoyage.JA_DepartReference;
					}
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo DepartureReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.DepartureReference); }
		}

		#endregion

		#region Voyage Number

		[MaxLength(Schema.VoyageNoMaxLength)]
		public ZString VoyageNo
		{
			get { return Consol.Voyage != null ? Consol.Voyage.JV_VoyageFlight : ZString.Empty; }
		}

		public ZPropertyInfo VoyageNoInfo
		{
			get { return GetZPropertyInfo(Schema.VoyageNo); }
		}

		#endregion

		#region Destination

		[MaxLength(Schema.DestinationMaxLength)]
		public ZString Destination
		{
			get
			{
				if (Consol.DischargePort != null && Consol.DischargePort.IsInNorthernIreland)
				{
					return Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes
						+ Consol.JK_RL_NKDischargePort.SubstringSafe(Constants.CountryCodes.UnitedKingdom.Length);
				}

				return Consol.JK_RL_NKDischargePort;
			}
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.Destination); }
		}

		#endregion

		#region Warehouse

		[MaxLength(Schema.WarehouseMaxLength)]
		public ZString Warehouse
		{
			get { return WarehouseCore; }
		}

		protected virtual ZString WarehouseCore
		{
			get
			{
				var orgHeader = LoadOrganisation(Consol.JK_OA_DepartureCTOAddress);

				return orgHeader == null
					? ZString.Empty
					: orgHeader.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode,
						Constants.CountryCodes.Germany, Consol.JK_OA_DepartureCTOAddress);
			}
		}

		public ZPropertyInfo WarehouseInfo
		{
			get { return GetZPropertyInfo(Schema.Warehouse, "Warehouse"); }
		}

		protected virtual string WarehouseErrMsg
		{
			get { return Res.GetString("f22cd86a-89fd-44d0-aa4d-42091e3fc7c8", "The DAKOSY Participant Code (DPC) must be populated. Consol > Departure > CTO Address > Details > Config > Registration Numbers / Codes > DPC Code for DE."); }
		}

		#endregion

		#region Shipper Code

		[MaxLength(Schema.ShipperCodeMaxLength)]
		public ZString ShipperCode
		{
			get { return GetOrgCusCode(Consol.SendingForwarder, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode); }
		}

		public ZPropertyInfo ShipperCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ShipperCode); }
		}

		#endregion

		#region Shipper Name

		[MaxLength(Schema.ShipperNameMaxLength)]
		public ZString ShipperName
		{
			get { return Consol.SendingForwarder != null ? Consol.SendingForwarder.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo ShipperNameInfo
		{
			get { return GetZPropertyInfo(Schema.ShipperName); }
		}

		#endregion

		#region Shipper Port Account

		[MaxLength(Schema.ShipperPortAccountMaxLength)]
		public ZString ShipperPortAccount
		{
			get { return GetOrgCusCode(Consol.SendingForwarder, GermanyOrgCusCodeInfo.OrgCusCodes.ZAP); }
		}

		public ZPropertyInfo ShipperPortAccountInfo
		{
			get { return GetZPropertyInfo(Schema.ShipperPortAccount); }
		}

		#endregion

		#region Shipper EORI

		public ZString ShipperEORI => GetShipperEORICore();

		public ZPropertyInfo ShipperEORIInfo => GetZPropertyInfo(Schema.ShipperEORI);

		protected virtual ZString GetShipperEORICore() => ZString.Empty;

		#endregion

		#region Agent EORI

		public ZString AgentEORI => GetAgentEORICore();

		public ZPropertyInfo AgentEORIInfo => GetZPropertyInfo(Schema.AgentEORI);

		protected virtual ZString GetAgentEORICore() => ZString.Empty;

		#endregion

		#endregion

		#region Dakosy Logs

		protected abstract IStmALogParent LogParent { get; }

		public FilteredLogsView DakosyLogs
		{
			get { return dakosyLogs ?? (dakosyLogs = new FilteredLogsView(LogParent, log => IsDakosyLog(log))); }
		}
		FilteredLogsView dakosyLogs;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool IsDakosyLog(StmALog log)
		{
			if (log.SL_Reference.Contains((NoResString)"|DEP=Dakosy", StringComparison.Ordinal) // Event Reference
				&& PortMessagingStatusRetriever.PortMessagingReceivedEvents
					.Union(PortMessagingStatusRetriever.PortMessagingSentEvents)
					.Select(x => x.Code)
					.Contains(log.SL_SE_NKEvent.ToString()))
			{
				return true;
			}

			return (log.SL_SE_NKEvent == Events.DataExportCode
				&& log.RelatedEDIMessage != null
				&& log.RelatedEDIMessage.Recipient == PortMessagingManager.DakosyRecipientCode);
		}

		#endregion

		#region Validation

		public virtual void ValidateAll()
		{
			CheckEntered(SenderCodeInfo, Res.GetString("a1baebff-3adc-4595-b528-f95e864e132a", "Paying Party Code must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));
			CheckEntered(AccountNoInfo, Res.GetString("0b99ee55-2132-44d6-b897-6620b3ee2923", "Port Account must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			CheckEntered(VesselNameInfo, Res.GetString("d19fd5bd-bd05-4961-a584-96873e8f869d", "Vessel must be populated. Consol -> Vessel."));
			CheckEntered(DepartureInfo, Res.GetString("334cb987-e85d-416c-b4e3-817d22a3f6d0", "Departure Date must be populated. Consol -> ETD."));
			CheckEntered(VoyageNoInfo, Res.GetString("7963f705-f89f-4c8f-8a9e-b513bb751280", "Voyage Number must be populated. Consol -> Voyage."));
			CheckEntered(DestinationInfo, Res.GetString("5a12ca2a-b557-4920-9fd4-fb1931d3814c", "Destination Code must be populated. Consol -> Last Disc."));
			CheckEntered(WarehouseInfo, WarehouseErrMsg);

			CheckEntered(ShipperNameInfo, Res.GetString("9993aef3-f3af-45e7-a037-0e8ecb696874", "Shipping Line must be populated. Consol -> Organizations -> Sending Agent."));
			CheckEntered(ShipperCodeInfo, Res.GetString("50664958-d641-4867-985d-e416083e87d8", "Agent /Issuer Code must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));
			CheckEntered(ShipperPortAccountInfo, Res.GetString("008b98b2-bdd6-4fa0-985a-ec085b5e38c4", "Shipping Line must be populated. Consol -> Organizations -> Sending Agent -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			CheckEntered(ShippingLineInfo,
				Consol.IsCoLoad ?
					Res.GetString("10377f28-484a-47d8-a396-2988c8046cc6", "Co-Load With must be populated. Consol -> Organizations -> Co-Load With.") :
					Res.GetString("13531d7a-59b0-4abc-be55-c395fd283bbf", "Shipping Line must be populated. Consol -> Organizations -> Carrier."));

			CheckWarehouseCTO();
			CheckDepartureReference();

			CheckCode(ShippingLineInfo, GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, Consol.IsCoLoad
				? Res.GetString("9CA5B750-8DF7-43ED-993F-348E1603826D",
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE.")
				: Res.GetString("E00AC15A-363E-4DB1-A779-65CF0A0EBBF9",
					"The Carrier Code for Hamburg (ZAP) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> ZAP Code for DE."));

			CheckCode(ShippingLineInfo, GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, Consol.IsCoLoad
				? Res.GetString("CDAC8165-0FD6-48E1-AFD3-1ADAE4C7CDD1",
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Co-Load With -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE.")
				: Res.GetString("9CA44CD5-7EE9-480A-90EB-0DB4669457CE",
					"The Dakosy Participant Code (DPC) must be populated. Consol -> Organizations -> Carrier -> Details -> Config -> Registration Numbers / Codes -> DPC Code for DE."));
		}

		void CheckEntered(ZPropertyInfo info, string errorMessage)
		{
			info.ClearAllNotifications();
			if (info.Value.IsEmpty)
			{
				info.AddMessageError(errorMessage);
			}
		}

		void CheckCode(ZPropertyInfo info, ZString code, string errorMessage)
		{
			var zapCarrier = GetOrgCusCode(Consol.IsCoLoad ? Consol.Creditor : Consol.ShippingLine, code);

			if (zapCarrier.IsEmpty)
			{
				info.AddMessageError(errorMessage);
			}
		}

		void CheckWarehouseCTO()
		{
			if (Warehouse == sammWarehouse && IsSACEntryType())
			{
				WarehouseInfo.AddMessageError(Res.GetString("7a26e18e-b22e-45ba-9d5c-b04523b3b2e1", "Warehouse '{0}' cannot be used with Entry Type '{1}' ({2})",
					sammWarehouse, EntryTypeList.Codes.ConsolidatedContainer, EntryTypeList.Descriptions.ConsolidatedContainer));
			}
		}
		const string sammWarehouse = "SAMM";

		public bool IsSACEntryType()
		{
			return CheckPortMessaging(x => x.EntryType == EntryTypeList.Codes.ConsolidatedContainer);
		}

		public bool IsAUSEntryType()
		{
			return CheckPortMessaging(x => x.EntryType == EntryTypeList.Codes.EmergencyConcept);
		}

		public bool IsMITEntryType()
		{
			return CheckPortMessaging(x => x.EntryType == EntryTypeList.Codes.Message);
		}

		void CheckDepartureReference()
		{
			if (IsExportConsolForDakosy && DepartureReference.IsEmpty)
			{
				DepartureReferenceInfo.AddMessageError(Res.GetString("35a055ec-6be8-4cfe-b988-8f016ead6066",
					"Load Port Departure Reference must be populated. Operate > Schedules > Sailing Schedule > Load Ports > Departure Ref."));
			}
		}

		bool IsExportConsolForDakosy
		{
			get
			{
				return Consol != null
					   && (Consol.JK_RL_NKLoadPort == "DEHAM" && Consol.IsSea
						   || Consol.Transports.Cast<Transport>().Any(c => c.JW_RL_NKLoadPort == "DEHAM" && c.IsSea));
			}
		}

		protected virtual void CheckEORI() { }

		protected abstract bool CheckPortMessaging(Func<IPortMessaging, bool> predicate);

		#endregion

		#region Implementation

		public ForwardingConsol Consol
		{
			get { return consol; }
		}
		readonly ForwardingConsol consol;

		protected OrgHeader LoadOrganisation(ZGuid addressPK)
		{
			if (addressPK.IsValid)
			{
				var address = Factory.Load<OrgAddress>(addressPK);
				if (address != null)
				{
					return address.Header;
				}
			}

			return null;
		}

		protected ZString GetOrgCusCode(OrgHeader header, ZString cusCodeType)
		{
			if (header != null)
			{
				var cusCode = header.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType.Equals(cusCodeType));
				return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
			}

			return ZString.Empty;
		}

		#endregion
	}
}
