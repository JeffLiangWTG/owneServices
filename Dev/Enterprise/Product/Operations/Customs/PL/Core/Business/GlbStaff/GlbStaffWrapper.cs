using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PL;

namespace Enterprise.Customs.PL.Business;

public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper, IPLGlbStaffWrapper
{
	public GlbStaffWrapper(GlbStaff staff)
		: base(staff)
	{
	}

	public static GlbStaffWrapper Get(GlbStaff staff)
	{
		return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));
	}

	public ZBool IsNCTSPhase5
	{
		get
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			return nctsSettings.IsUsingPhase5(countryCode);
		}
	}

	public GlbExternalPassword_PL GlbExternalPassword
	{
		get
		{
			if (glbExternalPassword == null)
			{
				glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_PL>(PasswordTypesList.Codes.PLB, GlbCompany.CurrentCompany.PK);
				RegisterEditableChildObject(glbExternalPassword);
			}

			return glbExternalPassword;
		}
	}
	GlbExternalPassword_PL glbExternalPassword;

	public SeapId SeapId
	{
		get
		{
			if (seapId == null)
			{
				seapId = GetGlbExternalPasswordOrCreateNew<SeapId>(PasswordTypesList.Codes.PLN, GlbCompany.CurrentCompany.PK);
				RegisterEditableChildObject(seapId);
			}

			return seapId;
		}
	}
	SeapId seapId;

	public CommunicationChannel CommunicationChannel
	{
		get
		{
			if (communicationChannel == null)
			{
				communicationChannel = GetGlbExternalPasswordOrCreateNew<CommunicationChannel>(PasswordTypesList.Codes.PLC, GlbCompany.CurrentCompany.PK);
				RegisterEditableChildObject(communicationChannel);
			}

			return communicationChannel;
		}
	}
	CommunicationChannel communicationChannel;

	IGlbExternalPasswordWithCertificate IPLGlbStaffWrapper.PLBPassword => GlbExternalPassword;
}
