using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Header = Enterprise.Warehouse.Transactions.DataTransfer.WhsDocketConstants.HeaderRecord;
using Line = Enterprise.Warehouse.Transactions.DataTransfer.WhsDocketConstants.LineRecord;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract partial class WhsDocketDataConverter : FlatFileConverter<Xsd.WhsDockets>
	{
		protected WhsDocketDataConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(Xsd.WhsDockets valueObject, FlatFileDataRowCollection rows)
		{
			Xsd.WhsDockets xsdWhsDockets = valueObject;
			Xsd.WhsDocket xsdWhsDocket = null;

			if (rows.Count > 0 && rows[0].GetField(Header.Version) == WhsDocketConstants.Version)
			{
				foreach (FlatFileDataRow row in rows)
				{
					if (IsHeaderRecord(row))
					{
						xsdWhsDocket = xsdWhsDockets.WhsDocket.AddNew();
						PopulateDocketHeader(xsdWhsDocket, row);
					}
					else if (IsLineRecord(row))
					{
						if (xsdWhsDocket != null)
						{
							Xsd.WhsDocketLine xsdWhsDocketLine = xsdWhsDocket.DocketLines.AddNew();
							PopulateDocketLine(xsdWhsDocketLine, row);
						}
					}
					else
					{
						Notification.Notify(new ErrorNotification(ErrorType.UnknownRecordType, Res.GetString("03cdec0d-6fc3-4e79-b23e-b3e5b39d94c9", "Unidentifiable line. Record Type unknown:") + " " + row[0]));
						break;
					}
				}
			}
			else
			{
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, Res.GetString("2a845353-b787-4632-a475-cf61d4d79ba6", "Invalid File - The file is either empty or its version number is not supported.")));
			}
		}

		#region Implementation

		protected virtual void PopulateDocketHeader(Xsd.WhsDocket xsdWhsDocket, FlatFileDataRow row)
		{
			xsdWhsDocket.Identifier.Client = GetXsdOrganisation(row[Header.ClientCode], row[Header.ClientName],
								row[Header.ClientAddress1], row[Header.ClientAddress2],
								row[Header.ClientCity],
								row[Header.ClientState],
								row[Header.ClientPostCode],
								row[Header.ClientUNLOCO],
								row[Header.ClientEmailAddress],
								row[Header.ClientPhone],
								row[Header.ClientContact]
								);

			xsdWhsDocket.Identifier.Reference = row[Header.OrderNumber];

			xsdWhsDocket.DocketDetail.WarehouseCode = row[Header.WarehouseCode];
			xsdWhsDocket.DocketDetail.CustomerReference = row[Header.CustomerReference];
			xsdWhsDocket.DocketDetail.TransportReference = row[Header.TransportReference];
			xsdWhsDocket.DocketDetail.TransportServiceLevel = row[Header.TransportServiceLevel];
			xsdWhsDocket.DocketDetail.ServiceLevel = row[Header.ServiceLevel];
			xsdWhsDocket.DocketDetail.Units = row.GetFieldAsZDecimal(Header.TotalUnits);

			if (!row[Header.TransportCoCode].IsEmpty || !row[Header.TransportCoName].IsEmpty)
			{
				PopulateXsdDocAddress(xsdWhsDocket.DocketDetail.TransportCompany, Xsd.DocAddressAddressType.TRA, row[Header.TransportCoCode], row[Header.TransportCoName],
									row[Header.TransportCoAddress1], row[Header.TransportCoAddress2],
									row[Header.TransportCoCity],
									row[Header.TransportCoState],
									row[Header.TransportCoPostCode],
									row[Header.TransportCoISOCountryCode],
									row[Header.TransportCoUNLOCO],
									row[Header.TransportCoEmailAddress],
									row[Header.TransportCoPhone],
									row[Header.TransportCoContact]
									);
			}

			if (!row[Header.TransportBilledToCode].IsEmpty || !row[Header.TransportBilledToName].IsEmpty)
			{
				PopulateXsdDocAddress(xsdWhsDocket.DocketDetail.TransportBilledTo, Xsd.DocAddressAddressType.TBT, row[Header.TransportBilledToCode], row[Header.TransportBilledToName],
									row[Header.TransportBilledToAddress1], row[Header.TransportBilledToAddress2],
									row[Header.TransportBilledToCity],
									row[Header.TransportBilledToState],
									row[Header.TransportBilledToPostCode],
									row[Header.TransportBilledToISOCountryCode],
									row[Header.TransportBilledToUNLOCO],
									row[Header.TransportBilledToEmailAddress],
									row[Header.TransportBilledToPhone],
									row[Header.TransportBilledToContact]
									);
			}

			PopulateNotes(xsdWhsDocket, row[Header.GoodsHandlingNotes], Xsd.NotesNoteNoteType.HandlingInstructions);
			PopulateNotes(xsdWhsDocket, row[Header.DGAdditionalHandlingNotes], Xsd.NotesNoteNoteType.DangerousGoodsAdditionalHandlingInformation);
			PopulateNotes(xsdWhsDocket, row[Header.Deliveryinstructions], Xsd.NotesNoteNoteType.DeliveryInstructionsNote);

			if (!row[Header.ThirdPartyCarrierAccountNumber].IsEmpty)
			{
				var tpcReference = xsdWhsDocket.DocketDetail.References.AddNew();
				tpcReference.Type = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
				tpcReference.Value = row[Header.ThirdPartyCarrierAccountNumber];
			}
		}

		protected virtual void PopulateDocketLine(Xsd.WhsDocketLine xsdWhsDocketLine, FlatFileDataRow row)
		{
			xsdWhsDocketLine.LineNumber = row.GetFieldAsZShort(Line.ClientOrderLineNumber);
			xsdWhsDocketLine.Product = row[Line.ProductCode];
			xsdWhsDocketLine.Description = row[Line.ProductDescription];

			ZDecimal quantityFromClientOrder = row.GetFieldAsZDecimal(Line.QuantityFromClientOrder);
			ZDecimal quantityActuallyOrdered = row.GetFieldAsZDecimal(Line.QuantityActuallyOrdered);
			xsdWhsDocketLine.QuantityFromClientOrder = quantityFromClientOrder == ZDecimal.Zero ? quantityActuallyOrdered : quantityFromClientOrder;
			xsdWhsDocketLine.QuantityActuallyOrdered = quantityActuallyOrdered;
			xsdWhsDocketLine.ProductUQ = row[Line.ProductUQ];

			xsdWhsDocketLine.LineAttributes.BondedEntryKey = row[Line.BondedEntryKey];
			xsdWhsDocketLine.LineAttributes.ExpiryDate = row.GetFieldAsZDateTime(Line.ExpiryDate, "yyyyMMdd").Date;
			xsdWhsDocketLine.LineAttributes.PackingDate = row.GetFieldAsZDateTime(Line.PackingDate, "yyyyMMdd").Date;
			xsdWhsDocketLine.LineAttributes.PartAttribute1 = row[Line.PartAttribute1];
			xsdWhsDocketLine.LineAttributes.PartAttribute2 = row[Line.PartAttribute2];
			xsdWhsDocketLine.LineAttributes.PartAttribute3 = row[Line.PartAttribute3];
			xsdWhsDocketLine.LineComments = row[Line.LineComments];

			if (!row[Line.BondEntryNumber].IsEmpty && !row[Line.BondEntryLineNumber].IsEmpty)
			{
				xsdWhsDocketLine.CustomsData.EntryKey = row[Line.BondEntryNumber];
				xsdWhsDocketLine.CustomsData.EntryLineNumber = row.GetFieldAsZShort(Line.BondEntryLineNumber);
				xsdWhsDocketLine.CustomsData.EntryDate = row.GetFieldAsZDateTime(Line.BondEntryDate, "yyyyMMdd").Date;
				xsdWhsDocketLine.CustomsData.DeclarationReference = row[Line.BondDeclarationRef];
				xsdWhsDocketLine.CustomsData.CustomsQuantity = row.GetFieldAsZDecimal(Line.BondCustomsQuantity);
				xsdWhsDocketLine.CustomsData.CustomsQuantityUnit = row[Line.BondCustomsQuantityUnit];
				xsdWhsDocketLine.CustomsData.BondedWhsQuantity = row.GetFieldAsZDecimal(Line.BondWarehouseQuantity);
				xsdWhsDocketLine.CustomsData.BondedWhsQuantityUnit = row[Line.BondWarehouseQuantityUnit];
				xsdWhsDocketLine.CustomsData.ValueForDuty = row.GetFieldAsZDecimal(Line.BondValueForDuty);
				xsdWhsDocketLine.CustomsData.TILVAmount = row.GetFieldAsZDecimal(Line.BondTILVAmount);
				xsdWhsDocketLine.CustomsData.TILVCurrency = row[Line.BondTILVCurrency];
				xsdWhsDocketLine.CustomsData.AddInfo = row[Line.BondAddInfo];
			}
		}

		bool IsHeaderRecord(FlatFileDataRow row)
		{
			return row[0] == WhsDocketConstants.HeaderType;
		}

		bool IsLineRecord(FlatFileDataRow row)
		{
			return row[0] == WhsDocketConstants.LineType;
		}

		void PopulateNotes(Xsd.WhsDocket xsdWhsDocket, ZString noteData, Xsd.NotesNoteNoteType noteType)
		{
			if (!noteData.IsEmpty)
			{
				Xsd.NotesNote note1 = xsdWhsDocket.Notes.AddNew();
				note1.NoteType = noteType;
				note1.NoteData = noteData;
			}
		}

		protected void PopulateXsdDocAddress(Xsd.DocAddress docAddress, Xsd.DocAddressAddressType addressType, ZString code, ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString uNLOCO, ZString emailAddress, ZString phone, ZString contactName)
		{
			docAddress.AddressReference.AddressSequenceRef = 1;
			docAddress.AddressReference.IsSpecified = true;
			docAddress.AddressReference.Organisation = GetXsdOrganisation(code, companyName, address1, address2, city, state, postCode, uNLOCO, emailAddress, phone, contactName);

			docAddress.AddressLine1 = address1;
			docAddress.AddressLine2 = address2;
			docAddress.AddressType = addressType;
			docAddress.AddressTypeSpecified = true;
			docAddress.CityOrSuburb = city;
			docAddress.CompanyName = companyName;
			docAddress.ContactName = contactName;
			docAddress.CountryCode = countryCode;
			docAddress.Email = emailAddress;
			docAddress.PostCode = postCode;
			docAddress.StateOrProvince = state;
			docAddress.TelephoneNumbers.AddNew().Value = phone;
		}

		protected Xsd.Organisation GetXsdOrganisation(ZString code, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString uNLOCO, ZString emailAddress, ZString phone, ZString contactName)
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails = new Xsd.OrganisationDetail();
			result.OrganisationDetails.IsSpecified = true;

			result.OwnerCode = code;
			result.OrganisationDetails.Name = name;
			Xsd.OrgAddress address = result.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = address1;
			address.AddressLine2 = address2;
			address.CityOrSuburb = city;
			address.CompanyName = name;
			address.StateOrProvince = state;
			address.PostCode = postCode;
			address.Email = emailAddress;
			Xsd.TelephoneNumber phoneNumber = address.TelephoneNumbers.AddNew();
			phoneNumber.Value = phone;
			phoneNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
			result.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, uNLOCO);
			Xsd.OrgContact contact = result.OrganisationDetails.Contacts.AddNew();
			contact.Name = contactName;
			result.OrganisationDetails.Contacts.IsSpecified = true;

			return result;
		}

		#endregion
	}
}

#if DEBUG

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract partial class WhsDocketDataConverter : FlatFileConverter<Xsd.WhsDockets>
	{
		public void MapImportForTest(Xsd.WhsDockets valueObject, FlatFileDataRowCollection rows)
		{
			MapImport(valueObject, rows);
		}
	}
}

#endif
