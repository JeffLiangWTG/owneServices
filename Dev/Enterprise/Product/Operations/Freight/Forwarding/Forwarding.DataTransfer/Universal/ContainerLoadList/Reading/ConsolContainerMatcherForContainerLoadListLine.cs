using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolContainerMatcherForContainerLoadListLine
	{
		readonly ForwardingConsol consol;

		public ConsolContainerMatcherForContainerLoadListLine(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public IList<ForwardingContainer> GetMatchedList(UniversalContainer universalContainer)
			=> consol
				.Containers
				.OfType<ForwardingContainer>()
				.Where(container => MatchContainer(container, universalContainer))
				.OrderByDescending(container => container.JC_ContainerCount)
				.ToList();

		public static bool MatchContainer(ForwardingContainer container, UniversalContainer universalContainer)
		{
			var containerNum = universalContainer.ContainerNumber.GetValueOrDefault();
			var refContainerCode = universalContainer.ContainerType?.Code ?? ZString.Empty;
			var containerModeCode = universalContainer.FCL_LCL_AIR?.Code ?? ZString.Empty;
			var deliveryMode = universalContainer.DeliveryMode ?? ZString.Empty;

			return ((containerNum.IsEmpty || containerNum == container.JC_ContainerNum)
				&& (refContainerCode.IsEmpty || refContainerCode == (container.Container?.RC_Code ?? ZString.Empty))
				&& (containerModeCode.IsEmpty || containerModeCode == container.JC_ContainerMode)
				&& (deliveryMode.IsEmpty || deliveryMode == container.JC_DeliveryMode)
				&& (!containerNum.IsEmpty || !refContainerCode.IsEmpty));
		}
	}
}
