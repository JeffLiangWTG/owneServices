using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISuppressHookHelper
	{
		IDisposable SuppressFieldOnChangeHook();

		IDisposable SuppressFieldOnChangeHook(bool suppress);

		bool IsSuppressFieldOnChangeHook();
	}
}
