using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects
{
	public class DA306DeclarationWrapper
	{
		public DA306DeclarationWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public JobDocAddress Importer => declaration.ImporterDocumentaryAddress;

		public ZString ImporterAddress
		{
			get
			{
				var formatter = new AddressFormatter(declaration.Factory, declaration.ImporterDocumentaryAddress, GlbCompany.CurrentCompany, true);
				return formatter.PostalAddressWithoutCompanyName().Replace("\n", System.Environment.NewLine);
			}
		}

		public ZString ImporterID
		{
			get
			{
				var result = declaration.Importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode);
				if (result.IsEmpty)
				{
					result = declaration.Importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID);
				}
				if (result.IsEmpty)
				{
					result = declaration.Importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.IDNumber);
				}
				return result;
			}
		}

		public ZString SupplierName => declaration.SupplierName;

		public ZString TransportDetails => $"{declaration.JE_MasterBill} / {declaration.JE_MasterBillIssuedDate:dd-MMM-yyyy} / {declaration.JE_HouseBill}";

		public ZString VoyageFlight
		{
			get
			{
				var result = ZString.Empty;
				if (declaration.JE_TransportMode == TransportModes.Air)
				{
					result = $"{declaration.JE_VoyageFlightNo} / {declaration.JE_DateAtOrigin:dd-MMM-yyyy}";
				}
				if (declaration.JE_TransportMode == TransportModes.Sea)
				{
					result = $"{declaration.JE_VesselName} / {declaration.JE_VoyageFlightNo} / {declaration.JE_LloydsIMO}";
				}
				return result;
			}
		}

		public ZString VoyageFlightNo => declaration.JE_VoyageFlightNo;

		public ZString MasterBillNumber => declaration.JE_MasterBill;

		public ZString HouseBillNumber => declaration.JE_HouseBill;

		public ZInt TotalPackages => declaration.JE_TotalNoOfPacks;

		public ZString TotalPackagesUnit => declaration.JE_TotalNoOfPacksPackType;

		public ZDecimal CustomsValue => declaration.TotalCustomsValueInLocalCurrency;

		public ZString MarksNumbers
		{
			get
			{
				var stringBuilder = new ZStringBuilder()
					.AppendIfNotEmpty($"{declaration.MarksAndNumbers}")
					.AppendIfNotEmpty($"{declaration.JE_TotalWeight:0.00} {declaration.JE_TotalWeightUnit}")
					.AppendIfNotEmpty(ZString.Join(", ", declaration.CusContainers.ContainerNumbers.ToArray()));
				return stringBuilder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString GoodsDescription => declaration.JE_GoodsDescription;

		public ZString PortOfDischarge => declaration.PortOfArrival.RL_PortName;

		public IEnumerable<DA306InvoiceLineWrapper> InvoiceLines =>
			invoiceLines ??= declaration.InvoiceLines.Cast<JobComInvoiceLine>().Select(invoiceLine => new DA306InvoiceLineWrapper(invoiceLine));

		IEnumerable<DA306InvoiceLineWrapper> invoiceLines;
	}
}
