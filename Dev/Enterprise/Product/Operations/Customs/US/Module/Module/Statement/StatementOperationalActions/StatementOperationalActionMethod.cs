using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module
{
	public class StatementOperationalActionMethod : USOperationalActionMethod
	{
		public StatementOperationalActionMethod()
			: base(new ZGuid("A80EC886-FD52-41E6-9085-655C3DD7610F"))
		{
		}

		public override string Name => "Send ACH Payment Authorization operational action";

		public override string Description => "Send ACH Payment Authorization (US)";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new StatementActionMethodApplicator(factory);

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new USStatementOperationActionControl();

		public override bool HasSettings => false;

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(FilterConstants.Country, new string[] { Constants.CountryCodes.UnitedStates });
			return result;
		}
	}
}
