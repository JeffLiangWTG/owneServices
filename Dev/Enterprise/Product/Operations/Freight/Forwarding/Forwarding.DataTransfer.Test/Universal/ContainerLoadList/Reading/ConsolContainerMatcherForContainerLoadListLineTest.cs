using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerMode = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerMode;
using UniversalContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ConsolContainerMatcherForContainerLoadListLineTest : TestCaseWithFactory
	{
		public void TestGetMatchedList()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			var matcher = new ConsolContainerMatcherForContainerLoadListLine(consol);

			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var containerDataObject = new UniversalContainer
			{
				ContainerNumber = "VHVW2615198",
				ContainerType = new UniversalContainerType { Code = "20GP" },
				FCL_LCL_AIR = new UniversalContainerMode { Code = "FCL" },
				DeliveryMode = "CY/CY"
			};

			foreach (var containerNum in new ZString[] { "VHVW2615198", "KOMB9524910", ZString.Empty })
			{
				foreach (ICodeDescription containerMode in container.JC_ContainerMode_List)
				{
					foreach (ICodeDescription deliveryMode in container.JC_DeliveryMode_List)
					{
						foreach (var refContainerPK in new ZGuid[] { refContainer20GP.PK, refContainer40GP.PK, ZGuid.Empty })
						{
							container.JC_ContainerNum = containerNum;
							container.JC_ContainerMode = containerMode.Code;
							container.JC_DeliveryMode = deliveryMode.Code;
							container.JC_RC = refContainerPK;

							var matchedList = matcher.GetMatchedList(containerDataObject);

							if (containerNum == "VHVW2615198" && containerMode.Code == "FCL" && deliveryMode.Code == "CY/CY" && refContainerPK == refContainer20GP.PK)
							{
								AssertEquals(1, matchedList.Count);
							}
							else
							{
								AssertEquals(0, matchedList.Count);
							}
						}
					}
				}
			}

			container.JC_ContainerNum = "KOMB9524910";
			container.JC_ContainerMode = "FCL";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_RC = refContainer40GP.PK;

			foreach (var containerNum in new ZString[] { "VHVW2615198", "KOMB9524910", ZString.Empty })
			{
				foreach (var containerModeCode in container.JC_ContainerMode_List.GetAllCodes().Union(new string[] { string.Empty }))
				{
					foreach (var deliveryMode in container.JC_DeliveryMode_List.OfType<ICodeDescription>().Union(new ICodeDescription[] { null }))
					{
						foreach (var refContainerCode in new ZString[] { refContainer20GP.RC_Code, refContainer40GP.RC_Code, ZString.Empty })
						{
							containerDataObject.ContainerNumber = containerNum;
							containerDataObject.ContainerType = new UniversalContainerType { Code = refContainerCode };
							containerDataObject.FCL_LCL_AIR = new UniversalContainerMode { Code = containerModeCode };
							containerDataObject.DeliveryMode = deliveryMode?.Code ?? string.Empty;

							var matchedList = matcher.GetMatchedList(containerDataObject);

							if ((containerNum == string.Empty || containerNum == "KOMB9524910")
								&& (refContainerCode == string.Empty || refContainerCode == "40GP")
								&& (string.IsNullOrEmpty(containerModeCode) || containerModeCode == "FCL")
								&& (deliveryMode == null || string.IsNullOrEmpty(deliveryMode.Code) || deliveryMode.Code == "CY/CY")
								&& (containerNum != string.Empty || refContainerCode != string.Empty))
							{
								AssertEquals(1, matchedList.Count);
							}
							else
							{
								AssertEquals(0, matchedList.Count);
							}
						}
					}
				}
			}
		}
	}
}
