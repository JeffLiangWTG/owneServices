using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal
{
	[ModuleID(ModuleId.RefCusTradeGroup)]
	public class RefCusTradeGroupCollection : ActiveBusinessObjectCollection<RefCusTradeGroup>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter names")]
		public static class FilterName
		{
			public const string Code = "Code";
			public const string Name = "Name";
			public const string EconomicGroup = "Economic Group";
		}

		public RefCusTradeGroupCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		public RefCusTradeGroupCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{ }
	}
}
