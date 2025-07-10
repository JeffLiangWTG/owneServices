using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public static class SuspenderExtensions
	{
		public static IDisposable GetSettingHasChangesSuspender(this BusinessObject businessObject, bool suspendSettingHasChanges)
		{
			var settingHasChangesSuspender = suspendSettingHasChanges ? businessObject?.SuspendSettingHasChanges() : null;
			return new DisposableAction(() => settingHasChangesSuspender?.Dispose());
		}
	}
}
