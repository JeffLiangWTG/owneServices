using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.AddNew5106toImporterFileProcessingResults)]
	public class ImporterConsigneeCreateUpdateMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			var organisation = Message.OriginalMessage != null ? (OrgHeader)Message.OriginalMessage.EM_LinkedObject : null;
			Message.EM_LinkedObject = organisation;

			var hasRejected = ZBool.False;
			var importerNumber = ZString.Empty;
			var importerName = ZString.Empty;
			HtmlTableCreator htmlBody = null;
			var emailBodyBuilder = new ZStringBuilder();
			var emailBody = ProcessBlocksAndGenerateEmailBody(ref hasRejected, ref importerNumber, ref importerName);

			if (!importerNumber.IsEmpty)
			{
				var message = ZString.Empty;
				if (!hasRejected)
				{
					message = "The importer number, '" + importerNumber + "' is now registered at Customs.";
				}

				if (organisation == null)
				{
					message += "System could not attach this number to any Organization records." + (importerName.IsEmpty ? "" : " This number however is said to belong to this name, '" + importerName + "'.");
				}

				if (!message.IsEmpty)
				{
					htmlBody = new HtmlTableCreator(new string[] { "Message" });
					htmlBody.WriteRow(message);
				}
			}

			if (organisation != null)
			{
				var orgWrapper = OrgHeaderWrapper.New(organisation);
				SaveImporterNumber(orgWrapper, importerNumber);

				if (!hasRejected)
				{
					orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
				}
				else
				{
					orgWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
				}
			}

			string organizationCode = "Organisation Unknown";
			string uri = "";

			if (organisation != null)
			{
				organizationCode = "Organisation " + organisation.OH_Code;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, organisation.PK.ToGuid());
			}

			if (htmlBody != null)
			{
				emailBodyBuilder.Append(htmlBody.ToHtml());
				emailBodyBuilder.Append("<br>");
			}

			if (!string.IsNullOrEmpty(emailBody))
			{
				emailBodyBuilder.Append(emailBody);
			}
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, organizationCode, "Importer/Consignee Create/Update", emailBodyBuilder.ToString(), hasRejected, branch, organisation);
		}

		#region Implementation

		void SaveImporterNumber(OrgHeaderWrapper organisationWrapper, ZString importerNumber)
		{
			if (!importerNumber.IsEmpty)
			{
				//NN-NNNNNNNXX		EIN (aka IRS) Number
				//NNN-NN-NNNN		Social Security Number
				//YYDDPP-NNNNN		CBP Assigned Number

				if (importerNumber.IndexOf('-') == 6)//CBP Assigned Number
				{
					organisationWrapper.organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, importerNumber, Core.Constants.CountryCodes.UnitedStates);
				}
			}
		}

		#endregion

		string ProcessBlocksAndGenerateEmailBody(ref ZBool hasRejected, ref ZString importerNumber, ref ZString importerName)
		{
			var tableCreator = new HtmlTableCreator();
			tableCreator.EnableHTMLEncoding = false;
			foreach (var messageBlock in Message.MessageBlock.MessageBlocks)
			{
				var e0Block = messageBlock as AICCE0;
				var e1Block = messageBlock as AICCE1;
				if (e0Block != null)
				{
					var extendedSerialisedValues = e0Block.GetExtendedHumanFriendlySerialisedValues(nameof(e0Block.ReferenceDataText));
					if (extendedSerialisedValues != null && extendedSerialisedValues.Any())
					{
						var refDataTypeDes = Factory.GetCachedValue<DrawbackReturnedEntrySummaryReferenceDataTypeList>().GetDescriptionFromCode(e0Block.ReferenceDataTypeCode);
						var rowValue = string.Join(" ", extendedSerialisedValues.Where(x => x.Value.Trim().Length > 0).Select(x => Regex.Replace(x.Title.Trim(), @"\s{1}\(\d+-\d+\)", string.Empty) + ": <strong>" + x.Value.Trim() + "</strong>"));

						tableCreator.WriteRowWithFormatting(new CellWithFormatting(refDataTypeDes, new NameValueCollection { { "colspan", "4" }, { "align", "left" } }, true));
						tableCreator.WriteRowWithFormatting(new CellWithFormatting(rowValue, new NameValueCollection { { "colspan", "4" } }));

						if (e0Block.ReferenceDataTypeCode == ImporterConsigneeCreateUpdateReferenceDataTypeList.Codes.IMPACC)
						{
							importerNumber = e0Block.ReferenceDataText.Left(12);
							importerName = e0Block.ReferenceDataText.SubstringSafe(12, 32).Trim();
						}
					}
				}
				else if (e1Block != null)
				{
					if (!hasRejected)
					{
						hasRejected = e1Block.DispositionTypeCode == ACESeverityList.Codes.Error || e1Block.DispositionTypeCode == ACESeverityList.Codes.RejectedError;
					}

					tableCreator.WriteRowWithFormatting(new CellWithFormatting("Disposition", true), new CellWithFormatting("Condition Code", true), new CellWithFormatting("Text", true));
					tableCreator.WriteRow(Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1Block.DispositionTypeCode), e1Block.ConditionCode, e1Block.NarrativeText);
				}
			}

			return tableCreator.ToHtml();
		}
	}
}
