using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse)]
	public class ACEDrawbackSummaryMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			var declaration = OriginalMessageLinker.Link<JobDeclaration>(Message);
			bool hasFailure = false, hasCensusWarning = false;

			var emailBody = ProcessBlocksAndGenerateEmailBody(ref hasFailure, ref hasCensusWarning);

			if (declaration != null)
			{
				CalculateStatus(declaration, hasFailure, hasCensusWarning);
			}

			string url = declaration == null ? "" : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration);
			var branch = declaration != null ? declaration.Branch : null;
			string jobNumber = declaration == null ? "Unknown" : declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "ACE Drawback Summary", emailBody, hasFailure, branch, declaration);
		}

		string ProcessBlocksAndGenerateEmailBody(ref bool hasFailure, ref bool hasCensusWarning)
		{
			var tableCreator = new HtmlTableCreator();
			tableCreator.EnableHTMLEncoding = false;
			foreach (var messageBlock in Message.MessageBlock.MessageBlocks)
			{
				var e0Block = messageBlock as ADRWE0;
				var e1Block = messageBlock as ADRWE1;
				if (e0Block != null)
				{
					var extendedSerialisedValues = e0Block.GetExtendedHumanFriendlySerialisedValues(nameof(e0Block.ReferenceDataText));
					var refDataTypeDes = string.Empty;
					var rowValue = string.Empty;
					if (e0Block.ReferenceDataTypeCode != DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.IMPORT)
					{
						if (extendedSerialisedValues != null && extendedSerialisedValues.Any())
						{
							refDataTypeDes = Factory.GetCachedValue<DrawbackReturnedEntrySummaryReferenceDataTypeList>().GetDescriptionFromCode(e0Block.ReferenceDataTypeCode);
							rowValue = string.Join(" ", extendedSerialisedValues.Where(x => x.Value.Trim().Length > 0).Select(x => Regex.Replace(x.Title.Trim(), @"\s{1}\(\d+-\d+\)", string.Empty) + ": <strong>" + x.Value.Trim() + "</strong>"));
						}
					}
					else
					{
						refDataTypeDes = Factory.GetCachedValue<DrawbackReturnedEntrySummaryReferenceDataTypeList>().GetDescriptionFromCode(e0Block.ReferenceDataTypeCode);
						rowValue = string.Join(" ", extendedSerialisedValues.Where(x => x.Value.Trim().Length > 0).Select(x => Regex.Replace(x.Title.Trim(), @"\s{1}\(\d+-\d+\)", string.Empty) + ": " + x.Value.Trim()));
					}

					tableCreator.WriteRowWithFormatting(new CellWithFormatting(refDataTypeDes, new NameValueCollection { { "colspan", "4" }, { "align", "left" } }, true));
					tableCreator.WriteRowWithFormatting(new CellWithFormatting(rowValue, new NameValueCollection { { "colspan", "4" } }));
				}
				else if (e1Block != null)
				{
					if (!hasFailure)
					{
						hasFailure = e1Block.DispositionTypeCode == ACESeverityList.Codes.Rejected;
					}

					if (!hasCensusWarning)
					{
						hasCensusWarning = e1Block.SeverityCode == ACESeverityList.Codes.CensusWarning;
					}

					tableCreator.WriteRowWithFormatting(new CellWithFormatting("Disposition", true), new CellWithFormatting("Severity", true), new CellWithFormatting("Condition Code", true), new CellWithFormatting("Text", true));
					tableCreator.WriteRow(Factory.GetCachedValue<ACESeverityList>().GetDescriptionFromCode(e1Block.DispositionTypeCode), e1Block.SeverityCode, e1Block.ConditionCode, " <strong>" + e1Block.NarrativeText + "</strong>");
				}
			}

			return tableCreator.ToHtml();
		}

		void CalculateStatus(JobDeclaration declaration, bool isFailure, bool isCensusWarning)
		{
			if (declaration != null)
			{
				var responseStatus = ABIResponseStatus.Cleared;

				if (isFailure)
				{
					responseStatus = ABIResponseStatus.Rejected;
				}
				else if (isCensusWarning)
				{
					responseStatus = ABIResponseStatus.CensusWarning;
				}

				var status = new DrawbackSummaryMessageStatusCalculator().Calculate(Message, responseStatus);
				if (!status.IsEmpty)
				{
					declaration.JE_MessageStatus = status;
				}
			}
		}
	}
}
