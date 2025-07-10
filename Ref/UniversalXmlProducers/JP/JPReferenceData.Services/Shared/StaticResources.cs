using System;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class StaticResources
	{
		public static DateTime DefaultZZD_StartDate => new DateTime(1900, 01, 01, 00, 00, 00);
		public static DateTime DefaultZZD_EndDate => new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
