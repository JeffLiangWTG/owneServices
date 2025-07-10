using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for BaseRefPacks.
	/// </summary>
	[CodeProperty(BaseRefPacks.Schema.RP_Code)]
	public class BaseRefPacks : AutoRefPacks, IDocManagerSupport
	{
		#region Schema
		public new class Schema : AutoRefPacks.Schema
		{
			public const string RP_Code = "RP_Code";
		}
		#endregion

		public BaseRefPacks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RP_CustomsCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Properties
		[ReadOnlyMember(nameof(RP_OH_Supplier_ReadOnly))]
		[List(nameof(RP_OH_Supplier_List))]
		public override ZGuid RP_OH_Supplier
		{
			get => base.RP_OH_Supplier;
			set => base.RP_OH_Supplier = value;
		}

		protected bool RP_OH_Supplier_ReadOnly => RP_Type == RPTypeList.Codes.PackingDeclaration;

		[List(nameof(RP_Type_List))]
		public override ZString RP_Type
		{
			get { return base.RP_Type; }
			set
			{
				var oldValue = base.RP_Type;
				base.RP_Type = value;
				if (!IsCopying && oldValue != RP_Type)
				{
					if (RP_Type == RPTypeList.Codes.AMSManifest)
					{
						RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
					}
					else if (RP_Type == RPTypeList.Codes.AFRManifest)
					{
						RP_CustomsCountry = Core.Constants.CountryCodes.Japan;
					}

					if (!RP_CustomsPack.IsEmpty)
					{
						Validation.ValidateRP_CustomsPack();
					}
				}
			}
		}

		[List(nameof(RP_CustomsPack_List))]
		[MaxLength(6)]
		public override ZString RP_CustomsPack
		{
			get { return base.RP_CustomsPack; }
			set { base.RP_CustomsPack = value; }
		}

		[List(nameof(RP_CommercialPack_List))]
		public override ZString RP_CommercialPack
		{
			get { return base.RP_CommercialPack; }
			set { base.RP_CommercialPack = value; }
		}

		public ZString RP_Code
		{
			get
			{
				ZString code = ZString.Empty;
				OrgHeader supplier = (OrgHeader)Factory.Load(typeof(OrgHeader), RP_OH_Supplier);

				if (supplier != null)
				{
					code += supplier.OH_Code + "-";
				}

				code += RP_CustomsPack + "-" + RP_CommercialPack;

				return code;
			}
		}

		public ZPropertyInfo RP_CodeInfo
		{
			get
			{
				return GetZPropertyInfo(BaseRefPacks.Schema.RP_Code);
			}
		}

		#endregion

		public virtual CodeDescriptionPairList RP_Type_List
		{
			get { return Factory.GetCachedValue<RPTypeList>(); }
		}

		public virtual CodeDescriptionPairList RP_CommercialPack_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		public virtual CodeDescriptionPairList RP_CustomsPack_List
		{
			get { return new CodeDescriptionPairList(); }
		}

		public ConsignorCollection RP_OH_Supplier_List
		{
			get { return new ConsignorCollection(Factory); }
		}

		[ReadOnlyMember(nameof(RP_CustomsCountryReadOnly))]
		[List(nameof(RP_CustomsCountry_List))]
		public override ZString RP_CustomsCountry
		{
			get { return base.RP_CustomsCountry; }
			set
			{
				var oldValue = base.RP_CustomsCountry;
				base.RP_CustomsCountry = value;
				if (!RP_CustomsPack.IsEmpty && !IsCopying && oldValue != RP_CustomsCountry)
				{
					Validation.ValidateRP_CustomsPack();
				}
			}
		}

		public bool RP_CustomsCountryReadOnly => (RP_Type == RPTypeList.Codes.AMSManifest && RP_CustomsCountry == Core.Constants.CountryCodes.UnitedStates)
											|| (RP_Type == RPTypeList.Codes.AFRManifest && RP_CustomsCountry == Core.Constants.CountryCodes.Japan);

		public RefCountryCollection RP_CustomsCountry_List
		{
			get { return new RefCountryCollection(Factory); }
		}

		[ReadOnlyMember(nameof(RP_OH_Supplier_ReadOnly))]
		public override ZDecimal RP_ConversionFactor
		{
			get { return base.RP_ConversionFactor; }
			set { base.RP_ConversionFactor = value; }
		}

		[ReadOnly(true)]
		public override ZBool RP_IsSystem
		{
			get { return base.RP_IsSystem; }
			set { base.RP_IsSystem = value; }
		}

		public override bool ReadOnly
		{
			get { return RP_IsSystem; }
			set { base.ReadOnly = value; }
		}

		public override bool CanDelete
		{
			get { return !RP_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return RP_IsSystem ? ResString.GetMultilingualString("4C61D14F-D35D-4080-83AB-79033EDEA36B", "Cannot delete a system-defined pack") : base.ReasonForNotAbleToDelete; }
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.PacksConversion);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
