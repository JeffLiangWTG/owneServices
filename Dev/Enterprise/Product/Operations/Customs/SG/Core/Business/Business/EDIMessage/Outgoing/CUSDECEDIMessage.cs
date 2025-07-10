using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Edifact;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CUSDECEDIMessage : SGEDIMessage
	{
		public const string Declaration = "DEC";
		public const string Amendment = "UPD";
		public const string Refund = "REF";
		public const string Cancellation = "CAN";

		public CUSDECEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Edifact.Auto.SegmentGroup EdifactMessage
		{
			get
			{
				if (edifactMessage == null)
				{
					if (EM_MessageText.Contains("+TCODEC:0:1:RT:"))
					{
						var coodci09b = (Edifact.D09B.Messages.TCODEC.TCODECMessage)GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
						if (coodci09b != null && coodci09b.UNH[0].MessageIdentifier.AssociationAssignedCode == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne)
						{
							edifactMessage = coodci09b;
						}
						else
						{
							edifactMessage = (Edifact.D05B.Messages.TCODEC.TCODECMessage)GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
						}
					}
				}

				if (edifactMessage == null)
				{
					edifactMessage = GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
					if (edifactMessage == null)
					{
						edifactMessage = GetAutoEdifactMessageUsingNamedFactory(Sg09bEdifactMessageFactory.SG41MessageFactory, new UNOASGCharacterSet());
					}
				}

				return edifactMessage;
			}
		}
		Edifact.Auto.SegmentGroup edifactMessage;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = Direction.Transmit;
		}

		protected override void PopulateMessageNumber()
		{
			EM_MessageNum = GetMessageNumber();
			string uEN = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Trim();
			string paddedUEN = "";
			if (!IsTradeNet41Declaration)
			{
				paddedUEN = uEN.PadRight(20, ' ');
			}
			else
			{
				paddedUEN = uEN.PadRight(17, ' ');
			}

			string uRN = paddedUEN + EM_MessageNum;
			EM_ApplicationReference = uRN;
			EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolder, uRN.PadRight(MessageNumberPlaceHolder.Length));
		}

		string GetMessageNumber()
		{
			// this is a rollover fountain now
			var connection = Db.Connection;
			long urnSeqNo;
			var smnNumberFountains = SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(GlbCompany.CurrentCompany)?.TryGetNumberFountain();

			if (smnNumberFountains != null)
			{
				urnSeqNo = smnNumberFountains.GetNext(connection);
			}
			else
			{
				var nextSequenceNumber = Env.NumberFountains.SGMessageNumberSequence.GetNext(connection);
				urnSeqNo = nextSequenceNumber + SGCustomsDataRegistry.Instance.MessageNumberOffset.Value;
				if (urnSeqNo > 9999)
				{
					urnSeqNo = urnSeqNo - 9999;
				}
			}

			return ZDateTime.Today.ToString("yyyyMMdd") + urnSeqNo.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get { return "PERMIT APPLICATION"; }
		}

		#region Supporting Documents

		public CodeDescriptionPairList SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new CodeDescriptionPairList();

					CUSDECMessage cUSDEC = (CUSDECMessage)GetAutoEdifactMessageUsingNamedFactory(Sg05bEdifactMessageFactory.SG4MessageFactory, new UNOASGCharacterSet());
					if (cUSDEC != null)
					{
						foreach (SegmentGroup5 group5 in cUSDEC.Group5)
						{
							foreach (DOCSegment doc in group5.DOC)
							{
								if (doc.DocumentMessageName.DocumentName == CustomsMessaging.CUSDEC.DocDocumentAttachment)
								{
									supportingDocuments.AddPair(group5.DOC[0].DocumentMessageDetails.DocumentIdentifier, group5.DOC[0].DocumentMessageDetails.DocumentSourceDescription);
								}
							}
						}
					}
				}

				return supportingDocuments;
			}
		}
		CodeDescriptionPairList supportingDocuments;

		#endregion

		bool IsTradeNet41Declaration
		{
			get
			{
				var msgEntry = (CusEntryHeader)this.EM_LinkedObject;
				var msgDec = msgEntry != null ? msgEntry.Declaration : null;
				return msgDec != null && (msgDec.JE_ApplicationCode == SGConstants.TradeNetVersion.FourPointOne);
			}
		}
	}
}
