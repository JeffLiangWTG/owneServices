using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.DataTransfer
{
	public abstract class RatingHeaderDataContextManager<T>
		: EventDataContextManager<T> where T : RatingHeader
	{
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var dataContextKey = matchingValues.Key;
			var contextKeyParts = dataContextKey.SplitIgnoringEscapedDelimiter('~', '!');

			// Old xml files can have only 4 parts in the context key. Let's cover it for a while.
			if (contextKeyParts.Length is not (4 or 5))
			{
				return null;
			}

			var query = new ZDBOnlyQuery(typeof(RatingHeader));
			var companyPk = new ZGuid(contextKeyParts[1]);
			query.AddToFilter(RatingHeaderSchema.TH_GC, companyPk.IsEmpty ? null : companyPk)
				.AddToFilter(RatingHeaderSchema.TH_RateType, contextKeyParts[2]);

			// backward compatibility for 4-part context key, even though it was not correct.
			var companyTariffLevel = contextKeyParts.Length == 4 ? (byte)0 : byte.Parse(contextKeyParts[4]);
			query.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, companyTariffLevel);

			return AddAdditionalMatchingQuery(query, contextKeyParts);
		}

		protected virtual ZQuery AddAdditionalMatchingQuery(ZDBOnlyQuery query, ZString[] contextKey)
		{
			query.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, ZString.Empty);

			// Company tariff does not have an OrgHeader, handle it with a null value.
			var orgHeaderPk = new ZGuid(contextKey[0]);
			query.AddToFilter(RatingHeaderSchema.TH_OH, orgHeaderPk.IsEmpty ? null : orgHeaderPk);

			return query;
		}

		public override DataContextType DataContextType
		{
			get { return GetDataContextTypeCore(); }
		}

		protected abstract DataContextType GetDataContextTypeCore();

		public override ZString DataContextKey
		{
			get
			{
				var list = new List<string>
				{
					ParentBO.TH_OH.IsEmpty ? string.Empty : ParentBO.TH_OH.ToString(),
					ParentBO.TH_GC.IsEmpty ? string.Empty : ParentBO.TH_GC.ToString(),
					ParentBO.TH_RateType
				};

				list.Add(ParentBO.TH_QuoteNumber);
				list.Add(ParentBO.TH_GlobalRateLevel.ToString());

				return string.Join("~", list);
			}
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new RatingHeaderEventParentFinder(factory, this, logger);
		}

		class RatingHeaderEventParentFinder : EventParentFinder
		{
			internal RatingHeaderEventParentFinder(BusinessObjectFactory factory, RatingHeaderDataContextManager<T> manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
			{
				return null;
			}
		}
	}
}
