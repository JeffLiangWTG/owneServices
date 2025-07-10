using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAJobDeclarationIAccIntegrationDataProviderTest : TestCaseWithFactory
	{
		public void TestAutoRatingDSBShouldNotHappenTwice()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";
			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();
			var option = new Customs.DataRegistry.Business.AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.PreApprovalBillingJob = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CSTA", "CustomsStatus");
			var clr = helper.CreateCusCodeList("ZA", "CSTA", "1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateCusCodeListAttribute(clr.PK, "IPostCustomsAPInvoice", "true");
			var type1p1 = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Constants.RateTypes.Duty);
			helper.LoadOrCreateNewCusRateCode(Factory, "1P1", type1p1.PK);
			Factory.Save();
			var tracker = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
			declaration.Tracker = tracker;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_BGMReference = "0001";
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("1P1", 100m);
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "0002";
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate("1P1", 200m);
			var message1 = Factory.New<CUSRESEDIMessageForTest>();
			message1.EM_MessageText = CUSRES_GIS6_Message;
			entryHeader1.Messages.Add(message1);
			var message2 = Factory.New<CUSRESEDIMessageForTest>();
			message2.EM_MessageText = CUSRES_GIS26_Message;
			entryHeader2.Messages.Add(message2);
			Factory.Save();
			new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours.AutoRateDSB, declaration.ActiveEntryHeaders.Select(x => x.PK), declaration.PK, true, Factory));
			AssertNotNull(declaration.Job);
			CombineAssertions("Initial Rating", () =>
			{
				var invoiceJob = new BusinessObjectFactory().Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);
				AssertEquals(2, invoiceJob.Charges.Count);
				Assert(invoiceJob.Charges.OfType<JobCharge>().Any(x => x.JR_APInvoiceNum == "0001"));
				Assert(invoiceJob.Charges.OfType<JobCharge>().Any(x => x.JR_APInvoiceNum == "0002"));
			});
			entryHeader1.MovementReferenceNumberSetter("0003", ZDateTime.Today);
			entryHeader2.CH_BGMReference = "0004";
			declaration.SpecialThingToDoAfterAutoBilling = () =>
			{
				entryHeader1.MovementReferenceNumberSetter("0005", ZDateTime.Today);
			};
			entryHeader1.MarkNeedsAutoRateDSB();
			entryHeader2.MarkNeedsAutoRateDSB();
			entryHeader1.CH_EntryStatus = "1";
			Factory.Save();
			AssertNotNull(declaration.Job);
			CombineAssertions("ReRatingShouldHappen to latest", () =>
			{
				var invoiceJob = new BusinessObjectFactory().Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);
				AssertEquals(2, invoiceJob.Charges.Count);
				Assert(invoiceJob.Charges.OfType<JobCharge>().Any(x => x.JR_APInvoiceNum == "0003/CUSDSB"));
				Assert(invoiceJob.Charges.OfType<JobCharge>().Any(x => x.JR_APInvoiceNum == "0004/CUSDSB"));
			});
			AssertEquals("", tracker.ToStringWithNewLineBetweenAppends());
			entryHeader2.CH_EntryStatus = "1";
			Factory.Save();
			AssertEquals(string.Format(@"AutoRateDSB, SendEmail
{0}
{1}", entryHeader1.PK, entryHeader2.PK), tracker.ToStringWithNewLineBetweenAppends());
		}

		const string CUSRES_GIS6_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655BBR20160513000034::00001'DTM+9:20160513143147:202'TDT+20'LOC+22+BBR'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+BH:00505655'RFF+AAS:HENRYMASTER1'RFF+ACD:35'ERP+1:0000'ERC+0000'FTX+AAO+++Line number may not be 0'UNT+15+1'";
		const string CUSRES_GIS26_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:152'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal ZStringBuilder Tracker;
			internal Action SpecialThingToDoAfterAutoBilling;
			protected override JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider()
			{
				return new ZAJobDeclarationIAccIntegrationDataProviderForTest(this, Tracker, SpecialThingToDoAfterAutoBilling);
			}

			protected override bool IsJobReadyForPost
			{
				get
				{
					return false;
				}
			}
		}

		sealed class ZAJobDeclarationIAccIntegrationDataProviderForTest : ZAJobDeclarationIAccIntegrationDataProvider
		{
			public ZAJobDeclarationIAccIntegrationDataProviderForTest(JobDeclaration declaration, ZStringBuilder tracker, Action specialThingToDoAfterAutoBilling) : base(declaration)
			{
				this.tracker = tracker;
				this.specialThingToDoAfterAutoBilling = specialThingToDoAfterAutoBilling;
			}

			readonly ZStringBuilder tracker;
			readonly Action specialThingToDoAfterAutoBilling;
			protected override void OnIntegratedWithAccountingSuccessfully()
			{
				base.OnIntegratedWithAccountingSuccessfully();
				tracker.Append(actions.ToString());
				foreach (var entryHeaderHandled in this.entryHeaderPKs)
				{
					tracker.Append(entryHeaderHandled.ToString());
				}

				specialThingToDoAfterAutoBilling();
			}
		}
	}
}
