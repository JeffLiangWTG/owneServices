using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IImportRelatedActivityDeciderFactory
	{
		string Reason { get; }

		T GetIfAvailable<T>() where T : class, IImportRelatedActivityDecider;
		void AddDecider(Type type, IImportRelatedActivityDecider decider);
	}

	public abstract class ImportRelatedActivityDeciderFactory : IImportRelatedActivityDeciderFactory
	{
		public abstract string Reason
		{
			get;
		}

		public T GetIfAvailable<T>()
			where T : class, IImportRelatedActivityDecider
		{
			IImportRelatedActivityDecider decider = null;
			if (!OverridenDeciders.TryGetValue(typeof(T), out decider))
			{
				decider = GetIfAvailableCore<T>();
			}

			return (T)decider;
		}

		protected abstract T GetIfAvailableCore<T>() where T : class, IImportRelatedActivityDecider;

		public void AddDecider(Type type, IImportRelatedActivityDecider decider)
		{
			OverridenDeciders.Add(type, decider);
		}

		readonly Dictionary<Type, IImportRelatedActivityDecider> OverridenDeciders = new Dictionary<Type, IImportRelatedActivityDecider>();
	}
}