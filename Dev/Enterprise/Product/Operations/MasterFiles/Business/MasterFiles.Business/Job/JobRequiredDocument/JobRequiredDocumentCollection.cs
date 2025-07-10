using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentCollection : BusinessObjectCollection<JobRequiredDocument>
	{
		public JobRequiredDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobRequiredDocumentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public Type ParentType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parentType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { parentType = value; }
		}

		Type parentType;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			JobRequiredDocument doc = (JobRequiredDocument)bizOAdded;
			if (parentType != null)
			{
				doc.ParentType = parentType;
			}
			base.OnAdded(doc);
		}
	}
}
