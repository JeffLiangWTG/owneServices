using System.IO;
using System.Text;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public class TrialTransaction
	{
		TrialTransaction(string xmlTemplateName, string uuid, string previousTransactionHash, Supplier supplier, Customer customer)
		{
			XmlTemplateName = Argument.NotNull(xmlTemplateName, nameof(xmlTemplateName));
			Uuid = Argument.NotNull(uuid, nameof(uuid));
			PreviousTransactionHash = Argument.NotNull(previousTransactionHash, nameof(previousTransactionHash));
			Supplier = Argument.NotNull(supplier, nameof(supplier));
			Customer = Argument.NotNull(customer, nameof(customer));
		}

		public string Uuid { get; }
		string XmlTemplateName { get; }
		string PreviousTransactionHash { get; }
		Supplier Supplier { get; }
		Customer Customer { get; }

		public static TrialTransaction Invoice(Supplier supplier, Customer customer) =>
			new TrialTransaction(
				xmlTemplateName: "CSIDTestInvoice",
				uuid: "8d487816-70b8-4ade-a618-9d620b73814a",
				//hard coded hash will be replaced in the next WI.
				previousTransactionHash: "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
				supplier,
				customer);

		public static TrialTransaction CreditNote(Supplier supplier, Customer customer, string previousTransactionHash) =>
			new TrialTransaction(
				xmlTemplateName: "CSIDTestCreditNote",
				uuid: "322efd74-5b1b-41e0-843c-1d13bec4475e",
				previousTransactionHash,
				supplier,
				customer);

		public static TrialTransaction DebitNote(Supplier supplier, Customer customer, string previousTransactionHash) =>
			new TrialTransaction(
				xmlTemplateName: "CSIDTestDebitNote",
				uuid: "ad2c3a8b-1cc8-4060-8a35-0d6aa8ff6fcc",
				previousTransactionHash,
				supplier,
				customer);

		public string ToXml()
		{
			return string.Format(GetEmbeddedResourceStream(XmlTemplateName),
				Uuid,
				PreviousTransactionHash,
				GetIdentificationXml(Supplier.IdentificationType, Supplier.IdentificationNumber),
				Supplier.StreetName,
				Supplier.BuildingNumber,
				Supplier.CitySubdivisionName,
				Supplier.City,
				Supplier.PostCode,
				Supplier.State,
				Supplier.CountryCode,
				GetVatCodeXml(Supplier.VatCode),
				Supplier.CompanyName,
				GetIdentificationXml(Customer.IdentificationType, Customer.IdentificationNumber),
				Customer.StreetName,
				Customer.BuildingNumber,
				Customer.CitySubdivisionName,
				Customer.City,
				Customer.PostCode,
				Customer.State,
				Customer.CountryCode,
				GetVatCodeXml(Customer.VatCode),
				Customer.CompanyName
			);
		}

		static string GetIdentificationXml(string identificationType, string identificationNumber) => string.IsNullOrEmpty(identificationType) || string.IsNullOrEmpty(identificationNumber)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{identificationType}"">{identificationNumber}</cbc:ID>
			</cac:PartyIdentification>";

		static string GetVatCodeXml(string vatCode) => string.IsNullOrEmpty(vatCode)
			? string.Empty
			: $@"
				<cbc:CompanyID>{vatCode}</cbc:CompanyID>";

		string GetEmbeddedResourceStream(string resourceName)
		{
			var stream = typeof(BranchRegistrationForSAInvoicing).Assembly.GetManifestResourceStream(resourceName);
			using (var streamReader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true, 32768, leaveOpen: true))
			{
				var s = streamReader.ReadToEnd();
				return (s ?? string.Empty).Trim('\ufeff');
			}
		}
	}
}
