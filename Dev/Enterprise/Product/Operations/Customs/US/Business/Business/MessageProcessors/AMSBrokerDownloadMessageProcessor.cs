using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload, Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	public class AMSBrokerDownloadMessageProcessor : ACSABIProcessor
	{
		HtmlTableCreator headerDetails;
		HtmlTableCreator billDetails;
		HtmlTableCreator additionalDetails;
		HtmlTableCreator partyDetails;
		HtmlTableCreator containerDetails;
		HtmlTableCreator vinDetails;
		HtmlTableCreator tariffLinesDetails;
		HtmlTableCreator c4lineDetails;
		HtmlTableCreator marksAndNumberDetails;
		HtmlTableCreator hazardousDetails;

		PartyDetails oneParty;
		HazardousMaterialDetails oneHazardous;

		ZStringBuilder billOfLadingBody;
		ZString currentBillNum;
		ZString currentIssuerSCACCode;

		public override void Process()
		{
			Message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;

			var emailBody = new ZStringBuilder();

			headerDetails = new HtmlTableCreator(new string[] { "Column", "Data" });

			billOfLadingBody = new ZStringBuilder();

			foreach (MessageBlock block in messageBlocks)
			{
				if (block is IAMS1M)
				{
					var ams1m = block as IAMS1M;
					ProcessAMS1M(ams1m);
				}
				else if (block is IAMS2M)
				{
					var ams2m = block as IAMS2M;
					WriteToHtmlTable(headerDetails, "Carrier Assigned Batch Number", ams2m.CarrierAssignedBatchNumber);
				}
				else if (block is IAMS1P)
				{
					var ams1p = block as IAMS1P;
					ProcessAMS1P(ams1p);
				}
				else if (block is IAMS1J)//beginning of bill of lading
				{
					var ams1j = block as IAMS1J;
					ProcessAMS1J(ams1j);
				}
				else if (block is IAMS1A)
				{
					var ams1a = block as IAMS1A;
					ProcessAMS1A(ams1a);
				}
				else if (block is IAMS1B)//mandatory
				{
					var ams1b = block as IAMS1B;
					ProcessAMS1B(ams1b);
				}
				else if (block is IAMS2B)
				{
					var ams2b = block as IAMS2B;
					ProcessAMS2B(ams2b);
				}
				else if (block is IAMS4B)
				{
					var ams4b = block as IAMS4B;

					ProcessAMS4B(ams4b);
				}
				else if (block is IAMS0N)
				{
					var ams0n = block as IAMS0N;//start of party details

					ProcessAMS0N(ams0n);
				}
				else if (block is IAMS2N)
				{
					if (oneParty == null)
					{
						oneParty = new PartyDetails();
					}

					var ams2n = block as IAMS2N;
					oneParty.Address.Append(ams2n.EntityPartyAddress);
					oneParty.Address.Append(ams2n.EntityPartyAddress1);
				}
				else if (block is IAMS3N)
				{
					if (oneParty == null)
					{
						oneParty = new PartyDetails();
					}

					var ams3n = block as IAMS3N;
					ProcessAMS3N(ams3n);
				}
				else if (block is IAMS4N)
				{
					if (oneParty == null)
					{
						oneParty = new PartyDetails();
					}

					var ams4n = block as IAMS4N;
					ProcessAMS4N(ams4n);
				}
				else if (block is IAMS1I)
				{
					var ams1i = block as IAMS1I;

					ProcessAMS1I(ams1i);
				}
				else if (block is IAMS2I)
				{
					var ams2i = block as IAMS2I;

					WriteToHtmlTable(billDetails, "IT Export Transportation Mode", ams2i.TransportationIndicator);
					WriteToHtmlTable(billDetails, "IT Export Vessel Name", ams2i.VesselName);
				}
				else if (block is IAMS1C)
				{
					if (containerDetails == null)
					{
						containerDetails = new HtmlTableCreator(new string[] { "Equipment Number", "Seal Number 1", "Seal Number 2", "Container/Equipment Type Code", "Load/Empty Status Code" });
					}

					var ams1c = block as IAMS1C;

					string statusDesc = new ManifestContainerLoadEmptyStatusList().GetDescriptionFromCode(ams1c.LoadEmptyStatusCode);
					containerDetails.WriteRow(ams1c.EquipmentInitial + ams1c.EquipmentNumber, ams1c.SealNumber1, ams1c.SealNumber2, ams1c.ContainerEquipmentDescriptionCode, statusDesc ?? "");
				}
				else if (block is IAMS2C)
				{
					if (vinDetails == null)
					{
						vinDetails = new HtmlTableCreator(new string[] { "VIN Number", "Foreign Port of Lading", "Factory Car Order Number" });
					}

					var ams2c = block as IAMS2C;
					vinDetails.WriteRow(ams2c.VIN, ams2c.ForeignPort, ams2c.FactoryCarOrderNumber);
				}
				else if (block is IAMS0D)
				{
					if (tariffLinesDetails == null)
					{
						tariffLinesDetails = new HtmlTableCreator(new string[] { "Harmonized Tariff Number", "Value", "Weight" });
					}

					var ams0d = block as IAMS0D;
					tariffLinesDetails.WriteRow(ams0d.HarmonizedNumber, ams0d.Value, ams0d.Weight + " " + ams0d.WeightUnit);
				}
				else if (block is IAMS1D)
				{
					if (c4lineDetails == null)
					{
						c4lineDetails = new HtmlTableCreator(new string[] { "C4 Number", "Description", "Piece Count", "Country" });
					}

					var ams1d = block as IAMS1D;
					c4lineDetails.WriteRow(ams1d.C4Number, ams1d.Description, ams1d.PieceCount + " " + ams1d.ManifestUnitCode, ams1d.CountryCode);
				}
				else if (block is IAMS2D)
				{
					if (marksAndNumberDetails == null)
					{
						marksAndNumberDetails = new HtmlTableCreator(new string[] { "Marks and Numbers" });
					}

					var ams2D = block as IAMS2D;
					marksAndNumberDetails.WriteRow(ams2D.MarksAndNumbers);
				}
				else if (block is IAMS1V)
				{
					var ams1V = block as IAMS1V;
					ProcessAMS1V(ams1V);
				}
				else if (block is IAMS2V)
				{
					if (oneHazardous == null)
					{
						oneHazardous = new HazardousMaterialDetails();
					}

					var ams2v = block as IAMS2V;
					oneHazardous.FlashpointTemp = ams2v.FlashpointTemperature;
					oneHazardous.UQ = ams2v.UnitOfMeasureCode;

					//'N' means Negative
					if (ams2v.NegativeIndicator == "N")
					{
						oneHazardous.FlashpointTemp *= -1;
					}
				}
				else if (block is IAMS3V)
				{
					if (oneHazardous == null)
					{
						oneHazardous = new HazardousMaterialDetails();
					}

					var ams3v = block as IAMS3V;
					oneHazardous.DetailedDescription = ams3v.HazardousMaterialDescription;
					oneHazardous.Classification = ams3v.HazardousMaterialClassification;
				}
			}

			SerialiseOneBillDetailToEmailBody();

			emailBody.Append("Manifest Header Details<br />");
			emailBody.Append(headerDetails.ToHtml());
			emailBody.Append(billOfLadingBody.ToString());

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup("", "", "AMS Broker Download", emailBody.ToString(), false, branch, null);
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.AMSBrokerDownloadMessagesGroup;
		}

		#region Process an individual block

		void ProcessAMS1V(IAMS1V ams1V)
		{
			if (hazardousDetails == null)
			{
				hazardousDetails = new HtmlTableCreator(new string[] { "Code", "Class", "Code Qualifier", "Description", "Contact Number", "Flash Point Temperature", "Detailed Description", "Classification or Division or Label Requirement" });
			}

			WriteHazardousMaterialRow();

			oneHazardous = new HazardousMaterialDetails();
			oneHazardous.Code = ams1V.HazardousMaterialCode;
			oneHazardous.Class = ams1V.HazardousMaterialClass;
			oneHazardous.CodeQualifier = ams1V.HazardousMaterialCodeQualifier;
			oneHazardous.Desc = ams1V.HazardousMaterialDescription;
			oneHazardous.Contact = ams1V.HazardousMaterialContact;
			oneHazardous.UNPageNumber = ams1V.UNHazardousMaterialPage;
		}

		void WriteHazardousMaterialRow()
		{
			if (hazardousDetails != null && oneHazardous != null)
			{
				hazardousDetails.WriteRow(oneHazardous.Code, oneHazardous.Class, oneHazardous.CodeQualifier, oneHazardous.Desc, oneHazardous.Contact, oneHazardous.UNPageNumber, oneHazardous.FlashpointTemp + " " + oneHazardous.UQ, oneHazardous.DetailedDescription, oneHazardous.Classification);
				oneHazardous = null;
			}
		}

		void ProcessAMS1I(IAMS1I ams1i)
		{
			WriteToHtmlTable(billDetails, "FDA/BTA Confirmation Indicator", new ZString(ams1i.FDABTAConfirmationIndicator == "Y" ? "PN on file with FDA" : "No PIN on file"));
			WriteToHtmlTable(billDetails, "Conventional In-Bond Number", ams1i.ConventionalInBondNumber);
			WriteToHtmlTable(billDetails, "In-Bond Carrier Code (SCAC) assuming liability for in-bond movement", ams1i.InBondCarrierCode);
			WriteToHtmlTable(billDetails, "Foreign Destination (62 or 63 only)", ams1i.ForeignDestination);
			WriteToHtmlTable(billDetails, "Value", ams1i.Value);
			WriteToHtmlTable(billDetails, "Bonded Carrier ID", ams1i.BondedCarrierIDNumber);
			WriteToHtmlTable(billDetails, "Paperless In-Bond Number", ams1i.PaperlessInBond);
			WriteToHtmlTable(billDetails, "Shipment Control Number In-Bond", ams1i.ShipmentControlNumberInBond);
		}

		void ProcessAMS1J(IAMS1J ams1j)
		{
			SerialiseOneBillDetailToEmailBody();
			billDetails = new HtmlTableCreator(new string[] { "Column", "Data" });
			currentIssuerSCACCode = ams1j.IssuerCode;
			WriteToHtmlTable(billDetails, "Issuer SCAC Code", ams1j.IssuerCode);
		}

		void ProcessAMS4N(IAMS4N ams4n)
		{
			oneParty.ContactName = ams4n.ContactName;
			oneParty.ContactNumber.Append("(" + ams4n.CommNumberQualifier + ")");
			oneParty.ContactNumber.Append(ams4n.CommunicationsNumber);
		}

		void ProcessAMS3N(IAMS3N ams3n)
		{
			oneParty.Address.Append(ams3n.CityName);
			oneParty.Address.Append(ams3n.StateProvince.IsEmpty ? ams3n.LocationIdentifier : ams3n.StateProvince);
			oneParty.Address.Append(ams3n.PostalCode);
			oneParty.Address.Append(ams3n.CountryCode);
		}

		void ProcessAMS0N(IAMS0N ams0n)
		{
			if (partyDetails == null)
			{
				partyDetails = new HtmlTableCreator(new string[] { "Name", "Entity Type", "ID Code Qualifier", "ID Code", "Address", "Contact Name", "Contact Number" });
			}

			if (oneParty != null)
			{
				partyDetails.WriteRow(oneParty.Name, oneParty.EntityType, oneParty.IDCodeQualifier, oneParty.IDCode, oneParty.Address.ToStringWithDelimiterBetweenAppends(" "), oneParty.ContactName, oneParty.ContactNumber.ToString());
			}

			oneParty = new PartyDetails();

			oneParty.Name = ams0n.Name;
			string entityTypeDesc = new ManifestRelatedPartyCodeList().GetDescriptionFromCode(ams0n.EntityIDCode);
			oneParty.EntityType = new ZString(entityTypeDesc ?? "");
			oneParty.IDCodeQualifier = new ZString(ams0n.CodeQualifier == "9" ? "DUNS+4" : "ABI Routing Code");
			oneParty.IDCode = ams0n.IDCode;
		}

		void ProcessAMS4B(IAMS4B ams4b)
		{
			if (additionalDetails == null)
			{
				additionalDetails = new HtmlTableCreator(new string[] { "Reference Type", "Reference Number" });
			}

			if (!ams4b.ReferenceNumber.IsEmpty)
			{
				string desc = new ReferenceQualifierList().GetDescriptionFromCode(ams4b.ReferenceQualifier);
				additionalDetails.WriteRow(desc ?? "", ams4b.ReferenceNumber);
			}
		}

		void ProcessAMS2B(IAMS2B ams2b)
		{
			WriteToHtmlTable(billDetails, "Measurement", new ZString(ams2b.Measurement + " " + ams2b.MeasurementUnit));
			WriteToHtmlTable(billDetails, "Place of Receipt by Pre-carrier", ams2b.PlaceOfReceiptByPrecarrier);
			WriteToHtmlTable(billDetails, "Space Charter B/L Reference", ams2b.SpaceCharterBLReference);
			WriteToHtmlTable(billDetails, "Secondary Notifiy 1 Party SCAC code", ams2b.CarrierCode);
			WriteToHtmlTable(billDetails, "Secondary Notifiy 2 Party SCAC code", ams2b.CarrierCode1);
		}

		void ProcessAMS1B(IAMS1B ams1b)
		{
			currentBillNum = ams1b.BillOfLading;

			var billOfLading = new IssuerAndBillNumber.Loader(Factory).LoadTop1(Message, currentIssuerSCACCode + currentBillNum);
			if (billOfLading == null)
			{
				billOfLading = Factory.New<IssuerAndBillNumber>();
				billOfLading.CY_ParentID = Message.PK;
				billOfLading.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
				billOfLading.CY_Data = currentIssuerSCACCode + currentBillNum;
			}

			WriteToHtmlTable(billDetails, "Foreign Port of Lading", ams1b.ForeignPortOfLading);
			WriteToHtmlTable(billDetails, "Manifest Quantity", new ZString(ams1b.ManifestQuantity + " " + ams1b.ManifestUnits));
			WriteToHtmlTable(billDetails, "Weight", new ZString(ams1b.Weight + " " + GetDescriptionFromWeightUnit(ams1b.WeightUnit)));
			WriteToHtmlTable(billDetails, "Bill of Lading Status", GetStatusDescription(ams1b.BillOfLadingStatusIndicator));
			WriteToHtmlTable(billDetails, "Master In-Bond Indicator", new ZString(ams1b.MasterInBondIndicator == "1" ? "Y" : "N"));
			WriteToHtmlTable(billDetails, "In-Bond Entry Type", ams1b.InBondEntryType);
			WriteToHtmlTable(billDetails, "In-Bond Destination Port", ams1b.InBondPortOfDestination);
		}

		void ProcessAMS1A(IAMS1A ams1a)
		{
			WriteToHtmlTable(billDetails, "Carrier SCAC Code", ams1a.CarrierCode);
			WriteToHtmlTable(billDetails, "Port of Crossing", ams1a.CBPDistrictPort);
			WriteToHtmlTable(billDetails, "Action Code", GetDescriptionFromActionCode(ams1a.ActionCode.ToString()));
			WriteToHtmlTable(billDetails, "Quantity", ams1a.Quantity);

			var amendmentDesc = new ManifestAmendmentCodeList().GetDescriptionFromCode(ams1a.AmendmentCode);
			WriteToHtmlTable(billDetails, "Amendment Code", new ZString(amendmentDesc ?? string.Empty));
			WriteToHtmlTable(billDetails, "House Bill Number", ams1a.HouseBillNumber);
			WriteToHtmlTable(billDetails, "Secondary Notify Party SCAC code", ams1a.CarrierCode1);
			WriteToHtmlTable(billDetails, "House Bill Issuer Code", ams1a.IssuerCode);
		}

		void ProcessAMS1P(IAMS1P ams1p)
		{
			WriteToHtmlTable(headerDetails, "District Port of Unlading", ams1p.DistrictPortOfUnlading);

			if (ams1p.OriginalScheduledDateOfArrival.IsValid)
			{
				ZDateTime arrival = new ZDateTime(ams1p.OriginalScheduledDateOfArrival.Year,
					ams1p.OriginalScheduledDateOfArrival.Month,
					ams1p.OriginalScheduledDateOfArrival.Day,
					ZInt.ParseEmptyAsZero(ams1p.Time.Left(2)),
					ZInt.ParseEmptyAsZero(ams1p.Time.Right(2)),
					0);
				WriteToHtmlTable(headerDetails, "Estimated Arrival Date/Time", (ZString)arrival.ToLongTimeString());
			}

			WriteToHtmlTable(headerDetails, "Number of Bills of Lading for Port", ams1p.NumberOfBillsOfLadingForPort);
			WriteToHtmlTable(headerDetails, "FIRMS code", ams1p.FIRMSCode);
		}

		void ProcessAMS1M(IAMS1M ams1m)
		{
			WriteToHtmlTable(headerDetails, "Carrier Code", ams1m.CarrierCode);
			WriteToHtmlTable(headerDetails, "Transport Mode", GetDescriptionFromTransportModeCode(ams1m.TransportationIndicator.ToString()));
			WriteToHtmlTable(headerDetails, "Country Code of Importing Conveyance", ams1m.CountryCodeOfImportingConveyance);
			WriteToHtmlTable(headerDetails, "Importing Conveyance Name", ams1m.ImportingConveyanceName);

			WriteToHtmlTable(headerDetails, "Number of Bills of Lading", ams1m.NumberOfBillsOfLading);
			WriteToHtmlTable(headerDetails, "Manifest Sequence Number", ams1m.ManifestSequenceNumber);
			WriteToHtmlTable(headerDetails, "AMS MIB Paperless Participant", ams1m.AMSMIBPaperlessParticipant.IsEmpty ? new ZString("N") : ams1m.AMSMIBPaperlessParticipant);

			string typeDesc = new ManifestTypeList().GetDescriptionFromCode(ams1m.ManifestTypeCode);
			WriteToHtmlTable(headerDetails, "Manifest Type Code", new ZString(typeDesc ?? ""));
		}

		#endregion

		#region Building a body of the email

		void SerialiseOneBillDetailToEmailBody()
		{
			if (billDetails != null)
			{
				billOfLadingBody.Append("<br />");
				billOfLadingBody.Append("<hr />");
			}

			AddDetailsToBillOfLadingBody(ref billDetails, "Bill of Lading Details");

			AddDetailsToBillOfLadingBody(ref additionalDetails, "Additional Reference Numbers");

			if (partyDetails != null && oneParty != null)
			{
				partyDetails.WriteRow(oneParty.Name, oneParty.EntityType, oneParty.IDCodeQualifier, oneParty.IDCode, oneParty.Address.ToStringWithDelimiterBetweenAppends(" "), oneParty.ContactName, oneParty.ContactNumber.ToString());
				oneParty = null;

				AddDetailsToBillOfLadingBody(ref partyDetails, "Entities involved");
			}

			AddDetailsToBillOfLadingBody(ref containerDetails, "Equipment Details");

			AddDetailsToBillOfLadingBody(ref vinDetails, "VIN Details");

			AddDetailsToBillOfLadingBody(ref tariffLinesDetails, "Tariff Lines Details");

			AddDetailsToBillOfLadingBody(ref c4lineDetails, "C4 Lines Details");

			AddDetailsToBillOfLadingBody(ref marksAndNumberDetails, "Marks and Numbers");

			WriteHazardousMaterialRow();
			AddDetailsToBillOfLadingBody(ref hazardousDetails, "Hazardous Materials");
		}

		void AddDetailsToBillOfLadingBody(ref HtmlTableCreator table, string tableHeaderDesc)
		{
			if (table != null)
			{
				billOfLadingBody.Append(string.Format("<br />" + tableHeaderDesc + " for {0}<br />", currentBillNum));
				billOfLadingBody.Append(table.ToHtml());
				table = null;
			}
		}

		void WriteToHtmlTable(HtmlTableCreator table, string columnHeader, IZType data)
		{
			if (!data.IsEmpty)
			{
				table.WriteRow(columnHeader, data);
			}
		}

		#endregion

		#region GetDescriptionFromCode

		ZString GetStatusDescription(string code)
		{
			string result = "";

			if (string.IsNullOrWhiteSpace(code))
			{
				result = ManifestStatusList.Codes._0;
			}
			else
			{
				result = new ManifestStatusList().GetDescriptionFromCode(code);
				if (result == null)
				{
					result = "";
				}
			}
			return result;
		}

		ZString GetDescriptionFromWeightUnit(string code)
		{
			switch (code)
			{
				case "G":
					return "Grams";
				case "L":
					return "Pounds";
				case "K":
					return "Kilograms";
				case "O":
					return "Ounces";
				case "T":
					return "Tons";
				default:
					return "";
			}
		}

		ZString GetDescriptionFromActionCode(string code)
		{
			switch (code)
			{
				case "A":
					return "Add bill of lading";
				case "D":
					return "Delete bill of lading";
				case "M":
					return "Replace segment. This is only used in Truck AMS";
				case "R":
					return "Replace the existing manifest quantity with new data";
				default:
					return "";
			}
		}

		ZString GetDescriptionFromTransportModeCode(string code)
		{
			string result = "";
			if (code == "30")
			{
				result = "land, non-container or unable to determine if containerised";
			}
			else
			{
				result = new TransportModeCodes().GetDescriptionFromCode(code);
				if (result == null)
				{
					result = "";
				}
			}
			return result;
		}

		#endregion

		#region Related Classes

		class PartyDetails
		{
			public ZString Name;
			public ZString EntityType;
			public ZString IDCodeQualifier;
			public ZString IDCode;
			public ZStringBuilder Address
			{
				get
				{
					if (fAddress == null)
					{
						fAddress = new ZStringBuilder();
					}
					return fAddress;
				}
			}
			ZStringBuilder fAddress;

			public ZString ContactName;
			public ZStringBuilder ContactNumber
			{
				get
				{
					if (fContactNumber == null)
					{
						fContactNumber = new ZStringBuilder();
					}
					return fContactNumber;
				}
			}
			ZStringBuilder fContactNumber;
		}

		class HazardousMaterialDetails
		{
			public ZString Code;
			public ZString Class;
			public ZString CodeQualifier;
			public ZString Desc;
			public ZString Contact;
			public ZString UNPageNumber;
			public ZInt FlashpointTemp;
			public ZString UQ;
			public ZString DetailedDescription;
			public ZString Classification;
		}
		#endregion
	}
}
