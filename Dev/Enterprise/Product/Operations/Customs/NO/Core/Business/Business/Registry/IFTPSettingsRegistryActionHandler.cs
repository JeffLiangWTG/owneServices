using System;
using Enterprise.Integration;

namespace Enterprise.Customs.NO.Registry;

interface IFTPSettingsRegistryActionHandler
{
	RegistryUpdateActionDelegate UpdateFtpCustomsSettings { get; }

	Action Save { get; }
}
