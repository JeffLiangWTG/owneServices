using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Test
{
	public class RatingFindBoxListProviderForOrganisationCodeTest : TestCaseWithFactory
	{
		public void TestBizObjFromCodeWithFilter()
		{
			var findBoxListProviders = new List<IFindBoxListProvider>();
			var clientCodes = new List<ZString>();
			var asciiCode = 65; // "A"

			var assembly = typeof(RateCollection).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes)
			{
				if (typeof(RateCollection).IsAssignableFrom(type) || typeof(CostingCollection).IsAssignableFrom(type))
				{
					var addNewMethod = type.GetMethod("AddNew", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					var orgHeaderQuery = new ZQuery();
					orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, ((char)asciiCode++).ToString());
					var header = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
					clientCodes.Add(header.OH_Code);
					findBoxListProviders.Add(GetProvider(type, addNewMethod.ReturnType, header));
				}
			}

			for (var i = 0; i < findBoxListProviders.Count; i++)
			{
				string clientCode = clientCodes[i];
				string nonMatchingCode = (i < clientCodes.Count - 1) ? clientCodes[i + 1] : clientCodes[0];

				AssertNotNull("should return a rating header with code of " + clientCode, findBoxListProviders[i].GetBusinessObjectFromCode(clientCode));
				AssertNull("should return null, shouldn't find a matching business object because it is a different rate type", findBoxListProviders[i].GetBusinessObjectFromCode(nonMatchingCode));
				AssertNull("should return null, shouldn't find a matching business object because it doesn't exist", findBoxListProviders[i].GetBusinessObjectFromCode("ZZZZZZ"));
			}
		}

		IFindBoxListProvider GetProvider(Type typeOfCollection, Type typeOfHeader, BusinessObject orgHeader)
		{
			var header = (RatingHeader)Factory.New(typeOfHeader);
			header.TH_OH = orgHeader.PK;
			var collection = (RatingHeaderCollection)Activator.CreateInstance(typeOfCollection, new object[] { Factory });
			return collection;
		}
	}

	public class RatingHeaderFindBoxListProviderForRateLevelTest : TestCaseWithFactory
	{
		public void TestBizObjFromCodeWithFilter()
		{
			var findBoxListProviders = new List<IFindBoxListProvider>();

			var assembly = typeof(CompanyTariffCollection).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes)
			{
				if (type == typeof(CompanyTariffCollection) || type.IsSubclassOf(typeof(CompanyTariffCollection)))
				{
					var addNewMethod = type.GetMethod("AddNew", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					findBoxListProviders.Add(GetProvider(type, addNewMethod.ReturnType, (byte)(findBoxListProviders.Count + 1)));
				}
			}

			for (var i = 0; i < findBoxListProviders.Count; i++)
			{
				var expectedRateLevel = i + 1;
				AssertNotNull("should return a tariff with code of " + expectedRateLevel, findBoxListProviders[i].GetBusinessObjectFromCode(expectedRateLevel.ToString()));
				AssertNull("should return null, shouldn't find a matching business object because it is a different rate type", findBoxListProviders[i].GetBusinessObjectFromCode((expectedRateLevel + 1).ToString()));
				AssertNull("should return null, shouldn't find a matching business object because it doesn't exist", findBoxListProviders[i].GetBusinessObjectFromCode("254"));
			}
		}

		IFindBoxListProvider GetProvider(Type typeOfCollection, Type typeOfHeader, ZByte companyTariffLevel)
		{
			var tariff = (CompanyTariff)Factory.New(typeOfHeader);
			tariff.TH_GlobalRateLevel = companyTariffLevel;
			var collection = (CompanyTariffCollection)Activator.CreateInstance(typeOfCollection, new object[] { Factory });

			return collection;
		}
	}
}
