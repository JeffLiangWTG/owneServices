using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public abstract class USOperationalActionMethod : OperationalActionMethod
	{
		public USOperationalActionMethod(ZGuid guid)
			: base(guid)
		{
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();

			result.Add(FilterConstants.Country, new string[]
			{
					Constants.CountryCodes.UnitedStates
			});

			return result;
		}
	}
}
