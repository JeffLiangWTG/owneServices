using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal sealed class OriginalBillNotesUpdaterForEventReceiver : OriginalBillNotesUpdater
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary values")]
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		static readonly Dictionary<string, string> EHBLAddressTypesAndDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
						{ "Sender", "Sender" },
						{ "Receiver", "Receiver" },
						{ "Originator", "Publisher" },
						{ "Holder", "Current Holder" },
						{ "FirstHolder", "First Holder" },
						{ "Shipper", "Shipper" },
						{ "Consignee", "Consignee" },
						{ "ToOrder", "To Order" },
						{ "PledgeeHolder", "Pledgee" },
						{ "SurrenderParty", "Surrender Agent" }
				};

		readonly UniversalEvent eventAdded;

		public OriginalBillNotesUpdaterForEventReceiver(ForwardingShipment shipment, UniversalEvent eventAdded) : base(shipment)
		{
			this.eventAdded = eventAdded;
		}

		public override void PopulateOriginalBillNotes()
		{
			var newNoteText = new ZStringBuilder();

			newNoteText.Append(ZDateTimeOffset.UtcNow.ToString());

			var relatedUniversalShipment = GetRelatedUniversalShipment();

			newNoteText.Append(" ").Append("WayBill");
			if (relatedUniversalShipment != null && relatedUniversalShipment.WayBillNumber.HasValue)
			{
				newNoteText.Append(" ").Append(relatedUniversalShipment.WayBillNumber.Value);
			}

			newNoteText.Append(" ").Append(eventAdded.EventParameters.Type.HasValue ? eventAdded.EventParameters.Type.ToString() : "");

			if (relatedUniversalShipment != null)
			{
				var organizationAddressDetails = AddOrganizationAddressDetailsToOriginalBillNotes(relatedUniversalShipment);
				if (organizationAddressDetails.Length > 0)
				{
					newNoteText.AppendLine().Append(organizationAddressDetails);
				}
			}

			var note = LoadOrCreateOriginalBillNotesStmNote();
			if (!note.ST_NoteText.IsEmpty)
			{
				newNoteText.AppendLine().AppendLine().Append(note.ST_NoteText.ToString());
			}

			note.ST_NoteText = newNoteText.ToString();
		}

		UniversalShipment GetRelatedUniversalShipment()
		{
			UniversalShipment uShipment = null;

			var bluLog = shipment.Logs.MostRecentLogByEventTime(Events.BillStatusUpdated);
			if (bluLog.RelatedEDIMessage?.Message is IEDIMessage linkedMessage
				&& linkedMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent
				&& !bluLog.RelatedEDIMessage.Message.EM_EI.IsEmpty)
			{
				var zQuery = new ZQuery();
				zQuery.AddToFilter(EDIMessageSchema.EM_EI, bluLog.RelatedEDIMessage.Message.EM_EI);
				zQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
				var uShipmentEDIMessage = factory.LoadTop1<IEDIMessage>(zQuery);
				uShipment = uShipmentEDIMessage?.GetEM_MessageTextReader().Parse<UniversalShipment>();
			}

			return uShipment;
		}

		string AddOrganizationAddressDetailsToOriginalBillNotes(UniversalShipment dataObject)
		{
			var newNoteStringBuilder = new ZStringBuilder();
			foreach (var valuePair in EHBLAddressTypesAndDescriptions)
			{
				var organizationAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(x => string.Equals(valuePair.Key, x.AddressType.GetValueOrDefault(), StringComparison.OrdinalIgnoreCase));
				var organizationAddressDetails = ZString.Empty;

				if (organizationAddress != null)
				{
					if (!organizationAddress.CompanyName.GetValueOrDefault().IsEmpty && organizationAddress.HasAddressDetails())
					{
						organizationAddressDetails = ConcatOrganizationAddressDetails(organizationAddress.CompanyName.GetValueOrDefault(), organizationAddress.Address1.GetValueOrDefault(), organizationAddress.Address2.GetValueOrDefault()
							, organizationAddress.City.GetValueOrDefault(), organizationAddress.State?.Code.GetValueOrDefault() ?? ZString.Empty, organizationAddress.Postcode.GetValueOrDefault()
							, organizationAddress.Country?.Name.GetValueOrDefault() ?? ZString.Empty);
					}
					else
					{
						var triValue = organizationAddress.RegistrationNumberCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == OrgCusCode.CodeTypes.BoleroTitleRegisterID)?.Value;

						if (!string.IsNullOrEmpty(triValue))
						{
							var query = new ZDBOnlyQuery(typeof(OrgHeader));

							var cusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
							cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.BoleroTitleRegisterID);
							cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, triValue);
							query.AddSubQuery(cusCodeSubQuery, JoinCondition.And);

							var orgHeader = factory.LoadTop1<OrgHeader>(query);

							if (orgHeader != null)
							{
								var triCustomsCode = orgHeader.CustomsCodes.Cast<OrgCusCode>()
									.First(x => x.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID && x.OK_CustomsRegNo.EqualsIgnoringCase(triValue.Value));
								var premisesAddress = triCustomsCode.PremisesAddress ?? orgHeader.MainAddress;

								organizationAddressDetails = ConcatOrganizationAddressDetails(premisesAddress.CompanyName, premisesAddress.Address1, premisesAddress.Address2
									, premisesAddress.City, premisesAddress.State, premisesAddress.Postcode
									, premisesAddress.Country?.RN_Desc ?? ZString.Empty);
							}
						}
					}
				}

				if (!organizationAddressDetails.IsEmpty)
				{
					if (!newNoteStringBuilder.IsEmpty)
					{
						newNoteStringBuilder.AppendLine();
					}

					newNoteStringBuilder.Append(valuePair.Value + " : " + organizationAddressDetails);
				}
			}

			return newNoteStringBuilder.ToString();
		}
	}
}
