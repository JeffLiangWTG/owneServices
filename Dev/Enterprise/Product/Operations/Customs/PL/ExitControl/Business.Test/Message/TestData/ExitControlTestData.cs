using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

public sealed record ExitControlTestData
{
	const string DefaultMRN = "123456";

	public JobDeclaration Declaration { get; }
	public CusEntryHeader EntryHeader { get; }

	public CusExitHeader ExitHeader { get; }
	public CusExitConsignment ExitConsignment { get; }
	public CusExitReport ExitReport { get; }

	public CusEntryNumber EntryNumberMRN { get; }
	public string MRN
	{
		get => ExitConsignment.CXC_ReferenceNumber;
		set
		{
			if (EntryNumberMRN != null)
			{
				EntryNumberMRN.CE_EntryNum = value;
			}
			ExitConsignment.CXC_ReferenceNumber = value;
		}
	}

	public ExitControlTestData(BusinessObjectFactory factory,
		bool linkedWithDeclaration = true,
		string declarationType = EUJobMessageTypeList.Codes.Import,
		string mrn = DefaultMRN)
	{
		if (linkedWithDeclaration)
		{
			Declaration = factory.New<JobDeclaration>();
			Declaration.JE_MessageType = declarationType;
			EntryHeader = Declaration.CustomsEntryHeaders.AddNew();

			EntryNumberMRN = factory.New<CusEntryNumber>();
			EntryNumberMRN.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			EntryNumberMRN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
			EntryNumberMRN.CE_EntryNum = mrn;
			EntryNumberMRN.Parent = EntryHeader;
		}

		ExitHeader = factory.New<CusExitHeader>();
		ExitHeader.Parent = Declaration;
		ExitConsignment = ExitHeader.CusExitConsignments.AddNew();
		ExitConsignment.CXC_MovementReference = mrn;
		ExitReport = ExitHeader.CusExitReports.AddNew();
		ExitReport.CER_CXC_Consignment = ExitConsignment.PK;
	}
}
