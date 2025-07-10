using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(CycleNoRegistryItem))]
	sealed class CycleNoRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CycleNoCollection>
	{
		protected override StronglyTypedRegistryItem<CycleNoCollection, CycleNoCollection> GetNewRegistryItem()
		{
			return new CycleNoRegistryItem("", (NoResString)"", "", "", new CycleNoCollection());
		}

		protected override CycleNoCollection ValidValue
		{
			get
			{
				CycleNoCollection collection = new CycleNoCollection();
				CycleNo cycleNo = collection.AddNew();
				cycleNo.CycleNum = 1;
				cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
				return collection;
			}
		}
	}
}
