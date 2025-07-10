using System.Collections.Generic;

namespace Enterprise.Rating.Integration
{
	public interface IRatingAdaptersProvider
	{
		/// <summary>
		///		Gets a value indicating if the adapters provider for the job wants to check results.
		/// </summary>
		bool NeedsHandleResult { get; }

		/// <summary>
		///		Gets called by autorating engine at the end of autorating if <see cref="NeedsHandleResult"/> is true
		///		so that the provider could check/edit created charges.
		/// </summary>
		/// <param name="charges">
		///		The autorated charges by rating adapter.
		/// </param>
		/// <returns>
		///		true, if the charges were updated, false, if not.
		/// </returns>
		bool HandleResult(IReadOnlyDictionary<IRatingAdapter, List<IAutoRatedCharge>> charges);
	}
}
