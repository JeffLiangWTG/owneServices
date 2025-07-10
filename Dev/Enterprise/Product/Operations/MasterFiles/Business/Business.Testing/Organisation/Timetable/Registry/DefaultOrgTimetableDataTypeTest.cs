using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetableDataType))]
	sealed class DefaultOrgTimetableDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultOrgTimetableDataType>
	{
		protected override DefaultOrgTimetableDataType GetNewDataType()
		{
			return new DefaultOrgTimetableDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultOrgTimetableRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new DefaultOrgTimetableSettingsCollection();
			var settings1 = collection1.AddNew();
			var item1 = settings1.Timetables.AddNew();
			item1.Type = OrgTimetableType.Codes.Pickup;
			item1.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item1.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item1.Day = "MON";

			var item2 = settings1.Timetables.AddNew();
			item2.Type = OrgTimetableType.Codes.Deliver;
			item2.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item2.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item2.Day = "MON";

			var collection2 = new DefaultOrgTimetableSettingsCollection();
			var settings2 = collection2.AddNew();
			var item3 = settings2.Timetables.AddNew();
			item3.Type = OrgTimetableType.Codes.Pickup;
			item3.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item3.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item3.Day = "TUE";

			var item4 = settings2.Timetables.AddNew();
			item4.Type = OrgTimetableType.Codes.Deliver;
			item4.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item4.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item4.Day = "TUE";

			var settings3 = collection2.AddNew();
			settings3.CountryCode = Core.Constants.CountryCodes.Australia;
			var item5 = settings3.Timetables.AddNew();
			item5.Type = OrgTimetableType.Codes.Pickup;
			item5.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item5.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item5.Day = "WED";

			return new ValidSampleAndBinaryValueInDB[]
				{
						new ValidSampleAndBinaryValueInDB(collection1, new DefaultOrgTimetableDataType().Serialise(collection1)),
						new ValidSampleAndBinaryValueInDB(collection2, new DefaultOrgTimetableDataType().Serialise(collection2))
				};
		}
	}
}
