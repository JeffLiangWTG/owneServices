using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration.Customs.NO;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NO.Business;

public sealed class GlbExternalPassword_NOD : GlbExternalPasswordWithCertificate,
	IGlbExternalPassword_NOD,
	IxTMessageAttributeProvider
{
	public GlbExternalPassword_NOD(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.NOD;
	}

	#region Overrided properties

	[ResourceStringData("B881B3BA-6475-4309-99AF-5C33920011AE", Caption = "Client id")]
	public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

	[ResourceStringData("4C7E62D4-20AC-4CFF-AADA-E9E6904BC402", Caption = "Password")]
	public override ZString CurrentDecryptedCertificatePassphrase { get => base.CurrentDecryptedCertificatePassphrase; set => base.CurrentDecryptedCertificatePassphrase = value; }

	[ReadOnly(true)]
	[ResourceStringData("C96D2664-EB05-4AE1-8A52-02684F202DF5", Caption = "Status")]
	public override ZString GP_PasswordStatus {  get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

	[ReadOnly(true)]
	[ResourceStringData("E5A8E191-40DC-425F-99E2-61AE9D2E863A", Caption = "Expiry Date")]
	public override ZDateTime GP_ExpiryDate { get => base.GP_ExpiryDate; set => base.GP_ExpiryDate = value; }

	#endregion

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPassword_NODValidation(this);

	Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
	{
		var t = (IxTMessageAttributeProvider)this;
		return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
	}

	public new GlbExternalPassword_NODValidation Validation => (GlbExternalPassword_NODValidation)base.Validation;
}
