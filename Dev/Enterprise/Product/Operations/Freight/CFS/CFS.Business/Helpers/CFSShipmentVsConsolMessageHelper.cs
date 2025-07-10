using System;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.Business
{
	public sealed partial class CFSShipmentVsConsolMessageHelper : ShipmentVsConsolMessageHelper
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
					instance = new CFSShipmentVsConsolMessageHelper();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static IShipmentVsConsolMessageHelper instance;

		protected override string ConsolName_PluralLower
		{
			get { return Res.GetString("ff1b81aa-60cf-4c94-8073-7f04bccff7b1", "load lists"); }
		}

		protected override string ConsolName_SingularLower
		{
			get { return Res.GetString("8fe20471-f554-4fe5-a029-f9a73038c6d1", "load list"); }
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.CFS.Business
{
	public sealed partial class CFSShipmentVsConsolMessageHelper
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
