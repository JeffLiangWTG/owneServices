using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageLineItem))]
class CusTempStorageLineItemTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObject(Factory);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject(Factory);
	}

	protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var customer = factory.NewWithValidTestData<OrgHeader>();
		customer.OH_Code = "C1";
		var presenter = factory.NewWithValidTestData<OrgAddress>();
		var representative = factory.NewWithValidTestData<OrgAddress>();

		var fromJob = CusTempStorageJobHeader.New(factory);
		fromJob.SJH_GB = GlbBranch.CurrentBranch.PK;
		fromJob.SJH_JobReference = "From1";
		fromJob.SJH_OH_Customer = customer.PK;
		fromJob.SJH_OA_Presenter = presenter.PK;
		fromJob.SJH_OA_Representative = representative.PK;

		var fromDec = fromJob.CusTempStorageDec;

		var storageLine = fromDec.CusTempStorageLines.FirstOrDefault() as CusTempStorageLine;
		storageLine.TSL_LineNo = 1;
		storageLine.TSL_ReferenceNumberLine = 1;

		return storageLine.CusTempStorageLineItems.AddNew();
	}
}
