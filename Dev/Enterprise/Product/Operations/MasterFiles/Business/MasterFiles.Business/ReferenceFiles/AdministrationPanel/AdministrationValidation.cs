using System;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Not Finished Function")]
	public class AdministrationValidation : ZValidation
	{
		public AdministrationValidation(AdministrationPanelManager administrationPanel)
			: base(administrationPanel)
		{
			this.ParentBizO = administrationPanel;
			// this.ZValidationInternals = (IValidationInternals)this;
		}

		protected readonly AdministrationPanelManager ParentBizO;
		// private readonly IValidationInternals ZValidationInternals;
		public override void ValidateAll()
		{
			throw new NotImplementedException();
		}

		public override Type AutoValidationType { get; }
	}
}
