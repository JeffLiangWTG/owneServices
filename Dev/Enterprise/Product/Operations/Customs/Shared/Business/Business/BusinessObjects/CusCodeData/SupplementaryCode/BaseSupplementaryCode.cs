using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCode : CusCodeDataWithOrder
	{
		public BaseSupplementaryCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region schema

		public new class Schema : CusCodeData.Schema
		{
		}

		#endregion

		public static string CodeDataType => BaseCusCodeDataTypeList.Codes.SupplementaryCode;

		public ISupplementaryCodeSupporter SupplementaryCodeSupporter => Parent as ISupplementaryCodeSupporter;

		public new class Loader : CusCodeDataWithOrder.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public TSupplementaryCode Load<TSupplementaryCode, TParent>(TParent parent, ZShort order)
				where TSupplementaryCode : BaseSupplementaryCode
				where TParent : BusinessObject, ISupplementaryCodeSupporter
			{
				return Load<TSupplementaryCode, TParent>(parent, order, CodeDataType);
			}

			public TSupplementaryCode LoadOrCreate<TSupplementaryCode, TParent>(TParent parent, short order)
				where TSupplementaryCode : BaseSupplementaryCode
				where TParent : BusinessObject, ISupplementaryCodeSupporter
			{
				return LoadOrCreate<TSupplementaryCode, TParent>(parent, order, CodeDataType);
			}

			public BaseSupplementaryCode Load<TParent>(TParent parent, ZShort order) where TParent : BusinessObject, ISupplementaryCodeSupporter
				=> Load<BaseSupplementaryCode, TParent>(parent, order);

			public BaseSupplementaryCode LoadOrCreate<TParent>(TParent parent, ZShort order) where TParent : BusinessObject, ISupplementaryCodeSupporter
				=> LoadOrCreate<BaseSupplementaryCode, TParent>(parent, order);

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(BaseSupplementaryCode);
		}

		public ZString SupplementaryCodesFieldType
		{
			get
			{
				return SupplementaryCodeSupporter != null ? SupplementaryCodeSupporter.SupplementaryCodesFieldType.ToString() : nameof(FieldType.Text);
			}
		}

		public override bool SupportsNotes => false;

		#region Override Properties

		[ResourceStringData("SupplementaryCode|CY_Code", Caption = "Supplementary Code")]
		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (!IsCopying && CY_Code != oldValue)
				{
					PropertyChangedNotifier.NotifyChange(Schema.CY_Code, oldValue, value);
				}
				MarkAsNeedingValidation();
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("4545D809-A03A-416A-854C-315BB29E995B", "Supplementary Code");

		protected internal override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(BaseJobComInvoiceLine)); }
		}

		protected override string CusCodeDataType => CodeDataType;

		public BaseSupplementaryCodePropertyChangedNotifier PropertyChangedNotifier => propertyChangedNotifier ??= GetNewPropertyChangedNotifier();
		BaseSupplementaryCodePropertyChangedNotifier propertyChangedNotifier;

		BaseSupplementaryCodePropertyChangedNotifier GetNewPropertyChangedNotifier() => Provider.GetNewSupplementaryCodePropertyChangedNotifier(this) ?? new BaseSupplementaryCodePropertyChangedNotifier(this);

		protected override CusCodeDataValidation GetNewValidation() => Provider.GetNewValidation(this) ?? new BaseSupplementaryCodeValidation(this);

		public new BaseSupplementaryCodeValidation Validation => (BaseSupplementaryCodeValidation)base.Validation;

		protected override CusCodeDataLookups GetNewLookups() => Provider.GetNewLookups(this) ?? new BaseSupplementaryCodeLookups(this);

		public new BaseSupplementaryCodeLookups Lookups => (BaseSupplementaryCodeLookups)base.Lookups;

		public override void OnSaving()
		{
			if (!IsDeleted && IsDataEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}
		bool IsDataEmpty => CY_Code.IsEmpty && CY_Data.IsEmpty && CY_Date.IsEmpty;

		#endregion

		public BaseSupplementaryCodeProvider Provider
		{
			get
			{
				var parentCountryCode = Supporter?.GetCountryCodeForCodeProvider() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				if (provider == null || provider.CountryCode != parentCountryCode)
				{
					provider = BaseSupplementaryCodeProvider.GetByCountryCode(parentCountryCode);
				}
				return provider;
			}
		}

		BaseSupplementaryCodeProvider provider;
	}
}
