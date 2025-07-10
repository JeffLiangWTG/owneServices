using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefShippingLineVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var shippingLine = new RefShippingLine
			{
				RSL_PK = Guid.NewGuid(),
				RSL_CarrierName = "Carrier",
				RSL_CargoWiseOneCode = "CW1",
				RSL_CargoSphereRatesAvailable = true,
				RSL_ContainerAutomationAvailable = true,
				RSL_GlobalSailingScheduleAvailable = true,
				RSL_InvoiceAvailable = true,
				RSL_IsActive = true,
				RSL_IsNVO = true,
				RSL_OceanCarrierMessagingAvailable = true,
				RSL_StandardCarrierAlphaCode = "TSTT",
				RSL_IsCW1User = false,
				RSL_EHubIds = string.Empty,
				RSL_ShippingLineLogo = new byte[1020]
			};
			result.Add(shippingLine);

			var shippingLineMessagingRequirement = new RefShippingLineMessagingRequirement
			{
				RSR_PK = Guid.NewGuid(),
				RSR_RSL_ShippingLine = shippingLine.RSL_PK,
				RSR_RST_NKType = "AAA",
				RSR_IsBookingRequest = false,
				RSR_IsShippingInstruction = false
			};
			result.Add(shippingLineMessagingRequirement);
			var eblProvider = new RefShippingLineEBLProvider
			{
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = shippingLine.RSL_PK,
				RSE_Name = "Name 1",
				RSE_IsAvailable = true,
				RSE_IsDefault = false
			};
			result.Add(eblProvider);
			return result.ToArray();
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefShippingLine
			{
				RSL_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				RSL_CarrierName = "BBBBB",
				RSL_CargoWiseOneCode = "BBB",
				RSL_CargoSphereRatesAvailable = true,
				RSL_ContainerAutomationAvailable = true,
				RSL_GlobalSailingScheduleAvailable = true,
				RSL_InvoiceAvailable = true,
				RSL_IsActive = true,
				RSL_IsNVO = true,
				RSL_OceanCarrierMessagingAvailable = true,
				RSL_StandardCarrierAlphaCode = "BBBB",
				RSL_IsCW1User = false,
				RSL_EHubIds = string.Empty
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefShippingLineMessagingRequirement requirement)
			{
				requirement.RSR_RSL_ShippingLine = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefShippingLineEBLProvider eblProvider)
			{
				eblProvider.RSE_RSL_ShippingLine = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefShippingLine shippingLine)
			{
				shippingLine.RSL_CargoSphereRatesAvailable = false;
			}
			if (data is RefShippingLineMessagingRequirement shippingLineMessagingRequirement)
			{
				shippingLineMessagingRequirement.RSR_IsShippingInstruction = true;
			}
			if (data is RefShippingLineEBLProvider eblProvider)
			{
				eblProvider.RSE_IsAvailable = false;
			}

			return true;
		}
	}
}
