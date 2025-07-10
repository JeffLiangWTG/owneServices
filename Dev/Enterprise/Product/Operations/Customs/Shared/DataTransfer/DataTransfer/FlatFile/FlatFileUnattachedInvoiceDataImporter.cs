using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class FlatFileUnattachedInvoiceDataImporter : FlatFileInvoiceDataImporter
	{
		public FlatFileUnattachedInvoiceDataImporter(string fileName) : base(fileName)
		{
			factory = new BusinessObjectFactory();
			this.supplierCodeMap = new NameValueCollection();
		}

		#region Overrides

		public override void Import()
		{
			base.Import();
			Save();
		}

		protected override BaseJobComInvoiceHeader[] FindInvoicesFromInvoiceNumber(ZString invoiceNumber)
		{
			List<BaseJobComInvoiceHeader> foundList = new List<BaseJobComInvoiceHeader>();
			foreach (BaseJobComInvoiceHeader header in InvoiceHeaderList)
			{
				if (header.JZ_InvoiceNumber == invoiceNumber)
				{
					foundList.Add(header);
				}
			}
			return foundList.ToArray();
		}

		protected override BaseJobComInvoiceHeader[] InvoiceHeaders
		{
			get { return InvoiceHeaderList.ToArray(); }
		}

		protected override BaseJobComInvoiceHeader AddNewInvoice()
		{
			BaseJobComInvoiceHeader header = factory.New<BaseJobComInvoiceHeader>();
			header.SuspendValidation();
			InvoiceHeaderList.Add(header);
			return header;
		}

		protected override BaseJobComInvoiceLine AddNewInvoiceLine()
		{
			return factory.New<BaseJobComInvoiceLine>();
		}

		#endregion

		#region Implementation

		List<BaseJobComInvoiceHeader> InvoiceHeaderList
		{
			get
			{
				if (fInvoiceHeaderList == null)
				{
					fInvoiceHeaderList = new List<BaseJobComInvoiceHeader>();
				}
				return fInvoiceHeaderList;
			}
		}
		List<BaseJobComInvoiceHeader> fInvoiceHeaderList;

		#endregion
	}
}
