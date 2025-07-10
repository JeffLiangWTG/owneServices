using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using LargeMessageHelper = Enterprise.Messaging.Business.LargeMessageHelper;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class CBPEDIInterchange : Enterprise.Messaging.Business.EDIInterchange
	{
		public CBPEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString EI_InterchangeText
		{
			get
			{
				return EI_ApplicationCode == ApplicationCodes.USeManifest ? base.EI_InterchangeText
						: (ZString)(EI_HeaderText.PadRight(80) + PaddedBody + EI_FooterText.PadRight(80));
			}
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return true; }
		}

		protected override LargeMessageHelper.TextPadder GetTextPadderForEI_InterchangeText()
		{
			return EI_ApplicationCode == ApplicationCodes.USeManifest ? base.GetTextPadderForEI_InterchangeText() : PadText;
		}

		ZString PadText(ZString text, LargeMessageHelper.TextPadderDataType dataType)
		{
			return text.IsEmpty && (dataType == LargeMessageHelper.TextPadderDataType.Header || dataType == LargeMessageHelper.TextPadderDataType.Footer) ? "".PadRight(80) : BlockPadder.Pad(text);
		}

		ZString PaddedBody
		{
			get { return PadText(EI_BodyText, LargeMessageHelper.TextPadderDataType.Body); }
		}

#if DEBUG
		internal const string ApplicationCodeForTesting = "£¢¥";
#endif
	}
}
