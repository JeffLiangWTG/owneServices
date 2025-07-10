using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Organization : IOrganization
	{
		public Organization(OrgHeader orgHeader)
		{
			this.orgHeader = orgHeader;
		}

		readonly OrgHeader orgHeader;

		public ZString Code => orgHeader?.OH_Code ?? ZString.Empty;

		public ZString Name => orgHeader?.OH_FullName ?? ZString.Empty;

		public ZGuid PK => orgHeader?.PK ?? ZGuid.Empty;

		public IUnloco Unloco => unloco ?? (unloco = new Unloco(orgHeader?.UNLOCO));
		Unloco unloco;

		public IAddress MainAddress => mainAddress ?? (mainAddress = new Address(orgHeader?.MainAddress));
		Address mainAddress;

		public IReadOnlyCollection<IRegistrationNumber> RegistrationNumbers
		{
			get
			{
				if (registrationNumbers == null)
				{
					registrationNumbers = (orgHeader?.CustomsCodes.OfType<OrgCusCode>().Select(code => new RegistrationNumber(code))
																 ?? Enumerable.Empty<RegistrationNumber>()).ToArray();
				}

				return registrationNumbers;
			}
		}
		IReadOnlyCollection<IRegistrationNumber> registrationNumbers;
	}
}
