using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ACEInputBlockControlGenerator : ABIInputBlockControlGenerator<AABIInputB, AABIInputY>
	{
		/// <summary>
		/// Statements are created from messages Customs sent. There is no Branch associated with a statement, instead, there are entry filer code and processing port code.
		/// </summary>
		/// <param name="companyPK">Mandatory. To retrieve a sender entry filer code from registry which might be different to entry filer code that prepends entry number for PSC situation</param>
		/// <param name="processingPortCode">Mandatory</param>
		/// <param name="processingOfficeCode">Optional</param>
		public ACEInputBlockControlGenerator(ZString filerCode, ZString processingPortCode, ZString processingOfficeCode)
		{
			B.ProcessingDistrictPortCode = processingPortCode;
			B.ProcessingFilerOfficeCode = processingOfficeCode;
			B.FilerPreparersUserDataText = EDIMessage.MessageNumberPlaceHolder;
			B.FilerCode = filerCode;
		}
		public ACEInputBlockControlGenerator(Guid companyPK, ZString processingPortCode, ZString processingOfficeCode)
			: this(USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).EntryFilerCode, processingPortCode, processingOfficeCode)
		{
		}

		public ACEInputBlockControlGenerator(IMessageAttacheeWithCBPSenderReference attachee)
			: this(attachee.CompanyPK, attachee.ProcessingDistrictPort, attachee.ProcessingOfficeCode)
		{
		}

		public ACEInputBlockControlGenerator(GlbBranch branch)
			: base(branch)
		{
		}

		public ACEInputBlockControlGenerator(GlbBranch branch, ZString processingPortCode)
			: base(branch, processingPortCode)
		{
		}

		protected override void SetupBlockYDetails()
		{
			Y.ApplicationIdentifierCode = B.ApplicationIdentifierCode;
			Y.FilerCode = B.FilerCode;
			Y.ProcessingDistrictPortCode = B.ProcessingDistrictPortCode;
			Y.ProcessingFilerOfficeCode = B.ProcessingFilerOfficeCode;
		}
	}

	public class ABIInputBlockControlGenerator : ABIInputBlockControlGenerator<APLB, APLY>
	{
		/// <summary>
		/// Statements are created from messages Customs sent. There is no Branch associated with a statement, instead, there are entry filer code and processing port code.
		/// </summary>
		/// <param name="entryFilerCode">Mandatory</param>
		/// <param name="processingPortCode">Mandatory</param>
		/// <param name="processingOfficeCode">Optional</param>
		public ABIInputBlockControlGenerator(ZString entryFilerCode, ZString processingPortCode, ZString processingOfficeCode)
			: base(entryFilerCode, processingPortCode, processingOfficeCode)
		{
			B.BlockNumber = 1;
		}

		public ABIInputBlockControlGenerator(GlbBranch branch)
			: base(branch)
		{
			B.BlockNumber = 1;
		}

		public ABIInputBlockControlGenerator(GlbBranch branch, ZString processingPortCode)
			: base(branch, processingPortCode)
		{
			B.BlockNumber = 1;
		}

		public ABIInputBlockControlGenerator(IMessageAttacheeWithCBPSenderReference attachee)
			: base(attachee)
		{
			B.BlockNumber = 1;
		}
	}

	public class BIRDInputBlockControlGenerator : ABIInputBlockControlGenerator<BRDAA, BRDZZ>
	{
		/// <summary>
		/// Statements are created from messages Customs sent. There is no Branch associated with a statement, instead, there are entry filer code and processing port code.
		/// </summary>
		/// <param name="entryFilerCode">Mandatory</param>
		/// <param name="processingPortCode">Mandatory</param>
		/// <param name="processingOfficeCode">Optional</param>
		public BIRDInputBlockControlGenerator(ZString entryFilerCode, ZString processingPortCode, ZString processingOfficeCode, string birdApplicationCode, string originatingBrokerRef)
			: base(entryFilerCode, processingPortCode, processingOfficeCode)
		{
			SetMandatoryFields(birdApplicationCode, originatingBrokerRef);
		}

		public BIRDInputBlockControlGenerator(GlbBranch branch, string birdApplicationCode, string originatingBrokerRef)
			: base(branch)
		{
			SetMandatoryFields(birdApplicationCode, originatingBrokerRef);
		}

		public BIRDInputBlockControlGenerator(IMessageAttacheeWithCBPSenderReference attachee, string birdApplicationCode, string originatingBrokerRef)
			: base(attachee)
		{
			SetMandatoryFields(birdApplicationCode, originatingBrokerRef);
		}

		void SetMandatoryFields(string birdApplicationCode, string originatingBrokerRef)
		{
			B.ApplicationCode = birdApplicationCode;
			Y.ApplicationCode = birdApplicationCode;
			B.OriginatingBrokerRef = originatingBrokerRef;

			ZDateTime now = ZDateTime.Now;
			B.CreationDate = now.Date;
			B.CreationTime = now.Hour.ToString().PadLeft(2, '0') + now.Minute.ToString().PadLeft(2, '0') + now.Second.ToString().PadLeft(2, '0');
		}
	}

	public class ABIInputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : ImportInputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IABIControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IABIControlMessageBlockY, new()
	{
		public ABIInputBlockControlGenerator()
		{
		}

		/// <summary>
		/// Statements are created from messages Customs sent. There is no Branch associated with a statement, instead, there are entry filer code and processing port code.
		/// </summary>
		/// <param name="entryFilerCode">Mandatory</param>
		/// <param name="processingPortCode">Mandatory</param>
		/// <param name="processingOfficeCode">Optional</param>
		public ABIInputBlockControlGenerator(ZString entryFilerCode, ZString processingPortCode, ZString processingOfficeCode)
		{
			B.ProcessingDistrictPortCode = processingPortCode;
			B.EntryFilerCode = entryFilerCode;
			B.ProcessingOfficeCode = processingOfficeCode;
			B.UserData = EDIMessage.MessageNumberPlaceHolder;
		}

		public ABIInputBlockControlGenerator(GlbBranch branch) : this(GetEntryFilerCode(branch), InputBlockControlGeneratorHelper.GetProcessingDistrictPortCode(branch), GetBRecordOfficeCode(branch))
		{
		}

		public ABIInputBlockControlGenerator(GlbBranch branch, ZString processingPortCode)
			: this(branch)
		{
			B.ProcessingDistrictPortCode = processingPortCode;
		}

		public ABIInputBlockControlGenerator(IMessageAttacheeWithCBPSenderReference attachee)
			: this(attachee.Branch)
		{
			B.EntryFilerCode = attachee.EntryFilerCode;
			B.ProcessingDistrictPortCode = attachee.ProcessingDistrictPort;
			B.ProcessingOfficeCode = attachee.ProcessingOfficeCode;
		}

		protected override void SetupBlockYDetails()
		{
			Y.ApplicationIdentifier = B.ApplicationIdentifier;
			Y.EntryFilerCode = B.EntryFilerCode;
			Y.ProcessingDistrictPortCode = B.ProcessingDistrictPortCode;
			Y.NumberOfTransactionDetailRecordsInTheBlock = messageBlocks.Count;
		}

		static string GetEntryFilerCode(GlbBranch branch)
		{
			return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
		}

		static string GetBRecordOfficeCode(GlbBranch branch)
		{
			return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
		}
	}

	public class AESInputBlockControlGenerator : ExportInputBlockControlGenerator<AESCommShipBXP, AESCommShipYXP>
	{
		public AESInputBlockControlGenerator()
		{
		}

		public AESInputBlockControlGenerator(IAESTIRMessageAttachee attachee)
		{
			Argument.NotNull(attachee, "attachee");

			IAESTIRParty usppi = attachee.USPPI;
			if (usppi != null)
			{
				B.USPPIID = usppi.PartyID;
				B.USPPIIDType = usppi.PartyIDType;
				B.USPPIName = usppi.PartyName;

				Y.USPPIID = B.USPPIID;
				Y.USPPIIDType = B.USPPIIDType;
				Y.USPPIName = B.USPPIName;
			}
		}

		protected override ZString ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.AES.CommodityShipment; }
		}
	}

	public class ImportInputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : InputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public ImportInputBlockControlGenerator()
			: base(CBPEDIInterchange.ApplicationCodes.USCustomsImport)
		{
		}
	}

	public class ExportInputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : InputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public ExportInputBlockControlGenerator()
			: base(CBPEDIInterchange.ApplicationCodes.USCustomsExport)
		{
		}
	}

	public abstract class InputBlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected InputBlockControlGenerator(ZString applicationCode)
			: base(applicationCode)
		{
		}

		protected override MessageBlockDeserialiser MessageBlockDeserialiser
		{
			get { return new InputMessageBlockDeserialiser(); }
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
