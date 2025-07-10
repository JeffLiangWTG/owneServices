
namespace Enterprise.Freight.Business
{
	using System.Collections;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Business.EventManagement;
	using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

	public abstract class CommonContainerProcessHandlingInfo : ProcessHandlingInfo
	{
		public CommonContainerProcessHandlingInfo(CommonContainer container)
			: base(container)
		{
			this.container = container;
		}

		readonly CommonContainer container;

		protected override bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode)
		{
			if (eventCode.EqualsIgnoringCase(Events.ChangeOfIdentifierCode))
			{
				return true;
			}

			return base.IsEventExcludedFromCascadingOrPropagation(eventCode);
		}

		protected PropagationLink GetDeclarationPropagationLink()
		{
			PropagationLink result = null;

			if (!container.IsDeleted)
			{
				var declaration = container.Declaration;

				var isCancellableAndNotCancelled = declaration is ICancellable cancellableDeclaration && !cancellableDeclaration.IsCancelled;
				var isNotCancellable = declaration != null && !(declaration is ICancellable);

				if (isCancellableAndNotCancelled || isNotCancellable)
				{
					result = new PropagationLink((IStmALogParent)declaration, GetContainersFromDeclaration(declaration), "Declaration Containers");
				}
			}

			return result;
		}

		IEnumerable<BusinessObject> GetContainersFromDeclaration(BusinessObject declaration)
		{
			foreach (BusinessObject cusContainer in (IEnumerable)declaration["CusContainers"])
			{
				yield return cusContainer["JobContainer"] as BusinessObject;
			}
		}

		protected override IEnumerable<string> PopulateEventParametersToMatchDuringPropagation(ZString eventCode)
		{
			switch (eventCode)
			{
				case Events.DehireCode:
					return System.Array.Empty<string>();

				case Events.QuantityVerifiedCode:
					return new string[] { Params.Type };

				default:
					return base.PopulateEventParametersToMatchDuringPropagation(eventCode);
			}
		}
	}
}
