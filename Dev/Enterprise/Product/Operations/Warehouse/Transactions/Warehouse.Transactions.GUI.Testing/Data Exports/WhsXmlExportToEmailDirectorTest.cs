using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public abstract class WhsXmlExportToEmailDirectorTest<TDocket, TValueObject> : WhsXmlExportDirectorTest<TDocket, TValueObject>
	where TDocket : WhsDocket
	where TValueObject : IValueObject
{
	#region TestExportWhenNoValidEmailAddress()

	public void TestExportWhenNoValidEmailAddress()
	{
		var mode = Client.EDICommunicationsModes[0];
		mode.EK_FileFormat = ExpectedEDICommunicationsModeFileFormat;
		mode.EK_Module = GetCommunicationsModeModule();
		mode.EK_Destination = "test@cargowise.com@com";
		mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;

		Docket.Factory.Save();

		ExportDirector.RunExport(Docket);
		Assert("Invalid email address", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The Client does not have a valid " + EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment + " email address. Please set it on"));
	}

	#endregion

	#region TestExportWhenNoEmailAddress()

	public void TestExportWhenNoEmailAddress()
	{
		var mode = Client.EDICommunicationsModes[0];
		mode.EK_FileFormat = ExpectedEDICommunicationsModeFileFormat;
		mode.EK_Module = GetCommunicationsModeModule();
		mode.EK_Destination = ZString.Empty;
		mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;

		Docket.Factory.Save();
		ExportDirector.RunExport(Docket);
		AssertContains("Invalid email address", "The Client does not have a valid EMA email address. Please set it on Client's Config->General->Organization Transmission Address", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	#endregion

	#region TestExportWhenValidEmailAddress()

	public void TestExportWhenValidEmailAddress()
	{
		Docket.Factory.Save();
		ExportDirector.RunExport(Docket);
		AssertEquals("Valid email address, shouldn't have any errors", GetExpectedSuccessNotification(), UnitTestUserNotification.Instance.LastMessage.Text);
	}

	#endregion

	#region TestExportSendEmailWithTheFileAttached()

	public void TestExportSendEmailWithTheFileAttached()
	{
		Env.OutgoingMailManager.EmailsCreated.Clear();

		Docket.Factory.Save();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ExportDirector.RunExport(Docket);

		AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		var email = Env.OutgoingMailManager.EmailsCreated[0];
		AssertEquals("Attachment", 1, email.Attachments.Count);
		AssertEquals("EmailSubject", GetExpectedEmailSubject(), email.Subject);
		AssertEquals("Email destination is not correct", "test@cargowise.com", email.Recipients[0]);
	}

	#endregion

	#region Overrides

	protected override ZString GetExpectedSuccessNotification()
	{
		return "Email with " + GetExpectedDocketTypeDescription() + " XML attachment successfully sent.";
	}

	protected override OrgHeader GetNewClient()
	{
		var result = base.GetNewClient();

		var mode1 = result.EDICommunicationsModes.AddNew();
		mode1.EK_FileFormat = ExpectedEDICommunicationsModeFileFormat;
		mode1.EK_Module = GetCommunicationsModeModule();
		mode1.EK_Destination = "c:\\XMLExport"; // This is a valid export directory
		mode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;

		var mode2 = result.EDICommunicationsModes.AddNew();
		mode2.EK_FileFormat = ExpectedEDICommunicationsModeFileFormat;
		mode2.EK_Module = GetCommunicationsModeModule();
		mode2.EK_Destination = "test@cargowise.com";
		mode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;

		return result;
	}

	#endregion

	#region Implementation

	protected virtual string ExpectedEDICommunicationsModeFileFormat => EDICommunicationsModeFileFormatList.Codes.XML;

	protected virtual ZString GetExpectedEmailSubject() => $"{Core.Constants.ProductName} Warehouse XML File";

	protected abstract ZString GetCommunicationsModeModule();

	#endregion
}
