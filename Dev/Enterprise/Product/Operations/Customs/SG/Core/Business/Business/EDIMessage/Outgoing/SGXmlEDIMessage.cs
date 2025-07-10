using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGXmlEDIMessage : XmlEDIMessage
	{
		#region Constant

		public const string Prefix = "X";

		public const string MessageNumberPlaceHolderXml = "# MSGNO PLACEHOLDER #";
		public const string MessageDateTimeCreatePlaceHolderXml = "# MESSAGE DATE TIME CREATE PLACE HOLDER #";
		public const string RecipientReferencePlaceHolderXml = "# RECIPIENT REFERENCE PLACE HOLDER #";
		public const string SendersReferencePlaceHolderXml = "# SENDERS REFERENCE PLACE HOLDER #";
		public const string UniqueBatchNumberPlaceHolderXml = "# UNIQUE BATCH NUMBER PLACE HOLDER #";
		public const string InterchangeNumberPlaceHolderXml = "# INTERCHANGE NUMBER PLACE HOLDER #";

		#endregion

		public SGXmlEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenetXML;
			EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		}

		protected override void PopulateMessageNumber()
		{
			if (!IsInDatabase)
			{
				PopulateNumberPropertyIfRequired(EM_MessageNumInfo, GetNewMessageNumber);
			}
			else if (EM_MessageNum.IsEmpty)
			{
				EM_MessageNum = GetNewMessageNumber(Factory);
			}
		}

		ZString GetNewMessageNumber(BusinessObjectFactory factory)
		{
			var date = ZDateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			var sequenceNumber = GetSequenceNumber();

			if (EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit)
			{
				SetMessagePlaceHolders(sequenceNumber.Number, date);
			}

			var companyId = BitConverter.ToString(Encoding.UTF8.GetBytes(GlbCompany.CurrentCompany.GC_Code)).Replace("-", string.Empty).PadLeft(6, '0');

			return string.Concat(Prefix, date, companyId, sequenceNumber.Tag, sequenceNumber.Number);
		}

		(string Tag, string Number) GetSequenceNumber()
		{
			long urnSeqNo;
			string tag = "0";
			var connection = Db.Connection;

			var smnNumberFountain = SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(GlbCompany.CurrentCompany)?.TryGetNumberFountain();
			if (smnNumberFountain != null)
			{
				urnSeqNo = smnNumberFountain.GetNext(connection);
			}
			else
			{
				var nextSequenceNumber = Env.NumberFountains.SGMessageNumberSequence.GetNext(connection);
				urnSeqNo = nextSequenceNumber + SGCustomsDataRegistry.Instance.MessageNumberOffset.Value;

				if (urnSeqNo > 9999)
				{
					urnSeqNo -= 9999;
				}
				tag = "1";
			}

			return (tag, urnSeqNo.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'));
		}

		void SetMessagePlaceHolders(string sequenceNumber, string date)
		{
			var uniqueReferenceNumber = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Trim().ToUpperInvariant();

			EM_ApplicationReference = string.Concat(uniqueReferenceNumber, date, sequenceNumber);

			EM_MessageText = EM_MessageText
				.Replace(MessageNumberPlaceHolderXml, uniqueReferenceNumber)
				.Replace(MessageDateTimeCreatePlaceHolderXml, date)
				.Replace(UniqueBatchNumberPlaceHolderXml, sequenceNumber);
		}

		public override ZString EM_MessageText
		{
			get => base.EM_MessageText;
			set
			{
				if (base.EM_MessageText != value)
				{
					base.EM_MessageText = value;

					if (IsTransmitMessage)
					{
						EM_MessageInterpretation = value.IsEmpty ? ZString.Empty : ConvertXmlToHtml(EM_MessageText);
					}

					tradenetDeclaration = null;
					tradenetResponse = null;
				}
			}
		}

		public TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = DeserializeCore<TradenetDeclaration>());
		TradenetDeclaration tradenetDeclaration;

		public TradenetResponse TradenetResponse => tradenetResponse ?? (tradenetResponse = DeserializeCore<TradenetResponse>());
		TradenetResponse tradenetResponse;

		T DeserializeCore<T>() where T : class
		{
			try
			{
				using (var stream = GetEM_MessageTextReader())
				{
					var serializer = ZXmlSerializer.New(typeof(T));
					return serializer.Deserialize(stream) as T;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return null;
			}
		}
	}
}
