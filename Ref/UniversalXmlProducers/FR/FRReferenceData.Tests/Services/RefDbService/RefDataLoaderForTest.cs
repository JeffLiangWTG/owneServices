using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	public class RefDataLoaderForTest : IRefDataLoader
	{
		public Dictionary<string, int> CallCount { get; } = new Dictionary<string, int>();

		public RefDataLoaderForTest()
		{
			RefCusCodeListData = CreateRefCusCodeListTestData;
			RefCusProcedureData = CreateRefCusProcedureTestData;
			RefCusTariffData = CreateRefCusTariffTestData;
		}

		public async Task<IEnumerable<T>> LoadData<T>(string urlQuery)
		{
			SetCallCount(typeof(T).Name);
			var list = new List<T>();

			foreach (var item in await Task.Factory.StartNew(() => CreateTestData<T>(urlQuery)))
			{
				list.Add(item);
			}

			return list;
		}

		void SetCallCount(string name)
		{
			if (CallCount.ContainsKey(name))
			{
				CallCount[name]++;
			}
			else
			{
				CallCount.Add(name, 1);
			}
		}

		IEnumerable<T> CreateTestData<T>(string urlQuery)
		{
			var list = new List<T>();

			if (urlQuery.Contains(nameof(RefCusCodeList)))
			{
				foreach (var x in RefCusCodeListData?.Invoke())
				{
					list.Add((T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture));
				}
			}

			if (urlQuery.Contains(nameof(RefCusProcedure)))
			{
				foreach (var x in RefCusProcedureData?.Invoke())
				{
					list.Add((T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture));
				}
			}

			if (urlQuery.Contains(nameof(RefCusTariff)))
			{
				foreach (var x in RefCusTariffData?.Invoke())
				{
					list.Add((T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture));
				}
			}

			return list;
		}

		public delegate IEnumerable<RefCusCodeList> CreateRefCusCodeListDelegate();
		public CreateRefCusCodeListDelegate RefCusCodeListData { get; set; }

		public delegate IEnumerable<RefCusProcedure> CreateRefCusProcedureDelegate();
		public CreateRefCusProcedureDelegate RefCusProcedureData { get; set; }

		public delegate IEnumerable<RefCusTariff> CreateRefCusTariffDelegate();
		public CreateRefCusTariffDelegate RefCusTariffData { get; set; }

		protected virtual IEnumerable<RefCusCodeList> CreateRefCusCodeListTestData()
		{
			yield return CreateRefCusCodeList("FR", "AAA", "ABCDEF");
		}

		protected virtual IEnumerable<RefCusProcedure> CreateRefCusProcedureTestData()
		{
			yield return CreateRefCusProcedure("FR", "10", "71", "C024");
		}
		protected virtual IEnumerable<RefCusTariff> CreateRefCusTariffTestData()
		{
			yield return CreateRefCusTariff("EUN", "0000000001");
		}

		public int SleepInterval { get; set; } = 1000;

		public static RefCusCodeList CreateRefCusCodeList(string dataGrouping, string codeType, string code) => new RefCusCodeList { ZZD_Code = code, ZZD_ZZK_NKCodeType = codeType, ZZD_ZZZ_NKDataGrouping = dataGrouping };

		public static RefCusProcedure CreateRefCusProcedure(string datagrouping, string procedure, string previousProcedure, string concession) => new RefCusProcedure()
																																					{
																																						ZZ6_ProcedureCode = procedure,
																																						ZZ6_PreviousProcedureCode = previousProcedure,
																																						ZZ6_Concession = concession,
																																						ZZ6_ZZZ_NKDataGrouping = datagrouping,
																																					};
		public static RefCusTariff CreateRefCusTariff(string datagrouping, string tariffCode) => new RefCusTariff() {	ZZ1_ZZZ_NKDataGrouping = datagrouping, ZZ1_TariffCode = tariffCode };
	}
}
