using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	static class GUIFormHelper
	{
		internal static T ArgumentNotNullWithFormDispose<T>(ZForm form, T argument, string argumentName)
		{
			if (argument != null)
			{
				return argument;
			}
			else
			{
				form.Dispose();
				throw new ArgumentNullException(argumentName);
			}
		}

		internal static void CheckParameterlessConstructorNotBeingCalledOutsideDesigner(ZForm form)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				form.Dispose();
				throw new InvalidOperationException("The parameterless constructor can only be used for designer.");
			}
		}
	}
}
