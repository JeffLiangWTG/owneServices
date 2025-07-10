using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	abstract class ABIApplicationControlGenerator<ControlMessageBlockA, ControlMessageBlockZ> : ApplicationControlGenerator<ControlMessageBlockA, ControlMessageBlockZ>
		where ControlMessageBlockA : MessageBlock, IABIControlMessageBlockA, new()
		where ControlMessageBlockZ : MessageBlock, IABIControlMessageBlockZ, new()
	{
		protected ABIApplicationControlGenerator(GlbBranch branch)
			: base(branch)
		{
			A.ReceiverSiteCode = USCustomsDataRegistry.Instance.ReceiverDistrictPort.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			A.ReceiverFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).EntryFilerCode;
			A.ReceiverOfficeCode = USCustomsDataRegistry.Instance.ARecordOfficeCode.Value;
			A.TransmissionDate = ZDate.Today;

			Z.ReceiverSiteCode = A.ReceiverSiteCode;
			Z.ReceiverFilerCode = A.ReceiverFilerCode;
			Z.ReceiverOfficeCode = A.ReceiverOfficeCode;
			Z.TransmissionDate = A.TransmissionDate;
		}
	}
}
