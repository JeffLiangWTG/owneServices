namespace Enterprise.MasterFiles.Business
{
	public interface IProcessTaskInitialiser
	{
		void SetDefaultsForNewTaskCore(ProcessTask task, bool useDefaultStaff);
		void SetDefaultParents(ProcessTask task);
	}

	public static class IProcessTaskInitialiserExtensions
	{
		public static void SetDefaultsForNewTask(this IProcessTaskInitialiser collection, ProcessTask task, bool useDefaultStaff)
		{
			collection.SetDefaultsForNewTaskCore(task, useDefaultStaff);
			collection.SetDefaultParents(task);
		}
	}
}
