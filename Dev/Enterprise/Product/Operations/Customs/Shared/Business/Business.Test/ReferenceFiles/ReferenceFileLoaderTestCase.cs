using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(ReferenceFileLoader))]
	public abstract class ReferenceFileLoaderTestCase<T> : LoaderTestCase where T : BusinessObject
	{
		protected virtual ZString Code
		{
			get { return "~~~"; }
		}

		protected virtual ZString InvalidCode
		{
			get { return ZString.Empty; }
		}

		public void TestLoadBestMatch()
		{
			T bO1 = Factory.New<T>();
			bO1[Loader.CodeSchema.Name] = Code;
			bO1[Loader.DateFromSchema.Name] = new ZDateTime(2003, 1, 1);
			bO1[Loader.DateToSchema.Name] = new ZDateTime(ZDateTime.MaxSmallDateTime);

			T bO2 = Factory.New<T>();
			bO2[Loader.CodeSchema.Name] = Code;
			bO2[Loader.DateFromSchema.Name] = new ZDateTime(2004, 1, 1);
			bO2[Loader.DateToSchema.Name] = new ZDateTime(ZDateTime.MaxSmallDateTime);

			T bO3 = Factory.New<T>();
			bO3[Loader.CodeSchema.Name] = Code;
			bO3[Loader.DateFromSchema.Name] = new ZDateTime(2004, 1, 1);
			bO3[Loader.DateToSchema.Name] = ZDateTime.Today.AddDays(1);

			AssertEquals("Correct record for date", bO3, Loader.LoadBestMatch(Code, new ZDateTime(2004, 6, 12)));
			AssertEquals("Correct record for date", null, Loader.LoadBestMatch(Code, new ZDateTime(0103, 11, 22)));
			AssertEquals("Correct record for date", bO2, Loader.LoadBestMatch(Code, ZDateTime.Today.AddDays(2)));
			AssertEquals("Correct record for date", bO1, Loader.LoadBestMatch(Code, new ZDateTime(2003, 6, 12)));
			AssertNull("Record should not be loaded once it has expired", Loader.LoadBestMatch(Code, new ZDateTime(2002, 6, 12)));
		}

		public void TestLoadExact()
		{
			ZDateTime earlyStart = new ZDateTime(2005, 1, 20);
			ZDateTime earlyEnd = earlyStart.AddDays(1);
			T early = Factory.New<T>();
			early[Loader.CodeSchema.Name] = Code;
			early[Loader.DateFromSchema.Name] = earlyStart;
			early[Loader.DateToSchema.Name] = earlyEnd;

			ZDateTime lateStart = new ZDateTime(2005, 2, 20);
			ZDateTime lateEnd = lateStart.AddDays(1);
			T late = Factory.New<T>();
			late[Loader.CodeSchema.Name] = Code;
			late[Loader.DateFromSchema.Name] = lateStart;
			late[Loader.DateToSchema.Name] = lateEnd;

			ZDateTime outOfRangeStart = new ZDateTime(2005, 1, 19);
			ZDateTime outOfRangeEnd = new ZDateTime(2005, 1, 21);

			AssertEquals("Found correct record", late, Loader.LoadExact(Code, lateStart, lateEnd));
			AssertEquals("Found correct record", early, Loader.LoadExact(Code, earlyStart, earlyEnd));
			AssertNull("Didn't find invalid record", Loader.LoadExact(Code, outOfRangeStart, outOfRangeEnd));
		}

		public void TestGetLoadBestMatchFilter()
		{
			AssertNotNull(Loader.GetLoadBestMatchFilter(InvalidCode, new ZDateTime(2004, 6, 12)));
		}

		protected ReferenceFileLoader Loader
		{
			get
			{
				if (fLoader == null)
				{
					fLoader = (ReferenceFileLoader)GetNewLoaderToTest();
				}

				return fLoader;
			}
		}
		ReferenceFileLoader fLoader;
	}
}
