using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	internal sealed class OriginalBillNotesUpdaterForSelfPublish : OriginalBillNotesUpdater
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary values")]
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		static readonly Dictionary<DocAddressType, string> EHBLAddressTypesAndDescriptions = new Dictionary<DocAddressType, string>()
				{
						{ DocAddressType.Holder, "First Holder" },
						{ DocAddressType.Shipper, "Shipper" },
						{ DocAddressType.ConsigneeElectronicBOLAddress, "Consignee" },
						{ DocAddressType.SurrenderParty, "Surrender Agent" }
				};

		readonly IStmALog log;

		public OriginalBillNotesUpdaterForSelfPublish(ForwardingShipment shipment, IStmALog log) : base(shipment)
		{
			this.log = log;
		}

		public override void PopulateOriginalBillNotes()
		{
			log.Parameters.TryGetValue(Params.Type, out var messageType);
			var newNoteText = new ZStringBuilder();
			newNoteText
				.Append(ZDateTimeOffset.UtcNow.ToString())
				.Append(" ").Append("WayBill")
				.Append(" ").Append(shipment.JS_HouseBill + "/" + shipment.JS_ElectronicBillOfLadingVersion)
				.Append(" ").Append(messageType);

			PopulateParties(newNoteText);

			var note = LoadOrCreateOriginalBillNotesStmNote();
			if (!note.ST_NoteText.IsEmpty)
			{
				newNoteText.AppendLine().AppendLine();
				newNoteText.Append(note.ST_NoteText.ToString());
			}

			note.ST_NoteText = newNoteText.ToString();
		}

		void PopulateParties(ZStringBuilder newNoteText)
		{
			JobDocAddress[] allParties = {
				shipment.HolderDocAddress, shipment.SurrenderPartyDocAddress, shipment.ShipperDocAddress,
				shipment.JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.Straight ? shipment.JS_ElectronicBillOfLadingConsigneeDocAddress : null
			};

			var parties = allParties.WhereNotNull().Where(address => address.IsInDatabase);

			foreach (var party in parties)
			{
				var addressDetails = GetCompanyAndAddressOfConsignee(party);
				if (!addressDetails.IsNullOrEmpty())
				{
					EHBLAddressTypesAndDescriptions.TryGetValue(party.DocAddressType, out var partyType);
					newNoteText.AppendLine();
					newNoteText.Append(partyType).Append(" : ").Append(addressDetails);
				}
			}
		}

		string GetCompanyAndAddressOfConsignee(JobDocAddress jobDocAddress)
		{
			if (!jobDocAddress.E2_AddressOverride)
			{
				var address = jobDocAddress.Address;
				if (address == null)
				{
					return ZString.Empty;
				}

				return ConcatOrganizationAddressDetails(address.CompanyName, address.Address1, address.Address2, address.City, address.State, address.Postcode, address.Country?.RN_Desc ?? ZString.Empty);
			}
			else
			{
				return ConcatOrganizationAddressDetails(jobDocAddress.CompanyName, jobDocAddress.Address1, jobDocAddress.Address2, jobDocAddress.City, jobDocAddress.State, jobDocAddress.Postcode, jobDocAddress.Country?.RN_Desc ?? ZString.Empty);
			}
		}
	}
}
