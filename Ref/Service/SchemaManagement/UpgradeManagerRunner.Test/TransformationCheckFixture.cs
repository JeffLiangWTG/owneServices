using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class TransformationCheckFixture
	{
		[Test]
		public void CheckRefDataSetInformationTransformation()
		{
			var typeList = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.GetName().Name == "CargoWise.RefDbRepo.Service.UpgradeManagerRunner")
				.SelectMany(x => x.GetTypes().Where(t => t.BaseType == typeof(RefDataSetInformationTransformation)));
			var assembly = typeList.First().Assembly;
			var dataSetIdList = new List<int>();
			foreach (var type in typeList)
			{
				var propertyInfo = type.GetProperty("DataSetId");
				var instance = assembly.CreateInstance(type.FullName, true, BindingFlags.Default, null, new object[] { 0 }, null, null);
				dataSetIdList.Add((int)propertyInfo.GetValue(instance));
			}
			if (dataSetIdList.Any())
			{
				dataSetIdList.Sort();
				Assert.AreEqual(true, dataSetIdList.SequenceEqual(Enumerable.Range(dataSetIdList.Min(), dataSetIdList.Count)));
			}
		}
	}
}
