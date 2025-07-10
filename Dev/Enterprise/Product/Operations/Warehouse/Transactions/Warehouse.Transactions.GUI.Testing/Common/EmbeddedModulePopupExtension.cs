using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public static class TestEmbeddedModulePopupExtension
	{
		public static void SelectStaffForEmbeddedModuleSelection(this EmbeddedModulePopup popup, GlbStaff bizO)
		{
			var handleSelectionMethodInfo = typeof(EmbeddedModulePopup).GetMethod("HandleSelection", BindingFlags.NonPublic | BindingFlags.Instance);
			handleSelectionMethodInfo.Invoke(popup, new Object[] { new BusinessObject[] { bizO } });
		}
	}
}
