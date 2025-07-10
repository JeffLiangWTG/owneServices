using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[CodeProperty(AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber), DescriptionProperty(AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber)]
	[SingleObjectAroundARow]
	public class CusReconDeclaration : CusReconBase.CusReconDeclaration, Integration.Customs.ICusReconDeclaration
	{
		public CusReconDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusReconDeclaration.Schema
		{
			public const string AuthorizationNumber = nameof(CusReconDeclaration.AuthorizationNumber);
			public const string BranchCode = nameof(CusReconDeclaration.BranchCode);
			public const string BranchName = nameof(CusReconDeclaration.BranchName);
			public const string CustomsStatusDescription = nameof(CusReconDeclaration.CustomsStatusDescription);
			public const string MessageStatusDescription = nameof(CusReconDeclaration.MessageStatusDescription);
			public const string OfficeDescription = nameof(CusReconDeclaration.OfficeDescription);
		}

		public new static readonly CusReconDeclarationTypeDecider TypeDecider = new CusReconDeclarationTypeDecider();

		[ResourceStringData("1524725D-F66D-4ED7-BE5B-7A00CD450BAC", Caption = "Entry Type")]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.ApplicationCodeList))]
		public override ZString CRD_ApplicationCode
		{
			get => base.CRD_ApplicationCode;
			set => base.CRD_ApplicationCode = value;
		}

		[ResourceStringData("85f609e0-8cfe-4247-a9f8-1846d49c5d44", Caption = "Customs Office")]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.CustomsOfficeList))]
		public override ZString CRD_CustomsOffice
		{
			get => base.CRD_CustomsOffice;
			set => base.CRD_CustomsOffice = value;
		}

		[ResourceStringData("4946E6F2-B2EB-4B63-8D54-B87611E7DA19", Caption = "Authorization")]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.AuthorizationList))]
		public override ZGuid CRD_CPH_ReconClearanceAuthorisation
		{
			get => base.CRD_CPH_ReconClearanceAuthorisation;
			set => base.CRD_CPH_ReconClearanceAuthorisation = value;
		}

		[ResourceStringData("35D61EAF-AFB7-49E7-884B-554C3888E6B6", Caption = "Declaration Type", MediumCaption = "Dec. Type", ShortCaption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.DeclarationTypeList))]
		public override ZString CRD_DeclarationType
		{
			get => base.CRD_DeclarationType;
			set => base.CRD_DeclarationType = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusReconDeclarationLookups.DeclarantTypeList))]
		public override ZString CRD_DeclarantType
		{
			get => base.CRD_DeclarantType;
			set => base.CRD_DeclarantType = value;
		}

		[ResourceStringData("E3E59021-307B-4344-98B4-79A0828E1802", Caption = "Declarant")]
		public override ZGuid CRD_OA_DeclarantAddress
		{
			get => base.CRD_OA_DeclarantAddress;
			set => base.CRD_OA_DeclarantAddress = value;
		}

		[ResourceStringData("C02C1CB4-714A-4F72-A7D1-9AC77A38CA50", Caption = "Representative")]
		public override ZGuid CRD_OA_RepresentativeAddress
		{
			get => base.CRD_OA_RepresentativeAddress;
			set => base.CRD_OA_RepresentativeAddress = value;
		}

		[ResourceStringData("3E15F1E1-C93E-4710-A4EA-134F20082C64", Caption = "Job Number")]
		public override ZString CRD_JobReferenceNumber
		{
			get => base.CRD_JobReferenceNumber;
			set => base.CRD_JobReferenceNumber = value;
		}

		[ResourceStringData("D8E6B38A-9FA8-46F4-96A5-3EE7AB4D951F", Caption = "Period From")]
		public override ZDate CRD_PeriodFrom
		{
			get => base.CRD_PeriodFrom;
			set => base.CRD_PeriodFrom = value;
		}

		[ResourceStringData("53096DBF-8744-44A4-976F-086BDA1E62A7", Caption = "Period To")]
		public override ZDate CRD_PeriodTo
		{
			get => base.CRD_PeriodTo;
			set => base.CRD_PeriodTo = value;
		}

		[ResourceStringData("2F59DA16-16E1-4C3A-AB4F-89E884EC8477", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Stat.")]
		public override ZString CRD_MessageStatus
		{
			get => base.CRD_MessageStatus;
			set => base.CRD_MessageStatus = value;
		}

		[ResourceStringData("3C1DF735-971B-45C6-A9BB-9E8800D1A58A", Caption = "Customs Status")]
		public override ZString CRD_CustomsStatus
		{
			get => base.CRD_CustomsStatus;
			set => base.CRD_CustomsStatus = value;
		}

		[ResourceStringData("EE6BA1D6-103B-414B-B574-17D682EC65F0", Caption = "Branch")]
		public override ZGuid CRD_GB_Branch
		{
			get => base.CRD_GB_Branch;
			set => base.CRD_GB_Branch = value;
		}

		[ResourceStringData("F132AC00-0534-4871-A6B8-CCBBAC914ACF", Caption = "Authorization Number")]
		public virtual ZString AuthorizationNumber => Factory.Load<CusAuthorisationHeader>(CRD_CPH_ReconClearanceAuthorisation)?.CPH_Number ?? ZString.Empty;

		[ResourceStringData("F92CF61A-2246-43D0-8086-B8D086F1E286", Caption = "Branch")]
		public virtual ZString BranchCode => Branch?.GB_Code ?? ZString.Empty;

		[ResourceStringData("3C7C0987-C348-4187-B5ED-7584A9E0005E", Caption = "Branch Name")]
		public virtual ZString BranchName => Branch?.GB_BranchName ?? ZString.Empty;

		[ResourceStringData("7E6C6FC5-2467-41E9-A879-42441E94065A", Caption = "Message Status Description")]
		public virtual ZString MessageStatusDescription
		{
			get
			{
				var status = CRD_MessageStatus;
				return Lookups.MessageStatusList.GetDescriptionFromCode(status) ?? (status.IsEmpty
					? Res.GetString("903AEAE6-5350-46BD-9A59-690240B86979", "Not Sent")
					: Res.GetString("C28F8CC1-41AF-43A1-BD27-2DF6B3FDDD13", "Unknown"));
			}
		}

		[ResourceStringData("23BC1262-D24D-442F-93BC-08326EFF638C", Caption = "Customs Status Description")]
		public virtual ZString CustomsStatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var status = CRD_CustomsStatus;
				if (!status.IsEmpty)
				{
					result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, status, CountryCode,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
						ZDateTime.Now)?.ZZD_Description ?? Res.GetString("B2D6B6AA-F9C0-4DD7-B3AD-15354BD48910", "Unknown");
				}
				return result;
			}
		}

		[ResourceStringData("A725CE7C-6C6C-42C0-BFEC-0534B36DA038", Caption = "Office Description")]
		public virtual ZString OfficeDescription => Factory.GetValue(ref officeDescriptionCached, () =>
		{
			var result = ZString.Empty;
			var officeCode = CRD_CustomsOffice;
			if (!officeCode.IsEmpty)
			{
				var list = Lookups.CustomsOfficeList;
				if (!list.IsLoaded)
				{
					list.Load();
				}
				result = list.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == officeCode)?.ZZD_Description ?? ZString.Empty;
			}
			return result;
		});
		CachedProperty<ZString> officeDescriptionCached;

		[ChildEditable(true)]
		public CusReconEntryCollection CusReconEntries
		{
			get
			{
				if (cusReconEntries == null)
				{
					cusReconEntries = CreateNewCusReconEntryCollection();
					RegisterEditableChildObject(cusReconEntries);
				}
				return cusReconEntries;
			}
		}
		CusReconEntryCollection cusReconEntries;

		protected virtual CusReconEntryCollection CreateNewCusReconEntryCollection() => new CusReconEntryCollection(this);

		public new CusReconDeclarationLookups Lookups => (CusReconDeclarationLookups)base.Lookups;
		protected override CusReconBase.CusReconDeclarationLookups GetNewLookups() => new CusReconDeclarationLookups(this);

		protected override CusReconBase.CusReconDeclarationValidation GetNewValidation() => new CusReconDeclarationValidation(this);

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("DAC517E7-F715-465A-9CEE-0BFC9CB13F17", "Monthly Closing Job");
				if (!CRD_JobReferenceNumber.IsEmpty)
				{
					result += " - " + CRD_JobReferenceNumber;
				}
				return result;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
		}
	}
}
