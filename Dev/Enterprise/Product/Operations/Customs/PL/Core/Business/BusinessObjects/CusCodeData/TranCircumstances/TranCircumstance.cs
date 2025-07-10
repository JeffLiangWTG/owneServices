using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TranCircumstance : CusCodeData
{
	public TranCircumstance(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : CusCodeData.Schema
	{
		public new const int CY_CodeMaxLength = 5;
	}

	[List(nameof(Lookups) + "." + nameof(TranCircumstanceLookups.TranCircumstancesList))]
	[MaxLength(Schema.CY_CodeMaxLength)]
	[ResourceStringData("3329CCCE-6E35-4F1C-9D37-406D98059AB0|CY_Code", Caption = "Additional Transaction Circumstances", MediumCaption = "Add. Tran. Circumstances", ShortCaption = "Add. T.C.")]
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
		}
	}

	public override ZShort CY_Order
	{
		get => base.CY_Order;
		set
		{
			if (Parent != null)
			{
				Parent.MarkAsNeedingValidation();
			}
			base.CY_Order = value;
		}
	}

	public static TranCircumstance Load<T>(T parent, ZShort order)
		where T : BusinessObject, ITranCircumstanceSupporter
	{
		TranCircumstance result = null;

		if (parent != null)
		{
			var query = GetZQuery(parent.PK);
			query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			result = parent.Factory.Load<TranCircumstance>(query).FirstOrDefault(x => x.CY_Order == order);
		}

		return result;
	}

	public static ZQuery GetZQuery(ZGuid pk)
	{
		var query = new ZQuery(CusCodeDataSchema.CY_ParentID, pk);

		query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.DV1);

		return query;
	}

	public static TranCircumstance LoadOrCreate<T>(T parent, short order)
		where T : BusinessObject, ITranCircumstanceSupporter
	{
		TranCircumstance result = null;

		if (parent != null)
		{
			result = Load(parent, order);
			if (result == null)
			{
				result = parent.Factory.New<TranCircumstance>();
				result.CY_ParentID = parent.PK;
				result.CY_ParentTableCode = parent.TablePrefix;
				result.CY_Order = order;
			}
		}

		return result;
	}

	public ZString TranCircumstanceFieldType
	{
		get
		{
			var invoiceHeader = Parent as JobComInvoiceHeader;
			return invoiceHeader != null ? invoiceHeader.TranCircumstanceFieldType.ToString() : nameof(FieldType.Text);
		}
	}

	public TranCircumstancePropertyChangedNotifier PropertyChangedNotifier => propertyChangedNotifier ?? (propertyChangedNotifier = GetNewPropertyChangedNotifier());
	TranCircumstancePropertyChangedNotifier propertyChangedNotifier;

	TranCircumstancePropertyChangedNotifier GetNewPropertyChangedNotifier() => new TranCircumstancePropertyChangedNotifier(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = CusCodeDataTypeList.Codes.DV1;
	}

	protected override CusCodeDataLookups GetNewLookups() => new TranCircumstanceLookups(this);
	public new TranCircumstanceLookups Lookups => (TranCircumstanceLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new TranCircumstanceValidation(this);
	public new TranCircumstanceValidation Validation => (TranCircumstanceValidation)base.Validation;

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceHeader));
}
