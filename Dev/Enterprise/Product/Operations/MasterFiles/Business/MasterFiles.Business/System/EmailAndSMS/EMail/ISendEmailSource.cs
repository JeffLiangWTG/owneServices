using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface ISendEmailSource : IDocManagerSupport, ISendEmailActionSource
	{
		AddressBookSelection GetAddressBookSelection();
		string EmailSubject { get; }
		string TemplateCategory { get; }
		string DefaultFromDisplayName { get; }
		string OverridingDefaultFromEmailAddress { get; }
		Type DocWrapperType { get; }
		Logs Logs { get; }
	}
}
