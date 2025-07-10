using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class BatchDocumentsOperationalActionMethod : OperationalActionMethod
	{
		public BatchDocumentsOperationalActionMethod(ZGuid methodID) : base(methodID)
		{
		}

		public override string Name => string.Empty;

		public override string Description => string.Empty;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PrintBatchDocumentsOperationalActionMethodApplicator(Name);
		}

		public override bool IsRunAgainDisabled => false;

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new PrintBatchDocumentsConfigurationControl();

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(FilterConstants.Country, new[] { Core.Constants.CountryCodes.Taiwan });
			return result;
		}
	}
}
