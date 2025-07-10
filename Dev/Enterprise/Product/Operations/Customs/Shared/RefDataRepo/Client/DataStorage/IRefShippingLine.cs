namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	using System;
	using System.Collections.Generic;
	using CargoWise.RefDbRepo.Client.Common;
	public partial interface IRefShippingLine : IDataSetStorage
	{
		Guid RSL_PK { get; set; }
		bool RSL_IsCW1User { get; set; }
		bool RSL_IsSystem { get; set; }
		bool RSL_IsActive { get; set; }
		bool RSL_IsNVO { get; set; }
		string RSL_CarrierName { get; set; }
		string RSL_StandardCarrierAlphaCode { get; set; }
		string RSL_CargoWiseOneCode { get; set; }
		bool RSL_OceanCarrierMessagingAvailable { get; set; }
		bool RSL_GlobalSailingScheduleAvailable { get; set; }
		bool RSL_ContainerAutomationAvailable { get; set; }
		bool RSL_CargoSphereRatesAvailable { get; set; }
		bool RSL_InvoiceAvailable { get; set; }
		bool RSL_BookingRequestAvailable { get; set; }
		bool RSL_ShippingInstructionAvailable { get; set; }
		bool RSL_VerifiedGrossContainerWeightAvailable { get; set; }
		bool RSL_ShippingOrderAvailable { get; set; }
		bool RSL_EManifestAvailable { get; set; }
		bool RSL_IsShippingLine { get; set; }
		byte[] RSL_ShippingLineLogo { get; set; }

		IEnumerable<IOrgHeader> OrgHeader { get; }
		IEnumerable<IRefShippingLineMessagingRequirement> RefShippingLineMessagingRequirement { get; }
	}
}
