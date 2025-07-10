using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterDocumentCollection : NonPersistentBusinessObjectCollection<EDIMessageContentFilterDocument>
	{
		public EDIMessageContentFilterDocumentCollection(EDIMessageContentFilterSpec parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
		}

		public EDIMessageContentFilterSpec Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EDIMessageContentFilterDocument(Parent);
	}
}
