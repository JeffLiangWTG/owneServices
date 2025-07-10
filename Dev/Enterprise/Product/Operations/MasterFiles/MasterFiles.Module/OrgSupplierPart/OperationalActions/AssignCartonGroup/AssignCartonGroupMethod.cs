using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class AssignCartonGroupMethod : OperationalActionMethod
	{
		public AssignCartonGroupMethod()
			: base(new ZGuid("2ec39e09-d0cf-4295-9d67-cd35f7eaa167"))
		{
		}

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AssignCartonGroupMethodApplicator(Name, factory);
		}

		#endregion

		#region NewGuiControl

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new AssignCartonGroupControl();
		}

		#endregion

		#region Name

		public override string Name
		{
			get { return Res.GetString("6c5e5c7a-9ced-4512-b743-93302c551a08", "Assign Carton Group to Product Relationships"); }
		}

		#endregion

		#region Description

		public override string Description
		{
			get { return Res.GetString("6c5e5c7a-9ced-4512-b743-93302c551a08", "Assign Carton Group to Product Relationships"); }
		}

		#endregion
	}
}
