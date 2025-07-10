using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public abstract class MessageSendingObject : BaseMessageSendingObject,
		IBriefCusDeclaration,
		IAdditionalSupportingDocument
	{
		public MessageSendingObject(AsycudaManifestHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			MasterBill = header.MasterBill;
		}

		public AsycudaManifestHeader Header { get; }
		public AsycudaBill MasterBill { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
			Action = ActionCodeList.Codes.Create;
		}

		ZString action;
		[List(nameof(ActionList))]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject|Action", Caption = "Action")]
		public ZString Action
		{
			get => action;
			set
			{
				CheckMaximumLength(ActionInfo, value);
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
			}
		}

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(nameof(Action));

		public CodeDescriptionPairList ActionList => Factory.GetCachedValue<ActionCodeList>();

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject|MessageType", Caption = "Message Type")]
		public abstract ZString MessageType { get; }

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject|Description", Caption = "Description")]
		public abstract ZString Description { get; }

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber => Header.DeclarationNumber;

		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					var allEDocs = GetAllEDocs();
					var availableEDocList = new AvailableEDocList(ExtensionFilter, allEDocs);
					supportingDocuments = new SupportingDocumentCollection(Factory, availableEDocList, allEDocs, TypeList, GetBillNumberList());
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		CodeDescriptionPairList TypeList => Factory.GetCachedValue<DocumentTypeCodeList>();

		protected virtual CodeDescriptionPairList GetBillNumberList()
		{
			var result = new CodeDescriptionPairList();
			foreach (var bill in Header.Bills)
			{
				result.AddPair(bill.ABL_BillNumber);
			}
			return result;
		}

		List<ZString> ExtensionFilter => new List<ZString>() { Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.GIF };

		#region IBriefCusDeclaration members
		ZDate IBriefCusDeclaration.AcceptanceDateTime => Header.DeclarationDate.Date;

		ZString IBriefCusDeclaration.FunctionCode => Action;

		ZString IBriefCusDeclaration.ID => EntryNumber;

		ZString IBriefCusDeclaration.TypeCode => Header.AMA_ManifestType;

		IPartyDetails IBriefCusDeclaration.Agent => GetAgentCore();

		ITransportMeans IBriefCusDeclaration.BorderTransportMeans => new BCDBorderTransportMeans(Header, MasterBill);

		IBCDConsignment IBriefCusDeclaration.Consignment => GetConsignmentCore();

		ZString IBriefCusDeclaration.RepresentativePersonName => Header.CustomsAgentDescription;

		IPartyDetails IBriefCusDeclaration.ExpressCarrier => GetExpressCarrierCore();

		IPartyDetails IBriefCusDeclaration.OnBoardCourier
		{
			get
			{
				IPartyDetails onBoardCourier = null;
				if (Header.Person?.Person is GlbPerson person)
				{
					onBoardCourier = new OnBoardCourier(person);
				}
				return onBoardCourier;
			}
		}
		#endregion

		protected virtual IPartyDetails GetAgentCore() => null;

		protected virtual IBCDConsignment GetConsignmentCore() => new Consignment(Header, MasterBill);

		protected virtual IPartyDetails GetExpressCarrierCore() => new PartyDetails(Header.Carrier);

		public IStorageDocsBaseCollection[] GetAllEDocs()
		{
			var result = new List<IStorageDocsBaseCollection>()
			{
				(Header as IDocManagerSupport)?.DocManagerInfo?.EDocsView
			};
			return result.ToArray();
		}

		public ZString SerializeToMessageString() => GetMessageBuilder().SerializeToMessageString(this, Action);

		protected abstract ITWMessageBuilder GetMessageBuilder();

		TW.Business.SupportingDocumentCollection IAdditionalSupportingDocument.GetSupportingDocuments() => SupportingDocuments;

		protected override void RunPreSaveValidationCore()
		{
			SupportingDocuments.Cast<SupportingDocument>().ForEach(supportingDocument => supportingDocument.ValidateAll());
			base.RunPreSaveValidationCore();
		}
	}
}
