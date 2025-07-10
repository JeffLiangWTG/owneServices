using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowDescriptorCommonPropertyWhiteListHelper
	{
		public static readonly List<KeyValuePair<string, string>> WhiteList = new List<KeyValuePair<string, string>>
		{
			new ("JobDeclarationEventDataModel", (NoResString)"Origin"),
			new ("AgencyShipmentEventDataModel", (NoResString)"Origin"),
			new ("AgencyShipmentEventDataModel", (NoResString)"Destination"),
			new ("ForwardingShipmentEventDataModel", (NoResString)"Origin"),
			new ("ForwardingShipmentEventDataModel", (NoResString)"Destination"),
		};
	}
}
