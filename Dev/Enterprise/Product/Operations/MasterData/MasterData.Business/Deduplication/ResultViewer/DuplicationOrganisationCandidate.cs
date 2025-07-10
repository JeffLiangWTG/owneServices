using System.ComponentModel;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common.Deduplication.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class DuplicationOrganisationCandidate : DuplicationCandidate
	{
		public DuplicationOrganisationCandidate(DeduplicationPresenterModel presenterModel, DeduplicationOrgHeader orgHeader)
			: base(presenterModel, orgHeader)
		{
			if (orgHeader is null)
			{
				return;
			}

			DeduplicationOrgHeader = orgHeader;
			Code = orgHeader.OH_Code;
			DebtorCompany = orgHeader.DebtorCompany;
			CreditorCompany = orgHeader.CreditorCompany;
			UNLOCO = orgHeader.UNLOCO?.RL_Code ?? ZString.Empty;

			if (ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI
				&& FactoryForLoadEDIOrgHeader.Load<OrgHeader>(orgHeader.OH_PK) is IEDIOrgHeader edi)
			{
				EnterpriseId = edi.LicenceEnterpriseID;
				EnterpriseCode = edi.LicenceEnterpriseCode;
				CompanyCode = edi.CompanyCode;
				ProductId = ZString.Join(", ", edi.ProductId.Distinct().ToArray());
			}
		}

		DuplicationOrganisationCandidate() : base() { }

		public DeduplicationOrgHeader DeduplicationOrgHeader { get; set; }

		public static DuplicationOrganisationCandidate Empty { get; } = new DuplicationOrganisationCandidate();

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public ZString Code { get; }

		public ZPropertyInfo UNLOCOInfo => GetZPropertyInfo(nameof(UNLOCO));
		public ZString UNLOCO { get; }

		public ZPropertyInfo DebtorCompanyInfo => GetZPropertyInfo(nameof(DebtorCompany));
		public ZString DebtorCompany { get; }

		public ZPropertyInfo CreditorCompanyInfo => GetZPropertyInfo(nameof(CreditorCompany));
		public ZString CreditorCompany { get; }

		#region MDM Admin Panel

		public ZPropertyInfo AssociatedConsolsCountInfo => GetZPropertyInfo(nameof(AssociatedConsolsCount));

		[ReadOnly(true)]
		public ZInt AssociatedConsolsCount
		{
			get => associatedConsolsCount;
			set
			{
				associatedConsolsCount = value;
				AssociatedConsolsCountInfo.RefreshBinding();
			}
		}
		int associatedConsolsCount;

		public ZPropertyInfo AssociatedDeclarationsCountInfo => GetZPropertyInfo(nameof(AssociatedDeclarationsCount));

		[ReadOnly(true)]
		public ZInt AssociatedDeclarationsCount
		{
			get => associatedDeclarationsCount;
			set
			{
				associatedDeclarationsCount = value;
				AssociatedDeclarationsCountInfo.RefreshBinding();
			}
		}
		int associatedDeclarationsCount;

		public ZPropertyInfo AssociatedShipmentsCountInfo => GetZPropertyInfo(nameof(AssociatedShipmentsCount));
		[ReadOnly(true)]
		public ZInt AssociatedShipmentsCount
		{
			get => associatedShipmentsCount;
			set
			{
				associatedShipmentsCount = value;
				AssociatedShipmentsCountInfo.RefreshBinding();
			}
		}
		int associatedShipmentsCount;

		public override void SetAdminPanelProperties(PatternMatchingResult matchingResult)
		{
			base.SetAdminPanelProperties(matchingResult);

			AssociatedConsolsCount = IsDummy ? 0 : OrgHeader.GetAssociatedConsols(TargetPK).Count;
			AssociatedDeclarationsCount = IsDummy ? 0 : OrgHeader.GetAssociatedDeclarations(TargetPK).Count;
			AssociatedShipmentsCount = IsDummy ? 0 : OrgHeader.GetAssociatedShipments(TargetPK).Count;
		}

		#endregion MDM Admin Panel

		#region EDI

		public ZPropertyInfo EnterpriseIdInfo => GetZPropertyInfo(nameof(EnterpriseId));
		public ZString EnterpriseId { get; }

		public ZPropertyInfo EnterpriseCodeInfo => GetZPropertyInfo(nameof(EnterpriseCode));
		public ZString EnterpriseCode { get; }

		public ZPropertyInfo CompanyCodeInfo => GetZPropertyInfo(nameof(CompanyCode));
		public ZString CompanyCode { get; }

		public ZPropertyInfo ProductIdInfo => GetZPropertyInfo(nameof(ProductId));
		public ZString ProductId { get; }

		#endregion EDI

		#region Implementation

		ReadOnlyBusinessObjectFactory FactoryForLoadEDIOrgHeader => factoryForLoadEDIOrgHeader ?? (factoryForLoadEDIOrgHeader = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factoryForLoadEDIOrgHeader;

		protected override string TranslatedMasterIsNullDescription => throw new System.NotImplementedException();

		#endregion Implementation
	}
}
