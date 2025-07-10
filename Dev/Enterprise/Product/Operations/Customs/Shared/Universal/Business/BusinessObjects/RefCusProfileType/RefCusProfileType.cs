using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileType : AutoRefCusProfileType
	{
		public RefCusProfileType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(TariffType))]
		public override ZGuid XXX_ZZI_TariffType { get => base.XXX_ZZI_TariffType; set => base.XXX_ZZI_TariffType = value; }

		public RefCusTariffType TariffType => Factory.Load<RefCusTariffType>(XXX_ZZI_TariffType);

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCusProfileType Load(ZString profileType, ZGuid tariffTypePK, ZString dataGrouping)
			{
				return Load(new[] { profileType }, tariffTypePK, dataGrouping).FirstOrDefault();
			}

			public RefCusProfileType[] Load(ZString[] profileTypes, ZGuid tariffTypePK, ZString dataGrouping)
			{
				Argument.NotNull(profileTypes, nameof(profileTypes));
				return Factory.Load<RefCusProfileType>(GetFilter(profileTypes, tariffTypePK, dataGrouping));
			}

			public RefCusProfileType[] Load(ZString tariffType, ZString dataGrouping)
			{
				return Factory.Load<RefCusProfileType>(GetFilter(tariffType, dataGrouping));
			}

			static ZQuery GetFilter(ZString[] profileTypes, ZGuid tariffTypePK, ZString dataGrouping)
			{
				if (profileTypes == null || profileTypes.Length == 0 || !tariffTypePK.IsValid || dataGrouping.IsEmpty)
				{
					return ZQuery.NoResultQuery;
				}

				return new ZQuery(RefCusProfileTypeSchema.XXX_ProfileType, profileTypes)
						.AddToFilter(RefCusProfileTypeSchema.XXX_ZZI_TariffType, tariffTypePK)
						.AddToFilter(RefCusProfileTypeSchema.XXX_ZZZ_NKDataGrouping, dataGrouping);
			}

			static ZQuery GetFilter(ZString tariffType, ZString dataGrouping)
			{
				if (tariffType.IsEmpty || dataGrouping.IsEmpty)
				{
					return ZQuery.NoResultQuery;
				}

				var dbQuery = new ZDBOnlyQuery(typeof(RefCusProfileType));
				var subQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), RefCusTariffTypeSchema.PK);
				subQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, tariffType);
				subQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, dataGrouping);
				dbQuery.AddSubQuery(RefCusProfileTypeSchema.XXX_ZZI_TariffType, subQuery, JoinCondition.And);
				dbQuery.AddToFilter(RefCusProfileTypeSchema.XXX_ZZZ_NKDataGrouping, dataGrouping);

				return dbQuery;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusProfileType);
		}
	}
}
