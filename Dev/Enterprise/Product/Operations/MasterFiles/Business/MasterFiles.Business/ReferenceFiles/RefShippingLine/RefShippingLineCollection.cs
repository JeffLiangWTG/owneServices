using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefShippingLine)]
	public class RefShippingLineCollection : ActiveBusinessObjectCollection<RefShippingLine>
	{
		public RefShippingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefShippingLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public RefShippingLineCollection(BusinessObjectFactory factory, OrgHeader orgHeader)
			: base(factory)
		{
			if (orgHeader != null)
			{
				FilterBusinessObjectDefaults.SetDynamicDefaultFilters(() => AddUserFilters(orgHeader));
			}
		}

		void AddUserFilters(OrgHeader orgHeader)
		{
			if (orgHeader.OH_IsShippingLine)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("NVO Status", "Property", (ZString)RefShippLineFilterCode.NotNVO));
			}
			else if (orgHeader.OH_IsSeaWholesaler)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("NVO Status", "Property", (ZString)RefShippLineFilterCode.NVO));
			}
			else if (FilterBusinessObjectDefaults.ContainsDefaultFor((NoResString)"NVO Status:Property"))
			{
				FilterBusinessObjectDefaults.Remove((NoResString)"NVO Status:Property");
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
	public static class RefShippLineFilterCode
	{
		public const string NVO = "NVO";
		public const string NotNVO = "Not NVO";
		public const string AllStatusForNVO = "All";
	}
}
