using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CommunicationChannelProvider : ICommunicationChannelType
{
	public CommunicationChannelProvider(GlbStaff glbStaff)
	{
		this.glbStaff = Argument.NotNull(glbStaff, nameof(glbStaff));
	}

	readonly GlbStaff glbStaff;

	public string EmailChannelAddress => CachedValueHelper.GetValue(ref emailAddress, GetEmailChannelAddress);
	CachedValue<string> emailAddress;

	public string SingleEntryAccessPointId => CachedValueHelper.GetValue(ref singleEntryAccessPointId, () => PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.Value.IsSeapID ? GetSeapId() : null);
	CachedValue<string> singleEntryAccessPointId;

	public string WebServiceUrl => null; // WI00574599: for now not to be used

	public IElectronicPlatformChannelType ElectronicPlatformChannel => null; // WI00574599: for now not to be used

	string GetSeapId()
	{
		var seapIdObj = PL.Business.GlbStaffWrapper.Get(glbStaff).GetGlbExternalPassword<SeapId>(PasswordTypesList.Codes.PLN, GlbCompany.CurrentCompany.PK);
		return seapIdObj != null ? MessageProviderHelper.ReturnNullIfEmpty(seapIdObj.GP_UserID) : null;
	}

	string GetEmailChannelAddress()
	{
		var result = ZString.Empty;

		if (PLCustomsDataRegistry.Instance.DefaultCommunicationChannelNCTSP5.Value.IsEmailChannel)
		{
			result = PL.Business.GlbStaffWrapper.Get(glbStaff)
				.GetGlbExternalPassword<CommunicationChannel>(PasswordTypesList.Codes.PLC, GlbCompany.CurrentCompany.PK)
				?.GP_MailBoxID ?? ZString.Empty;

			if (result.IsEmpty)
			{
				result = PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value;
			}
		}

		return MessageProviderHelper.ReturnNullIfEmpty(result);
	}
}
