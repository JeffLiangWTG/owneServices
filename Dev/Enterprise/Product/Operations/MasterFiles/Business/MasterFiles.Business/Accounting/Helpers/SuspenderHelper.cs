using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	interface IFunctionalitySuspender
	{
		IDisposable GetSuspender();
	}

	public class FunctionalitySuspenderWrapper : IFunctionalitySuspender
	{
		public FunctionalitySuspenderWrapper(Func<IDisposable> getSuspenderToWrap)
		{
			this.getSuspenderToWrap = getSuspenderToWrap;
		}

		readonly Func<IDisposable> getSuspenderToWrap;

		public IDisposable GetSuspender()
		{
			return new SuspenderWrapper(getSuspenderToWrap());
		}

		class SuspenderWrapper : IDisposable
		{
			public SuspenderWrapper(IDisposable disposableToWrap)
			{
				this.disposableToWrap = disposableToWrap;
			}

			readonly IDisposable disposableToWrap;

			void IDisposable.Dispose()
			{
				if (disposableToWrap != null)
				{
					disposableToWrap.Dispose();
				}
			}
		}
	}

	public class FunctionalitySuspender : IFunctionalitySuspender
	{
		public FunctionalitySuspender(Action onResumeAction = null, bool doActionOnlyIfRequestedWhenSuspended = false, Action onSuspendAction = null)
		{
			this.onResumeAction = onResumeAction;
			this.doActionOnlyIfRequestedWhenSuspended = doActionOnlyIfRequestedWhenSuspended;
			this.onSuspendAction = onSuspendAction;
		}

		public IDisposable GetSuspender()
		{
			return new Suspender(this);
		}

		public bool IsSuspended
		{
			get
			{
				var isSuspended = countOfCreatedSuspenders > 0;
				if (isSuspended)
				{
					isActionRequestedWhenSuspended = true;
				}
				return isSuspended;
			}
		}

		public static IEnumerable<IDisposable> SuspendCollection<T>(IEnumerable<T> collection, Func<T, IDisposable> getSuspender)
		{
			List<IDisposable> suspenders = new List<IDisposable>();
			foreach (var item in collection)
			{
				suspenders.Add(getSuspender(item));
			}

			return suspenders;
		}

		public static void ResumeCollection(IEnumerable<IDisposable> suspenders)
		{
			foreach (var suspender in suspenders)
			{
				suspender.Dispose();
			}
		}

		readonly Action onResumeAction;
		readonly Action onSuspendAction;
		readonly bool doActionOnlyIfRequestedWhenSuspended;
		bool isActionRequestedWhenSuspended;
		int countOfCreatedSuspenders;

		class Suspender : IDisposable
		{
			public Suspender(FunctionalitySuspender parent)
			{
				this.parent = parent;

				bool isNotSuspendedYet = this.parent.countOfCreatedSuspenders == 0;
				if (isNotSuspendedYet)
				{
					parent.onSuspendAction?.Invoke();
				}
				this.parent.countOfCreatedSuspenders++;
			}

			readonly FunctionalitySuspender parent;

			void IDisposable.Dispose()
			{
				parent.countOfCreatedSuspenders--;

				bool isLastSuspenderDisposed = parent.countOfCreatedSuspenders == 0;
				if (isLastSuspenderDisposed)
				{
					if (!parent.doActionOnlyIfRequestedWhenSuspended || parent.isActionRequestedWhenSuspended)
					{
						parent.onResumeAction?.Invoke();
					}
					parent.isActionRequestedWhenSuspended = false;
				}
			}
		}
	}
}
