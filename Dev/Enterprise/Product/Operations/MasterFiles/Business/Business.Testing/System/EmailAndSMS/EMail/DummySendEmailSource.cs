using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummySendEmailSource : ISendEmailSource
	{
		public string DefaultFromDisplayName
		{
			get; set;
		}

		public string OverridingDefaultFromEmailAddress
		{
			get; set;
		}

		public DocManagerInfo DocManagerInfo
		{
			get; set;
		}

		public Type DocWrapperType
		{
			get; set;
		}

		public string EmailSubject
		{
			get; set;
		}

		public Logs Logs
		{
			get; set;
		}

		public string TemplateCategory
		{
			get; set;
		}

		public AddressBookSelection GetAddressBookSelection()
		{
			return AddressBookSelectionForTest;
		}

		public AddressBookSelection AddressBookSelectionForTest;
	}
}
