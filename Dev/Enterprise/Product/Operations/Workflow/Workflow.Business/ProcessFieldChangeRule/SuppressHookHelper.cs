using System;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class SuppressHookHelper : ISuppressHookHelper
	{
		public IDisposable SuppressFieldOnChangeHook()
		{
			if (suppressProcessFieldOnChangeHook)
			{
				return null;
			}
			else
			{
				suppressProcessFieldOnChangeHook = true;
				return new DisposableAction(() => suppressProcessFieldOnChangeHook = false);
			}
		}

		public IDisposable SuppressFieldOnChangeHook(bool suppress)
		{
			return suppress ? SuppressFieldOnChangeHook() : null;
		}

		public bool IsSuppressFieldOnChangeHook()
		{
			return suppressProcessFieldOnChangeHook;
		}

		[ThreadStatic]
		static bool suppressProcessFieldOnChangeHook;
	}
}
