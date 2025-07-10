using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for AccComplianceSequence.
	/// </summary>
	public class AccComplianceSequenceController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccComplianceSequenceController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccComplianceSequence;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccComplianceSequence; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccComplianceSequence); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccComplianceSequenceForm((AccComplianceSequence)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.ComplianceSequencesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.ComplianceSequencesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.ComplianceSequencesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.ComplianceSequences;
			}
		}
	}
}
