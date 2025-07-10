using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsTestHelperFunctionsInvoice : WhsTestHelperFunctions
	{
		public WhsTestHelperFunctionsInvoice(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Create Invoice

		public WhsInvoice CreateInvoice(BusinessObjectFactory factory, OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			var result = factory.New<WhsInvoice>();
			result.ET_OH_Client = client.PK;
			result.ET_WW = warehouse.PK;
			result.ET_StorageFromDate = storageFromDate;
			result.ET_StorageToDate = storageToDate;
			return result;
		}

		public WhsInvoice CreateWhsInvoice(OrgHeader client, WhsWarehouse whs, ZDateTime storageFromDate, bool isPosted)
		{
			return CreateWhsInvoice(client, whs, storageFromDate, ZDateTime.Empty, isPosted);
		}

		public WhsInvoice CreateWhsInvoice(OrgHeader client, WhsWarehouse whs, ZDateTime storageFromDate, ZDateTime storageToDate, bool isPosted)
		{
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = whs.PK;
			invoice.ET_StorageFromDate = storageFromDate;

			if (!storageToDate.IsEmpty)
			{
				invoice.ET_StorageToDate = storageToDate;
			}

			invoice.IncludeInInvoicing = true;

			CreateJobHeaderAndCharge(invoice);

			if (isPosted)
			{
				invoice.PostInvoice();
				AssertEquals("Precondition - Charge was not posted.", true, invoice.JobHeader.Charges[0].IsRevenuePosted);
			}

			return invoice;
		}

		#endregion

		#region Create Invoice With JobHeader

		public WhsInvoice CreateInvoiceWithJobHeader(OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			return CreateInvoiceWithJobHeader(Factory, client, warehouse, storageFromDate, storageToDate);
		}

		public WhsInvoice CreateInvoiceWithJobHeader(BusinessObjectFactory factory, OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			var result = CreateInvoice(factory, client, warehouse, storageFromDate, storageToDate);
			CreateJobHeader(factory, result);
			result.Validation.ValidateAll(); // needed because of some refresh issues with ET_StorageFromDate and ET_StorageToDate
			return result;
		}

		#endregion

		#region CreateJobHeader

		public Job CreateJobHeader(BusinessObject bizO)
		{
			return CreateJobHeader(Factory, bizO);
		}

		public Job CreateJobHeader(BusinessObjectFactory factory, BusinessObject bizO)
		{
			var job = factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = bizO.TablePrefix;
			job.JH_ParentID = bizO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GetDepartment(FreightChargeCode).PK;
			job.PlugInData = (IJobInvoicingPlugIn)bizO;

			return job;
		}

		#endregion

		#region Create JobHeader And Charge

		public Job CreateJobHeaderAndCharge(IJobHeaderParent bizO)
		{
			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			return CreateJobHeaderAndCharge(bizO, FreightChargeCode, 100m);
		}

		AccChargeCode FreightChargeCode => Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

		public Job CreateJobHeaderAndCharge(IJobHeaderParent bizO, AccChargeCode chargeCode, ZDecimal localSellAmount)
		{
			var jobHeader = new Job.Loader(bizO).TryCreateWithMutex();
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GE = GetDepartment(chargeCode).PK;
			jobHeader.PlugInData = (IJobInvoicingPlugIn)bizO;
			CreateJobCharge(jobHeader, chargeCode, localSellAmount);
			return jobHeader;
		}

		GlbDepartment GetDepartment(AccChargeCode chargeCode)
		{
			var departmentCodes = chargeCode.AC_DepartmentFilterList
				.Split(',')
				.Select(x => x.Trim())
				.Where(x => x != "ALL");

			var query = departmentCodes.Any()
				? new ZQuery(GlbDepartmentSchema.GE_Code, departmentCodes)
				: new ZQuery();

			return Factory.LoadTop1<GlbDepartment>(query);
		}

		#endregion

		#region Create JobCharges

		public Charge CreateJobCharges(Job jobHeader)
		{
			var charge = jobHeader.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_SellRatingOverride = true;
			charge.JR_CostRatingOverride = true;

			return charge;
		}

		#endregion

		#region AutoRateJob

		public AutoRateInfoCollection AutoRateJob(WhsInvoice invoice)
		{
			var job = invoice.JobHeader ?? (CreateRatingJob(invoice));
			return AutoRateJob(invoice, job, new[] { GetIAutoRating(invoice) });
		}

		protected override IAutoRating GetIAutoRating(BusinessObject bizO)
		{
			if (bizO is WhsInvoice)
			{
				return new WhsInvoiceRatingAdapter((WhsInvoice)bizO);
			}

			return base.GetIAutoRating(bizO);
		}

		#endregion

		#region Create AutoRate Info Collection

		/// <summary>
		/// This is OBSOLETE use Helper.AutoRateJob(...)
		/// </summary>
		public AutoRateInfoCollection CreateAutoRateInfoCollection_Obsolete(WhsInvoice invoice)
		{
			var adaptersProvider = new Mock<IRatingAdaptersProvider>();
			var runner = new AutoRatingRunner(invoice, new RatingContext());
			var result = runner.RetrieveAllCharges(invoice.GetRatingAdapters().ToArray(), adaptersProvider.Object, CostSell.Revenue);
			result.Sort(AutoRateInfo.GetRateInfoComparer(AutoRateInfo.Schema.InvoiceLineDescription));

			return result;
		}

		#endregion

		#region Create OrgInvoice Rollup Or Group

		public OrgInvoiceRollupOrGroup CreateOrgInvoiceRollupOrGroup(OrgHeader client, ZString direction, ZString mode, ZString jobType, ZString display, ZString style, ZString invoice, ZString posting)
		{
			var result = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			result.PG_JobType = jobType;
			result.PG_ServiceDirection = direction;
			result.PG_TransportMode = mode;
			result.PG_GroupOrSubTotal = display;
			result.PG_GroupOrSubtotalStyle = style;
			result.PG_InvoiceLineDisplayOption = invoice;
			result.PG_InvoicePostingStyle = posting;
			return result;
		}

		#endregion
	}
}
