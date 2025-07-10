using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.PL.Business.CusTempStorage.CusTempStorageJobHeader>;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageJobHeader : EU.Business.CusTempStorage.CusTempStorageJobHeader
	, Integration.Customs.PL.ICusTempStorageJobHeader
{
	public CusTempStorageJobHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override AutologState AutoLoggingState => AutologState.AutoLogged;

	protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups GetNewLookups() => new CusTempStorageJobHeaderLookups(this);

	public new CusTempStorageJobHeaderLookups Lookups => (CusTempStorageJobHeaderLookups)base.Lookups;

	public new CusTempStorageDecCollection CusTempStorageDecs => (CusTempStorageDecCollection)base.CusTempStorageDecs;

	protected override EU.Business.CusTempStorage.CusTempStorageDecCollection CreateNewCusTempStorageDecs() => new CusTempStorageDecCollection(this);

	public new CusTempStorageDec CusTempStorageDec => (CusTempStorageDec)base.CusTempStorageDec;

	public static CusTempStorageJobHeader New(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageJobHeader>();
		var storageDec = CusTempStorageDec.New(header);
		storageDec.CusTempStorageLines.AddNew();
		return header;
	}

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateJobReferenceIfNeeded();
	}

	public void PopulateJobReferenceIfNeeded()
	{
		var branchCode = GlbBranch.CurrentBranch.GB_Code;
		var yearString = ZDateTime.Now.ToString();
		PopulateNumberPropertyIfRequired<ZString>(SJH_JobReferenceInfo, x => $"{branchCode}_Temporary_{yearString}");
	}

	protected override ZString HumanReadableNameCore => SJH_JobReference;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SJH_AppCode = "IST"; // TODO: proper code to be defined
	}
	[List(nameof(Lookups) + "." + nameof(CusTempStorageJobHeaderLookups.GuaranteeList))]
	public override ZGuid SJH_CPH_Guarantee { get => base.SJH_CPH_Guarantee; set => base.SJH_CPH_Guarantee = value; }

	[ResourceStringData("PLCusTempStorageJobHeader|SJH_TransportMeansCode", Caption = "Transport Means", MediumCaption = "Transp. Means", ShortCaption = "Transp.")]
	public override ZString SJH_TransportMeansCode { get => base.SJH_TransportMeansCode; set => base.SJH_TransportMeansCode = value; }

	[ResourceStringData("PLCusTempStorageJobHeader|SJH_DepartureDate", Caption = "Departure Date", MediumCaption = "Departure", ShortCaption = "Dep.")]
	public override ZDate SJH_DepartureDate { get => base.SJH_DepartureDate; set => base.SJH_DepartureDate = value; }

	[ResourceStringData("PLCusTempStorageJobHeader|SJH_TempStorageEndDateUtc", Caption = "Temporary Storage End Date", MediumCaption = "End Date", ShortCaption = "End")]
	public override ZDateTime SJH_TempStorageEndDateUtc { get => base.SJH_TempStorageEndDateUtc; set => base.SJH_TempStorageEndDateUtc = value; }
}
