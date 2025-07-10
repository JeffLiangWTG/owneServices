using System.Data;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMEDIMessage : EDIMessage
	{
		public UEMEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
		}

		string Sender => ApplicationCodeList.Codes.USExportManifest + "SND";

		string Receiver => ApplicationCodeList.Codes.USExportManifest + "RCV";

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		protected override string MessageNumberPlaceHolderOverride => UEMMessageNumberPlaceHolder;

		public override ZString EM_FormattedMessageText => XDocument.Parse(base.EM_MessageText).ToString();

		internal const string UEMMessageNumberPlaceHolder = "!MessageNumberPlaceHolder!";

		protected override string GetMessageReferenceNumber()
		{
			var company = Company ?? GlbCompany.CurrentCompany;
			return company.LicenceKeyIdentifier + "_" + Environment.Env.NumberFountains.EDIFACTNumberFountain("M", Sender, Receiver).GetNextFormatted(Factory);
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (fEM_MessageInterpretation.IsEmpty)
				{
					fEM_MessageInterpretation = new UEMEDIMessageHtmlPrettier().PrettyHtml(EM_MessageText);
				}
				return fEM_MessageInterpretation;
			}
		}
		ZString fEM_MessageInterpretation;

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;
	}
}
