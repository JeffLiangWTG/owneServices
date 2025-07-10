using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefSysConfig : AutoRefSysConfig, Integration.Customs.Shared.IRefSysConfig
	{
		public RefSysConfig(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefSysConfig.Loader, Integration.Customs.Shared.IRefSysConfigLoader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZBool GetBoolValue(ZString configCode, ZDateTime? startDate = null) => GetValue(configCode, ZBool.False, (s) => s.ZRC_BitValue, startDate);
			public ZString GetStringValue(ZString configCode, ZDateTime? startDate = null) => GetValue(configCode, ZString.Empty, (s) => s.ZRC_StringValue, startDate);
			public ZDecimal GetDecimalValue(ZString configCode, ZDecimal defaultValue, ZDateTime? startDate = null) => GetValue(configCode, defaultValue, (s) => s.ZRC_DecimalValue, startDate);

			T GetValue<T>(ZString configCode, T defaultValue, Func<RefSysConfig, T> valueFunc, ZDateTime? startDate = null)
			{
				var date = startDate ?? ZDateTime.UtcToday;
				if (configCode.IsEmpty || !date.IsValid)
				{
					return defaultValue;
				}

				return Factory.GetCachedValue("RefSysConfig-" + configCode + date.ToShortDateString(), () =>
				{
					var sysConfig = Load(configCode, date);
					return (sysConfig != null) ? valueFunc(sysConfig) : defaultValue;
				});
			}

			public RefSysConfig Load(ZString configCode, ZDateTime date)
			{
				if (configCode.IsEmpty || !date.IsValid)
				{
					return null;
				}

				var result = Factory.LoadFromNaturalKey<RefSysConfig>(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, configCode);
				return (result != null && result.ZRC_StartDate <= date) ? result : null;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefSysConfig);
		}
	}
}
