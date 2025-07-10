using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.GBIReferenceStatusUpdate)]
	public class GlobalBusinessIdentifierStatusNotificationProcessor : ACEABIProcessor
	{
		const string GBIIsValid = "010";
		const string GBIRejected = "002";

		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierNotification;

			GlobalBusinessIdentifierData gbiData = null;
			var emailBodyBuilder = new ZStringBuilder();
			var branch = GlbBranch.CurrentBranch;
			var organizationCode = "Unknown";
			var url = string.Empty;

			var branchAndOverallStatus = GetGlobalBusinessIdentifiersAndEmailBodyAndBranchAndOverallStatus(Factory, messageBlocks, emailBodyBuilder, Message, null);
			var isFailure = branchAndOverallStatus.OverallStatus == GBIRejected;
			gbiData = branchAndOverallStatus.GBIData;
			if (gbiData != null)
			{
				gbiData.Wrapper.Messages.Add(Message);
				var selectedAddress = gbiData.AddressDetails;

				void MarkOrgCusCodeVerifiedOrUnVerified(ZString codeType, ZBool verified)
				{
					var matchedOrgCusCode = gbiData.Organization.CustomsCodes.GetOrgCusCode(codeType, Core.Constants.CountryCodes.UnitedStates, selectedAddress.PK);
					if (matchedOrgCusCode != null)
					{
						matchedOrgCusCode.MarkOrgCusCodeVerifiedOrUnVerified(verified, "CBP", Message.EM_MessageText);
					}
				}

				MarkOrgCusCodeVerifiedOrUnVerified(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, branchAndOverallStatus.DUNVerified);
				MarkOrgCusCodeVerifiedOrUnVerified(OrgCusCode.USACodeTypes.GlobalLocationNumber, branchAndOverallStatus.GLNVerified);
				MarkOrgCusCodeVerifiedOrUnVerified(OrgCusCode.USACodeTypes.LegalEntityIdentifier, branchAndOverallStatus.LEIVerified);

				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(gbiData.Wrapper);
				organizationCode = gbiData.Organization.OH_Code;
				branch = branchAndOverallStatus.Branch;
				gbiData.GBIStatus = branchAndOverallStatus.OverallStatus;
			}

			if (new HtmlResponseEmailGenerator().TryGenerateEmail(url, organizationCode, "Global Business Identifier Status Notification", emailBodyBuilder.ToString(), "", isFailure, out EmailDef email, branch))
			{
				var registryItem = GetEmailGroupRegistryItem() as ManifestGroupNotificationRegistryItem;
				var groupNotification = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

				var shouldSendErrorNotificationsOnly = groupNotification.SendErrorOnly;
				if (!shouldSendErrorNotificationsOnly || (shouldSendErrorNotificationsOnly && isFailure))
				{
					var recipientList = new List<ZString>();

					if (gbiData?.Wrapper is OrgHeaderWrapper wrapper)
					{
						var outgoingGEMessages = wrapper.Messages.OfType<MQEDIMessage>().Where(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.USCustomsImport && x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete).OrderByDescending(m => m.EM_SystemCreateTimeUtc);

						foreach (var message in outgoingGEMessages)
						{
							var staff = message.UserWhoQueuedThisRecord;
							if (staff != null && !staff.GS_EmailAddress.IsEmpty && !recipientList.Contains(staff.GS_EmailAddress))
							{
								recipientList.Add(staff.GS_EmailAddress);
							}
						}
					}

					var recipientCalculator = new EmailRecipientCalculator(groupNotification.SendMode, groupNotification.SendGroupPK, recipientList.ToArray(), ZGuid.Empty);
					recipientCalculator.SendNotifications(Factory, email, registryItem);
				}
			}
		}

		static GlobalBusinessIdentifierData GetGlobalBusinessIdentifierDataUsingRegistrationNumbers(BusinessObjectFactory factory, ZString dunNum, ZString glnNum, ZString leiNum, OrgHeader orgHeader)
		{
			GlobalBusinessIdentifierData GetMatchedGlobalBusinessIdentifierData(OrgHeader header)
			{
				var data = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(header));
				foreach (var address in header.Addresses)
				{
					data.US_OA_AddressDetails = address.PK;
					if (data.US_DUNS.EqualsIgnoringCase(dunNum) && data.US_GLN.EqualsIgnoringCase(glnNum) && data.US_LEI.EqualsIgnoringCase(leiNum))
					{
						return data;
					}
				}

				return null;
			}

			if (!dunNum.IsEmpty || !glnNum.IsEmpty || !leiNum.IsEmpty)
			{
				if (orgHeader != null)
				{
					return GetMatchedGlobalBusinessIdentifierData(orgHeader);
				}
				else
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					AddCusCodeFilter(query, OrgCusCode.CodeTypes.DataUniversalNumberingSystem, dunNum);
					AddCusCodeFilter(query, OrgCusCode.USACodeTypes.GlobalLocationNumber, glnNum);
					AddCusCodeFilter(query, OrgCusCode.USACodeTypes.LegalEntityIdentifier, leiNum);

					var orgs = factory.Load<OrgHeader>(query);
					foreach (var org in orgs)
					{
						var gbiData = GetMatchedGlobalBusinessIdentifierData(org);
						if (gbiData != null)
						{
							return gbiData;
						}
					}
				}
			}

			return null;
		}

		static void AddCusCodeFilter(ZDBOnlyQuery query, ZString codeType, ZString regNo)
		{
			if (!regNo.IsEmpty)
			{
				var orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regNo);
				orgCusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				var orgCusCodeValiditySubQuery = new ZDBOnlySubQuery(typeof(OrgCusCodeValidity), OrgCusCodeValiditySchema.OCV_OK_OrgCusCode);
				orgCusCodeSubQuery.AddSubQuery(orgCusCodeValiditySubQuery, JoinCondition.And);
				query.AddSubQuery(orgCusCodeSubQuery, JoinCondition.And);
			}
		}

		public static (GlbBranch Branch, GlobalBusinessIdentifierData GBIData, ZString DUN, ZBool DUNVerified, ZString GLN, ZBool GLNVerified, ZString LEI, ZBool LEIVerified, ZString OverallStatus) GetGlobalBusinessIdentifiersAndEmailBodyAndBranchAndOverallStatus(BusinessObjectFactory factory, List<MessageBlock> messageBlocks, ZStringBuilder emailBodyBuilder, EDIMessage goMessage, OrgHeader orgHeader)
		{
			var overallStatus = ZString.Empty;
			var dunNum = ZString.Empty;
			var glnNum = ZString.Empty;
			var leiNum = ZString.Empty;

			var dunVerified = false;
			var glnVerified = false;
			var leiVerified = false;

			var messageDetails = new HtmlTableCreator(new string[] { "GBI Ref. ID Qualifier", "GBI Ref. ID", "Disposition Code", "Description" });
			var overallDetails = new HtmlTableCreator(new string[] { "GBI Overall Status", "Description" });
			var enumerator = messageBlocks.GetEnumerator();

			var lastProcessedReferenceIdentifierQualifier = ZString.Empty;
			var lastProcessedReferenceIdentifier = ZString.Empty;

			while (enumerator.MoveNext())
			{
				var block1 = enumerator.Current;
				if (block1 is AGO30 aGO30)
				{
					switch (lastProcessedReferenceIdentifierQualifier)
					{
						case OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier:
							dunVerified = aGO30.DispositionActionCode == GBIIsValid;
							break;
						case OrgCusCode.USACodeTypes.GlobalLocationNumber:
							glnVerified = aGO30.DispositionActionCode == GBIIsValid;
							break;
						case OrgCusCode.USACodeTypes.LegalEntityIdentifier:
							leiVerified = aGO30.DispositionActionCode == GBIIsValid;
							break;
						default:
							break;
					}

					messageDetails.WriteRow(lastProcessedReferenceIdentifierQualifier, lastProcessedReferenceIdentifier, aGO30.DispositionActionCode, aGO30.NarrativeMessage);
					lastProcessedReferenceIdentifierQualifier = ZString.Empty;
					lastProcessedReferenceIdentifier = ZString.Empty;
				}
				else
				{
					if (!lastProcessedReferenceIdentifierQualifier.IsEmpty && !lastProcessedReferenceIdentifier.IsEmpty)
					{
						messageDetails.WriteRow(lastProcessedReferenceIdentifierQualifier, lastProcessedReferenceIdentifier, ZString.Empty, string.Empty);
					}

					if (block1 is AGO20 aGO20)
					{
						lastProcessedReferenceIdentifierQualifier = aGO20.ReferenceIdentifierQualifier;
						lastProcessedReferenceIdentifier = aGO20.ReferenceIdentifier;

						switch (lastProcessedReferenceIdentifierQualifier)
						{
							case OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier:
								dunNum = aGO20.ReferenceIdentifier;
								break;
							case OrgCusCode.USACodeTypes.GlobalLocationNumber:
								glnNum = aGO20.ReferenceIdentifier;
								break;
							case OrgCusCode.USACodeTypes.LegalEntityIdentifier:
								leiNum = aGO20.ReferenceIdentifier;
								break;
							default:
								break;
						}
					}
					else if (block1 is AGO90 aGO90)
					{
						overallStatus = aGO90.DispositionActionCode;
						overallDetails.WriteRow(aGO90.DispositionActionCode, aGO90.NarrativeMessage);
					}
					else if (goMessage != null)
					{
						ErrorReporter.ReportOnce("Unrecognized message block for GO message", $"Message PK: {goMessage.PK}, Block Characters: {block1.Serialise()}");
					}
				}
			}

			ZString AppendGBIIdentifierString(List<AGE20> ge20Blocks, ZString identifierQualifier)
			{
				var identifierFromMatchedGE20 = ge20Blocks.FirstOrDefault(x => x.GBIIdentifierQualifier == identifierQualifier)?.GBIIdentifier ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails(identifierQualifier, identifierFromMatchedGE20);
				return identifierFromMatchedGE20;
			}

			void AppendGlobalBusinessIdentifierDetails(ZString caption, ZString stringToAppend)
			{
				emailBodyBuilder.Append($"{caption} : {stringToAppend} <br />");
			}

			var globalBusinessIdentifierData = GetGlobalBusinessIdentifierDataUsingRegistrationNumbers(factory, dunNum, glnNum, leiNum, orgHeader);
			var lastOutgoingGBIMessage = globalBusinessIdentifierData != null ? globalBusinessIdentifierData.Wrapper.Messages.OfType<MQEDIMessage>()
					.Where(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.USCustomsImport
							&& x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete
							&& x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
							&& x.EM_Status == EDIMessage.Status.Sent
							&& x.EM_MessageText.Contains($"{OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier}{globalBusinessIdentifierData.US_DUNS.ToUpper()}")
							&& x.EM_MessageText.Contains($"{OrgCusCode.USACodeTypes.GlobalLocationNumber} {globalBusinessIdentifierData.US_GLN.ToUpper()}")
							&& x.EM_MessageText.Contains($"{OrgCusCode.USACodeTypes.LegalEntityIdentifier} {globalBusinessIdentifierData.US_LEI.ToUpper()}")).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault() : null;

			if (globalBusinessIdentifierData == null)
			{
				ErrorReporter.ReportOnce("Cannot find global business identifier data.", $"DUNS: {dunNum}, GLN: {glnNum}, LEI: {leiNum}");
			}
			else if (lastOutgoingGBIMessage == null)
			{
				ErrorReporter.ReportOnce("Cannot find matched outgoing GE message", $"Organization: {globalBusinessIdentifierData.Organization.OH_Code}, DUNS: {globalBusinessIdentifierData.US_DUNS}, GLN: {globalBusinessIdentifierData.US_GLN}, LEI: {globalBusinessIdentifierData.US_LEI}");
			}
			else
			{
				var ge20MessageBlocks = lastOutgoingGBIMessage.GetMessageBlocks<AGE20>();
				var dataUniversalNumberingSystem = AppendGBIIdentifierString(ge20MessageBlocks, OrganisationDetails.DataUniversalNumberingSystemCodeInGlobalBusinessIdentifier);
				var globalLocationNumber = AppendGBIIdentifierString(ge20MessageBlocks, OrgCusCode.USACodeTypes.GlobalLocationNumber);
				var legalEntityIdentifier = AppendGBIIdentifierString(ge20MessageBlocks, OrgCusCode.USACodeTypes.LegalEntityIdentifier);

				emailBodyBuilder.Append("<br />");

				var firmName = lastOutgoingGBIMessage.GetMessageBlocks<AGE10>()[0]?.EntityName ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails("Firm Name", firmName);

				var addressLine1 = lastOutgoingGBIMessage.GetMessageBlocks<AGE30>().FirstOrDefault()?.EntityAddressLine1 ?? ZString.Empty;
				var addressLine2 = lastOutgoingGBIMessage.GetMessageBlocks<AGE31>().FirstOrDefault()?.EntityAddressLine2 ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails("Street Address", string.Join(",", addressLine1, addressLine2));

				var ge32 = lastOutgoingGBIMessage.GetMessageBlocks<AGE32>().FirstOrDefault();
				AppendGlobalBusinessIdentifierDetails("City", ge32?.City ?? ZString.Empty);
				AppendGlobalBusinessIdentifierDetails("State", ge32?.StateProvince ?? ZString.Empty);
				emailBodyBuilder.Append("<br />");
				AppendGlobalBusinessIdentifierDetails("Zip/Postal Code", ge32?.PostalCode ?? ZString.Empty);
				AppendGlobalBusinessIdentifierDetails("ISO Country Code", ge32?.Country ?? ZString.Empty);
				emailBodyBuilder.Append("<br />");

				var phoneNumber = lastOutgoingGBIMessage.GetMessageBlocks<AGE40>().FirstOrDefault()?.PhoneNumber ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails("Phone", phoneNumber);
				var webSite = lastOutgoingGBIMessage.GetMessageBlocks<AGE41>().FirstOrDefault()?.Website ?? ZString.Empty;
				AppendGlobalBusinessIdentifierDetails("Website URL", webSite);
				emailBodyBuilder.Append("<br />");

				string isManufacturer = ZString.Empty;
				string isShipper = ZString.Empty;
				string isSeller = ZString.Empty;
				string isExporter = ZString.Empty;
				string isPackager = ZString.Empty;
				string isDistributor = ZString.Empty;

				var ge21 = lastOutgoingGBIMessage.GetMessageBlocks<AGE21>().FirstOrDefault();
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
			}

			emailBodyBuilder.Append(overallDetails.ToHtml());
			emailBodyBuilder.Append("<br />");
			emailBodyBuilder.Append(messageDetails.ToHtml());
			emailBodyBuilder.Append("<br />");

			return (lastOutgoingGBIMessage?.Branch ?? GlbBranch.CurrentBranch, globalBusinessIdentifierData, dunNum, dunVerified, glnNum, glnVerified, leiNum, leiVerified, overallStatus);
		}
	}
}
