using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105ManufacturerWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105ManufacturerWrapper(TWJobDocAddress address, JobComInvoiceLine invoiceLine)
			: base(address?.Address, address?.Address, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier)
		{
			twJobDocAddress = Argument.NotNull(address, nameof(address));
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		bool IsForCMHeaderMessageTypeNX401 => (isForCMHeaderMessageTypeNX401 ?? (isForCMHeaderMessageTypeNX401 = invoiceLine.IsForCMHeaderMessageTypeNX401)).Value;
		bool? isForCMHeaderMessageTypeNX401;

		protected override ZString IDCore => ZString.Empty;

		protected override ZString NameCore => IsForCMHeaderMessageTypeNX401 ? ZString.Empty : EnglishAddress.CompanyName;

		protected override IAddress AddressCore => IsForCMHeaderMessageTypeNX401 ? null : base.AddressCore;

		protected override ZString GetAddressCountrySubDivisionIDCore() => ZString.Empty;

		protected override ZString GetAddressCountrySubDivisionNameCore() => ZString.Empty;

		protected override ZString TypeCodeCore => IDCore.IsEmpty ? ZString.Empty : new ZString(PartyIdentifierCodeList.Codes._58);

		protected override AddressData GetEnglishAddress() => new AddressData(twJobDocAddress, false, false, SharedHelper.GetEnglishLanguageCodes());

		protected readonly TWJobDocAddress twJobDocAddress;
		protected readonly JobComInvoiceLine invoiceLine;
	}
}
