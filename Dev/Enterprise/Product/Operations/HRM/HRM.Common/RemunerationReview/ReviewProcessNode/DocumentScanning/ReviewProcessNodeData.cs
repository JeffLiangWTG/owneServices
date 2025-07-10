using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ReviewProcessNodeData),
	Enterprise.Core.Constants.DocManagerCodes.ReviewProcessNode)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using Enterprise.HRM.Common;

	class ReviewProcessNodeData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(ReviewProcessNode);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
	}
}
