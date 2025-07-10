using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public struct QualityIterationTaskDescriptor
	{
		public QualityIterationTaskDescriptor()
		{
		}

		public ZString Type { get; set; } = null;

		public ZString Description { get; set; } = null;

		public ZString AssignedStaffCode { get; set; } = null;

		public ZDateTime EstDuration { get; set; } = ZDateTime.Empty;
	}
}
