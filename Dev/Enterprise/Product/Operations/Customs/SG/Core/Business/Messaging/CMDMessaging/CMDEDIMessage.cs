using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDEDIMessage : CIMEDIMessage
	{
		public new abstract class Schema : CIMEDIMessage.Schema
		{
			public const string EM_MessageSummary = "EM_MessageSummary";
			public const string EM_ReplySummary = "EM_ReplySummary";
		}

		public CMDEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(EM_Status), ConcurrencyPolicy.Observe);
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				if (collection is CMDEDIMessageCollection cmdCollection)
				{
					cmdCollection.Parent.RefreshMessageList();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_MessageType = ApplicationCode;
		}

		protected override ZString ApplicationCode
		{
			get { return EDIMessage.ApplicationCodes.SingaporeCMD; }
		}

		protected override INumberFountainProxy NumberFountain
		{
			get { return Env.NumberFountains.CMDNumber; }
		}

		#region EM_MessageSummary

		public ZString EM_MessageSummary
		{
			get { return GetMessageSummary(); }
		}

		public ZPropertyInfo EM_MessageSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.EM_MessageSummary); }
		}

		ZString GetMessageSummary()
		{
			ZString result;

			CMDParser parser = new CMDParser(EM_MessageText);
			if (parser.IsValid)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("Action: {0}\r\n", parser.ActionCode.ToString());
				builder.AppendFormat("Late Submission: {0}\r\n", ((ZBool)parser.IsLate).ToString());
				builder.AppendFormat("Sent To: {0}\r\n", EM_MessageSubType);
				builder.AppendFormat("MAWB: {0}\r\n", parser.MasterBillNumber.ToString());
				builder.AppendFormat("HAWB: {0}\r\n", parser.HouseBillNumber.ToString());
				result = builder.ToString();
			}
			else
			{
				result = "Unable to parse CMD Message Text. Invalid or unexpected format / structure. Message starts: " + EM_MessageText.SubstringSafe(0, 50);
			}

			return result;
		}

		#endregion

		#region MessageReply

		#region EM_ReplySummary

		public ZString EM_ReplySummary
		{
			get { return GetMessageReplySummary(); }
		}

		public ZPropertyInfo EM_ReplySummaryInfo
		{
			get { return GetZPropertyInfo(Schema.EM_ReplySummary); }
		}

		#endregion

		public CMDInbound Reply
		{
			get
			{
				if (reply == null)
				{
					ZQuery filter = new ZQuery(EDIMessageSchema.EM_LinkTable, EDIMessageSchema.Constants.TableName);
					filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.SingaporeCMD);
					filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
					filter.AddToFilter(EDIMessageSchema.EM_IsActive, ZBool.True);
					filter.OrderBy = EDIMessageSchema.Constants.EM_MessageNum + " DESC";
					var message = Factory.LoadTop1<CMDEDIMessage>(filter);

					if (message != null)
					{
						reply = new CMDInbound(message.EM_MessageText);
					}
				}

				return reply;
			}
#if DEBUG
			set { reply = value; }
#endif
		}

		ZString GetMessageReplySummary()
		{
			ZString result;

			if (Reply != null && Reply.IsValid)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("Sender: {0}\r\n", Reply.Sender);
				builder.AppendFormat("Error: {0}\r\n\r\n", ((ZBool)Reply.IsErrorMessage).ToString());
				builder.Append("REPLY:\r\n");
				builder.Append(Reply.MessageTextWithoutRoutingInfo);
				result = builder.ToString();
			}
			else
			{
				result = "REPLY: None";
			}

			return result;
		}

		public bool IsActiveAndHasFailureReply
		{
			get { return EM_IsActive && Reply != null && Reply.IsErrorMessage; }
		}

		CMDInbound reply;

		#endregion

		#region Overrides

		public override ZString EM_MessageText
		{
			get { return base.EM_MessageText; }
			set
			{
				if (value != base.EM_MessageText)
				{
					base.EM_MessageText = value;
					EM_MessageSummaryInfo.RefreshBinding();
				}
			}
		}

		#endregion
	}
}
