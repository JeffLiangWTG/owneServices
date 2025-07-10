using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public class MergedDeclarationCreator<T>
		where T : BaseJobDeclaration
	{
		public MergedDeclarationCreator(BusinessObjectFactory factory) : this(factory, string.Empty) { }
		
		public MergedDeclarationCreator(BusinessObjectFactory factory, string applicationCode)
		{
			this.Factory = factory;
			this.applicationCode = applicationCode;
		}

		public T Declaration
		{
			get
			{
				Setup();
				return fDeclaration;
			}
		}

		public Mock<T> MockDeclaration
		{
			get
			{
				Setup();
				return fMock;
			}
		}

		public CusEntryHeader Entry1
		{
			get { return Declaration.CustomsEntryHeaders[0]; }
		}

		public CusEntryLine EntryLine1
		{
			get { return Declaration.CustomsEntryHeaders[0].MergedLines[0]; }
		}

		public BaseJobComInvoiceLine InvoiceLine1
		{
			get { return Declaration.FilteredInvoiceLines[0]; }
		}

		public SendsMessagesToCustomsShutterUpperer Notifier
		{
			get { return (SendsMessagesToCustomsShutterUpperer)Declaration.MessageInitiator; }
		}

		protected T fDeclaration;
		protected Mock<T> fMock;

		protected void Setup()
		{
			if (!hasSetup)
			{
				hasSetup = true;
				fMock = Factory.NewMoq<T>();
				fMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(false);
				fDeclaration = fMock.Object;
				fDeclaration.FillWithValidTestData();
				fDeclaration.JE_OH_Supplier = Supplier.PK;
				fDeclaration.JE_OH_Importer = Buyer.PK;
				if (!string.IsNullOrEmpty(applicationCode))
				{
					fDeclaration.JE_ApplicationCode = applicationCode;
				}
				fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				fDeclaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
				fDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				var line = fDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				if (fDeclaration.IsEntryInstructionRequired)
				{
					var instruction = fDeclaration.CustomsEntryInstructions.AddNew();
					line.JI_CEI = instruction.PK;
				}
				fDeclaration.FilteredInvoiceLines.Add(line);
				SetupDeclarationPreMerge(fDeclaration);
				if (!fDeclaration.DoMerge())
				{
					var merger = new LineMerger(fDeclaration);
					merger.DoMerge();
				}
			}
		}
		bool hasSetup;

		public OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = OrgHeader.New(Factory);
					fSupplier.FillWithValidTestData();
					fSupplier.MiscServ.OM_RX_NKEXDefCurrency = Declaration.LocalCurrencyCode;
				}
				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		public OrgHeader Buyer
		{
			get
			{
				if (fBuyer == null)
				{
					fBuyer = OrgHeader.New(Factory);
					fBuyer.FillWithValidTestData();
				}
				return fBuyer;
			}
		}
		OrgHeader fBuyer;

		protected virtual void SetupDeclarationPreMerge(T declaration)
		{
		}

		protected readonly BusinessObjectFactory Factory;
		readonly string applicationCode;
	}
}
