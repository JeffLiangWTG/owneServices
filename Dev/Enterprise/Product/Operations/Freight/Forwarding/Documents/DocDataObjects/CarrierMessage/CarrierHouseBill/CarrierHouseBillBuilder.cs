using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CarrierHouseBillBuilder
	{
		public CarrierHouseBillBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = shipment ?? throw new ArgumentNullException(nameof(shipment));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public HouseBill Build()
		{
			var universalShipment = GetShippingInstructionUniversalXml();
			var houseBuilder = new HouseBillBuilder(shipment, parameters);

			var houseBill = houseBuilder.Build(false, true);
			houseBill.IsDraft = ZBool.True;

			PopulateShipmentInfosWithColoadShipments(houseBill, shipment, universalShipment);
			if (universalShipment != null)
			{
				PopulateAddressesAndDependentProperties(houseBill, universalShipment);
				PopulateOverrides(houseBill, universalShipment);
				PopulateNotes(houseBill, universalShipment);
			}
			PopulateRegistrationNumbers(houseBill);

			AddValidation(houseBill);

			return houseBill;
		}

		void PopulateShipmentInfosWithColoadShipments(HouseBill houseBillDocDataObject, ForwardingShipment shipmentBO, UniversalShipment shipemntDO)
		{
			houseBillDocDataObject.GoodsDescription = (shipemntDO?.GoodsDescription.HasValue ?? false) ? shipemntDO.GoodsDescription.Value : shipmentBO.JS_GoodsDescription;
			houseBillDocDataObject.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("acc89755-fe0a-46b3-ba26-e7de8dbf8d70", "Goods description is required."));

			houseBillDocDataObject.Clause = string.Join(", ", shipemntDO?.BillOfLadingClauseCollection?.Select(x => x.Type?.Description ?? ZString.Empty) ?? Enumerable.Empty<ZString>());

			if (shipmentBO != null
				&& shipmentBO.JS_ShipmentType != Core.Constants.ShipmentTypes.BuyersConsolLead
				&& houseBillDocDataObject.SubHouseBills != null)
			{
				foreach (var subHouseBill in houseBillDocDataObject.SubHouseBills)
				{
					var subShipmentBO = shipmentBO.CoLoadShipments.Cast<ForwardingShipment>().FirstOrDefault(s => s.JS_UniqueConsignRef == subHouseBill.ShipmentNumber);
					var subShipmentDO = shipemntDO?.SubShipmentCollection?.FirstOrDefault(x => subHouseBill.ShipmentNumber == GetShipmentNumber(x));

					if (subShipmentBO != null)
					{
						PopulateShipmentInfosWithColoadShipments(subHouseBill, subShipmentBO, subShipmentDO);
					}
				}
			}
		}

		public ZString GetShipmentNumber(UniversalShipment ushipment) => ushipment?.GetMatchingDataTarget(DataContextType.ForwardingShipment)?.Key.GetValueOrDefault() ?? string.Empty;

		void PopulateAddressesAndDependentProperties(HouseBill houseBill, UniversalShipment universalShipment)
		{
			houseBill.Shipper = AddressBuilder.Create(context, FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsignorDocumentaryAddress)));
			houseBill.Consignee = AddressBuilder.Create(context, FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsigneeDocumentaryAddress)));
			houseBill.NotifyParty = AddressBuilder.Create(context, FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.NotifyParty)));
		}

		OrganizationAddress FindOrReturnDefaultAddress(UniversalShipment universalShipment, string addressType)
		{
			return universalShipment
				?.OrganizationAddressCollection
				?.FirstOrDefault(address => addressType.Equals(address.AddressType, StringComparison.OrdinalIgnoreCase));
		}

		void PopulateNotes(HouseBill houseBill, UniversalShipment universalShipment)
		{
			var notes = new List<Note>();
			if (universalShipment?.NoteCollection != null)
			{
				foreach (var note in universalShipment.NoteCollection.Where(x => x.Description.HasValue && !x.Description.Value.IsEmpty))
				{
					notes.Add(new Note()
					{
						Text = note.NoteText.GetValueOrDefault(),
						Description = note.Description.GetValueOrDefault()
					});
				}
			}

			houseBill.Notes = notes;
		}

		void PopulateOverrides(HouseBill houseBill, UniversalShipment universalShipment)
		{
			houseBill.GoodsDetailsTextOverride = string.Empty;
			houseBill.HasGoodsDetailsTextOverride = false;
			houseBill.ChargesTextOverride = string.Empty;
			houseBill.HasGoodsDetailsTextOverride = false;
			houseBill.FollowOnTextOverride = string.Empty;
			houseBill.HasChargesTextOverride = false;

			if (universalShipment?.NoteCollection != null)
			{
				foreach (var note in universalShipment.NoteCollection.Where(x => x.Description.HasValue && !x.Description.Value.IsEmpty))
				{
					var noteDescription = note.Description.GetValueOrDefault();
					if (noteDescription == PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride.Code)
					{
						houseBill.GoodsDetailsTextOverride = note.NoteText.GetValueOrDefault();
						houseBill.HasGoodsDetailsTextOverride = true;
					}
					else if (noteDescription == PredefinedNoteTypes.Instance.HouseBillChargesOverride.Code)
					{
						houseBill.ChargesTextOverride = note.NoteText.GetValueOrDefault();
						houseBill.HasGoodsDetailsTextOverride = true;
					}
					else if (noteDescription == PredefinedNoteTypes.Instance.HouseBillFollowOnOverride.Code)
					{
						houseBill.FollowOnTextOverride = note.NoteText.GetValueOrDefault();
						houseBill.HasChargesTextOverride = true;
					}

					if (houseBill.HasGoodsDetailsTextOverride && houseBill.HasGoodsDetailsTextOverride && houseBill.HasChargesTextOverride)
					{
						return;
					}
				}
			}
		}

		void PopulateRegistrationNumbers(HouseBill houseBill)
		{
			var hirReference = shipment?.Numbers?.GetFirstReferenceNumberByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR);
			if (hirReference != null)
			{
				houseBill.HIRReference = new RegistrationNumber()
				{
					Value = hirReference.CE_EntryNum,
					Type = new CodeDescription(new ShipmentNonCustomsAdditionalReferenceCodesCodeList())
					{
						Code = hirReference.CE_EntryType
					},
					CountryOfIssue = new Country(context.Factory, context.Countries)
					{
						Code = hirReference.CE_RN_NKCountryCode
					}
				};
			}
		}

		UniversalShipment GetShippingInstructionUniversalXml()
		{
			var dataImportLogs = shipment
				?.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DataLinkedCode)
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.ToArray();

			foreach (var dataImportLog in dataImportLogs ?? Array.Empty<StmALog>())
			{
				if (dataImportLog.RelatedEDIMessage?.Message is IEDIMessage linkedMessage
					&& linkedMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
				{
					var usxml = dataImportLog.RelatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalShipment>();

					string documentName = usxml
						?.DataContext
						?.DocumentaryOverride
						?.DocumentName;

					if (documentName.Equals(ConsolDocumentNames.ShippingInstruction, StringComparison.InvariantCultureIgnoreCase))
					{
						return usxml;
					}
				}
			}

			return null;
		}

		void AddValidation(HouseBill houseBill)
		{
			AddAddressValidation(houseBill);

			houseBill.HouseBillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("37fbdc87-b3c0-4c70-8a01-4a8a703b2e22", "House Bill Number is required."));
			((Vessel)houseBill.Transports?.Main?.Vessel)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("7a03dee1-431a-4a44-b4f7-3d2c971dd8ce", "Vessel is required."));
			((Transport)houseBill.Transports?.Main)?.VoyageFlightNumberInfo.AddMessageErrorIfEmpty(Res.GetString("3a1aebed-ef9e-4fdb-a499-d3bc11f093f4", "Voyage is required."));
			((Unloco)houseBill.PortOfLoading)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("fc6285cf-8ad2-4ea1-8a98-f8bc7f4fb1d5", "Port of Loading is required."));
			((Unloco)houseBill.PortOfDischarge)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("4d77f6c3-c591-41ae-9e1b-b3135af6f2b3", "Port of Discharge is required."));
			((Unloco)houseBill.PortOfDestination)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("e01f191c-9e48-4f22-9b3a-8277da6b801a", "Destination is required."));
			((Unloco)houseBill.FreightPayableAt)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("57541c28-6b7f-4172-af1a-bef495adf323", "Freight Payable At is required."));

			((CodeDescription)houseBill.INCO)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("2afd6ba1-d487-46af-b05f-accce78197f0", "Inco term code is required."));

			houseBill.ValidateAllIncludingChildren();
		}

		void AddAddressValidation(HouseBill houseBill)
		{
			((Address)houseBill.Shipper)?.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("35d84bb2-d3e7-4ff2-ad2c-1255394f3fe0", "Shipper is required."));
			((Address)houseBill.Consignee)?.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("5cb57df5-c46f-4d87-9296-b149dc6b59b3", "Consignee is required."));
		}
	}
}
