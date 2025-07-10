using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemRequestLink : AutoWorkItemRequestLink, IPivotBusinessObject
	{
		public WorkItemRequestLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(WorkItem))]
		public override ZGuid WKL_WKI_WorkItem
		{
			get => base.WKL_WKI_WorkItem;
			set => base.WKL_WKI_WorkItem = value;
		}

		[RelatedBusinessObject(nameof(WorkRequest))]
		public override ZGuid WKL_WKR_Request
		{
			get => base.WKL_WKR_Request;
			set => base.WKL_WKR_Request = value;
		}

		#endregion

		#region Related Business Objects

		public WorkItem WorkItem => Factory.Load<WorkItem>(WKL_WKI_WorkItem);

		public WorkRequest WorkRequest => Factory.Load<WorkRequest>(WKL_WKR_Request);

		#endregion

		#region IPivotBusinessObject Members

		public ZGuid Relation1ID
		{
			get => WKL_WKR_Request;
			set => WKL_WKR_Request = value;
		}

		public BusinessObject Relation1Object => WorkRequest;

		public ZGuid Relation2ID
		{
			get => WKL_WKI_WorkItem;
			set => WKL_WKI_WorkItem = value;
		}

		public BusinessObject Relation2Object => WorkItem;

		#endregion
	}
}
