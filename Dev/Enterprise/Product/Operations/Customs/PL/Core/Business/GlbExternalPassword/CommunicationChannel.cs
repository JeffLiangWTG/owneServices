using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class CommunicationChannel : GlbExternalPassword
{
	public const string Name = "CommunicationChannel";

	public CommunicationChannel(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.PLC;
		GP_Name = Name;
	}

	[ResourceStringData("PLCommunicationChannel|GP_MailBoxID", Caption = "Communication Channel Email", MediumCaption = "Comm. Chan. Email", ShortCaption = "Comm. Email")]
	public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

	public new CommunicationChannelValidation Validation => (CommunicationChannelValidation)GetNewValidation();

	protected override GlbExternalPasswordValidation GetNewValidation() => new CommunicationChannelValidation(this);
}
