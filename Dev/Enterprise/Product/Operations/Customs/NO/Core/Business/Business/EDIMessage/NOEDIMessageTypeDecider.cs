using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.NO;

namespace Enterprise.Customs.NO.Business;

public class NOEDIMessageTypeDecider : TypeDecider, IEDIMessageTypeDecider
{
	public override Type GetTypeForBinding()
	{
		return null;
	}

	public override Type GetTypeForNew()
	{
		return null;
	}

	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim().ToUpperInvariant();
		return messageType switch
		{
			EDIMessageConstants.MessageTypes.CUSDEC => typeof(CUSDECEDIMessage),
			EDIMessageConstants.MessageTypes.CUSRES => typeof(CUSRESEDIMessage),
			EDIMessageConstants.MessageTypes.EMMA => typeof(EmmaEDIMessage),
			_ => typeof(NOEDIMessage)
		};
	}
}
