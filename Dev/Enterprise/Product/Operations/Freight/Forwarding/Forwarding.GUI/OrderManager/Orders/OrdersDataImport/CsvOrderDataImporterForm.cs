using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer
{
	public partial class CsvOrderDataImporterForm : DataImporterForm
	{
		protected CsvOrderDataImporterForm()
		{
			InitializeComponent();
		}

		public CsvOrderDataImporterForm(CsvOrderDataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName) // Interface name for billing purposes
		{
			InitializeComponent();
		}

		public static new CsvOrderDataImporterForm Create(BillingInterfaceName interfaceName)
		{
			var billingInterfaceName = interfaceName ?? defaultInterfaceName;
			return new CsvOrderDataImporterForm(new CsvOrderDataImporterBusinessObject(new BusinessObjectFactory()), null, billingInterfaceName);
		}
		static readonly BillingInterfaceName defaultInterfaceName = BillingInterfaceName.CsvOrderImport;

		public override string FormCaption
		{
			get { return Res.GetString("CsvOrderDataImporterForm|42d09331-6c81-4b0b-ba9f-d8482a602b9c", "Order CSV Data Importer"); }
		}

		protected override string ImportFileFilter
		{
			get
			{
				return Res.GetString("Forwarding|CsvFilesPattern", "CSV Files") + (NoResString)" (*.csv)|*.csv";
			} // File extensions should not be localized
		}
	}
}
