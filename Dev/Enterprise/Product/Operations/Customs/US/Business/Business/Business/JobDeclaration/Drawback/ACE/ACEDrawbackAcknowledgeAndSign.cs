using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackAcknowledgeAndSign : NonPersistentBusinessObject, IObsoleteValidation, IACEDrawbackAcknowledgeAndSign
	{
		public ACEDrawbackAcknowledgeAndSign(JobDeclaration declaration, UpdateActionCode actionCode)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			this.actionCode = actionCode;
		}
		readonly JobDeclaration declaration;
		readonly UpdateActionCode actionCode;

		public JobDeclaration Declaration
		{
			get { return declaration; }
		}

		#region Properties

		[BusinessObjectTestExclude]
		public ZBool US_SendMessage
		{
			get { return fUS_SendMessage; }
			set
			{
				SetNonPersistentPropertyValue(US_SendMessageInfo, ref fUS_SendMessage, value);
				ReGenerateMessageContents();

				if (!IsValidationSuspended)
				{
					ValidateUS_SendMessage();
				}
			}
		}
		ZBool fUS_SendMessage = false;

		public ZPropertyInfo US_SendMessageInfo
		{
			get { return GetZPropertyInfo(nameof(US_SendMessage)); }
		}

		[BusinessObjectTestExclude]
		public ZBool US_AcknowledgeAndSign
		{
			get { return fUS_AcknowledgeAndSign; }
			set
			{
				SetNonPersistentPropertyValue(US_AcknowledgeAndSignInfo, ref fUS_AcknowledgeAndSign, value);
				ReGenerateMessageContents();

				if (!IsValidationSuspended)
				{
					ValidateUS_AcknowledgeAndSign();
				}
			}
		}
		ZBool fUS_AcknowledgeAndSign = true;

		public ZPropertyInfo US_AcknowledgeAndSignInfo
		{
			get { return GetZPropertyInfo(nameof(US_AcknowledgeAndSign)); }
		}

		public bool US_AcknowledgeAndSign_ReadOnly
		{
			get { return !US_SendMessage; }
		}

		public ZString US_MessageContents
		{
			get { return GetSerialiseMessageContents(); }
		}

		ZString GetSerialiseMessageContents()
		{
			if (!messageContentsCached.HasValue)
			{
				messageContentsCached = new ACEDrawbackSummaryMessageBuilder(new JobDeclarationDrawbackSupporter(declaration, actionCode), this).GetSerialiseMessageContents();
			}

			return messageContentsCached.Value;
		}
		ZString? messageContentsCached;

		void ReGenerateMessageContents()
		{
			var oldMessageContents = ZString.Empty;
			if (messageContentsCached.HasValue)
			{
				oldMessageContents = messageContentsCached.Value;
				messageContentsCached = null;
			}
			US_MessageContentsInfo.RefreshBinding(oldMessageContents);
		}

		public ZPropertyInfo US_MessageContentsInfo
		{
			get { return GetZPropertyInfo(nameof(US_MessageContents)); }
		}

		public bool ShouldSendMessage
		{
			get { return fShouldSendMessage; }
			set { fShouldSendMessage = value; }
		}
		bool fShouldSendMessage;

		public void GenerateMessages()
		{
			new ACEDrawbackSummaryMessageBuilder(new JobDeclarationDrawbackSupporter(declaration, actionCode), this).Build();
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_SendMessage();
			ValidateUS_AcknowledgeAndSign();
		}

		void ValidateUS_SendMessage()
		{
			US_SendMessageInfo.ClearAllNotifications();

			var hasBeenLodgedAtCustoms = Declaration.HasBeenLodgedAtCustoms;
			if (hasBeenLodgedAtCustoms && actionCode == UpdateActionCode.Add)
			{
				US_SendMessageInfo.AddMessageError(OriginalSummaryHasBeenAccepted);
			}
			else
			{
				if (actionCode == UpdateActionCode.Replace && !hasBeenLodgedAtCustoms)
				{
					US_SendMessageInfo.AddMessageError(ReplaceMessageCannotBeSent);
				}
			}
		}
		internal const string OriginalSummaryHasBeenAccepted = "Your drawback filing was previously accepted. This should be filed as Replace.";
		internal const string ReplaceMessageCannotBeSent = "Your drawback filing had never been accepted. This should be field as Original.";

		void ValidateUS_AcknowledgeAndSign()
		{
			US_AcknowledgeAndSignInfo.ClearAllNotifications();

			if (US_SendMessage && !US_AcknowledgeAndSign)
			{
				US_AcknowledgeAndSignInfo.AddMessageError(CannotSendUnlessAcknowledgedAndSigned);
			}
		}
		internal const string CannotSendUnlessAcknowledgedAndSigned = "Should be acknowledged and signed.";

		#endregion

		#region IACEDrawbackAcknowledgeAndSign Members

		ZBool IACEDrawbackAcknowledgeAndSign.US_AcknowledgeAndSign
		{
			get { return US_AcknowledgeAndSign; }
		}

		#endregion
	}
}
