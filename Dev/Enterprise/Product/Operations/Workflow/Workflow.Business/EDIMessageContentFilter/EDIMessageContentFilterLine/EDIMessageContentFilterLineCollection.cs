using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterLineCollection : NonPersistentBusinessObjectCollection<EDIMessageContentFilterLine>
	{
		public EDIMessageContentFilterLineCollection(EDIMessageContentFilterSpec parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
		}

		public EDIMessageContentFilterSpec Parent { get; }
		protected override BusinessObject CreateNonPersistentBusinessObject() => new EDIMessageContentFilterLine(Parent);
	}
}
