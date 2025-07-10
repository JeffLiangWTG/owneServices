using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Registry;
using Enterprise.Messaging.Integration;
using static Enterprise.xTMessaging.Shared.Constants;

namespace Enterprise.Customs.NO.Business;

public sealed class EmmaEDIInterchange(BusinessObjectFactory factory, DataRow row) : Messaging.Business.EDIInterchange(factory, row),
	Integration.Customs.NO.IEmmaEDIInterchange,
	IxTMessageAttributeProvider
{
	Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
	{
		var settings = NOCustomsDataRegistry.Instance.FTPSettingsEMMADoc.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		return new()
		{
			{ xTMsgAttributes.FtpClientUser, settings.Username },
			{ xTMsgAttributes.FtpClientPassword, settings.Password },
		};
	}
}
