using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class GdmConfiguration : EU.Business.GdmConfiguration
	{
		protected override ZString WhatIsAWaiverCore => ZString.Empty;
	}
}
