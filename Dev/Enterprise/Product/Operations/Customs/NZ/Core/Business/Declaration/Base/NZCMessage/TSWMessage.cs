using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using TranshipmentRequest = Enterprise.Customs.NZ.Business.TranshipmentRequest;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	public class TSWMessage : NZCMessage
	{
		public TSWMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected TSWMessage message;

		public static class ResponseMessage
		{
			public static class MessageSubTypes
			{
				public const string TSWWCOResponse = "TWR";
				public const string Acknowledgement = "ACK";
				public const string TradeSingleWindow = "TSW";
				public const string MPIFood = "MPI";
				public const string MPIBIO = "BIO";
				public const string NZCustoms = "NZC";
			}
		}

		#region Overrides

		public override ZString EM_Status
		{
			get => base.EM_Status;
			set
			{
				var oldValue = EM_Status;
				base.EM_Status = value;

				if (!IsCopying && oldValue != EM_Status && EM_Status == EDIMessage.Status.Failed)
				{
					if (Parent is TranshipmentRequest transhipmentRequest && transhipmentRequest.C4_Status == CombinedMovementStatus.Codes.STC)
					{
						transhipmentRequest.C4_Status = ZString.Empty;
					}
				}
			}
		}

		protected override string SendersReferencePlaceHolderOverride
		{
			get { return TSWConstants.SendersReferencePlaceHolder; }
		}

		protected override string GetSendersReference()
		{
			ZString result = ZString.Empty;
			var entryHeader = Parent as CusEntryHeader;
			if (entryHeader != null)
			{
				if (entryHeader.CH_BGMReference.IsEmpty || entryHeader.CH_BGMReference == TSWConstants.SendersReferencePlaceHolder)
				{
					GetReference(entryHeader);
				}

				result = entryHeader.CH_BGMReference;
			}
			else
			{
				var declaration = Parent as JobDeclaration;
				if (declaration != null)
				{
					declaration.PopulateJE_DeclarationReferenceIfNeeded();
					result = declaration.JE_DeclarationReference;
				}
				else
				{
					var consol = Parent as ForwardingConsol;
					if (consol != null)
					{
						consol.PopulateJK_UniqueConsignRefIfNeeded();
						result = GetConsolReference(consol);
					}
					else
					{
						var mawb = Parent as Business.Express.CusMAWB;
						if (mawb != null)
						{
							result = mawb.CM_MessageReference;
						}
					}
				}
			}

			return result.Replace("/", "");
		}

		protected ZString GetReference(CusEntryHeader entryHeader)
		{
			if (entryHeader.Declaration != null)
			{
				entryHeader.PopulateCH_BGMReferenceIfNeeded();
			}

			return entryHeader.CH_BGMReference;
		}

		protected ZString GetConsolReference(ForwardingConsol consol)
		{
			var entryNumber = consol.Factory.LoadTop1<CusEntryNumber>(CusEntryNumber.GetEntryNumberFilter(consol));
			var result = entryNumber == null || entryNumber.CE_EntryLineReference.IsEmpty ? consol.JK_UniqueConsignRef : entryNumber.CE_EntryLineReference;
			if (result == TSWConstants.SendersReferencePlaceHolder)
			{
				var refPrefix = new ZString("OCR");
				entryNumber.CE_EntryLineReference = refPrefix + GetMessageSendersOCRReferenceNumber(refPrefix);
				result = EM_ApplicationReference = entryNumber.CE_EntryLineReference;
			}

			return result;
		}

		protected string GetMessageSendersOCRReferenceNumber(string refPrefix)
		{
			return Env.NumberFountains.NZOCRReferenceNumber.GetNextFormatted(Factory);
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return Factory.GetCachedValue<TransactionTypeList>(); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_Status = EDIMessage.Status.Queued;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase)
			{
				var entryHeader = Parent as CusEntryHeader;
				if (entryHeader != null)
				{
					EM_ApplicationReference = entryHeader.CH_BGMReference;
				}
				else
				{
					var consol = Parent as ForwardingConsol;
					if (consol != null && EM_ApplicationReference.IsEmpty)
					{
						EM_ApplicationReference = consol.JobNumber;
					}
				}

				var declaration = Parent as JobDeclaration;
				if (declaration != null && declaration.JE_GB.IsValid)
				{
					EM_GB = declaration.JE_GB;
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
				{
					var infoDisplay = new ZStringBuilder();
					infoDisplay.Append(GetHumanReadableMessageType + " " + EM_MessageType + " sent to Customs");
					infoDisplay.Append("Functional Reference ID: " + EM_ApplicationReference);
					infoDisplay.Append("Declarant: " + EM_SendingUser);
					DisplayFileAttachmentsIfAny(infoDisplay);
					return infoDisplay.ToStringWithNewLineBetweenAppends();
				}
				else
				{
					return base.EM_MessageInterpretation;
				}
			}
			set { base.EM_MessageInterpretation = value; }
		}

		void DisplayFileAttachmentsIfAny(ZStringBuilder infoDisplay)
		{
			if (MessageAttachments.Count > 0)
			{
				infoDisplay.Append("");
				infoDisplay.Append("Attached Documents:");
				foreach (EDIMessageAttach attachment in MessageAttachments)
				{
					infoDisplay.Append(attachment.EG_EdiMsgDocType + "\t" + attachment.EG_FileName);
				}
			}
		}

		string GetHumanReadableMessageType
		{
			get
			{
				var result = ZString.Empty;
				switch (EM_MessageSubType)
				{
					case Business.MessageSubTypeList.Codes.Original:
						result = Business.MessageSubTypeList.Descriptions.Original;
						break;
					case Business.MessageSubTypeList.Codes.Cancellation:
						result = Business.MessageSubTypeList.Descriptions.Cancellation;
						break;
					case Business.MessageSubTypeList.Codes.Completion:
						result = Business.MessageSubTypeList.Descriptions.Completion;
						break;
					case Business.MessageSubTypeList.Codes.Replacement:
						result = Business.MessageSubTypeList.Descriptions.Replacement;
						break;
				}

				return result;
			}
		}

		protected override IStreamFormatter MessageStreamFormatter
		{
			get { return new TSWMessageStreamFormatter(new ZStringBuilder().ToStringWithNewLineBetweenAppends()); }
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		#endregion

		#region Parent

		BusinessObject Parent
		{
			get
			{
				if (parent == null && !EM_LinkUniqueID.IsEmpty)
				{
					switch (EM_LinkTable)
					{
						case JobDeclaration.Schema.TableName:
							parent = Factory.Load<JobDeclaration>(EM_LinkUniqueID);
							break;
						case ForwardingConsol.Schema.TableName:
							parent = Factory.Load<ForwardingConsol>(EM_LinkUniqueID);
							break;
						case CusEntryHeader.Schema.TableName:
							parent = Factory.Load<CusEntryHeader>(EM_LinkUniqueID);
							break;
						case Business.Express.CusMAWB.Schema.TableName:
							parent = Factory.Load<Business.Express.CusMAWB>(EM_LinkUniqueID);
							break;
						case TranshipmentRequest.Schema.TableName:
							parent = Factory.Load<TranshipmentRequest>(EM_LinkUniqueID);
							break;
					}
				}
				return parent;
			}
		}
		BusinessObject parent;

		#endregion

		public CodeDescriptionPairList SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new CodeDescriptionPairList();

					//TODO: extract attached documents from xml message....
					// for testing specific job....   supportingDocuments.AddPair("BOE", "BOE - CBP_Form_3461.pdf");
				}

				return supportingDocuments;
			}
		}
		CodeDescriptionPairList supportingDocuments;

		public override ZString MsgTransMode
		{
			get { return Enterprise.Customs.NZ.Business.MsgTransportList.Codes.TSW; }
			set { }
		}
	}
}
