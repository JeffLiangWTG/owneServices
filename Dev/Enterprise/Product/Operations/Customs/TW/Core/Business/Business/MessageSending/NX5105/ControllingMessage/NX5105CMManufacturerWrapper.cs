using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMManufacturerWrapper : NX5105ManufacturerWrapper
	{
		public NX5105CMManufacturerWrapper(TWJobDocAddress address, JobComInvoiceLine invoiceLine) : base(address, invoiceLine)
		{
		}

		bool IsForCMHeaderMessageTypeNX601 => (isForCMHeaderMessageTypeNX601 ?? (isForCMHeaderMessageTypeNX601 = invoiceLine.IsForCMHeaderMessageTypeNX601)).Value;
		bool? isForCMHeaderMessageTypeNX601;

		bool IsForCMHeaderMessageTypeNX603 => (isForCMHeaderMessageTypeNX603 ?? (isForCMHeaderMessageTypeNX603 = invoiceLine.IsForCMHeaderMessageTypeNX603)).Value;
		bool? isForCMHeaderMessageTypeNX603;

		protected override ZString IDCore => IsForCMHeaderMessageTypeNX601 || IsForCMHeaderMessageTypeNX603 ? twJobDocAddress.IDCode : ZString.Empty;

		protected override ZString GetAddressCountrySubDivisionIDCore() => IsForCMHeaderMessageTypeNX601 ? EnglishAddress.StateCode : ZString.Empty;

		protected override ZString GetAddressCountrySubDivisionNameCore() => IsForCMHeaderMessageTypeNX601 ? EnglishAddress.StateDescription : ZString.Empty;

		protected override ZString Communications1IdCore => EnglishAddress.Phone;

		protected override ZString Communications2IdCore => EnglishAddress.EMail;

		protected override IEnumerable<ICommunication> CommunicationsCore => IsForCMHeaderMessageTypeNX601 ? base.CommunicationsCore : null;

		protected override ZString ContactNameCore => IsForCMHeaderMessageTypeNX601 ? twJobDocAddress.E2_Contact : ZString.Empty;
	}
}
