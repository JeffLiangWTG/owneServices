using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public sealed partial class DashboardShipmentVsConsolMessageHelper : ShipmentVsConsolMessageHelper
	{
		public static IShipmentVsConsolMessageHelper Instance
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && helperOverrideForTest != null)
				{
					return helperOverrideForTest;
				}
#endif

				if (instance == null)
				{
					instance = new DashboardShipmentVsConsolMessageHelper();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static IShipmentVsConsolMessageHelper instance;

		protected override IEnumerable<string> DisabledChecks => new string[] { CheckMethodNames.CheckShipmentHasJobAndChanges };

		protected override string ConsolName_PluralLower
		{
			get { return Res.GetString("e8b5baef-0434-4e9c-9baf-b2e7fab90649", "consols"); }
		}

		protected override string ConsolName_SingularLower
		{
			get { return Res.GetString("1ed80c9e-17cc-4a47-97d1-99818f21fd55", "consol"); }
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.Business
{
	public sealed partial class DashboardShipmentVsConsolMessageHelper
	{
		[ThreadStatic]
		static IShipmentVsConsolMessageHelper helperOverrideForTest;

		public static IDisposable OverrideHelperInstance(IShipmentVsConsolMessageHelper instance)
		{
			helperOverrideForTest = instance;
			return new DisposableAction(() => helperOverrideForTest = null);
		}
	}
}

#endif
#endregion
