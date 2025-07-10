using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ProductInformation : NonPersistentBusinessObject
	{
		public ProductInformation(
			BusinessObjectFactory factory,
			BaseJobDeclaration declaration,
			BaseJobComInvoiceLine[] invoiceLines
			) : base(factory)
		{
			Argument.NotNull(declaration, nameof(declaration));
			Argument.NotNull(invoiceLines, nameof(invoiceLines));

			if (invoiceLines.Length == 0)
			{
				ConstructionErrorMessage = ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|SelectAtLastOneLine", "Please select at least one invoice line.");
				return;
			}

			var errorLinesForNotHasSupplier = invoiceLines.Where(invoiceLine => invoiceLine.Supplier == null || invoiceLine.Supplier.IsDeleted)
				.Select(invoiceLine => $"{invoiceLine.InvoiceNumber} - {invoiceLine.JI_LineNo}").ToArray();

			if (errorLinesForNotHasSupplier.Length > 0)
			{
				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine(ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|InvoiceLineNotHasSupplier", "Cannot find the supplier for below Invoice Line(s):"));
				foreach (var errorMessageLine in errorLinesForNotHasSupplier)
				{
					errorMessageBuilder.AppendLine(errorMessageLine);
				}
				ConstructionErrorMessage = errorMessageBuilder.ToString();
				return;
			}

			var jobType = declaration.MessageTypeForHSAssist;
			if (jobType != SharedJobMessageTypeList.Codes.Import && jobType != SharedJobMessageTypeList.Codes.Export)
			{
				ConstructionErrorMessage = ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|JobTypeNotImportOrExport", "Job Type is not Import or Export. Cannot Create.");
				return;
			}

			if (declaration.Consignee is null)
			{
				ConstructionErrorMessage = ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|ConsigneeNotSet", "Declaration has no Consignee/Importer. Cannot Create.");
				return;
			}

			SendFromList = new CodeDescriptionPairList();
			foreach (var emailAddress in GlbStaff.CurrentUser.EmailAddresses)
			{
				SendFromList.AddPair(emailAddress.GSE_EmailAddress, emailAddress.EmailType);
			}

			var supplier2InvoiceLines = invoiceLines.GroupBy(invoiceLine => invoiceLine.Supplier);
			var supplierList = new CodeDescriptionPairList();
			supplier2InvoiceLines.Select(group => group.Key).OrderBy(supplier => supplier.OH_Code).ForEach(supplier =>
			{
				supplierList.AddPair(supplier.OH_Code, supplier.OH_FullName);
			});

			Declaration = declaration;
			SupplierList = supplierList;
			SupplierCode2InvoiceLines = supplier2InvoiceLines.ToDictionary(group => group.Key.OH_Code.ToString(), group => group.ToArray());
			JobType = jobType;
		}

		public string ConstructionErrorMessage { get; }

		BaseJobDeclaration Declaration { get; }

		public CodeDescriptionPairList SupplierList { get; }

		public Dictionary<string, BaseJobComInvoiceLine[]> SupplierCode2InvoiceLines { get; }

		string JobType { get; }

		#region Send From

		ZString sendFrom;

		[List(nameof(SendFromList))]
		[ResourceStringData("Enterprise.Customs.GUI.ProductInformation|SendFrom", Caption = "Send From")]
		public ZString SendFrom
		{
			get => sendFrom;
			set
			{
				SetNonPersistentPropertyValue(SendFromInfo, ref sendFrom, value);
				Validation.ValidateSendFrom();
			}
		}

		public ZPropertyInfo SendFromInfo => GetZPropertyInfo(nameof(SendFrom));

		public CodeDescriptionPairList SendFromList { get; protected set; }

		#endregion

		#region Delivery Contacts

		ProductInformationDeliveryContactCollection deliveryContacts;

		public ProductInformationDeliveryContactCollection DeliveryContacts
		{
			get
			{
				if (deliveryContacts == null)
				{
					deliveryContacts = new ProductInformationDeliveryContactCollection(Factory, SupplierList);
					RegisterEditableChildObject(deliveryContacts);
				}
				return deliveryContacts;
			}
		}

		public (string, ProductInformationDeliveryContext[]) GetDeliveryContext()
		{
			var errorMessage = string.Empty;
			ProductInformationDeliveryContext[] contexts = null;

			if (!string.IsNullOrEmpty(ConstructionErrorMessage))
			{
				errorMessage = ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|ConstructionErrorBlockedDeliver", "Some error(s) block to deliver messages: {0}", ConstructionErrorMessage);
			}
			else
			{
				RunPreSaveValidation();
				if (HasErrors)
				{
					var errorControl = new CustomsNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
					var errors = System.Environment.NewLine + string.Join(System.Environment.NewLine, errorControl.Select(messageError => messageError.Message));
					errorMessage = ResString.GetMultilingualString("Enterprise.Customs.GUI.ProductInformation|InputErrorBlockedDeliver", "Some input has error:{0}", errors);
				}
			}

			if (string.IsNullOrEmpty(errorMessage))
			{
				var noReceiverSuppliers = new HashSet<string>(SupplierCode2InvoiceLines.Keys);

				var supplierReceivers = DeliveryContacts.Cast<ProductInformationDeliveryContact>().GroupBy(contact => contact.Supplier).ToDictionary(
					group => group.Key,
					group => group.Select(contact => (string)contact.DeliveryAddress).Where(address => !string.IsNullOrEmpty(address)).ToArray()
				);

				var result = new List<ProductInformationDeliveryContext>();
				foreach (var supplierReceiver in supplierReceivers)
				{
					var supplierCode = supplierReceiver.Key;
					var receivers = supplierReceiver.Value;
					var supplierName = SupplierList.GetDescriptionFromCode(supplierCode);
					if (receivers.Length > 0)
					{
						noReceiverSuppliers.Remove(supplierCode);
						result.Add(NewProductInformationDeliveryContext(supplierCode, supplierName, receivers));
					}
				}

				if (noReceiverSuppliers.Count > 0)
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine(Res.GetString("Enterprise.Customs.GUI.ProductInformation|SupplierNoEmailAddress", "Each supplier must have at least one email address entered. Below supplier(s) have no email address:"));
					foreach (var supplierCode in noReceiverSuppliers)
					{
						errorMessageBuilder.AppendLine(supplierCode);
					}
					errorMessage = errorMessageBuilder.ToString();
				}
				contexts = result.ToArray();
			}
			return (errorMessage, contexts);
		}

		protected ProductInformationDeliveryContext NewProductInformationDeliveryContext(string supplierCode, string supplierName, string[] receivers)
		{
			var result = new ProductInformationDeliveryContext
			{
				sender = SendFrom.IsEmpty ? Env.CurrentUser.EmailAddress : SendFrom,
				receivers = receivers,
				integration = new()
				{
					type = (NoResString)"cwnext-declaration",
					ehub_id = GlbCompany.CurrentCompany.LicenceKeyIdentifier,
					declaration_number = Declaration.JobNumber
				},
				context = new()
				{
					supplier = supplierName,
					consignee = Declaration.Consignee.OH_FullName,
					masterbill = Declaration.JE_MasterBill,
					housebill = Declaration.JE_HouseBill,
					voyage_flight = Declaration.JE_VoyageFlightNo,
					owner_ref = Declaration.JE_OwnerRef,
					port_of_loading = Declaration.JE_RL_NKPortOfLoading
				},
				requested_classifications =
				[
					new()
					{
						imp_exp = JobType == SharedJobMessageTypeList.Codes.Import ? ProductInformationDeliveryContext.JobTypeImport : ProductInformationDeliveryContext.JobTypeExport,
						country = Declaration.CountryCode.ToLower()
					}
				],
				lines = SupplierCode2InvoiceLines[supplierCode].Select(invoiceLine => new ProductInformationDeliveryContext.Line
				{
					integration = new()
					{
						invoice_number = invoiceLine.InvoiceHeader.JZ_InvoiceNumber,
						invoice_line = invoiceLine.JI_LineNo.ToString()
					},
					context = new() { part_no = invoiceLine.JI_PartNo },
					desc = invoiceLine.JI_Description
				}).ToArray()
			};

			return result;
		}

		#endregion

		ProductInformationValidation Validation => new ProductInformationValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}
	}
}
