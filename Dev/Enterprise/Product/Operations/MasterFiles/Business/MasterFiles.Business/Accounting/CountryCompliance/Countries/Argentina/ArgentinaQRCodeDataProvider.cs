using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Newtonsoft.Json;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Argentina
{
	class ArgentinaQRCodeDataProvider : IQRCodeDataProvider
	{
		public ArgentinaQRCodeDataProvider()
		{
			ArgentinaEInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
		}

		readonly IArgentinaEInvoicingExtension ArgentinaEInvoicingExtension;

		const int seriePrefixLength = 5;

		class QRText
		{
			public int ver { get; set; }
			public string fecha { get; set; }
			[JsonConverter(typeof(StringToLongConverter))]
			public string cuit { get; set; }
			[JsonConverter(typeof(StringToIntConverter))]
			public string ptoVta { get; set; }
			[JsonConverter(typeof(StringToShortConverter))]
			public string tipoCmp { get; set; }
			[JsonConverter(typeof(StringToLongConverter))]
			public string nroCmp { get; set; }
			public decimal importe { get; set; }
			public string moneda { get; set; }
			[JsonConverter(typeof(StringToDecimalConverter))]
			public string ctz { get; set; }
			[JsonConverter(typeof(StringToIntConverter))]
			public string tipoDocRec { get; set; }
			[JsonConverter(typeof(StringToLongConverter))]
			public string nroDocRec { get; set; }
			public string tipoCodAut { get; set; }
			[JsonConverter(typeof(StringToLongConverter))]
			public string codAut { get; set; }
		}

		string IQRCodeDataProvider.GetTransactionQRCodeString(ITransactionQRCodeDataProvider transactionData)
		{
			var result = string.Empty;

			var governmentAllocatedNumber = (ZString)transactionData.EInvoicingGovernmentAllocatedNumber.ToString();
			if (governmentAllocatedNumber.IsEmpty)
			{
				return result;
			}

			(ZString regType, ZString regNumber) regNo = ArgentinaEInvoicingExtension.GetRegistrationNumberOrganization(transactionData);

			(ZString equivalentCurrency, ZString exChangeRate) currencyInfo = ArgentinaEInvoicingExtension.GetCurrencyAndExchangeRate(transactionData);

			var transactionReference = (transactionData.TransactionReference).RemoveNonNumericCharacters();

			var qrText = new QRText();
			qrText.ver = 1;
			qrText.tipoCodAut = "E";
			qrText.fecha = transactionData.InvoiceDate.ToString("yyyy-MM-dd");
			qrText.cuit = GetCompanyCustomsRegNo(transactionData);
			qrText.ptoVta = transactionReference.SubstringSafe(0, seriePrefixLength);
			qrText.nroCmp = transactionReference.SubstringSafe(seriePrefixLength, transactionReference.Length);
			qrText.tipoCmp = ArgentinaEInvoicingExtension.GetDocumentType(transactionData.ComplianceSubType);
			qrText.importe = GetTotalAmount(transactionData);
			qrText.moneda = currencyInfo.equivalentCurrency;
			qrText.ctz = currencyInfo.exChangeRate;
			qrText.tipoDocRec = regNo.regType;
			qrText.nroDocRec = regNo.regNumber;
			qrText.codAut = governmentAllocatedNumber;

			var serializeOptions = new JsonSerializerSettings
			{
				Converters =
				{
					new StringConverter(),
					new StringToShortConverter(),
					new StringToIntConverter(),
					new StringToLongConverter(),
					new StringToDecimalConverter()
				}
			};

			result = JsonConvert.SerializeObject(qrText, serializeOptions);
			result = "https://www.afip.gob.ar/fe/qr/?p=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(result)); // Text in QR Code

			return result;
		}

		decimal GetTotalAmount(ITransactionQRCodeDataProvider transactionData)
		{
			var result = decimal.Round(transactionData.OSInvoiceTotal * (transactionData.TransactionType == "CRD" ? -1 : 1), 2);

			return result;
		}

		ZString GetCompanyCustomsRegNo(ITransactionQRCodeDataProvider transactionData)
		{
			var loginCompanyCUIT = ZString.Empty;

			if (transactionData.Company != null && transactionData.Company.OrgProxy != null)
			{
				var orgProxyCustomsCodes = transactionData.Company.OrgProxy.CustomsCodes;

				if (orgProxyCustomsCodes != null)
				{
					loginCompanyCUIT = orgProxyCustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, CountryCodes.Argentina)?.OK_CustomsRegNo ?? ZString.Empty;
				}
			}

			return loginCompanyCUIT.RemoveNonNumericCharacters();
		}

		#region Custom JsonConverter

		abstract class JsonConverterString : JsonConverter<String>
		{
			public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				return reader.ToString();
			}
		}

		class StringConverter : JsonConverterString
		{
			public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
			{
				writer.WriteValue(value);
			}
		}

		class StringToShortConverter : JsonConverterString
		{
			public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
			{
				Int16.TryParse(value, out Int16 convertedValue);
				writer.WriteValue(convertedValue);
			}
		}

		class StringToIntConverter : JsonConverterString
		{
			public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
			{
				Int32.TryParse(value, out Int32 convertedValue);
				writer.WriteValue(convertedValue);
			}
		}

		class StringToLongConverter : JsonConverterString
		{
			public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
			{
				Int64.TryParse(value, out Int64 convertedValue);
				writer.WriteValue(convertedValue);
			}
		}

		class StringToDecimalConverter : JsonConverterString
		{
			public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
			{
				Decimal.TryParse(value, out Decimal convertedValue);
				writer.WriteValue(convertedValue);
			}
		}

		#endregion
	}
}
