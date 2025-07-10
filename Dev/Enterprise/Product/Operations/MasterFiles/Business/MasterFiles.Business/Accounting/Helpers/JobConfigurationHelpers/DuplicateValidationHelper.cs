using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IDuplicateValidationHelper
	{
		void CheckDuplicates<T>(IDuplicateValidationCollectionProvider<T> parent, string errorMessage) where T : BusinessObject;
	}

	public class DuplicateValidationHelper : IDuplicateValidationHelper
	{
		void IDuplicateValidationHelper.CheckDuplicates<T>(IDuplicateValidationCollectionProvider<T> parent, string errorMessage)
		{
			IReadOnlyList<IDuplicateValidationItem<T>> collection = parent.GetDuplicateValidationCollection();

			foreach (var item in collection)
			{
				item.RemoveRowError(errorMessage);
			}

			for (int i = 0; i < collection.Count; i++)
			{
				var item1 = collection[i];
				for (int j = i + 1; j < collection.Count; j++)
				{
					var item2 = collection[j];
					if (item1.IsDuplicated((T)item2))
					{
						item1.AddRowError(errorMessage);
						item2.AddRowError(errorMessage);
					}
				}
			}
		}
	}
}
