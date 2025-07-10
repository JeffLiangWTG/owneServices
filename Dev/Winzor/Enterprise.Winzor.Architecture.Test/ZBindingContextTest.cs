using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class ZBindingContextTest
{
	[Test]
	public async Task TestWorkItemBindingContextEntryCanRetrieveDataMemberAndWeakReference()
	{
		using var ctx = new EnterpriseTestContext();
		WorkItemForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var workitem = factory.NewWithValidTestData<WorkItem>();

			form = new WorkItemForm(workitem);
			return form;
		});

		Assert.That(form.BindingContext, Is.InstanceOf<ZBindingContext>());
		var entryList = form.BindingContext.ToList<DictionaryEntry>();
		Assert.That(entryList.Count, Is.GreaterThan(0));

		var dataMemberField = entryList[0].Key.GetType().GetField("dataMember", BindingFlags.Instance | BindingFlags.NonPublic);
		var weakReferenceField = entryList[0].Key.GetType().GetField("wRef", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(dataMemberField, Is.Not.Null);
		Assert.That(weakReferenceField, Is.Not.Null);
	}
}
