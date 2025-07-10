using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	sealed class TariffProvTariffModuleFilter : ModuleTextFilter
	{
		public TariffProvTariffModuleFilter(ZString description, Func<ZString, ZString, ZQuery> getQuery)
			: base(description, DummyQuery)
		{
			this.getQuery = getQuery;
		}
		readonly Func<ZString, ZString, ZQuery> getQuery;

		public void SetItemDescriptions(ResourceStringData description1, ResourceStringData description2)
		{
			if (description1.IsEmpty() || description2.IsEmpty())
			{
				throw new ArgumentException(Description + " filter Item Descriptions cannot be empty.");
			}

			ItemDescription1 = description1;
			ItemDescription2 = description2;
		}

		public ResourceStringData ItemDescription1
		{
			get;
			private set;
		}

		public ResourceStringData ItemDescription2
		{
			get;
			private set;
		}

		public override ZString Property
		{
			get => DisplayTariff.DisplayFormat(base.Property);
			set
			{
				base.Property = DisplayTariff.Format(value);
				PropertyInfo.RefreshBinding();
			}
		}

		ZString provTariff;
		public ZString ProvTariff
		{
			get => DisplayTariff.DisplayFormat(provTariff);
			set
			{
				provTariff = DisplayTariff.Format(value);
				ProvTariffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ProvTariffInfo => GetZPropertyInfo(nameof(ProvTariff));

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("ProvTariff", ProvTariff);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "ProvTariff")
			{
				ProvTariff = reader.ReadElementString("ProvTariff");
			}
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			ProvTariff = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && ProvTariff.IsEmpty;

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();

			if (!IsEmpty)
			{
				query = getQuery(Property, ProvTariff);
			}

			return query;
		}

		TariffFormatter displayTariff;
		TariffFormatter DisplayTariff => displayTariff ?? (displayTariff = new TariffFormatter());

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery();
	}
}
