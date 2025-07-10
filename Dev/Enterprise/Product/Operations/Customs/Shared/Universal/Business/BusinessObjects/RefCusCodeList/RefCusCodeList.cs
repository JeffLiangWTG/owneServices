using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeList : AutoRefCusCodeList
	{
		public RefCusCodeList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Collection

		[ChildEditable]
		public RefCusCodeListAttributeCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new RefCusCodeListAttributeCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}
		RefCusCodeListAttributeCollection attributes;

		#endregion

		#region Override Method

		public override void Delete()
		{
			Attributes.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Override Properties

		[RelatedBusinessObject("CusCodeType")]
		public override ZString ZZD_ZZK_NKCodeType
		{
			get { return base.ZZD_ZZK_NKCodeType; }
			set { base.ZZD_ZZK_NKCodeType = value; }
		}

		public RefCusCodeType CusCodeType
		{
			get
			{
				var query = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, ZZD_ZZK_NKCodeType);
				query.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, new ZString[] { ZZD_ZZZ_NKDataGrouping, ZString.Empty });
				return Factory.Load<RefCusCodeType>(query).OrderByDescending(x => x.ZZK_ZZZ_NKDataGrouping).FirstOrDefault();
			}
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZD_ZZZ_NKDataGrouping
		{
			get { return base.ZZD_ZZZ_NKDataGrouping; }
			set { base.ZZD_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZD_ZZZ_NKDataGrouping); }
		}

		#endregion

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZD_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			if (ZZD_StartDate >= ZZD_EndDate)
			{
				var swapHolder = ZZD_StartDate;
				ZZD_StartDate = ZZD_EndDate;
				ZZD_EndDate = swapHolder;
			}
		}
#endif
	}
}
