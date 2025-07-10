using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class ContactWrapper : IContact
{
	public ContactWrapper(GlbStaff glbStaff)
	{
		Argument.NotNull(glbStaff, nameof(glbStaff));
		Name = glbStaff.GS_FullName;
		Communications = GetCommunications(glbStaff.GS_WorkPhone, glbStaff.GS_EmailAddress);
	}

	public ContactWrapper(OrgContact orgContact)
	{
		Argument.NotNull(orgContact, nameof(orgContact));
		Name = orgContact.Name;
		Communications = GetCommunications(orgContact.OC_Phone, orgContact.OC_Email);
	}

	public string Name { get; }

	public IReadOnlyCollection<ICommunication> Communications { get; }

	public string PhoneNumber => string.Empty;

	public string EMailAddress => string.Empty;

	IReadOnlyCollection<ICommunication> GetCommunications(ZString phone, ZString email)
	{
		var result = new List<ICommunication>();
		if (!phone.IsEmpty)
		{
			var communication = new CommunicationWrapper(phone, NLConstants.DMSMessageValues.TypeCodeTelephone, 1);
			result.Add(communication);
		}
		if (!email.IsEmpty)
		{
			var communication = new CommunicationWrapper(email, NLConstants.DMSMessageValues.TypeCodeMail, 2);
			result.Add(communication);
		}
		return result;
	}
}
