using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using NUnit.Framework;

	public class NZCClassFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatch()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			AssertEquals("2203.00.02.01L", ((IFindBoxListProvider)listProvider).NearestMatch("2203", explicitAutoComplete: true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			AssertEquals("Beer made from malt not more than 1.15% vol in cans", ((IFindBoxListProvider)listProvider).DescriptionFromCode("2203.00.02.01L"));
		}

		public void TestNearestMatchReturnsMostRecentMatchingCode()
		{
			NZCClassification class1 = Factory.New<NZCClassification>();
			class1.U0_Tariff = "2203.00";
			class1.U0_DateActiveFrom = new ZDateTime(2000, 1, 1);
			class1.U0_DateActiveTo = ZDateTime.Empty;

			NZCClassification class2 = Factory.New<NZCClassification>();
			class2.U0_Tariff = "2203.00.01.01K";
			class2.U0_DateActiveFrom = new ZDateTime(2003, 1, 1);
			class2.U0_DateActiveTo = ZDateTime.Empty;

			NZCClassification class3 = Factory.New<NZCClassification>();
			class3.U0_Tariff = "2203.00.02.01L";
			class3.U0_DateActiveFrom = new ZDateTime(2005, 1, 1);
			class3.U0_DateActiveTo = ZDateTime.Empty;

			DummyNZCClassFindBoxListProvider listProvider = new DummyNZCClassFindBoxListProvider();
			listProvider.SetFactoryForTesting(Factory);
			AssertEquals("2203.00.01.01K", ((IFindBoxListProvider)listProvider).NearestMatch("2203", explicitAutoComplete: true, -1).Item1);
		}

		public void TestListThrowsApplicationException()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			bool gotThrown = false;
			try
			{
				IBusinessObjectCollection collection = ((IFindBoxListProvider)listProvider).List;
			}
			catch (ApplicationException e)
			{
				gotThrown = true;
				AssertEquals("IFindBoxListProvider.List should not be referred to", e.Message);
			}
			Assert("Expected List to throw ApplicationException", gotThrown);
		}

		public void TestGetBusinessObjectFromCodeThrowsNotSupportedException()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			bool gotThrown = false;
			try
			{
				BusinessObject bizO = ((IFindBoxListProvider)listProvider).GetBusinessObjectFromCode("0123456");
			}
			catch (NotSupportedException)
			{
				gotThrown = true;
			}
			Assert("Expected GetBusinessObjectFromCode to throw NotSupportedException", gotThrown);
		}

		public void TestPrimaryKeyFromCodeThrowsNotSupportedException()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			bool gotThrown = false;
			try
			{
				ZGuid pK = ((IFindBoxListProvider)listProvider).PrimaryKeyFromCode("0123456");
			}
			catch (NotSupportedException)
			{
				gotThrown = true;
			}
			Assert("Expected PrimaryKeyFromCode to throw NotSupportedException", gotThrown);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestDescriptionFromPrimaryKey()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			((IFindBoxListProvider)listProvider).DescriptionFromPrimaryKey(ZGuid.Empty);
		}

		public void TestCodeFromPrimaryKeyThrowsNotSupportedException()
		{
			NZCClassFindBoxListProvider listProvider = new NZCClassFindBoxListProvider();
			bool gotThrown = false;
			try
			{
				String code = ((IFindBoxListProvider)listProvider).CodeFromPrimaryKey(ZGuid.NewZGuid());
			}
			catch (NotSupportedException)
			{
				gotThrown = true;
			}
			Assert("Expected CodeFromPrimaryKey to throw NotSupportedException", gotThrown);
		}
	}

	#region DummyNZCClassFindBoxListProvider

	class DummyNZCClassFindBoxListProvider : NZCClassFindBoxListProvider
	{
		public void SetFactoryForTesting(BusinessObjectFactory factory)
		{
			base.fFactory = factory;
		}
	}

	#endregion DummyNZCClassFindBoxListProvider
}
