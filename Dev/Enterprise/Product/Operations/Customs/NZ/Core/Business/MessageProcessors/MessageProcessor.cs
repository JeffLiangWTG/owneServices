using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Edifact.D98A.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	public abstract class MessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		protected MessageProcessor(LoggingInformation logger, string messageFriendlyName)
			: base(logger, EDIInterchange.ApplicationCodes.NewZealandCustoms, messageFriendlyName)
		{
		}

		protected CUSRESMessage cUSRESMessage;
		protected NZCMessage responseMessage;

		protected virtual void SetupPropertiesForMessageProcessing(NZCMessage message)
		{
			cUSRESMessage = message.MessageAsCUSRESD98A;
			builder = new ResponseEmailBuilder();
			responseMessage = message;
		}
		protected ResponseEmailBuilder builder;

		protected abstract void WriteEntryStatus();

		protected override string DoProcessingReturningStatus(EDIMessage ediMessage)
		{
			Argument.NotNull(ediMessage, "ediMessage");
			NZCMessage message = ediMessage as NZCMessage
				?? throw new InvalidOperationException("You must supply an NZCMessage when calling ProcessMessage on an NZ Message Processor.");

			SetupPropertiesForMessageProcessing(message);
			EmailDef email = new EmailDef();

			try
			{
				if (cUSRESMessage == null)
				{
					throw new MessageProcessingException("Corrupted or Malformed response message. Message does not conform to UN-EDIFACT standard. Cannot process.");
				}

				ProcessGroup0(cUSRESMessage);
				WriteEntryStatus();
				email.Subject = GetEmailSubject(message);
				email.Body = builder.HeaderString.ToString() + builder.BodyString.ToString();
				SendSuccessfullyProcessedResultEmail(email);

				message.EM_MessageInterpretation = email.Subject + "\r\n\r\n" + email.Body;
				return EDIMessage.Status.Received;
			}
			catch (MessageProcessingException messageProcessingException)
			{
				builder.OutputHeaderLine(messageProcessingException.Message);
				message.EM_Status = EDIMessage.Status.Error;

				email.Subject = "Error Processing Response for " + JobTypeDescription + ": " + message.GetJobNumber();
				email.Body = messageProcessingException.Message;
				if (cUSRESMessage != null)
				{
					email.Body += "\r\n\r\n" + cUSRESMessage.ToString(new UNOACharacterSet());
				}

				SendProcessingFailureResultEmail(email);
				return EDIMessage.Status.Failed;
			}
		}

		protected abstract void SendSuccessfullyProcessedResultEmail(EmailDef email);
		protected abstract void SendProcessingFailureResultEmail(EmailDef email);

		protected string GetEmailSubject(NZCMessage message)
		{
			return "[" + ResponseTypeForEmailSubject + "] Response for " + JobTypeDescription + ": " + message.GetJobNumber();
		}

		public abstract string JobTypeDescription { get; }
		public abstract string ResponseTypeForEmailSubject { get; }

		protected abstract void ProcessGroup0(CUSRESMessage cUSRESMessage);

		public void ProcessGroup4(SegmentGroup4 segmentGroup4)
		{
			ERPSegment eRPSegment = segmentGroup4.ERP[0];
			ERCSegment eRCSegment = segmentGroup4.ERC[0];
			CheckRequiredSegmentNotNull(eRPSegment);
			CheckRequiredSegmentNotNull(eRCSegment);

			string errorSection = eRPSegment.ErrorPointDetails.MessageSectionCoded.ToString();
			string errorItemNumber = eRPSegment.ErrorPointDetails.MessageItemNumber;
			string fieldCode = eRPSegment.ErrorPointDetails.MessageSubItemNumber;
			string errorCode = eRCSegment.ApplicationErrorDetail.ApplicationErrorIdentification;

			AddErrorForItem(errorSection, errorItemNumber, fieldCode, errorCode);
		}

		protected virtual void AddErrorForItem(string errorSection, string errorItemNumber, string fieldCode, string errorCode)
		{
			string errorPlace = GetErrorPointFromCode(errorSection);
			string itemNumber = GetItemNumberDescription(errorItemNumber, errorSection);
			string fieldDescription = GetFieldNameFromCode(fieldCode);
			string errorDescription = GetErrorDecriptionFromCode(errorCode);

			builder.OutputBodyLine("**Error** in " + errorPlace + itemNumber + ", " + fieldDescription + ":-\r\n  " + errorDescription + ".");
		}

		public void CheckRequiredSegmentNotNull(Segment segment)
		{
			if (segment == null)
			{
				throw new MessageProcessingException("Expected " + segment.SegmentName + " Segment Missing from Message - Cannot complete processing.");
			}
		}

		#region EDIFACT Code > Description Getters
		protected abstract string GetResponseTypeFromCode(string responseTypeCode);
		protected abstract string GetItemNumberDescription(string errorItemNumber, string errorSection);
		protected abstract string GetErrorPointFromCode(string errorPointCode);

		internal string GetErrorDecriptionFromCode(string errorCode)
		{
			// This should stay in this parent class. 
			// Error Codes are common between ECI Writeoffs and Formal Entries.
			// These have been modified from the original NZ Spec'd text, 
			// and do not want to lose the changes made by Ben Govett.

			string retVal = "Unknown Error Code: " + errorCode;
			string portMessage = "\r\n     (Check the UNLOCO for the port you are trying to enter)";
			string flightVesselMessage = "\r\n     (https://www.customs.govt.nz/business/import/lodge-your-import-entry/craft-names-and-flight-numbers/\r\n     for a full list of valid Flight Numbers and Vessel Names)";

			switch (errorCode)
			{
				case "101":
					retVal = "Flight No. : Not specified" + flightVesselMessage;
					break;
				case "102":
					retVal = "Craft Name : Not specified" + flightVesselMessage;
					break;
				case "103":
					retVal = "Transport Mode : Not specified or invalid";
					break;
				case "104":
					retVal = "Total Gross Weight : Not specified";
					break;
				case "105":
					retVal = "Total Gross Weight : Exceeds 10,000 kg for Air Consignment";
					break;
				case "106":
					retVal = "Total Gross Weight : Exceeds 199,999,999 kg for Sea Consignment";
					break;
				case "107":
					retVal = "Total Gross Weight : Should be greater than aggregate of Stats Quantity";
					break;
				case "108":
					retVal = "Date of Export: Entry lodged too many days prior to export";
					break;
				case "109":
					retVal = "Date of Export: Entry lodged too many days after export";
					break;
				case "110":
					retVal = "Port of Discharge : Not specified or invalid" + portMessage;
					break;
				case "111":
					retVal = "Port of Loading : Not specified or invalid" + portMessage;
					break;
				case "112":
					retVal = "Voyage Number : Not specified";
					break;
				case "113":
					retVal = "Date of Import : Not specified or invalid";
					break;
				case "114":
					retVal = "Date of Export : Not specified or invalid";
					break;
				case "115":
					retVal = "Override Indicator : Invalid";
					break;
				case "116":
					retVal = "Payment Method : Invalid";
					break;
				case "117":
					retVal = "Sold/Consignment Indicator: Not specified or invalid";
					break;
				case "118":
					retVal = "Total Gross Weight : Exceeds 100,000 kg for Air Consignment";
					break;
				case "119":
					retVal = "Total Gross Weight : Exceeds 500,000 kg for Sea Consignment";
					break;
				case "120":
					retVal = "Supplier Code : Not specified or invalid";
					break;
				case "121":
					retVal = "Supplier Code : Not on file";
					break;
				case "122":
					retVal = "Supplier Code : Not current";
					break;
				case "123":
					retVal = "Supplier Name : Not specified";
					break;
				case "124":
					retVal = "Payment Method: Entry cleared - cannot be changed";
					break;
				case "135":
					retVal = "Customs Controlled Area : Not specified or invalid";
					break;
				case "136":
					retVal = "Customs Controlled Area : Not on file";
					break;
				case "137":
					retVal = "Customs Controlled Area : Not current";
					break;
				case "138":
					retVal = "Client Code : Not specified or invalid";
					break;
				case "139":
					retVal = "Broker Code : Invalid";
					break;
				case "140":
					retVal = "Client Code : Not on file";
					break;
				case "141":
					retVal = "Broker Code : Not on file";
					break;
				case "142":
					retVal = "Client Code : Not current";
					break;
				case "143":
					retVal = "Broker Code : Not current";
					break;
				case "144":
					retVal = "Client Name : Not specified";
					break;
				case "145":
					retVal = "Declarant Code : Not specified or invalid";
					break;
				case "146":
					retVal = "Declarant Code : Not on file";
					break;
				case "147":
					retVal = "Declarant Code : Not current";
					break;
				case "148":
					retVal = "Carrier Name : Not specified";
					break;
				case "150":
					retVal = "Broker Code: Not the same as the original entry";
					break;
				case "151":
					retVal = "Broker Code: Entry cleared - cannot be changed";
					break;
				case "152":
					retVal = "Client Code: Entry cleared - cannot be changed";
					break;
				case "153":
					retVal = "Country/Region of Origin : Not specified or invalid";
					break;
				case "154":
					retVal = "Country/Region of Export : Not specified or invalid";
					break;
				case "156":
					retVal = "Country/Region of Origin : Not current";
					break;
				case "157":
					retVal = "Country/Region of Export : Not current";
					break;
				case "158":
					retVal = "Country/Region of Export : Not same as Port of Loading";
					break;
				case "166":
					retVal = "Country/Region of Destination : Not specified or invalid";
					break;
				case "167":
					retVal = "Country/Region of Destination : Not current";
					break;
				case "168":
					retVal = "Country/Region of Destination : Not same as Port of Discharge";
					break;
				case "172":
					retVal = "Value in NZ Dollars : Not specified or invalid";
					break;
				case "173":
					retVal = "Value in Currency : Not specified or invalid";
					break;
				case "174":
					retVal = "Freight : Not specified or invalid";
					break;
				case "175":
					retVal = "Insurance : Not specified or invalid";
					break;
				case "176":
					retVal = "Total Duty : Diplomatic entry - No duty payable";
					break;
				case "177":
					retVal = "Total Duty : Memorandum/Letter of Understanding or Deed of Covenant - No duty payable";
					break;
				case "181":
					retVal = "CIF : Must in range 0 to 250,000,000";
					break;
				case "186":
					retVal = "Bill Type : Not specified or Invalid";
					break;
				case "187":
					retVal = "Bill/Parcel Number : Not specified or invalid";
					break;
				case "188":
					retVal = "Number of Packages : Not specified or invalid";
					break;
				case "189":
					retVal = "Type of Package : Not specified or invalid";
					break;
				case "190":
					retVal = "Bill Number : Only one MasterBill allowed with no packaging or container information";
					break;
				case "191":
					retVal = "Concession Code : Invalid";
					break;
				case "192":
					retVal = "Concession Code : Not current";
					break;
				case "193":
					retVal = "Concession Code : Not on file";
					break;
				case "194":
					retVal = "Concession Code : Not valid for Tariff Item";
					break;
				case "196":
					retVal = "Concession Code : Not applicable for Country/Region Of Origin";
					break;
				case "197":
					retVal = "Tariff Item : Goods imported using Tariff Item from Country/Region of Origin for first time";
					break;
				case "198":
					retVal = "Tariff Item : Goods exported using Tariff Item to Country/Region of Destination for first time";
					break;
				case "199":
					retVal = "Tariff Item : Not specified or invalid";
					break;
				case "200":
					retVal = "Tariff Item : Not on file";
					break;
				case "201":
					retVal = "Tariff Item : Not current";
					break;
				case "202":
					retVal = "Prohibited Code : Invalid";
					break;
				case "203":
					retVal = "Unit Quantity : Unit value outside expected value";
					break;
				case "204":
					retVal = "Unit of Measurement: Not required";
					break;
				case "205":
					retVal = "Unit of Measurement: Invalid";
					break;
				case "206":
					retVal = "Unit Quantity: Exceeds 99,999,999 kg";
					break;
				case "207":
					retVal = "Unit Quantity : Invalid";
					break;
				case "208":
					retVal = "Unit Quantity: When in tonnes must be in range 0 to 149,999";
					break;
				case "209":
					retVal = "Unit Quantity: When Other Information is PTS, quantity must be zero";
					break;
				case "210":
					retVal = "Unit Quantity: Exceeds 500,000 kg";
					break;
				case "213":
					retVal = "Tariff Item : Alternative Tariff Item not on file";
					break;
				case "214":
					retVal = "Tariff Item : Alternative Tariff Item not current";
					break;
				case "215":
					retVal = "Concession Code : Approval item and no valid concession quoted";
					break;
				case "245":
					retVal = "Exchange Rate : Not specified or invalid (Make sure your EDI Transmit Date is set to the First Date this Job was Submitted to Customs)";
					break;
				case "246":
					retVal = "Other Information Data : Not specified or invalid";
					break;
				case "247":
					retVal = "Other Information Code : Not on file";
					break;
				case "248":
					retVal = "LOU - single use LOU has already been used";
					break;
				case "249":
					retVal = "Tariff Item incompatible with LOU quoted";
					break;
				case "250":
					retVal = "Value exceeds limit for LOU quoted";
					break;
				case "251":
					retVal = "Statistical Quantity : Not specified";
					break;
				case "252":
					retVal = "Other Information code not valid for this entry type";
					break;
				case "270":
					retVal = "Tariff Item : Incorrect for threshold rate claimed";
					break;
				case "271":
					retVal = "Duty Payable : Not specified or invalid";
					break;
				case "272":
					retVal = "Duty Payable : Duty free when re-exported from Niue, Tokelau or Cook Islands";
					break;
				case "273":
					retVal = "Duty Payable : Duty as been determined as free";
					break;
				case "274":
					retVal = "Duty Payable : Could not be calculated - No rates found";
					break;
				case "275":
					retVal = "Tariff Item : Alternative Tariff Item applies";
					break;
				case "277":
					retVal = "Duty Payable : Manual calculation required by Customs";
					break;
				case "278":
					retVal = "Duty Payable : Preferential Rates claimed, but does not qualify";
					break;
				case "279":
					retVal = "Duty Payable : Warning in Calculation";
					break;
				case "280":
					retVal = "Total Value in NZ Dollars : Not specified or invalid";
					break;
				case "282":
					retVal = "Total Duty : Not specified or invalid";
					break;
				case "283":
					retVal = "Total ALAC Levies : Not specified or invalid";
					break;
				case "284":
					retVal = "Total ALAC Levies : Error in calculation";
					break;
				case "285":
					retVal = "Total Steel Levies : Not specified or invalid";
					break;
				case "286":
					retVal = "Total Steel Levies : Error in calculation";
					break;
				case "287":
					retVal = "Total Excise Duty Credits : Error in calculation";
					break;
				case "288":
					retVal = "Total Amount : Not specified or invalid";
					break;
				case "289":
					retVal = "Total Amount : Error in calculation";
					break;
				case "290":
					retVal = "Total Excise Duty Credits : Not specified or invalid";
					break;
				case "291":
					retVal = "Deposit Refund : Not specified or invalid";
					break;
				case "292":
					retVal = "Tariff Item : Manual calculation required by Customs - Check alternative Tariff Item";
					break;
				case "293":
					retVal = "Total Credit Amount : Excise Duty Credits or Deposit Refund not allowed on this type of entry";
					break;
				case "294":
					retVal = "Total Credit Amount : Less than Total Amount Payable";
					break;
				case "334":
					retVal = "Other Information Code : Duplicates not allowed";
					break;
				case "364":
					retVal = "ALAC Levy : Required for Tariff Item";
					break;
				case "365":
					retVal = "Steel Levy : Required for Tariff Item";
					break;
				case "366":
					retVal = "ALAC Levy : Error in calculation";
					break;
				case "367":
					retVal = "Steel Levy : Error in calculation";
					break;
				case "368":
					retVal = "ACC Levy : Required for Tariff Item";
					break;
				case "369":
					retVal = "ACC Levy : Error in calculation";
					break;
				case "375":
					retVal = "Total Amount : Diplomatic entry - not required";
					break;
				case "376":
					retVal = "Total Amount : Memorandum/Letter of Understanding or Deed of Covenant - Not required";
					break;
				case "377":
					retVal = "Value in NZ Dollars : Error in calculation";
					break;
				case "378":
					retVal = "Duty Payable : Error in calculation";
					break;
				case "379":
					retVal = "Excise Duty Credit : Not allowed for this type of entry";
					break;
				case "380":
					retVal = "Excise Duty Credit: Value may not exceed Excise Duty";
					break;
				case "381":
					retVal = "Total Value in NZ Dollars : Error in calculation";
					break;
				case "382":
					retVal = "Total Duty : Error in calculation";
					break;
				case "383":
					retVal = "Total Anti-Dumping Duties : Not specified or invalid";
					break;
				case "384":
					retVal = "Total Anti-Dumping Duties : Error in calculation";
					break;
				case "385":
					retVal = "Total Countervailing Duties : Not specified or invalid";
					break;
				case "386":
					retVal = "Total Countervailing Duties : Error in calculation";
					break;
				case "387":
					retVal = "Total ACC Levies : Not Specified or Invalid.";
					break;
				case "388":
					retVal = "Total ACC Levies : Error in Calculation";
					break;
				case "400":
					retVal = "Total GST : Not specified or invalid";
					break;
				case "401":
					retVal = "Total GST : Error in calculation";
					break;
				case "402":
					retVal = "Total GST : Diplomatic entry - No GST payable";
					break;
				case "403":
					retVal = "Total GST : Memorandum/Letter of Understanding or Deed of Covenant - No GST Payable";
					break;
				case "404":
					retVal = "GST Payable : Not specified or invalid";
					break;
				case "405":
					retVal = "GST Payable : Error in calculation";
					break;
				case "406":
					retVal = "GST Payable : Diplomatic entry - No GST payable";
					break;
				case "426":
					retVal = "Tariff Item : Exempt GST - no GST payable";
					break;
				case "436":
					retVal = "Entry Period : Not specified or invalid";
					break;
				case "437":
					retVal = "Entry Period : Entry not expected for this period";
					break;
				case "444":
					retVal = "Total Amount: Not required - Goods being delivered to a duty exempt CCA";
					break;
				case "455":
					retVal = "Entry Period : Entry already exists for this period";
					break;
				case "457":
					retVal = "Exchange Rate : No longer held for lodgement date";
					break;
				case "458":
					retVal = "Currency Code : Not specified or invalid";
					break;
				case "459":
					retVal = "Exchange Rate Indicator: Not specified or invalid";
					break;
				case "460":
					retVal = "Authentication Result : PIN Failure";
					break;
				case "462":
					retVal = "Voyage Number : Not required for air or mail";
					break;
				case "474":
					retVal = "Invoice Terms : Not specified or invalid";
					break;
				case "475":
					retVal = "Preference Indicator : Not specified or invalid";
					break;
				case "476":
					retVal = "Delivery Authority Code : Not current";
					break;
				case "477":
					retVal = "Delivery Authority Code : Not on file";
					break;
				case "478":
					retVal = "Other Information Code : Invalid on Detail Line";
					break;
				case "479":
					retVal = "Delivery Authority Code : Invalid";
					break;
				case "481":
					retVal = "Container Number : Not specified or invalid";
					break;
				case "482":
					retVal = "Container Status : Not specified or invalid";
					break;
				case "483":
					retVal = "Relationship Indicator : Not specified or invalid";
					break;
				case "484":
					retVal = "Permit Authority Code : Not current";
					break;
				case "485":
					retVal = "Permit Authority Number : Invalid format";
					break;
				case "486":
					retVal = "Port of Loading : Not current" + portMessage;
					break;
				case "487":
					retVal = "Client Reference Number : Duplicates not allowed";
					break;
				case "488":
					retVal = "Processing Port : Not specified or invalid";
					break;
				case "489":
					retVal = "Port of Loading : Not a recognised port for air consignment" + portMessage;
					break;
				case "490":
					retVal = "Port of Discharge : Not a recognised port for air consignment" + portMessage;
					break;
				case "491":
					retVal = "Port of Arrival/Departure : Not specified or invalid" + portMessage;
					break;
				case "492":
					retVal = "Port of Arrival/Departure : Not current" + portMessage;
					break;
				case "515":
					retVal = "Total Invoices : The number of invoices received does not match total specified";
					break;
				case "516":
					retVal = "Total Packages : The accumulated number of packages does not match total specified";
					break;
				case "517":
					retVal = "Total Detail Lines : The number of lines received does not match total specified";
					break;
				case "518":
					retVal = "Total Packages : No shipping details present";
					break;
				case "519":
					retVal = "Invoice Number : Not specified";
					break;
				case "520":
					retVal = "Date Of Import : Entry lodged more than 20 days after importation";
					break;
				case "521":
					retVal = "Date of Import : Entry lodged too many days prior to import";
					break;
				case "522":
					retVal = "Date of Arrival/Departure : Not specified or invalid";
					break;
				case "523":
					retVal = "Override Indicator : Entry force accepted - Customs to investigate and correct";
					break;
				case "524":
					retVal = "Total Containers : The number of containers does not match total specified";
					break;
				case "525":
					retVal = "Total Consignments : The number of consignments received does not match total specified";
					break;
				case "543":
					retVal = "Other Information Code : Invalid on Header";
					break;
				case "544":
					retVal = "Port of Discharge : Not current" + portMessage;
					break;
				case "546":
					retVal = "Processing Port : Not current" + portMessage;
					break;
				case "548":
					retVal = "Permit Authority Code : Not on file";
					break;
				case "549":
					retVal = "Permit Authority Code : Not specified";
					break;
				case "550":
					retVal = "Permit Authority Number : Not in valid range";
					break;
				case "551":
					retVal = "Permit Authority Number : Cancelled";
					break;
				case "552":
					retVal = "Permit Authority Number : Not specified";
					break;
				case "554":
					retVal = "Total Amount : Credit limit exceeded";
					break;
				case "556":
					retVal = "Goods Description : Not specified";
					break;
				case "558":
					retVal = "Client Reference Number : Not specified";
					break;
				case "572":
					retVal = "Permit Authority Number : Usage incompatible with previous usage";
					break;
				case "582":
					retVal = "Number of Packages : Only one package expected for bulk consignment";
					break;
				case "590":
					retVal = "EDI Adjustment - Refund must be verified by Customs";
					break;
				case "591":
					retVal = "EDI Adjustment - Assessment must be verified by Customs";
					break;
				case "601":
					retVal = "Transaction Number : Not specified or invalid";
					break;
				case "602":
					retVal = "Class of Entry : Not specified or invalid";
					break;
				case "603":
					retVal = "Class of Entry : Not the same as that quoted on original entry";
					break;
				case "604":
					retVal = "Transaction Type : Not specified or invalid";
					break;
				case "605":
					retVal = "Transaction Type : Original entry already exists";
					break;
				case "606":
					retVal = "Transaction Type : Entry not rejected - cannot be replaced";
					break;
				case "607":
					retVal = "Transaction Type : Completion can only be used for TIE or Sight entries";
					break;
				case "608":
					retVal = "Transaction Type : Entry already cancelled";
					break;
				case "609":
					retVal = "Entry Type : Not specified or invalid";
					break;
				case "610":
					retVal = "Remarks : Not specified";
					break;
				case "611":
					retVal = "Detail Line : Line does not exist - cannot be replaced";
					break;
				case "612":
					retVal = "Detail Line : Line does not exist - cannot be cancelled";
					break;
				case "613":
					retVal = "Entry Number : Not specified or invalid";
					break;
				case "614":
					retVal = "Transaction Type : Entry only has one line - cannot be cancelled";
					break;
				case "615":
					retVal = "Detail Line : Line already exists";
					break;
				case "616":
					retVal = "Detail Line : Line already cancelled";
					break;
				case "617":
					retVal = "Total Detail Lines : Maximum of 250 lines accepted on original entry";
					break;
				case "618":
					retVal = "Client Reference Number: Not the same as the quoted on original";
					break;
				case "619":
					retVal = "Entry Type: Not the same as that quoted on original entry";
					break;
				case "620":
					retVal = "Entry Type: Not allowed for class of entry specified";
					break;
				case "621":
					retVal = "Consignment Number : Consignment does not exist - cannot be replaced";
					break;
				case "622":
					retVal = "Consignment Number : Consignment does not exist - cannot be cancelled";
					break;
				case "623":
					retVal = "Consignment Number : Consignment already exists";
					break;
				case "624":
					retVal = "Consignment Number : Consignment already cancelled";
					break;
				case "625":
					retVal = "ECI Number : Not specified or invalid";
					break;
				case "626":
					retVal = "Transaction Type : No original ECI to adjust";
					break;
				case "627":
					retVal = "Transaction Type : ECI already cancelled";
					break;
				case "628":
					retVal = "Transaction Type : ECI only has one consignment - cannot be cancelled";
					break;
				case "629":
					retVal = "Transaction Type : ECI cancelled, cannot be adjusted";
					break;
				case "630":
					retVal = "Transaction Type : Maximum number of 999 lines exceeded - adjustment rejected";
					break;
				case "631":
					retVal = "Transaction Type : Maximum number of 99 versions exceeded - adjustment rejected";
					break;
				case "632":
					retVal = "Consignee Name : Not specified";
					break;
				case "633":
					retVal = "Consignor Name : Not specified";
					break;
				case "634":
					retVal = "Consignee Address : Not specified";
					break;
				case "635":
					retVal = "Consignor Address : Not specified";
					break;
				case "640":
					retVal = "Goods Item Number : Not specified or invalid";
					break;
				case "641":
					retVal = "Goods Item Number : Not Unique";
					break;
				case "650":
					retVal = "Part Full Manifest Indicator : Not specified or invalid";
					break;
				case "651":
					retVal = "Container Number : Container number not within container list";
					break;
				case "652":
					retVal = "Transaction type : Invalid for rejected Entry";
					break;
				case "653":
					retVal = "Place of Final Delivery : Not specified or invalid" + portMessage;
					break;
				case "654":
					retVal = "Place of Final Delivery : Not current" + portMessage;
					break;
				case "655":
					retVal = "Place of Transhipment : Not specified or invalid" + portMessage;
					break;
				case "656":
					retVal = "Place of Transhipment : Not current" + portMessage;
					break;
				case "657":
					retVal = "Port of Delivery : Not specified or invalid" + portMessage;
					break;
				case "658":
					retVal = "Port of Delivery : Not current" + portMessage;
					break;
				case "659":
					retVal = "Container Size/Type Code : Not specified or invalid";
					break;
				case "660":
					retVal = "Attached Equipment Indicator : Must be 'Y' or 'N'";
					break;
				case "661":
					retVal = "Quarantine Indicator : Not specified or invalid";
					break;
				case "662":
					retVal = "Prohibited Packaging Indicator : Must be 'Y' or 'N'";
					break;
				case "663":
					retVal = "Place of Origin : Not specified or invalid" + portMessage;
					break;
				case "664":
					retVal = "Place of Origin : Not current" + portMessage;
					break;
				case "665":
					retVal = "Container Pack Location : Not specified or invalid" + portMessage;
					break;
				case "666":
					retVal = "Container Pack Location : Not current" + portMessage;
					break;
				case "667":
					retVal = "Type of Package : Invalid Cargo Type";
					break;
				case "668":
					retVal = "Type of Package : Invalid Packaging Material";
					break;
				case "669":
					retVal = "EDI Cancellation - Must be verified by a Customs Officer";
					break;
				case "670":
					retVal = "EDI Adjustment to Held entry can only be cleared by an officer of Customs";
					break;
				case "671":
					retVal = "Date of Arrival : ECI lodged too many days prior to import";
					break;
				case "672":
					retVal = "Date of Arrival : ECI lodged too many days after import";
					break;
				case "673":
					retVal = "Date of Departure : ECI lodged too many days prior to export";
					break;
				case "674":
					retVal = "Date of Departure : ECI lodged too many days after export";
					break;
				case "675":
					retVal = "Container Number : Invalid format";
					break;
				case "676":
					retVal = "Craft Name : Not on file" + flightVesselMessage;
					break;
				case "677":
					retVal = "Flight No : Not of file" + flightVesselMessage;
					break;
				case "678":
					retVal = "Clearance Number : Not specified or invalid";
					break;
				case "679":
					retVal = "Detail Line : Not specified or invalid";
					break;
				case "680":
					retVal = "Outward Report Number : Not specified or invalid";
					break;
				case "689":
					retVal = "Seal Number : Not specified or invalid";
					break;
				case "690":
					retVal = "Other Information Code: MCD not supplied";
					break;
				case "691":
					retVal = "Seal Number : SEP not available for LCL containers";
					break;
				case "692":
					retVal = "Goods Description : MPI container release ECIs must show 'MPI CONTAINER MOVEMENT REQUEST'";
					break;
				case "695":
					retVal = "ECI must contain at least one consignment and one goods item";
					break;
				case "696":
					retVal = "Other Information Code: MCD declaration only valid on FCL, Empty, and Bulk containers";
					break;
				case "697":
					retVal = "Other Information Code specified not for this client code";
					break;
				case "698":
					retVal = "Other Information Code: ATF not supplied";
					break;
				case "699":
					retVal = "Other Information Data: Invalid ATF";
					break;
				case "701":
					retVal = "Entry requires verification by a Customs Officer";
					break;
				case "997":
					retVal = "Customs Processing Error - EDI Interface failures";
					break;
				case "998":
					retVal = "Customs Processing Error - Transaction 'broken twice' in pipe";
					break;
				case "999":
					retVal = "Customs Processing Error - EDI translation failure";
					break;
			}
			return retVal;
		}

		internal string GetFieldNameFromCode(string fieldCode)
		{
			// This should stay in this parent class. 
			// Field Names are common between ECI Writeoffs and Formal Entries.
			// These have been modified from the original NZ Spec'd text, 
			// and do not want to lose the changes made by Ben Govett.

			string retVal = "Unknown Field Number: " + fieldCode;
			switch (fieldCode)
			{
				case "1":
					retVal = "Transaction Number";
					break;
				case "2":
					retVal = "Entry Number";
					break;
				case "3":
					retVal = "Class of Entry";
					break;
				case "4":
					retVal = "Transaction Type";
					break;
				case "5":
					retVal = "Entry Type";
					break;
				case "15":
					retVal = "ECI Number";
					break;
				case "20":
					retVal = "Client Code";
					break;
				case "21":
					retVal = "Client Name";
					break;
				case "39":
					retVal = "Broker Code";
					break;
				case "41":
					retVal = "Declarant Code";
					break;
				case "44":
					retVal = "Client Reference Number";
					break;
				case "47":
					retVal = "Delivery Authority Code";
					break;
				case "50":
					retVal = "Customs Controlled Area";
					break;
				case "65":
					retVal = "Total Detail Lines";
					break;
				case "70":
					retVal = "Date of Import";
					break;
				case "71":
					retVal = "Entry Period";
					break;
				case "72":
					retVal = "Date of Export";
					break;
				case "75":
					retVal = "Total Invoices";
					break;
				case "80":
					retVal = "Craft/Flight No.";
					break;
				case "81":
					retVal = "Transport Mode";
					break;
				case "85":
					retVal = "Voyage Number";
					break;
				case "90":
					retVal = "Port of Loading";
					break;
				case "95":
					retVal = "Port of Discharge";
					break;
				case "96":
					retVal = "Country/Region of Destination";
					break;
				case "100":
					retVal = "Relationship indicator";
					break;
				case "105":
					retVal = "Processing Port";
					break;
				case "110":
					retVal = "Total Packages";
					break;
				case "115":
					retVal = "Total Gross Weight";
					break;
				case "120":
					retVal = "Total Value in NZ Dollars";
					break;
				case "125":
					retVal = "Total Import Duty (Total Duty, accumulated from import tariff items)";
					break;
				case "127":
					retVal = "Total Excise Duty (Total Duty, Accumulated from excise tariff items) ";
					break;
				case "135":
					retVal = "Total " + GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
					break;
				case "140":
					retVal = "Total ALAC Levy";
					break;
				case "141":
					retVal = "Total HERA (Steel) Levy";
					break;
				case "142":
					retVal = "Total Anti-Dumping Duty";
					break;
				case "143":
					retVal = "Total Countervailing Duty";
					break;
				case "145":
					retVal = "Total Payable (Total Amount on Import and Excise entries)";
					break;
				case "146":
					retVal = "Total Drawback (Total Amount on Export entries)";
					break;
				case "147":
					retVal = "Total Excise Duty Credits (Total Credit Amount on Excise entries)";
					break;
				case "148":
					retVal = "Total Deposit Refund (Total Credit Amount on Completion entries)";
					break;
				case "149":
					retVal = "Total ACC Levy";
					break;
				case "150":
					retVal = "Total PFML";
					break;
				case "152":
					retVal = "Total SGG Levy";
					break;
				case "160":
					retVal = "Method of Payment";
					break;
				case "165":
					retVal = "Override Indicator";
					break;
				case "170":
					retVal = "Remarks";
					break;
				case "175":
					retVal = "Sold/Consignment Indicator";
					break;
				case "200":
					retVal = "Other information Code";
					break;
				case "201":
					retVal = "Other Information Data";
					break;
				case "250":
					retVal = "Permit Authority Code";
					break;
				case "251":
					retVal = "Permit Authority Number";
					break;
				case "299":
					retVal = "Bill Type";
					break;
				case "300":
					retVal = "Bill/Parcel Number";
					break;
				case "301":
					retVal = "Container Number";
					break;
				case "302":
					retVal = "Container Status";
					break;
				case "303":
					retVal = "Number of Packages";
					break;
				case "304":
					retVal = "Type of Package";
					break;
				case "321":
					retVal = "Invoice Number";
					break;
				case "325":
					retVal = "Invoice Terms";
					break;
				case "350":
					retVal = "Detail Line Number";
					break;
				case "355":
					retVal = "Goods Description";
					break;
				case "360":
					retVal = "Tariff Item";
					break;
				case "365":
					retVal = "Supplier Code";
					break;
				case "370":
					retVal = "Concession Code";
					break;
				case "380":
					retVal = "Statistical Unit";
					break;
				case "382":
					retVal = "Supplementary Unit";
					break;
				case "385":
					retVal = "Statistical Quantity";
					break;
				case "387":
					retVal = "Supplementary Quantity";
					break;
				case "400":
					retVal = "Currency Code";
					break;
				case "405":
					retVal = "Value in Currency";
					break;
				case "410":
					retVal = "Exchange Rate";
					break;
				case "411":
					retVal = "Exchange Rate Indicator";
					break;
				case "415":
					retVal = "Value in NZ Dollars";
					break;
				case "420":
					retVal = "Import Duty (applicable to import tariff items)";
					break;
				case "422":
					retVal = "Excise Duty (applicable to excise tariff items)";
					break;
				case "423":
					retVal = "Excise Duty Credits";
					break;
				case "425":
					retVal = "Preference Indicator";
					break;
				case "430":
					retVal = "Country/Region of Origin";
					break;
				case "435":
					retVal = "Country/Region of Export";
					break;
				case "440":
					retVal = "Cost, Insurance & Freight";
					break;
				case "450":
					retVal = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " Payable";
					break;
				case "460":
					retVal = "ALAC Levy";
					break;
				case "461":
					retVal = "HERA (Steel) Levy";
					break;
				case "462":
					retVal = "Anti-Dumping Duty";
					break;
				case "463":
					retVal = "Countervailing Duty";
					break;
				case "465":
					retVal = "ACC Levy";
					break;
				case "470":
					retVal = "Prohibited Code";
					break;
				case "475":
					retVal = "Unit Value";
					break;
				case "500":
					retVal = "Total Consignments";
					break;
				case "505":
					retVal = "Date of Arrival/Departure";
					break;
				case "510":
					retVal = "Full/Part Manifest Indicator";
					break;
				case "515":
					retVal = "Port of Arrival/Departure";
					break;
				case "520":
					retVal = "Consignment Number";
					break;
				case "525":
					retVal = "Carrier Code";
					break;
				case "530":
					retVal = "Carrier Name";
					break;
				case "535":
					retVal = "Consolidator Name";
					break;
				case "540":
					retVal = "Total Containers";
					break;
				case "545":
					retVal = "Consignee Name";
					break;
				case "550":
					retVal = "Consignor Name";
					break;
				case "555":
					retVal = "Contact Party Name";
					break;
				case "560":
					retVal = "Consignee Address";
					break;
				case "565":
					retVal = "Consignor Address";
					break;
				case "570":
					retVal = "Contact Party Address";
					break;
				case "575":
					retVal = "Goods Item Number";
					break;
				case "580":
					retVal = "Gross Weight";
					break;
				case "610":
					retVal = "ECI Type";
					break;
				case "615":
					retVal = "Place of Final Delivery";
					break;
				case "620":
					retVal = "Place of Transhipment";
					break;
				case "625":
					retVal = "Port of Delivery";
					break;
				case "630":
					retVal = "Container Size/Type Code";
					break;
				case "635":
					retVal = "Attached Equipment Indicator";
					break;
				case "640":
					retVal = "Quarantine Indicator";
					break;
				case "645":
					retVal = "Prohibited Packaging Indicator";
					break;
				case "650":
					retVal = "Dangerous Goods Code";
					break;
				case "655":
					retVal = "Dangerous Goods Number";
					break;
				case "660":
					retVal = "Flashpoint";
					break;
				case "665":
					retVal = "Place of Origin";
					break;
				case "670":
					retVal = "Container Pack Location";
					break;
				case "675":
					retVal = "Vehicle Identification Number";
					break;
			}
			return retVal;
		}
		#endregion

		internal abstract string GetMasterBill();

		internal abstract NZCMessage GetLastOutgoingMessage();

		public abstract ZString EntryNumber { get; set; }
		public abstract ZString EntryStatus { get; set; }
		public abstract ZString CustomsDeliveryInstructions { get; set; }

		internal virtual void FinaliseCancellation()
		{
		}
	}
}
