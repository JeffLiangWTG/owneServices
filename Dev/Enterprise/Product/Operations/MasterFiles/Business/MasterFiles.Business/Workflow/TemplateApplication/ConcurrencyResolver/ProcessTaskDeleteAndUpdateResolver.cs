using System;

namespace Enterprise.MasterFiles.Business.ConcurrencyResolver
{
	public class ProcessTaskDeleteAndUpdateResolver : IConcurrencyDeleteAndUpdateResolver<ProcessTask>
	{
		public delegate void ProcessTaskConcurrencyResolverUpdate(ProcessTask original, ProcessTask duplicate);

		readonly ProcessTaskConcurrencyResolverUpdate _update;

		public ProcessTaskDeleteAndUpdateResolver(ProcessTaskConcurrencyResolverUpdate updateFunction)
		{
			_update = updateFunction;
		}

		public ConcurrencyResolutionResult<ProcessTask> DeleteDuplicateAndUpdateOriginalWhenRequired(ProcessTask original, ProcessTask duplicate)
		{
			CheckInputs(original, duplicate);
			var builder = ConcurrencyResolutionResult<ProcessTask>.Builder();
			if (!HasActualDate(original) && duplicate != null)
			{
				_update(original, duplicate);
			}

			builder.ResolvedRecord(original);
			if (duplicate != null)
			{
				builder.AddForDeletion(duplicate);
			}

			return builder.Build();
		}

		static bool HasActualDate(ProcessTask task) => task?.P9_ActualDate.IsValid ?? false;

		static void CheckInputs(ProcessTask original, ProcessTask duplicate)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original));
			}
			if (duplicate == null)
			{
				throw new ArgumentNullException(nameof(duplicate));
			}
		}
	}
}
