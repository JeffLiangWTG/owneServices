using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business
{
	public class ABIOutputBlockControlGenerator : ABIOutputBlockControlGenerator<APLB, APLY>
	{
		public ABIOutputBlockControlGenerator()
			: this(ZString.Empty)
		{
		}

		public ABIOutputBlockControlGenerator(ZString alternativeAppIdCode)
			: base(alternativeAppIdCode)
		{
		}
	}

	public class ABIOutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : ImportOutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IABIControlMessageBlockY, new()
	{
		public ABIOutputBlockControlGenerator()
			: this(ZString.Empty)
		{
		}

		public ABIOutputBlockControlGenerator(ZString alternativeAppIdCode)
			: base()
		{
			this.alternativeAppIdCode = alternativeAppIdCode;
		}

		protected override void SetupBlockYDetails()
		{
			Y.ApplicationIdentifier = B.ApplicationIdentifier;
			Y.EntryFilerCode = B.EntryFilerCode;
			Y.ProcessingDistrictPortCode = B.ProcessingDistrictPortCode;
			Y.NumberOfTransactionDetailRecordsInTheBlock = messageBlocks.Count;
		}

		readonly ZString alternativeAppIdCode;

		protected override ZString ApplicationIdentifier
		{
			get { return string.IsNullOrEmpty(base.ApplicationIdentifier) ? alternativeAppIdCode : base.ApplicationIdentifier; }
		}
	}

	public class ImportOutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : OutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public ImportOutputBlockControlGenerator()
			: base(CBPEDIInterchange.ApplicationCodes.USCustomsImport)
		{
		}
	}

	public class ExportOutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : OutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public ExportOutputBlockControlGenerator()
			: base(CBPEDIInterchange.ApplicationCodes.USCustomsExport)
		{
		}
	}

	public abstract class OutputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected OutputBlockControlGenerator(ZString applicationCode)
			: base(applicationCode)
		{
		}

		protected override MessageBlockDeserialiser MessageBlockDeserialiser
		{
			get { return new OutputMessageBlockDeserialiser(); }
		}

		protected override ZString ApplicationIdentifier
		{
			get { return B.ApplicationIdentifier; }
		}

		protected override void EnsureBBlockCanBeDeserialise(string message)
		{
			if (!message.StartsWith("B") && !message.StartsWith("AA"))
			{
				throw new InvalidMessageFormatException("message does not start with a 'B' or 'AA' block");
			}
		}

		protected override void EnsureYBlockCanBeDeserialise(string yBlock)
		{
			if (!yBlock.StartsWith("Y") && !yBlock.StartsWith("ZZ"))
			{
				throw new InvalidMessageFormatException("message does not end with a 'Y' or 'ZZ' block");
			}
		}
	}
}
