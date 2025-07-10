using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVConsignmentController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is CusUSLVConsignment consignment)
			{
				return new CusUSLVConsignmentForm(consignment);
			}
			else if (businessEntity is JobDeclaration declaration)
			{
				return new JobDeclarationForm(declaration);
			}
			else if (businessEntity is USConsignmentCombined view)
			{
				if (view.UBV_JobType == USConsignmentCombinedJobTypes.Codes.Consignment)
				{
					return new CusUSLVConsignmentForm(Factory.Load<CusUSLVConsignment>(view.PK));
				}
				else if (view.UBV_JobType == USConsignmentCombinedJobTypes.Codes.Declaration)
				{
					return new JobDeclarationForm(Factory.Load<JobDeclaration>(view.PK));
				}
			}

			throw new InvalidOperationException("Unexpected error loading form for selected bill");
		}

		public override ControllerID ID => ControllerIDs.Customs.US.USLowValueEntriesBill;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USLowValueEntriesBill;

		public override Type TypeOfTopLevelBusinessObject => typeof(USConsignmentCombined);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USLVClearanceView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USLVClearanceEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion
	}
}
