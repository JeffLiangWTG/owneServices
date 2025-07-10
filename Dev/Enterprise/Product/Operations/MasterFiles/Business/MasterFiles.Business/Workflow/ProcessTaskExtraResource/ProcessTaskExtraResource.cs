using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskExtraResource : AutoProcessTaskExtraResource
	{
		public ProcessTaskExtraResource(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[RelatedBusinessObject("Task")]
		public override ZGuid PE_P9
		{
			get { return base.PE_P9; }
			set { base.PE_P9 = value; }
		}

		[List("Lookups.StaffOrResources")]
		public override ZString PE_GS_NKStaffOrResource
		{
			get
			{
				return base.PE_GS_NKStaffOrResource;
			}
			set
			{
				base.PE_GS_NKStaffOrResource = value;
			}
		}

		public ProcessTask Task
		{
			get { return Factory.Load<ProcessTask>(PE_P9); }
		}

		#endregion

		public override bool ReadOnly
		{
			get
			{
				var readOnly = false;
				var task = ProcessTask;
				if (task != null)
				{
					readOnly = task.ReadOnly;
				}

				return readOnly || base.ReadOnly;
			}

			set => base.ReadOnly = value;
		}
	}
}
