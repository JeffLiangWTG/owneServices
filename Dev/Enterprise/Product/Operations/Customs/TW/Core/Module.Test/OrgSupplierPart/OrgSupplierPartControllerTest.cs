using System;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	public sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(OrgSupplierPartController);
			}
		}

		protected override string CountryCode
		{
			get
			{
				return Enterprise.Core.Constants.CountryCodes.Taiwan;
			}
		}
	}
}
