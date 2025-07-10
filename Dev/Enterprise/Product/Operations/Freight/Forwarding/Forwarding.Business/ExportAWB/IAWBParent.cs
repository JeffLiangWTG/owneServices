using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public interface IAWBParent
	{
		ExportAWBHeader AWBHeader { get; }
		void PopulateAWB();
		void ResetAddressPickerDropLists();
		ExportAWBHeader LoadOrCreateAWB();
		bool IsAWBLoaded { get; }
		bool IsAWBHeaderAccessible { get; }
		ZBool IsAWBValuesOverriddenProperty { get; set; }
		ZPropertyInfo IsAWBValuesOverriddenPropertyInfo { get; }
		bool IsOverrideAllowed { get; }
		ZString HAWB { get; }
		ZString MAWB { get; }
		void NotifyConcurrencyHandled();
		event EventHandler IsAWBHeaderAccessibleChanged;
	}
}
