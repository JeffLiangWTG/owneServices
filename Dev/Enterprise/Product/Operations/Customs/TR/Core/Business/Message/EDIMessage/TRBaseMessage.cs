using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class TRBaseMessage : EDIMessage
	{
		public TRBaseMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.TRMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);

		public ZBool NeedToSignMessage => TRBaseMessageExtensions.IsMessageSigningRequired(this.GetType(), EM_MessageType);

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		public override ZString EM_FormattedMessageText => EM_MessageInterpretation;

		public override ZString EM_MessageInterpretation
		{
			get => MessageInterpretationNoteManager.Value;
			set
			{
				MessageInterpretationNoteManager.Value = value;
				EM_MessageInterpretationInfo.RefreshBinding();
			}
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			return true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = GetApplicationCodeCore();
		}

		protected virtual ZString GetApplicationCodeCore()
		{
			return ZString.Empty;
		}

		public override bool UsesPlaceHolders => false;

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				return Factory.GetCachedValue(nameof(TRBaseMessage) + nameof(MessageSubTypeList), () =>
				{
					var result = new EDIMessageSubTypeList();
					result.AddPairIfNotExist(Business.MessageSubTypeList.Codes.Normal, Business.MessageSubTypeList.Descriptions.Normal);
					result.AddPairIfNotExist(Business.MessageSubTypeList.Codes.Error, Business.MessageSubTypeList.Descriptions.Error);

					return result;
				});
			}
		}

		SoapMessageObject messageObject;
		public SoapMessageObject MessageObject => messageObject ?? (messageObject = GetMessageObject());

		protected virtual SoapMessageObject GetMessageObject() => null;
	}
}
