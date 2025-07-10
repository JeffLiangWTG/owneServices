using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfile : AutoRefCusProfile
	{
		public RefCusProfile(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(ProfileType))]
		public override ZGuid XX0_XXX_ProfileType { get => base.XX0_XXX_ProfileType; set => base.XX0_XXX_ProfileType = value; }

		public RefCusProfileType ProfileType => Factory.Load<RefCusProfileType>(XX0_XXX_ProfileType);

		RefCusProfileAttribute FindAttributeByName(string attributeName)
		{
			return Attributes.OfType<RefCusProfileAttribute>().FirstOrDefault(x => x.XXY_Name.EqualsIgnoringCase(attributeName));
		}

		public ZString GetAttribute(ZString attributeName)
		{
			return FindAttributeByName(attributeName)?.XXY_Value ?? ZString.Empty;
		}

		[ChildEditable(true)]
		public RefCusProfileAttributeCollection Attributes
		{
			get
			{
				if (fAttributes == null)
				{
					fAttributes = new RefCusProfileAttributeCollection(this);
					RegisterEditableChildObject(fAttributes);
				}

				return fAttributes;
			}
		}
		RefCusProfileAttributeCollection fAttributes;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(RefCusProfile profile)
				: base(profile)
			{
			}

			protected new RefCusProfile BusinessObject
			{
				get { return (RefCusProfile)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(RefCusProfileAttributeSchema.XXY_XX0_Profile, BusinessObject.PK);
			}
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCusProfile[] Load(ZString profileType, ZGuid tariffTypePK, ZString tariffCode, ZString dataGrouping, ZDateTime effectiveDate, IEnumerable<(string Name, string[] Value)> attributes = null)
			{
				return Load(new[] { profileType }, tariffTypePK, tariffCode, dataGrouping, effectiveDate, attributes);
			}

			public RefCusProfile[] Load(ZString[] profileTypes, ZGuid tariffTypePK, ZString tariffCode, ZString dataGrouping, ZDateTime date, IEnumerable<(string Name, string[] Value)> attributes = null)
			{
				var effectiveDate = date.IsValid ? date.Date : ZDateTime.Today;

				var profileTypePKs = new RefCusProfileType.Loader(Factory).Load(profileTypes, tariffTypePK, dataGrouping).Select(x => x.PK).ToArray();
				return Factory.Load<RefCusProfile>(GetFilter(profileTypePKs, tariffCode, dataGrouping, effectiveDate, attributes));
			}

			public static ZQuery GetFilter(ZGuid profileType, ZString tariffCode, ZString dataGrouping, ZDateTime effectiveDate, IEnumerable<(string Name, string[] Values)> attributes = null)
			{
				return GetFilter(new[] { profileType }, tariffCode, dataGrouping, effectiveDate, attributes);
			}

			public static ZQuery GetFilter(ZGuid[] profileTypePK, ZString tariffCode, ZString dataGrouping, ZDateTime effectiveDate, IEnumerable<(string Name, string[] Values)> attributes = null)
			{
				if (profileTypePK == null || profileTypePK.Length == 0 || tariffCode.IsEmpty || dataGrouping.IsEmpty || !effectiveDate.IsValid)
				{
					return ZQuery.NoResultQuery;
				}

				var profileQuery = new ZDBOnlyQuery(typeof(RefCusProfile));
				profileQuery.AddToFilter(RefCusProfileSchema.XX0_XXX_ProfileType, profileTypePK);
				profileQuery.AddToFilter(RefCusProfileSchema.XX0_ZZZ_NKDataGrouping, dataGrouping);
				profileQuery.AddToFilter(RefCusProfileSchema.XX0_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				profileQuery.AddToFilter(RefCusProfileSchema.XX0_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				profileQuery.AddFilterAndZSQLParameterCollection(ZString.Format("@TariffCode LIKE {1} + '%'", tariffCode, RefCusProfileSchema.XX0_TariffCode.Name),
					new ZSqlParameterCollection(ZSqlParameter.New("@TariffCode", tariffCode, RefCusProfileSchema.XX0_TariffCode)));

				if (attributes?.Any() ?? false)
				{
					foreach (var (name, value) in attributes)
					{
						var attributeQuery = new ZDBOnlySubQuery(typeof(RefCusProfileAttribute), RefCusProfileAttributeSchema.XXY_XX0_Profile, RefCusProfileSchema.PK);
						attributeQuery.AddToFilter(RefCusProfileAttributeSchema.XXY_Name, name);
						attributeQuery.AddToFilter(RefCusProfileAttributeSchema.XXY_Value, value);
						profileQuery.AddSubQuery(attributeQuery, JoinCondition.And);
					}
				}

				return profileQuery;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusProfile);
		}
	}
}
