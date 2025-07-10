using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public interface IRatingHeader : IRate, IFactoryProvider
	{
		OrgHeader Header { get; }
		ZString TH_QuoteNumber { get; }
		ZByte TH_GlobalRateLevel { get; }
		ZGuid TH_OH { get; }
		GlbCompany Company { get; }
		ZString TH_GlobalRateDescription { get; }
		MultilingualString TH_GlobalRateDescriptionMultilingual { get; }
		ZBool TH_OneTimeQuote { get; }
		ZGuid PK { get; }
		IEnumerable<IRateEntry> ChildRateEntries { get; }
		ZString TH_RateType { get; }
		IEnumerable<IRateEntry> LoadRateEntriesForAutoRater(ZQuery odFilter);

		/// <summary>
		/// Generic description of this type of RatingHeader, e.g., "Costing" or "Client Rate".
		/// Does not contain details identifying this specific instance such as the client name or code.
		/// </summary>
		ZString RatingHeaderTypeDescription { get; }

		/// <summary>
		/// Human readable shortcut name. Contains instance specific info such as the client org code.
		/// </summary>
		ZString DisplayInfo();
	}

	public interface IRate
	{
		string InvalidReason { get; }
	}

	public static class IRateExtensions
	{
		public static bool IsValidRate(this IRate rate) => rate != null && string.IsNullOrWhiteSpace(rate.InvalidReason);
	}
}
