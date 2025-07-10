using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingEDIMessageColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((ZString)((EDIMessage)null).EM_MessageNum);
			AddToDictionaryAsRequired(new ZTextEditColumn(Res.GetString("9f9f8ffb-95be-4829-9138-eebf08251f8b", "Message No."), EDIMessageSchema.Constants.EM_MessageNum) { ColumnKey = WebTracker.Grids.EDIMessage.MessageNo });

			ZBindToChecker.CheckBindTo((ZString)((EDIMessage)null).EM_MessageType);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("db4ef086-7ef5-4aa2-9b88-1730920e0088", "Message Type"), EDIMessageSchema.Constants.EM_MessageType) { ColumnKey = WebTracker.Grids.EDIMessage.MessageType });

			ZBindToChecker.CheckBindTo((ZString)((EDIMessage)null).EM_Status);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a61187f8-a343-4357-81ab-277dff5a23fe", "Status"), EDIMessageSchema.Constants.EM_Status) { ColumnKey = WebTracker.Grids.EDIMessage.Status });

			ZBindToChecker.CheckBindTo((ZDateTime)((EDIMessage)null).EM_SystemCreateTimeUtc);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("6277fe52-c467-4073-857c-1064f8d49475", "Date/Time Created"), EDIMessageSchema.Constants.EM_SystemCreateTimeUtc) { ColumnKey = WebTracker.Grids.EDIMessage.DateTimeCreated });

			ZBindToChecker.CheckBindTo((ZString)((EDIMessage)null).EM_FormattedMessageText);
			var messageTextColumn = new ZNewRowColumn(EDIMessage.Schema.EM_FormattedMessageText)
			{
				Collapsable = true,
				ColumnKey = WebTracker.Grids.EDIMessage.MessageText
			};
			messageTextColumn.ItemTemplate = new ZTextEditColumnItemTemplate(messageTextColumn);
			AddToDictionaryAsDefault(messageTextColumn);
		}
	}
}
