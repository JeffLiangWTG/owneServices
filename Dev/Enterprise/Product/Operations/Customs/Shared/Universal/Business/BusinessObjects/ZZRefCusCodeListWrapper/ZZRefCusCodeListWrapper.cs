using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(ZZRefCusCodeListWrapper.Schema.Code), DescriptionProperty(ZZRefCusCodeListWrapper.Schema.Description)]
	public class ZZRefCusCodeListWrapper : NonPersistentBusinessObject, IWrapPersistentBizO
	{
		public ZZRefCusCodeListWrapper(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList.Factory, ((INeedRow)cusCodeList).Row)
		{
			CusCodeList = Argument.NotNull(cusCodeList, nameof(cusCodeList));
		}
		public readonly ZZRefCusCodeListCombined CusCodeList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "This class is designed to be inherited by other classes, allowing shared functionality without enforcing static behavior.")]
		public class Schema
		{
			public const string TableName = ZZRefCusCodeListCombinedSchema.Constants.TableName;
			public const string PK = ZZRefCusCodeListCombinedSchema.Constants.PK;
			public const string Code = nameof(ZZRefCusCodeListWrapper.Code);
			public const string Description = nameof(ZZRefCusCodeListWrapper.Description);
		}

		BusinessObject IWrapPersistentBizO.Parent => CusCodeList;

		public ZString Code => CusCodeList.ZZD_Code;
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(ZZRefCusCodeListWrapper.Schema.Code);

		public ZString Description => CusCodeList.ZZD_Description;
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(ZZRefCusCodeListWrapper.Schema.Description);

		public override SchemaGuidColumn PKSchemaColumn => ZZRefCusCodeListCombinedSchema.PK;

		#region Strategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new Strategy(this);
		}

		class Strategy : BusinessObjectFetchStrategy
		{
			public Strategy(ZZRefCusCodeListWrapper wrapper)
				: base(wrapper.CusCodeList)
			{
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				Factory.AddFetchHint(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, BusinessObject.PK);
			}
		}

		#endregion
	}
}
