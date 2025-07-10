using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFStatusAdvisoryMessageParser
	{
		public ISFStatusAdvisoryMessageParser(IImporterSecurityFiling parent, MessageBlock[] messageBlocks)
		{
			ParseData(parent, messageBlocks);
		}

		public ZString HtmlData
		{
			get { return htmlData; }
		}
		ZString htmlData;

		public ZString HtmlMessageDataOnly
		{
			get { return htmlMessageDataOnly; }
		}
		ZString htmlMessageDataOnly;

		public ZString JobReference
		{
			get { return jobReference; }
		}
		ZString jobReference;

		public ZString URI
		{
			get { return uri; }
		}
		ZString uri;

		void ParseData(IImporterSecurityFiling parent, MessageBlock[] messageBlocks)
		{
			ISFSA10 isfsa10 = messageBlocks[0] as ISFSA10 ?? throw new InvalidMessageFormatException("First record is not a 'ISFSA10' record.");

			HtmlTableCreator creator = null;
			HtmlTableCreator userDefinedReferenceDetailCreator = null;
			StringBuilder html = new StringBuilder();

			html.Append(string.Format("Transaction Number {0}<br /><br />", isfsa10.ISFTransactionNumber));

			ISFSA20 isfsa20 = null;
			ISFSA30 isfsa30 = null;
			ISFSA50 isfsa50 = null;
			foreach (MessageBlock block in messageBlocks)
			{
				isfsa20 = block as ISFSA20;
				if (isfsa20 != null)
				{
					if (isfsa20.CodeQualifier == ReferenceDataCodeList.Codes.UserDefinedReferenceNumber)
					{
						if (userDefinedReferenceDetailCreator == null)
						{
							userDefinedReferenceDetailCreator = new HtmlTableCreator(GetColumnTitlesSA20());
						}
						userDefinedReferenceDetailCreator.WriteRow(isfsa20.ReferenceData);
					}
				}
				else if (block is ISFSA30)
				{
					isfsa30 = (ISFSA30)block;
					isfsa50 = null;
				}
				else if (block is ISFSA50 && isfsa30 != null)
				{
					isfsa50 = (ISFSA50)block;
					if (creator == null)
					{
						creator = new HtmlTableCreator(GetColumnTitles());
					}
					AddColumnValues(creator, isfsa30, isfsa50);
				}
			}

			ZStringBuilder htmlMessageDataBuilder = new ZStringBuilder();
			if (userDefinedReferenceDetailCreator != null)
			{
				string userDefinedDetail = userDefinedReferenceDetailCreator.ToHtml();
				htmlMessageDataBuilder.Append(userDefinedDetail);
			}
			if (creator != null)
			{
				string messageDetail = creator.ToHtml();
				htmlMessageDataBuilder.Append(messageDetail);
			}
			htmlMessageDataOnly = htmlMessageDataBuilder.ToStringWithDelimiterBetweenAppends("<br /><br />");
			html.Append(htmlMessageDataOnly);

			if (parent != null)
			{
				jobReference = ((BusinessObject)parent).HumanReadableName;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, parent.PK.ToGuid());
			}
			else
			{
				jobReference = "Unknown";
				uri = "";
			}
			htmlData = html.ToString();
		}

		void AddColumnValues(HtmlTableCreator creator, ISFSA30 isfsa30, ISFSA50 isfsa50)
		{
			creator.WriteRow(isfsa30.BillNumber, isfsa50.DispositionCode, List.GetDescriptionFromCode(isfsa50.DispositionCode) ?? isfsa50.Remarks);
		}

		DispositionCodeList List
		{
			get { return list ?? (list = new DispositionCodeList()); }
		}
		DispositionCodeList list;

		IEnumerable<string> GetColumnTitles()
		{
			return new string[] { "Bill Number", "Disposition Code", "Remarks" };
		}

		IEnumerable<string> GetColumnTitlesSA20()
		{
			return new string[] { "User-defined Reference Number" };
		}
	}
}
