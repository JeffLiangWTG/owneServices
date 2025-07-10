using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDeleteResponse)]
	public class GlobalBusinessIdentifierMessageProcessor : ACEABIProcessor
	{
		const string MessageRejected = "01";
		const string MessageAccepted = "02";
		const string RecordRejected = "11";

		public override void Process()
		{
			var emailBodyBuilder = new ZStringBuilder();
			var globalIdentifiers = GetGlobalIdentifiersAndEmailBody(emailBodyBuilder);
			var submissionResult = GetLastMessageBlock<AGE90>().MessageTypeCode;

			var branch = GlbBranch.CurrentBranch;
			var organizationCode = "Unknown";
			var url = string.Empty;

			var originalMessage = Message.OriginalMessage;
			var linkedOrganisation = originalMessage?.EM_LinkedObject as OrgHeader;
			if (linkedOrganisation != null)
			{
				branch = originalMessage.Branch;
				var orgHeaderWrapper = OrgHeaderWrapper.New(linkedOrganisation);
				organizationCode = linkedOrganisation.OH_Code;
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(orgHeaderWrapper);
				orgHeaderWrapper.Messages.Add(Message);

				OrgAddress addressMatchedGlobalBusinessIdentifiers = null;
				foreach (OrgAddress address in linkedOrganisation.Addresses)
				{
					var dunsFromAddress = linkedOrganisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates, address.PK);
					var gbiFromAddress = linkedOrganisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.GlobalLocationNumber, Core.Constants.CountryCodes.UnitedStates, address.PK);
					var leiFromAddress = linkedOrganisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Core.Constants.CountryCodes.UnitedStates, address.PK);

					if (dunsFromAddress.EqualsIgnoringCase(globalIdentifiers.DataUniversalNumberingSystem) && gbiFromAddress.EqualsIgnoringCase(globalIdentifiers.GlobalLocationNumber) && leiFromAddress.EqualsIgnoringCase(globalIdentifiers.LegalEntityIdentifier))
					{
						addressMatchedGlobalBusinessIdentifiers = address;
						break;
					}
				}

				if (addressMatchedGlobalBusinessIdentifiers != null)
				{
					var galobalBusinessIdentifierData = new GlobalBusinessIdentifierData(orgHeaderWrapper);
					galobalBusinessIdentifierData.US_OA_AddressDetails = addressMatchedGlobalBusinessIdentifiers.PK;

					var statusCalculator = new GlobalBusinessIdentifierMessageStatusCalculator(galobalBusinessIdentifierData);
					statusCalculator.CalculateStatus(Message, ConvertToResultCode(submissionResult));
				}
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(url, organizationCode, "Global Business Identifier Enrollment", emailBodyBuilder.ToString(), submissionResult == MessageRejected, branch, linkedOrganisation);
		}

		ABIResponseStatus ConvertToResultCode(ZString submissionResult)
		{
			var result = ABIResponseStatus.Undefined;
			if (submissionResult == MessageRejected)
			{
				result = ABIResponseStatus.Rejected;
			}
			else if (submissionResult == MessageAccepted)
			{
				result = ABIResponseStatus.Cleared;
			}
			return result;
		}

		(ZString DataUniversalNumberingSystem, ZString GlobalLocationNumber, ZString LegalEntityIdentifier) GetGlobalIdentifiersAndEmailBody(ZStringBuilder emailBodyBuilder)
		{
			ZString AppendGBIIdentifierString(AGE20[] messageBlocks, ZString identifierQualifier)
			{
				var identifierFromMatchedGE20 = messageBlocks.FirstOrDefault(x => x.GBIIdentifierQualifier == identifierQualifier)?.GBIIdentifier ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails(identifierQualifier, identifierFromMatchedGE20);
				return identifierFromMatchedGE20;
			}

			void AppendGlobalBusinessIdentifierDetails(ZString caption, ZString stringToAppend)
			{
				emailBodyBuilder.Append($"{caption} : {stringToAppend} <br />");
			}

			var ge20MessageBlocks = GetMessageBlocks<AGE20>(3);
			var dataUniversalNumberingSystem = AppendGBIIdentifierString(ge20MessageBlocks, OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier);
			var globalLocationNumber = AppendGBIIdentifierString(ge20MessageBlocks, OrgCusCode.USACodeTypes.GlobalLocationNumber);
			var legalEntityIdentifier = AppendGBIIdentifierString(ge20MessageBlocks, OrgCusCode.USACodeTypes.LegalEntityIdentifier);

			emailBodyBuilder.Append("<br />");

			var firmName = GetFirstMessageBlock<AGE10>()?.EntityName ?? ZString.Empty;
			AppendGlobalBusinessIdentifierDetails("Firm Name", firmName);

			var addressLine1 = GetFirstMessageBlock<AGE30>()?.EntityAddressLine1 ?? ZString.Empty;
			var addressLine2 = GetFirstMessageBlock<AGE31>()?.EntityAddressLine2 ?? ZString.Empty;
			AppendGlobalBusinessIdentifierDetails("Street Address", string.Join(",", addressLine1, addressLine2));

			var ge32 = GetFirstMessageBlock<AGE32>();
			AppendGlobalBusinessIdentifierDetails("City", ge32?.City ?? ZString.Empty);
			AppendGlobalBusinessIdentifierDetails("State", ge32?.StateProvince ?? ZString.Empty);
			emailBodyBuilder.Append("<br />");
			AppendGlobalBusinessIdentifierDetails("Zip/Postal Code", ge32?.PostalCode ?? ZString.Empty);
			AppendGlobalBusinessIdentifierDetails("ISO Country Code", ge32?.Country ?? ZString.Empty);
			emailBodyBuilder.Append("<br />");

			var phoneNumber = GetFirstMessageBlock<AGE40>()?.PhoneNumber ?? ZString.Empty;
			AppendGlobalBusinessIdentifierDetails("Phone", phoneNumber);
			var webSite = GetFirstMessageBlock<AGE41>()?.Website ?? ZString.Empty;
			AppendGlobalBusinessIdentifierDetails("Website URL", webSite);
			emailBodyBuilder.Append("<br />");

			string isManufacturer = ZString.Empty;
			string isShipper = ZString.Empty;
			string isSeller = ZString.Empty;
			string isExporter = ZString.Empty;
			string isPackager = ZString.Empty;
			string isDistributor = ZString.Empty;
			var ge21 = GetFirstMessageBlock<AGE21>();
			if (ge21 != null)
			{
				isManufacturer = ge21.ManufacturerRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
				isShipper = ge21.ShipperRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
				isSeller = ge21.SellerRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
				isExporter = ge21.ExporterRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
				isPackager = ge21.PackagerRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
				isDistributor = ge21.DistributorRole.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;
			}

			var roleDetails = new HtmlTableCreator(new string[] { "Roles", "" });
			roleDetails.WriteRow("Manufacturer", isManufacturer);
			roleDetails.WriteRow("Shipper", isShipper);
			roleDetails.WriteRow("Seller", isSeller);
			roleDetails.WriteRow("Exporter", isExporter);
			roleDetails.WriteRow("Packager", isPackager);
			roleDetails.WriteRow("Distributor", isDistributor);
			emailBodyBuilder.Append(roleDetails.ToHtml());
			emailBodyBuilder.Append("<br />");

			var messageDetails = new HtmlTableCreator(new string[] { "Data Reference", "Condition Code", "Message" });
			foreach (var messageBlock in messageBlocks)
			{
				if (messageBlock is AGE90 aGE90)
				{
					if (aGE90.MessageTypeCode == RecordRejected)
					{
						messageDetails.WriteRow(ZString.Empty, aGE90.MessageIdentifierCode, aGE90.NarrativeMessageText);
					}
				}
				else
				{
					messageDetails.WriteRow(messageBlock.MandatoryCharacters, ZString.Empty, messageBlock.Serialise().Substring(4));
				}
			}
			emailBodyBuilder.Append(messageDetails.ToHtml());
			emailBodyBuilder.Append("<br />");

			return (dataUniversalNumberingSystem, globalLocationNumber, legalEntityIdentifier);
		}
	}
}
