using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListProcessTaskCollection : ProcessTaskCollection
	{
		public RefComplianceListProcessTaskCollection(BusinessObject parent) : base(parent)
		{
		}

		public RefComplianceListProcessTaskCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefComplianceListProcessTaskCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public RefComplianceListProcessTaskCollection(BusinessObject parent, ZQuery additionalFilter) : base(parent, additionalFilter)
		{
		}

		public new RefComplianceListProcessTask this[int index]
		{
			get { return (RefComplianceListProcessTask)Elements[index]; }
		}

		public new RefComplianceListProcessTask AddNew()
		{
			return (RefComplianceListProcessTask)base.AddNew();
		}

		public new RefComplianceList Parent
		{
			get { return (RefComplianceList)base.Parent; }
		}
	}
}
