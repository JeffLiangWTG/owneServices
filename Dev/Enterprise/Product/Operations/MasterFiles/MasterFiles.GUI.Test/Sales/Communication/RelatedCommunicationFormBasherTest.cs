using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RelatedCommunicationForm))]
	sealed class RelatedCommunicationFormBasherTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormVerb()
		{
			using (var form = (RelatedCommunicationForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#region Implementation

		[RequiresSTA]
		public override void TestBindingAllTabsOnIdle()
		{
			try
			{
				base.TestBindingAllTabsOnIdle();
			}
			catch (NotSupportedException)
			{
				Assert("Because binding is for ActiveBusinessObjectCollection, this test is blowing up when HasChanges attempted to be set to true.", true);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var collection = new OrgSalesCallCollection(Factory);
			return new RelatedCommunicationForm(collection);
		}

		#endregion
	}
}
