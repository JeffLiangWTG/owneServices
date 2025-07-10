using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class SeapId : GlbExternalPassword
{
	public const string Name = "SeapId";

	public SeapId(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.PLN;
		GP_Name = Name;
	}

	[ResourceStringData("PLSeapId|GP_UserID", Caption = "SEAP ID")]
	public override ZString GP_UserID
	{
		get => base.GP_UserID; set => base.GP_UserID = value;
	}
}
