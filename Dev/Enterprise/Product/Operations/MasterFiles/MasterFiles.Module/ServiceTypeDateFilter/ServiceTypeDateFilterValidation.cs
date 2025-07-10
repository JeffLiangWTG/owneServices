using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class ServiceTypeDateFilterValidation : ModuleFilterDateValidation
	{
		public ServiceTypeDateFilterValidation(ServiceTypeDateFilter parent) : base(parent)
		{
			this.Parent = parent;
		}

		protected new readonly ServiceTypeDateFilter Parent;

		public void ValidateJobServiceType()
		{
			ValidateCalculatedProperty(Parent.JobServiceTypeInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJobServiceType();
		}

		protected virtual void CheckJobServiceType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JobServiceTypeInfo, Parent.JobServiceType_List);
		}
	}
}
