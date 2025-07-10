using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarContainer
	{
		ZString ContainerNumber { get; }
		ZString ContainerTypeISO { get; }  // ISO type etc		
		ContainerStatus GetContainerStatus(string countryCode);  // import, export, transit, etc
		EmptyFullServiceType GetEmptyFullServiceType(string countryCode);
		ZDecimal GrossMassInKilos { get; }
		ZDecimal VerifiedGrossMassInKilos { get; }
		IEnumerable<ICusCarSeal> GetSeals(string countryCode);
	}

	public enum ContainerStatus
	{   /// <summary>
		/// DO NOT USE THIS, it's just to make the stupid Code Analyser STFU (CA1008)
		/// </summary>
		None = 0,
		Transit = 1,
		Export = 2,
		Import = 3,
		Transhipment = 6
	}

	public enum EmptyFullServiceType
	{
		/// <summary>
		/// DO NOT USE THIS, it's just to make the stupid Code Analyser STFU (CA1008)
		/// </summary>
		None = 0,
		Empty = 4,
		FullFCLGroupage = 5,
		FullMixedConsignmentLCL = 7,
		FullSingleConsignmentFCL = 8
	}
}
