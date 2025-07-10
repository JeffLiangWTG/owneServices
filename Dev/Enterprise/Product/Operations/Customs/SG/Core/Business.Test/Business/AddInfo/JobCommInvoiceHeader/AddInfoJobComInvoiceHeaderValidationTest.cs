using CargoWise.ComponentModel;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceHeaderValidationTest : SGAddInfoValidationTest
	{
		public void TestGSTRate()
		{
			AddInfoJobComInvoiceHeader.SG_GSTRate = -1;
			Validation.ValidateSG_GSTRate();
			AssertEquals(true, AddInfoJobComInvoiceHeader.SG_GSTRateInfo.HasErrors());
			AddInfoJobComInvoiceHeader.SG_GSTRate = 21;
			Validation.ValidateSG_GSTRate();
			AssertEquals(true, AddInfoJobComInvoiceHeader.SG_GSTRateInfo.HasWarnings());
			AddInfoJobComInvoiceHeader.SG_GSTRate = 7;
			Validation.ValidateSG_GSTRate();
			AssertEquals(false, AddInfoJobComInvoiceHeader.SG_GSTRateInfo.HasNotifications());
		}

		protected override AddInfo GetNewAddInfo()
		{
			return AddInfoJobComInvoiceHeader;
		}

		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageType;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		protected virtual string MessageType
		{
			get
			{
				return "";
			}
		}

		#endregion
		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region Validation
		protected AddInfoJobComInvoiceHeaderValidation Validation
		{
			get
			{
				return AddInfoJobComInvoiceHeader.Validation;
			}
		}

		protected AddInfoJobComInvoiceHeader AddInfoJobComInvoiceHeader
		{
			get
			{
				return addInfoJobComInvoiceHeader ?? (addInfoJobComInvoiceHeader = new AddInfoJobComInvoiceHeader(InvoiceHeader.JZ_AddInfoInfo));
			}
		}

		AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader;
		#endregion
		#endregion
	}
}
