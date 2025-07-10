using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CommonCusReference : CusReference, Integration.Customs.ICusReference
	{
		public CommonCusReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusReference.Schema
		{
			public const string OwnerOrgPK = nameof(CommonCusReference.OwnerOrgPK);
		}

		public new CommonCusReferenceLookups Lookups => (CommonCusReferenceLookups)base.Lookups;
		protected override CusReferenceLookups GetNewLookups() => new CommonCusReferenceLookups(this);

		public new CommonCusReferenceValidation Validation => (CommonCusReferenceValidation)base.Validation;

		protected override CusReferenceValidation GetNewValidation() => new CommonCusReferenceValidation(this);

		[List(nameof(Lookups) + "." + nameof(CommonCusReferenceLookups.CodeList))]
		[ResourceStringData("39F9D529-4A4F-4C29-BB1A-F1B15AA6B6C1", Caption = "Code")]
		public override ZString CFR_Code
		{
			get => base.CFR_Code;
			set => base.CFR_Code = value;
		}

		[ResourceStringData("97DEC2F8-028B-4EDA-8990-A215A96F153F", Caption = "Reference")]
		[ReadOnlyMember(nameof(CFR_Reference_ReadOnly))]
		public override ZString CFR_Reference
		{
			get => base.CFR_Reference;
			set => base.CFR_Reference = value;
		}

		[ResourceStringData("D8A2D800-AB35-4A59-9ABF-D26F02393DB4", Caption = "Address")]
		public override ZGuid CFR_OA_Owner
		{
			get => base.CFR_OA_Owner;
			set => base.CFR_OA_Owner = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonCusReferenceLookups.OwnersList))]
		[ResourceStringData("2A19B42F-D2A5-438A-8E0A-8B536C166F51", Caption = "Owner")]
		public virtual ZGuid OwnerOrgPK
		{
			get => CFR_OA_Owner_ZAddress.OrgPK;
			set
			{
				var oldValue = OwnerOrgPK;
				CFR_OA_Owner_ZAddress.OrgPK = value;
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateOwnerOrgPK();
				}
				OwnerOrgPKInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo OwnerOrgPKInfo => GetZPropertyInfo(Schema.OwnerOrgPK);

		protected override ZAddress GetNewCFR_OA_Owner_ZAddress()
		{
			var address = base.GetNewCFR_OA_Owner_ZAddress();
			address.GetDefaultAddress = (header) => header?.MainAddress.PK ?? ZGuid.Empty;
			return address;
		}

		protected ZBool CFR_Reference_ReadOnly => !CFR_OA_Owner.IsEmpty;

		protected virtual ZString DataGroupingCode => ParentTableCodeIsJIOrCEI ? (Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty) : ZString.Empty;

		public BaseJobDeclaration Declaration => DeclarationCore;

		protected virtual BaseJobDeclaration DeclarationCore
		{
			get
			{
				if (ParentTableCodeIsJIOrCEI)
				{
					if (declaration == null || declaration.IsDeleted)
					{
						if (Parent != null)
						{
							if (CFR_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix)
							{
								declaration = ((BaseJobComInvoiceLine)Parent).Declaration;
							}
							else
							{
								declaration = ((CusEntryInstruction)Parent).JobDeclaration;
							}
						}
					}
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;

		ZBool ParentTableCodeIsJIOrCEI => CFR_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix || CFR_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;
	}
}
