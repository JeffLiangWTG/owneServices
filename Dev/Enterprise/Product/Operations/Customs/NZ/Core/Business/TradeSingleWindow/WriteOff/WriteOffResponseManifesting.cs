using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class WriteOffResponseManifesting : WriteOffResponseDeclaration
	{
		internal WriteOffResponseManifesting(BaseTSWResponse response)
			: base(response)
		{
		}

		public new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}

		#region Overrides

		protected override IEnumerable<ZString> ConsignmentIDs => EntryHeader.Declarations.Cast<Declaration.JobDeclaration>().Select(declaration => declaration.JE_HouseBill);

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return new ConsignmentDeclaration(this, id, status);
		}

		protected override string GetJobID()
		{
			return EntryHeader.CH_BGMReference;
		}

		protected override string GetJobName()
		{
			return "ECI Manifest";
		}

		protected override ZString MasterBillCore
		{
			get { return EntryHeader.ManifestWrapper.FormattedMasterBill; }
		}

		protected override void SetCustomsStatus(ZString newValue)
		{
			base.SetCustomsStatus(newValue);
			foreach (Declaration.JobDeclaration declaration in EntryHeader.Declarations)
			{
				var decITR = declaration.TranshipmentRequest;
				if (decITR != null && (!decITR.C4_ModeOfMovement.IsEmpty || !decITR.C4_TranshipModeOfMovement.IsEmpty))
				{
					decITR.C4_Status = TSWStatus.CalculateCombinedMovementStatus();
				}
			}
		}

		#endregion // Overrides

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}
	}
}

// Tested in
//	- CREMessageProcessorManifestingTest
//	- ICRMessageProcessorManifestingTest
