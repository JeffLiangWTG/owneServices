using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Defines the contract for a phone number updater.
	/// </summary>
	public interface IPhoneNumberUpdater
	{
		/// <summary>
		/// Normalizes a phone number field in a specific table into the standard E164 international format.
		/// </summary>
		/// <param name="logger">The logger to be used.</param>
		/// <param name="phoneNumberColumnName">The name of the database column that stores the phone number.</param>
		/// <param name="pkAndDefaultCountryCodePairs">The primary keys of the rows to be normalized and their corresponding default country code.</param>
		/// <returns>The update results for each row.</returns>
		PhoneNumberUpdateResult[] Normalize(ILogger logger, ZString phoneNumberColumnName, Tuple<ZGuid, ZString>[] pkAndDefaultCountryCodePairs);
	}
}