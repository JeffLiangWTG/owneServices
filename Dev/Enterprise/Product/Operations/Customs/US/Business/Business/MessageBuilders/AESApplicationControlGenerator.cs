using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	sealed class AESApplicationControlGenerator : ApplicationControlGenerator<AESCommShipAXP, AESCommShipZXP>
	{
		public AESApplicationControlGenerator(GlbBranch branch)
			: base(branch)
		{
			var filer = USCustomsDataRegistry.Instance.ExportEntryFilerID.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

			var filerIDAlphanumeric = filer.EntryFilerID.KeepAlphanumericCharacters();
			var filerID = filer.EntryFilerIDType == "E" && filerIDAlphanumeric.Length == 11 && filerIDAlphanumeric.EndsWith("00") ? filerIDAlphanumeric.TrimEnd('0', '0') : filerIDAlphanumeric;

			A.FilerID = filerID;
			A.FilerIDType = filer.EntryFilerIDType;
			A.ApplicationIdentifier = ApplicationIdentifierCodeList.AES.CommodityShipment;
			A.TransmitterDate = ZDate.Today;
			A.TransmitterID = filerID;

			Z.FilerID = filerID;
			Z.FilerIDType = A.FilerIDType;
			Z.ApplicationIdentifier = A.ApplicationIdentifier;
			Z.TransmitterDate = A.TransmitterDate;
			Z.TransmitterID = filerID;
		}

		protected override void AddExtraInfo(Enterprise.Messaging.Business.EDIMessage message)
		{
			ZInt messageNum = ZInt.ParseEmptyAsZero(message.EM_MessageNum);
			A.BatchControlNumber = AESTIRMessageNumberEncoder.Encode(messageNum);
			Z.BatchControlNumber = A.BatchControlNumber;
		}
	}
}
