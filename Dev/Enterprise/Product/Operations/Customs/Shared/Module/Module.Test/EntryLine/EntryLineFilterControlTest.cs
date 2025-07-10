using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	class EntryLineFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			using (ZForm form = new ZForm())
			{
				EntryLineFilterControl filterControl = new EntryLineFilterControl(collection, filterBizObj);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		GlobalCusEntryLineCollection collection;
		EntryLineFilterBusinessObject filterBizObj;
		protected override void SetUp()
		{
			base.SetUp();
			collection = new GlobalCusEntryLineCollection(Factory);
			filterBizObj = new EntryLineFilterBusinessObject(collection);
		}
	}
}
