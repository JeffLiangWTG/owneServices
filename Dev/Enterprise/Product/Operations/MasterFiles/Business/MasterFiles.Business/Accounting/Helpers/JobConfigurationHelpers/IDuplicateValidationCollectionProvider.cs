using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IDuplicateValidationItem<T>
	{
		bool IsDuplicated(T anotherItem);
		void AddRowError(string message);
		void RemoveRowError(string message);
	}

	public interface IDuplicateValidationCollectionProvider<T>
	{
		IReadOnlyList<IDuplicateValidationItem<T>> GetDuplicateValidationCollection();
	}
}
