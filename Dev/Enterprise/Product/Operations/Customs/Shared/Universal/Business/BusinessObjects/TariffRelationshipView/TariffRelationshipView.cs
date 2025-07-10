using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class TariffRelationshipView : AutoTariffRelationshipView, ITariffEffectiveDatesRelatedBusinessObject
	{
		public TariffRelationshipView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTariffRelationshipView.Schema
		{
			public const string RelatedTariffFromTypeCode = "RelatedTariffFromTypeCode";
			public const string RelatedTariffFromCode = "RelatedTariffFromCode";
			public const string ZZH_ZZI_TariffTypeCode = "ZZH_ZZI_TariffTypeCode";
			public const string ZZH_ZZI_TariffTypeDesc = "ZZH_ZZI_TariffTypeDesc";
		}

		[RelatedBusinessObject("RelatedTariffFrom")]
		public override ZGuid ZZH_ZZ1_LinkedTariffOrNationalCode
		{
			get { return base.ZZH_ZZ1_LinkedTariffOrNationalCode; }
			set
			{
				var oldValue = ZZH_ZZ1_LinkedTariffOrNationalCode;
				base.ZZH_ZZ1_LinkedTariffOrNationalCode = value;
				if (oldValue != ZZH_ZZ1_LinkedTariffOrNationalCode)
				{
					DefaultDerivedProperties();
				}
			}
		}

		void DefaultDerivedProperties()
		{
			var relatedTariffFrom = RelatedTariffFrom;
			if (relatedTariffFrom != null)
			{
				ZZH_RelatedTariffCode = relatedTariffFrom.ZZ1_TariffCode.Left(Schema.ZZH_RelatedTariffCodeMaxLength);
				ZZH_ZZI_RelatedTariffType = relatedTariffFrom.ZZ1_ZZI_TariffType;
				ZZH_ZZZ_RelatedTariffDataGrouping = relatedTariffFrom.ZZ1_ZZZ_NKDataGrouping;
			}
			else
			{
				ZZH_RelatedTariffCode = ZString.Empty;
				ZZH_ZZI_RelatedTariffType = ZGuid.Empty;
				ZZH_ZZZ_RelatedTariffDataGrouping = ZString.Empty;
			}
		}

		[RelatedBusinessObject("CusTariffType")]
		public override ZGuid ZZH_ZZI_TariffType
		{
			get { return base.ZZH_ZZI_TariffType; }
			set { base.ZZH_ZZI_TariffType = value; }
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffRelationshipView|ZZH_TariffCode", Caption = "Tariff")]
		public override ZString ZZH_TariffCode { get => base.ZZH_TariffCode; set => base.ZZH_TariffCode = value; }

		#region Related BusinessObjects

		public TariffView RelatedTariffFrom => Factory.Load<TariffView>(ZZH_ZZ1_LinkedTariffOrNationalCode);

		public RefCusTariffType CusTariffType
		{
			get
			{
				if (cusTariffType == null || (cusTariffType.PK != ZZH_ZZI_TariffType))
				{
					cusTariffType = Factory.Load<RefCusTariffType>(ZZH_ZZI_TariffType);
				}
				return cusTariffType;
			}
		}
		RefCusTariffType cusTariffType;

		public RefCusTariffType RelatedTariffType
		{
			get
			{
				if (relatedTariffType == null || (relatedTariffType.PK != ZZH_ZZI_RelatedTariffType))
				{
					relatedTariffType = Factory.Load<RefCusTariffType>(ZZH_ZZI_RelatedTariffType);
				}
				return relatedTariffType;
			}
		}
		RefCusTariffType relatedTariffType;

		#endregion

		#region New Properties

		#region RelatedTariffFromTypeCode

		public ZString RelatedTariffFromTypeCode => RelatedTariffType?.ZZI_TariffType ?? ZString.Empty;
		public ZPropertyInfo RelatedTariffFromTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedTariffFromTypeCode); }
		}

		#endregion

		#region RelatedTariffFromCode

		public ZString RelatedTariffFromCode => ZZH_RelatedTariffCode;
		public ZPropertyInfo RelatedTariffFromCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedTariffFromCode); }
		}

		#endregion

		public ZString CountryCode => ZZH_ZZZ_RelatedTariffDataGrouping;

		[ResourceStringData("Enterprise.Customs.Universal.TariffRelationshipView|ZZH_ZZI_TariffTypeCode", Caption = "Type")]
		public ZString ZZH_ZZI_TariffTypeCode => CusTariffType?.ZZI_TariffType ?? ZString.Empty;

		public ZPropertyInfo ZZH_ZZI_TariffTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ZZH_ZZI_TariffTypeCode); }
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffRelationshipView|ZZH_ZZI_TariffTypeDesc", Caption = "Type Description")]
		public ZString ZZH_ZZI_TariffTypeDesc => CusTariffType?.ZZI_Description ?? ZString.Empty;

		public ZPropertyInfo ZZH_ZZI_TariffTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.ZZH_ZZI_TariffTypeDesc); }
		}

		#region ITariffEffectiveDatesRelatedBusinessObject
		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.StartDate => RelatedTariffFrom.ZZ1_StartDate;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.EndDate => RelatedTariffFrom.ZZ1_EndDate;

		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => RelatedTariffFrom.ZZ1_ZZZ_NKDataGrouping;
		#endregion
		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(TariffRelationshipView tariffRelationshipView)
				: base(tariffRelationshipView)
			{
			}

			protected new TariffRelationshipView BusinessObject
			{
				get { return (TariffRelationshipView)base.BusinessObject; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(TariffViewSchema.PK, BusinessObject.ZZH_ZZ1_LinkedTariffOrNationalCode); // Tariff is accessed just after load
				Factory.AddFetchHint(RefCusTariffTypeSchema.PK, BusinessObject.ZZH_ZZI_TariffType); // Tariff type is accessed just after load
			}
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}
#endif
	}
}
