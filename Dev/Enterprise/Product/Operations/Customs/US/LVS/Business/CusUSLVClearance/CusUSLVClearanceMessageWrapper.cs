using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceMessageWrapper : NonPersistentBusinessObject
	{
		public CusUSLVClearanceMessageWrapper(CusUSLVClearance clearance)
			: base(clearance.Factory)
		{
			Clearance = clearance;
		}

		public CusUSLVClearanceMessageWrapper(CusUSLVClearance clearance, CusUSLVConsignment consignment)
			: this(clearance, new[] { consignment })
		{
		}

		public CusUSLVClearanceMessageWrapper(CusUSLVClearance clearance, IEnumerable<CusUSLVConsignment> selectedConsignments)
			: base(clearance.Factory)
		{
			Clearance = clearance;
			SelectedConsignments.AddRange(selectedConsignments);
		}

		public CusUSLVClearance Clearance { get; }

		List<CusUSLVConsignment> SelectedConsignments => selectedConsignments ?? (selectedConsignments = new List<CusUSLVConsignment>());
		List<CusUSLVConsignment> selectedConsignments;

		public CusUSLVConsignmentForMessagingCollection CusUSLVConsignmentsToSend
		{
			get
			{
				if (cusUSLVConsignmentsToSend == null)
				{
					if (SelectedConsignments.Count == 0)
					{
						cusUSLVConsignmentsToSend = new CusUSLVConsignmentForMessagingCollection(Clearance);
					}
					else
					{
						cusUSLVConsignmentsToSend = new CusUSLVConsignmentForMessagingCollection(Factory, SelectedConsignments);
					}
				}
				return cusUSLVConsignmentsToSend;
			}
		}
		CusUSLVConsignmentForMessagingCollection cusUSLVConsignmentsToSend;

		public void ResetCusUSLVConsignmentsActions()
		{
			if (SelectedConsignments.Count == 0)
			{
				Clearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().ForEach(x => x.ResetAction());
			}
			else
			{
				SelectedConsignments.ForEach(consignment => consignment.ResetAction());
			}
		}
	}
}
