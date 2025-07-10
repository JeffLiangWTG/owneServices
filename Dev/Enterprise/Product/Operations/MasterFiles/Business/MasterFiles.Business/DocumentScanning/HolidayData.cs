using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HolidayData),
	Enterprise.Core.Constants.DocManagerCodes.Holiday)]

namespace Enterprise.MasterFiles.Business
{
	using System;

	class HolidayData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GlbStaffHoliday);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
	}
}
