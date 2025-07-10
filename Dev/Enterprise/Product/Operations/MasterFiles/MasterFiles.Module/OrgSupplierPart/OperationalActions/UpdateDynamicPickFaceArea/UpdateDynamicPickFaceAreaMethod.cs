using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateDynamicPickFaceAreaMethod : OperationalActionMethod
	{
		public UpdateDynamicPickFaceAreaMethod()
			: base(new ZGuid("fddf5d42-e769-4c9f-a489-61cf46059081"))
		{
		}

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateDynamicPickFaceAreaMethodApplicator(Name, factory);
		}

		#endregion

		#region NewGuiControl

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new UpdateDynamicPickFaceAreaControl();

		#endregion

		#region Name

		public override string Name => name;

		#endregion

		#region Description

		public override string Description => name;

		string name => Res.GetString("32a17b6c-8847-428a-8e14-896c1ddeeb72", "Update Dynamic Pick Face Area");

		#endregion
	}
}
