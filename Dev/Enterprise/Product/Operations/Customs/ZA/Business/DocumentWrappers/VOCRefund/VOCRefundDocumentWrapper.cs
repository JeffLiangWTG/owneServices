using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class VOCRefundDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper
	{
		public VOCRefundDocumentWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			this.EntryHeader = cusEntryHeader;
			var messageSendingObject = new MessageSendingObject(EntryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			CUSDECSource = messageSendingObject;
		}

		VOCRefundDocumentWrapper(CUSDECEDIMessage message)
			: base(message?.Factory ?? new BusinessObjectFactory())
		{
			var linkedEntryHeader = message?.EM_LinkedObject as CusEntryHeader;
			this.EntryHeader = linkedEntryHeader;
			SourceMessage = message;
			CUSDECSource = CUSDECMessageHelper.New(message);
		}

		internal static VOCRefundDocumentWrapper NewForMessage(CUSDECEDIMessage message)
		{
			VOCRefundDocumentWrapper result = null;
			if ((message?.EM_LinkedObject as CusEntryHeader) != null)
			{
				var resultwrapper = new VOCRefundDocumentWrapper(message);
				if (resultwrapper.CUSDECSource != null)
				{
					result = resultwrapper;
				}
			}
			return result;
		}

		#region Related Objects

		public CusEntryHeader EntryHeader { get; private set; }

		internal ZAMessage SourceMessage;
		internal ICUSDECMessageDataProvider CUSDECSource;

		#endregion

		public ZString LocalReferenceNumber => CUSDECSource.LocalReferenceNumber;

		public ZString MovementReferenceNumber => CUSDECSource.OriginalMRN;

		public ZString CaseNumber => CUSDECSource.CaseNumber;

		public ZString AgentCode => CUSDECSource.AgentCode;

		public GlbStaff DeclarantUser => GlbStaff.CurrentUser;

		public ZString VOCReason => SourceMessage?.VOCReason ?? ZString.Empty;

		public ZDecimal RefundAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (SourceMessage != null)
				{
					result = (SourceMessage as IVOCAfterValues).GetAmountDue() - (SourceMessage as IVOCBeforeValues).GetAmountDue();
				}
				else
				{
					result = EntryHeader.AmountDueDifference;
				}

				if (result < ZDecimal.Zero)
				{
					result = Math.Abs(result);
				}
				else
				{
					result = decimal.Zero;
				}

				return result.Round(2);
			}
		}

		#region Organizations Related

		public OrgHeader EffectiveAgent => OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, Core.Constants.CountryCodes.SouthAfrica);

		public ZString EffectiveAgentAddress => EffectiveAgent != null
			? (ZString)new AddressFormatter(Factory, EffectiveAgent, GlbCompany.CurrentCompany, false).PostalAddress()
			: ZString.Empty;

		#endregion

		#region PlaceHolder

		public ZString CheckBox1PlaceHolder { get; }
		public ZString CheckBox2PlaceHolder { get; }
		public ZString CheckBox3PlaceHolder { get; }
		public ZString CheckBox4PlaceHolder { get; }
		public ZString CheckBox5PlaceHolder { get; }
		public ZString CheckBox6PlaceHolder { get; }
		public ZString CheckBox7PlaceHolder { get; }
		public ZString OtherTypePlaceHolder { get; }
		public ZString GoodsAbandonedPlaceHolder { get; }
		public ZString SignedLocationPlaceHolder { get; }

		#endregion

		#region Properties

		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => this.EntryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
