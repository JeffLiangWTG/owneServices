using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class EIDOShipmentMessagingData
	{
		public static IEIDOMessagingData NewOriginal(AgencyShipmentContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			if (container.Booking == null)
			{
				throw new ArgumentException("container must be attached to a shipment", nameof(container));
			}

			IEIDOEquiptmentData[] equiptment = new IEIDOEquiptmentData[] { new EquiptmentData(container) };
			return new MessageData(container.Booking, container.JC_ContainerImportDORelease, EIDOMessageFunction.Original, equiptment);
		}

		public static IEIDOMessagingData NewCancellation(AgencyShipmentContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			if (container.Booking == null)
			{
				throw new ArgumentException("container must be attached to a shipment", nameof(container));
			}

			IEIDOEquiptmentData[] equiptment = new IEIDOEquiptmentData[] { new EquiptmentData(container) };
			return new MessageData(container.Booking, container.JC_ContainerImportDORelease, EIDOMessageFunction.Cancelation, equiptment);
		}

		public static bool HasChangesAffectingEIDO(AgencyShipmentContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			return !container.IsInDatabase
				|| HasChanges(container.JC_JS_FCLBookingOnlyLinkInfo)
				|| HasChanges(container.JC_RCInfo)
				|| HasChanges(container.JC_ContainerNumInfo)
				|| HasChanges(container.JC_OA_ArrivalContainerYardAddressInfo)
				|| HasChanges(container.JC_SealNumInfo)
				|| HasChanges(container.JC_AdditionalSealNumInfo)
				|| HasChanges(container.JC_Additional2SealNumInfo)
				;
		}

		public static bool HasChangesAffectingEIDO(AgencyShipment shipment)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			return !shipment.IsInDatabase
				|| HasChanges(shipment.JS_HouseBillInfo)
				|| HasChanges(shipment.JS_UniqueConsignRefInfo)
				|| HasChanges(shipment.JS_OH_DeliveryAgentInfo)
				|| HasChanges(shipment.JS_JXInfo)
				;
		}

		#region Implementation

		static bool HasChanges(ZPropertyInfo info)
		{
			return !object.Equals(info.OriginalValue, info.Value);
		}

		static decimal Kilograms(decimal value, string unit)
		{
			if (value > 0 && Core.Constants.Weight.ContainsCode(unit))
			{
				return Core.Constants.Weight.Convert(value, unit, Core.Constants.Weight.Kilograms);
			}
			else
			{
				return 0m;
			}
		}

		static string Name(OrgHeader header)
		{
			return header == null ? null : header.OH_FullNameTruncated;
		}

		static string ACOS(OrgHeader header)
		{
			if (header == null)
			{
				return string.Empty;
			}
			else
			{
				OrgCusCode code = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(
					OrgCusCode.CodeTypes.OneStopCode, Core.Constants.CountryCodes.Australia);

				return code == null ? string.Empty : code.OK_CustomsRegNo.ToString();
			}
		}

		static DateTime? NullableDateTime(ZDateTime dateTime)
		{
			return dateTime.IsEmpty ? null : dateTime.ToDateTime();
		}

		#endregion

		#region MessageData

		[System.Diagnostics.DebuggerDisplay("billOfLading")]
		class MessageData : IEIDOMessagingData
		{
			public MessageData(AgencyShipment shipment, ZString pin, EIDOMessageFunction messageFunction, IEIDOEquiptmentData[] equiptment)
			{
				EIDOMessagingHeader detail = AgencyRegistry.Instance.EIDOMessagingDetails.Value;
				EIDOMessagingIdentity identity = detail.GetIdentity(shipment.JS_OH_DeliveryAgent);

				this.messagePrepared = ZDateTime.Now.ToDateTime();
				this.billOfLading = shipment.JS_HouseBill;
				this.referenceNumber = shipment.JS_UniqueConsignRef;
				this.password = identity == null ? "" : identity.Password.ToString();
				this.pin = pin;

				this.carrierName = Name(shipment.BookedShippingLine);
				this.carrierACOS = ACOS(shipment.BookedShippingLine);

				this.principal = OrganisationData.New(shipment.Principal);

				this.messageSender = OrganisationData.New(GlbBranch.CurrentBranch.OrgProxy);

				this.messageFunction = messageFunction;

				Transport transport = shipment.TransportsIncludingRelated.ArrivalTransport;
				if (transport != null)
				{
					RefVessel vessel = transport.Vessel;
					this.vesselLloyds = vessel == null ? ZString.Empty : vessel.RV_LloydsNumber;

					this.vesselName = transport.JW_Vessel;
					this.voyage = transport.JW_VoyageFlight;
					this.dischargePort = transport.JW_RL_NKDiscPort;
					this.estimatedArrivalDate = NullableDateTime(transport.JW_ETA);

					JobSailing sailing = transport.Sailing;
					this.messageRecipient = sailing == null ? null : OrganisationData.New(sailing.Destination.ArrivalCTOAddress);
				}
				else
				{
					this.vesselLloyds = "";
					this.vesselName = "";
					this.voyage = "";
					this.dischargePort = "";
					this.estimatedArrivalDate = null;
					this.messageRecipient = null;
				}

				this.equiptment = equiptment;
			}

			#region IEIDOMessagingData Members

			DateTime IEIDOMessagingData.MessagePrepared
			{
				get { return messagePrepared; }
			}

			DateTime? IEIDOMessagingData.EstimatedArrivalDate
			{
				get { return estimatedArrivalDate; }
			}

			string IEIDOMessagingData.Password
			{
				get { return password; }
			}

			string IEIDOMessagingData.PIN
			{
				get { return pin; }
			}

			string IEIDOMessagingData.ReferenceNumber
			{
				get { return referenceNumber; }
			}

			string IEIDOMessagingData.BillOfLading
			{
				get { return billOfLading; }
			}

			string IEIDOMessagingData.VesselName
			{
				get { return vesselName; }
			}

			string IEIDOMessagingData.VesselLloyds
			{
				get { return vesselLloyds; }
			}

			string IEIDOMessagingData.Voyage
			{
				get { return voyage; }
			}

			string IEIDOMessagingData.DischargePort
			{
				get { return dischargePort; }
			}

			string IEIDOMessagingData.CarrierACOS
			{
				get { return carrierACOS; }
			}

			string IEIDOMessagingData.CarrierName
			{
				get { return carrierName; }
			}

			EIDOMessageFunction IEIDOMessagingData.MessageFunction
			{
				get { return messageFunction; }
			}

			IEIDOOrganisation IEIDOMessagingData.MessageRecipient
			{
				get { return messageRecipient; }
			}

			IEIDOOrganisation IEIDOMessagingData.MessageSender
			{
				get { return messageSender; }
			}

			IEIDOOrganisation IEIDOMessagingData.Issuer
			{
				get { return principal; }
			}

			IEIDOOrganisation IEIDOMessagingData.CargoCollection
			{
				get { return messageRecipient; }
			}

			IEnumerable<IEIDOEquiptmentData> IEIDOMessagingData.Equipment
			{
				get { return equiptment; }
			}

			#endregion

			readonly DateTime messagePrepared;
			readonly DateTime? estimatedArrivalDate;

			readonly string billOfLading;
			readonly string referenceNumber;
			readonly string pin;
			readonly string password;
			readonly string vesselName;
			readonly string vesselLloyds;
			readonly string voyage;
			readonly string dischargePort;
			readonly string carrierACOS;
			readonly string carrierName;

			readonly EIDOMessageFunction messageFunction;

			readonly IEIDOOrganisation messageRecipient;
			readonly IEIDOOrganisation messageSender;
			readonly IEIDOOrganisation principal;

			readonly IEIDOEquiptmentData[] equiptment;
		}

		#endregion

		#region OrganisationData

		[System.Diagnostics.DebuggerDisplay("{code} - {nameAndAddress}")]
		class OrganisationData : IEIDOOrganisation
		{
			public static IEIDOOrganisation New(OrgHeader header)
			{
				if (header == null)
				{
					return null;
				}
				else
				{
					return new OrganisationData(
						ACOS(header),
						NameAndAddress(header.MainAddress)
						);
				}
			}

			public static IEIDOOrganisation New(OrgAddress address)
			{
				if (address == null)
				{
					return null;
				}
				else
				{
					return new OrganisationData(
						ACOS(address.Header),
						NameAndAddress(address)
						);
				}
			}

			OrganisationData(string code, string nameAndAddress)
			{
				this.code = code;
				this.nameAndAddress = nameAndAddress;
			}

			#region Implementation

			static string NameAndAddress(OrgAddress address)
			{
				if (address == null)
				{
					throw new ArgumentNullException(nameof(address));
				}

				return string.Format(CultureInfo.InvariantCulture,
					"{0}\n{1}\n{2}\n{3}\n{4} {5}",
					address.Header.OH_FullNameTruncated,
					address.OA_Address1,
					address.OA_Address2,
					address.OA_City,
					address.OA_State,
					address.OA_PostCode);
			}

			#endregion

			#region IEIDOOrganisation Members

			string IEIDOOrganisation.AcosCode
			{
				get { return code; }
			}

			string IEIDOOrganisation.NameAndAddress
			{
				get { return nameAndAddress; }
			}

			#endregion

			readonly string code;
			readonly string nameAndAddress;
		}

		#endregion

		#region EquiptmentData

		[System.Diagnostics.DebuggerDisplay("{containerNum}")]
		class EquiptmentData : IEIDOEquiptmentData
		{
			public EquiptmentData(AgencyShipmentContainer container)
			{
				containerNum = container.JC_ContainerNum;
				isEmpty = container.JC_IsEmptyContainer;
				emptyReturn = OrganisationData.New(container.ArrivalContainerYardAddress);
				emptyReturnBy = NullableDateTime(container.JC_EmptyReturnedBy);
				grossKilograms = Kilograms(container.JC_GrossWeight, container.JC_GrossWeightUQ);
				isoCode = container.Container == null ? ZString.Empty : container.Container.RC_ISOType;

				sealNumbers = new List<string>();
				if (!container.JC_SealNum.IsEmpty)
				{
					sealNumbers.Add(container.JC_SealNum);
				}

				if (!container.JC_AdditionalSealNum.IsEmpty)
				{
					sealNumbers.Add(container.JC_AdditionalSealNum);
				}

				UNDGSubstance substance = null;
				foreach (AgencyShipmentPackLine packline in container.PackLines)
				{
					substance = packline.UNDGs.Count > 0 ? packline.UNDGs[0].Substance : null;
					if (substance != null)
					{
						break;
					}
				}

				if (substance != null)
				{
					this.imdgClass = substance.DG_Class;
					this.imdgClassCode = substance.DG_UNNO;
				}
				else
				{
					this.imdgClass = "";
					this.imdgClassCode = "";
				}
			}

			#region IEIDOEquiptmentData Members

			string IEIDOEquiptmentData.ContainerNumber
			{
				get { return containerNum; }
			}

			string IEIDOEquiptmentData.ContainerISOCode
			{
				get { return isoCode; }
			}

			List<string> IEIDOEquiptmentData.SealNumbers
			{
				get { return sealNumbers; }
			}

			string IEIDOEquiptmentData.GoodsDescription
			{
				get { return ""; }
			}

			string IEIDOEquiptmentData.HandlingInstructions
			{
				get { return ""; }
			}

			string IEIDOEquiptmentData.IMDGClassCode
			{
				get { return imdgClassCode; }
			}

			string IEIDOEquiptmentData.IMDGClass
			{
				get { return imdgClass; }
			}

			DateTime? IEIDOEquiptmentData.EmptyReturnBy
			{
				get { return emptyReturnBy; }
			}

			decimal IEIDOEquiptmentData.GrossKilograms
			{
				get { return grossKilograms; }
			}

			bool IEIDOEquiptmentData.IsEmpty
			{
				get { return isEmpty; }
			}

			IEIDOOrganisation IEIDOEquiptmentData.EmptyReturn
			{
				get { return emptyReturn; }
			}

			#endregion

			readonly string containerNum;
			readonly string isoCode;
			readonly string imdgClass;
			readonly string imdgClassCode;
			readonly List<string> sealNumbers;

			readonly DateTime? emptyReturnBy;
			readonly decimal grossKilograms;
			readonly bool isEmpty;

			readonly IEIDOOrganisation emptyReturn;
		}

		#endregion
	}
}




