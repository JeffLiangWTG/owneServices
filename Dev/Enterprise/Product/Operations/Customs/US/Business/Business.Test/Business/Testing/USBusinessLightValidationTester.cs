using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USBusinessLightValidationTester : LightValidationTester
	{
		public USBusinessLightValidationTester(BusinessObject bo) : base(bo)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			var skippedProperties = new List<string> { "JE_ExportDate", "JE_DateOfArrival", "JE_DateOfFirstArrival" };
			if (skippedProperties.Contains(info.Name))
			{
				return false;
			}

			return base.ShouldTestProperty(info);
		}
	}
}
