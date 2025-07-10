using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	public class CBPMessageForTesting : CBPEDIMessage, Integration.Customs.US.ICBPMessageForTesting
	{
		public CBPMessageForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BlockControlGenerator MessageBlockForTesting = new BlockControlGeneratorForTesting();
		protected override BlockControlGenerator GetMessageBlock()
		{
			MessageBlockForTesting.Deserialise(BlockPadder.Pad(EM_MessageText));
			return MessageBlockForTesting;
		}

		public bool AllowExceeding9999LimitForTesting;
		protected override bool AllowExceeding9999Limit
		{
			get { return AllowExceeding9999LimitForTesting; }
		}

		public string MessageReferenceNumberForTesting;
		protected override string GetMessageReferenceNumber()
		{
			if (!string.IsNullOrEmpty(MessageReferenceNumberForTesting))
			{
				return MessageReferenceNumberForTesting;
			}

			return Environment.Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", "CBP").GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = CBPEDIInterchange.ApplicationCodeForTesting;
		}

		public string MessageNumberPlaceHolderOverrideForTesting;
		protected override string MessageNumberPlaceHolderOverride
		{
			get { return MessageNumberPlaceHolderOverrideForTesting ?? base.MessageNumberPlaceHolderOverride; }
		}

		protected override bool IsMessageSendWithMessageErrorsCorrectCore()
		{
			return Factory.GetValue<FactoryValues>() == null || Factory.GetValue<FactoryValues>().Value;
		}

		public void PopulateMessageNumberForTest()
		{
			PopulateMessageNumber();
		}
	}
}
