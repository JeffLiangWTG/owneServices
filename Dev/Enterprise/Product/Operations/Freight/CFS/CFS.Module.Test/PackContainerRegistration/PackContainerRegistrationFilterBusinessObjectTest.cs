using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using JobSailing = Enterprise.Freight.Business.JobSailing;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(PackContainerRegistrationFilterBusinessObject))]
	sealed class PackContainerRegistrationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region IsCFSRegistered Test

		public void TestIsCFSRegisteredFilter()
		{
			Container1.JC_IsCFSRegistered = true;
			Container2.JC_IsCFSRegistered = false;

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Flags Filter Tests

		public void TestAttachedFilter()
		{
			Container1.JC_JK = ZGuid.Empty;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Attached To Load List"];

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property = "IAL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property = "NAL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Number Filter Tests

		public void TestContainerNumberFilter()
		{
			Container1.JC_ContainerNum = "11100011";
			Container2.JC_ContainerNum = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Container];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestJobNumberFilter()
		{
			Container1.JC_ContainerJobID = "11100011";
			Container2.JC_ContainerJobID = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Job];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestMasterBillNumberFilter()
		{
			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Container1.JC_FCLStorageModuleOnlyMaster = "11100011";
			Container2.JC_FCLStorageModuleOnlyMaster = "22000222";

			LoadList1.JK_MasterBillNum = "33399933";
			LoadList2.JK_MasterBillNum = "44999444";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.MasterBill];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "44999444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "999";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "777";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestCustomsEntryNumberFilter()
		{
			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			LoadList1.JK_CustomsReference = "11100011";
			LoadList2.JK_CustomsReference = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.CustomsEntryNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestCommonNumberFilter()
		{
			Container1.JC_ContainerNum = "11111111";
			Container2.JC_ContainerNum = "22222222";

			Container1.JC_ContainerJobID = "33333333";
			Container2.JC_ContainerJobID = "44444444";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Common Numbers"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Date Filter Tests

		public void TestETDDateFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();

			origin1.JA_E_DEP = new ZDateTime(2000, 1, 1);
			origin2.JA_E_DEP = new ZDateTime(2000, 2, 2);

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETD];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestETDDateFilter_HasNoDate()
		{
			var originWithNoETD = Factory.NewWithValidTestData<VoyageOrigin>();
			var originWithETD = Factory.NewWithValidTestData<VoyageOrigin>();

			originWithNoETD.JA_E_DEP = ZDateTime.Empty;
			originWithETD.JA_E_DEP = new ZDateTime(2000, 2, 2);

			var sailingWithNoETD = Factory.NewWithValidTestData<JobSailing>();
			var sailingWithETD = Factory.NewWithValidTestData<JobSailing>();

			sailingWithNoETD.JX_JA = originWithNoETD.PK;
			sailingWithETD.JX_JA = originWithETD.PK;

			// Container with a Consol with transport but no Job Sailing
			Container1.JC_JK = LoadList1.PK;
			LoadList1.Transports.AddNew();

			// Container with a Consol with transport and Job Sailing and not null ETD
			Container3.JC_JK = LoadList3.PK;
			var transport3 = LoadList3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailingWithETD.PK;

			// Container with a Consol without a transport
			var container4 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList4 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container4.JC_JK = loadList4.PK;

			// Container with a Consol with transport and Job Sailing and null ETD
			var container5 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList5 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container5.JC_JK = loadList5.PK;
			var transport5 = loadList5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailingWithNoETD.PK;

			// Container with a Job Sailing and not null ETD
			var container6 = Factory.NewWithValidTestData<CFSContainer>();
			container6.JC_JX = sailingWithETD.PK;

			// Container with a Job Sailing and null ETD
			var container7 = Factory.NewWithValidTestData<CFSContainer>();
			container7.JC_JX = sailingWithNoETD.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETD];

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			CombineAssertions(() =>
			{
				Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
				Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));  // Container with no Consol nor Job Sailing
				Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
				Assert("Expecting collection to contain Container4", FilterCollection.Contains(container4));
				Assert("Expecting collection to contain Container5", FilterCollection.Contains(container5));
				Assert("Expecting collection not to contain Container6", !FilterCollection.Contains(container6));
				Assert("Expecting collection to contain Container7", FilterCollection.Contains(container7));
			});
		}

		public void TestETDDateFilter_HasDate()
		{
			var originWithNoETD = Factory.NewWithValidTestData<VoyageOrigin>();
			var originWithETD = Factory.NewWithValidTestData<VoyageOrigin>();

			originWithNoETD.JA_E_DEP = ZDateTime.Empty;
			originWithETD.JA_E_DEP = new ZDateTime(2000, 2, 2);

			var sailingWithNoETD = Factory.NewWithValidTestData<JobSailing>();
			var sailingWithETD = Factory.NewWithValidTestData<JobSailing>();

			sailingWithNoETD.JX_JA = originWithNoETD.PK;
			sailingWithETD.JX_JA = originWithETD.PK;

			// Container with a Consol with transport but no Job Sailing
			Container1.JC_JK = LoadList1.PK;
			LoadList1.Transports.AddNew();

			// Container with a Consol with transport and Job Sailing and not null ETD
			Container3.JC_JK = LoadList3.PK;
			var transport3 = LoadList3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailingWithETD.PK;

			// Container with a Consol without a transport
			var container4 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList4 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container4.JC_JK = loadList4.PK;

			// Container with a Consol with transport and Job Sailing and null ETD
			var container5 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList5 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container5.JC_JK = loadList5.PK;
			var transport5 = loadList5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailingWithNoETD.PK;

			// Container with a Job Sailing and not null ETD
			var container6 = Factory.NewWithValidTestData<CFSContainer>();
			container6.JC_JX = sailingWithETD.PK;

			// Container with a Job Sailing and null ETD
			var container7 = Factory.NewWithValidTestData<CFSContainer>();
			container7.JC_JX = sailingWithNoETD.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETD];

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));  // Container with no Consol nor Job Sailing
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));
			Assert("Expecting collection not to contain Container4", !FilterCollection.Contains(container4));
			Assert("Expecting collection not to contain Container5", !FilterCollection.Contains(container5));
			Assert("Expecting collection to contain Container6", FilterCollection.Contains(container6));
			Assert("Expecting collection not to contain Container7", !FilterCollection.Contains(container7));
		}

		public void TestETADateFilter()
		{
			VoyageDestination destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			VoyageDestination destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			destination1.JB_E_ARV = new ZDateTime(2000, 1, 1);
			destination2.JB_E_ARV = new ZDateTime(2000, 2, 2);

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETA];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestETADateFilter_HasNoDate()
		{
			var destinationWithNoETD = Factory.NewWithValidTestData<VoyageDestination>();
			var destinationWithETD = Factory.NewWithValidTestData<VoyageDestination>();

			destinationWithNoETD.JB_E_ARV = ZDateTime.Empty;
			destinationWithETD.JB_E_ARV = new ZDateTime(2000, 2, 2);

			var sailingWithNoETD = Factory.NewWithValidTestData<JobSailing>();
			var sailingWithETD = Factory.NewWithValidTestData<JobSailing>();

			sailingWithNoETD.JX_JB = destinationWithNoETD.PK;
			sailingWithETD.JX_JB = destinationWithETD.PK;

			// Container with a Consol with transport but no Job Sailing
			Container1.JC_JK = LoadList1.PK;
			LoadList1.Transports.AddNew();

			// Container with a Consol with transport and Job Sailing and not null ETD
			Container3.JC_JK = LoadList3.PK;
			var transport3 = LoadList3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailingWithETD.PK;

			// Container with a Consol without a transport
			var container4 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList4 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container4.JC_JK = loadList4.PK;

			// Container with a Consol with transport and Job Sailing and null ETD
			var container5 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList5 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container5.JC_JK = loadList5.PK;
			var transport5 = loadList5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailingWithNoETD.PK;

			// Container with a Job Sailing and not null ETD
			var container6 = Factory.NewWithValidTestData<CFSContainer>();
			container6.JC_JX = sailingWithETD.PK;

			// Container with a Job Sailing and null ETD
			var container7 = Factory.NewWithValidTestData<CFSContainer>();
			container7.JC_JX = sailingWithNoETD.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETA];

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));  // Container with no Consol nor Job Sailing
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
			Assert("Expecting collection to contain Container4", FilterCollection.Contains(container4));
			Assert("Expecting collection to contain Container5", FilterCollection.Contains(container5));
			Assert("Expecting collection not to contain Container6", !FilterCollection.Contains(container6));
			Assert("Expecting collection to contain Container7", FilterCollection.Contains(container7));
		}

		public void TestETADateFilter_HasDate()
		{
			var destinationWithNoETD = Factory.NewWithValidTestData<VoyageDestination>();
			var destinationWithETD = Factory.NewWithValidTestData<VoyageDestination>();

			destinationWithNoETD.JB_E_ARV = ZDateTime.Empty;
			destinationWithETD.JB_E_ARV = new ZDateTime(2000, 2, 2);

			var sailingWithNoETD = Factory.NewWithValidTestData<JobSailing>();
			var sailingWithETD = Factory.NewWithValidTestData<JobSailing>();

			sailingWithNoETD.JX_JB = destinationWithNoETD.PK;
			sailingWithETD.JX_JB = destinationWithETD.PK;

			// Container with a Consol with transport but no Job Sailing
			Container1.JC_JK = LoadList1.PK;
			LoadList1.Transports.AddNew();

			// Container with a Consol with transport and Job Sailing and not null ETD
			Container3.JC_JK = LoadList3.PK;
			var transport3 = LoadList3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailingWithETD.PK;

			// Container with a Consol without a transport
			var container4 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList4 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container4.JC_JK = loadList4.PK;

			// Container with a Consol with transport and Job Sailing and null ETD
			var container5 = Factory.NewWithValidTestData<CFSContainer>();
			var loadList5 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			container5.JC_JK = loadList5.PK;
			var transport5 = loadList5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailingWithNoETD.PK;

			// Container with a Job Sailing and not null ETD
			var container6 = Factory.NewWithValidTestData<CFSContainer>();
			container6.JC_JX = sailingWithETD.PK;

			// Container with a Job Sailing and null ETD
			var container7 = Factory.NewWithValidTestData<CFSContainer>();
			container7.JC_JX = sailingWithNoETD.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETA];

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));  // Container with no Consol nor Job Sailing
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));
			Assert("Expecting collection not to contain Container4", !FilterCollection.Contains(container4));
			Assert("Expecting collection not to contain Container5", !FilterCollection.Contains(container5));
			Assert("Expecting collection to contain Container6", FilterCollection.Contains(container6));
			Assert("Expecting collection not to contain Container7", !FilterCollection.Contains(container7));
		}

		public void TestUnpackDateFilter()
		{
			Container1.JC_LCLUnpack = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			Container2.JC_LCLUnpack = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Unpack];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestAvailableDateFilter()
		{
			Container1.JC_LCLAvailable = new ZDateTime(2000, 1, 1, 11, 0, 0);       // 2 Jan 11:00
			Container2.JC_LCLAvailable = new ZDateTime(2000, 2, 2, 22, 0, 0);       // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Available];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestStorageDateFilter()
		{
			Container1.JC_LCLStorageCommences = new ZDateTime(2000, 1, 1, 11, 0, 0);        // 2 Jan 11:00
			Container2.JC_LCLStorageCommences = new ZDateTime(2000, 2, 2, 22, 0, 0);        // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Storage];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestPackDateFilter()
		{
			Container1.JC_PackDate = new ZDateTime(2000, 1, 1, 11, 0, 0);       // 2 Jan 11:00
			Container2.JC_PackDate = new ZDateTime(2000, 2, 2, 22, 0, 0);       // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Pack];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestArrivalDateFilter()
		{
			ContainerLeg1 = Container1.OriginCFSArrival;
			ContainerLeg2 = Container2.OriginCFSArrival;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Arrival];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestArrivalDateFilter_HasNoDate()
		{
			ContainerLeg1 = Container1.OriginCFSArrival;
			ContainerLeg2 = Container2.OriginCFSArrival;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = ZDateTime.Empty;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Arrival];

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));
		}

		public void TestArrivalDateFilter_HasDate()
		{
			ContainerLeg1 = Container1.OriginCFSArrival;
			ContainerLeg2 = Container2.OriginCFSArrival;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = ZDateTime.Empty;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Arrival];

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
		}

		public void TestDepartureDateFilter()
		{
			ContainerLeg1 = Container1.OriginCFSDeparture;
			ContainerLeg2 = Container2.OriginCFSDeparture;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Departure];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestDepartureDateFilter_HasNoDate()
		{
			ContainerLeg1 = Container1.OriginCFSDeparture;
			ContainerLeg2 = Container2.OriginCFSDeparture;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = ZDateTime.Empty;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Departure];

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));
		}

		public void TestDepartureDateFilter_HasDate()
		{
			ContainerLeg1 = Container1.OriginCFSDeparture;
			ContainerLeg2 = Container2.OriginCFSDeparture;

			ContainerLeg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			ContainerLeg2.EU_PickupDeliveryTime = ZDateTime.Empty;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Departure];

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
		}

		#endregion

		#region Organisation Filter Tests

		public void TestClientFilter()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();

			client1.OH_IsDebtor = ZBool.True;
			client2.OH_IsDebtor = ZBool.True;
			client3.OH_IsDebtor = ZBool.True;

			Container1.JC_OH_CFSClient = client1.PK;
			Container2.JC_OH_CFSClient = client2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[ConstantsAndReusables.OrgFilterTypes.Client];

			filter.Property = client1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = client3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestShippingLineFilter()
		{
			OrgHeader shippingLine1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader shippingLine2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader shippingLine3 = Factory.NewWithValidTestData<OrgHeader>();

			shippingLine1.OH_IsShippingLine = ZBool.True;
			shippingLine2.OH_IsShippingLine = ZBool.True;
			shippingLine3.OH_IsShippingLine = ZBool.True;

			LoadList1.JK_OA_ShippingLineAddress = shippingLine1.MainAddress.PK;
			LoadList2.JK_OA_ShippingLineAddress = shippingLine2.MainAddress.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[ConstantsAndReusables.OrgFilterTypes.Line];

			filter.Property = shippingLine1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = shippingLine3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Location Filter Tests

		public void TestLoadDischargeFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageDestination destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			VoyageDestination destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			origin1.JA_RL_NKPortOfLoading = "KRSEL";
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;
			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ConstantsAndReusables.PortFilterTypes.LoadDischarge];

			filter.Property1 = "KR";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property1 = "AUBNE";
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Vessel / Voyage Filter Tests

		public void TestVesselFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.Vessel = "XAAAX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Vessel = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Vessel = "YAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
		}

		public void TestVoyageFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();

			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.VoyageFlightNo = "XAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "YAAAY";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "AAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Mode / Type Filter Tests

		public void TestPurposeTypeFilter()
		{
			Container1.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			Container2.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Purpose Type"];

			filter.Property = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestContainerModeFilter()
		{
			Container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			Container2.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Container Mode"];

			filter.Property = Constants.ContainerModes.FCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = Constants.ContainerModes.LCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property = Constants.ContainerModes.Bulk;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region List Tests

		public void TestListProperties()
		{
			AssertNotNull(FilterBO.Client_List);
			AssertNotNull(FilterBO.ShippingLine_List);
			AssertNotNull(FilterBO.Organisation_List);
			AssertNotNull(FilterBO.Location_List);
			AssertNotNull(FilterBO.Vessel_List);
			AssertNotNull(FilterBO.PurposeType_List);
			AssertNotNull(FilterBO.ContainerMode_List);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PackContainerRegistrationFilterBusinessObject();
		}

		CFSContainer Container1;
		CFSContainer Container2;
		CFSContainer Container3;

		CFSLoadListConsol LoadList1;
		CFSLoadListConsol LoadList2;
		CFSLoadListConsol LoadList3;

		CommonPickupDeliveryConfirm ContainerLeg1;
		CommonPickupDeliveryConfirm ContainerLeg2;

		CFSContainerRegistrationList FilterCollection;
		PackContainerRegistrationFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Container1 = Factory.NewWithValidTestData<CFSContainer>();
			Container2 = Factory.NewWithValidTestData<CFSContainer>();
			Container3 = Factory.NewWithValidTestData<CFSContainer>();

			Container1.JC_IsCFSRegistered = ZBool.True;
			Container2.JC_IsCFSRegistered = ZBool.True;
			Container3.JC_IsCFSRegistered = ZBool.True;

			LoadList1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			LoadList2 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			LoadList3 = Factory.NewWithValidTestData<CFSLoadListConsol>();

			LoadList1.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList2.JK_TransportMode = Constants.TransportModes.Sea;
			LoadList3.JK_TransportMode = Constants.TransportModes.Sea;

			ContainerLeg1 = Factory.NewWithValidTestData<CommonPickupDeliveryConfirm>();
			ContainerLeg2 = Factory.NewWithValidTestData<CommonPickupDeliveryConfirm>();

			FilterCollection = new CFSContainerRegistrationList(Factory);
			FilterBO = (PackContainerRegistrationFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
