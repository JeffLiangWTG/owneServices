using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using Castle.Core.Internal;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests;

public class Isztar4Test
{
	static readonly Type DataGroupInterfaceType = typeof(IDataGroup<>);

	[Test]
	[TestCaseSource(nameof(GetTestCases))]
	public void TestIsztarHistoryResponseIsztarHistoryItem(Type itemType)
	{
		var doesImplementDataGroupInterface = itemType.GetInterfaces()
			.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == DataGroupInterfaceType);
		var dataPointsPropertyInfo = itemType.GetProperty(nameof(IDataGroup<IDataPoint>.DataPoints));

		Assert.Multiple(() =>
		{
			Assert.That(doesImplementDataGroupInterface, Is.True, "{0} type does not implement interface {1}", itemType, DataGroupInterfaceType);
			Assert.That(dataPointsPropertyInfo?.GetAttribute<XmlIgnoreAttribute>(), Is.Not.Null, "{0} type has no DataPoints property decorated with XmlIgnoreAttribute.", itemType);
		});
	}

	static IEnumerable<TestCaseData> GetTestCases()
	{
		var historyResponseType = typeof(IsztarHistoryResponseIsztarHistoryItem);
		var itemPropertyInfo = historyResponseType.GetProperty(nameof(IsztarHistoryResponseIsztarHistoryItem.Item));
		var xmlElementAttributes = itemPropertyInfo.GetAttributes<XmlElementAttribute>();
		var itemTypes = xmlElementAttributes.Select(element => element.Type).Where(type => type != null);
		return itemTypes.Select(itemType => new TestCaseData(itemType).SetName(itemType.Name));
	}
}
