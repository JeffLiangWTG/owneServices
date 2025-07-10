using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Interface for storing and providing read-only universal validation rule records loaded from the db.
	/// Will cache rules for a short time to reduce database hits.
	/// </summary>
	public interface IUniversalValidationRuleStore
	{
		/// <summary>
		/// Get active rules that match the given data context.
		/// </summary>
		/// <param name="factory">factory to load rules if needed. May not be used if rules were recently loaded.</param>
		IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByDataContext(BusinessObjectFactory factory, string dataContextType);

		/// <summary>
		/// Get active rules that match the given codes.
		/// </summary>
		/// <param name="factory">factory to load rules if needed. May not be used if rules were recently loaded.</param>
		IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByCode(BusinessObjectFactory factory, string dataContextType, IEnumerable<string> codes);
	}
}
