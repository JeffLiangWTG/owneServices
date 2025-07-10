using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public abstract class WhsXmlExportToFileDirectorTest<TDocket, TValueObject> : WhsXmlExportDirectorTest<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		#region TestExportSavesTheFileToThePathProvided

		[TestDate(2006, 10, 5, 12, 0, 0, 0)]
		public void TestExportSavesTheFileToThePathProvided()
		{
			try
			{
				Docket.Factory.Save();
				ExportDirector.RunExport(Docket);
				Assert(File.Exists(GetExpectedFileFullPath()));
			}
			finally
			{
				DeleteIfExists(GetExpectedFileFullPath());
			}
		}

		#endregion

		#region TestRequestFileNameFromUser

		public void TestRequestFileNameFromUser()
		{
			AssertEquals(true, exportToFileDirector.RequestFileNameFromUser);
		}

		#endregion

		#region Overrides

		protected override WhsXmlExportDirector<TDocket, TValueObject> GetNewExportDirector()
		{
			ZFormModaliser.FileNameToSelectInShowCommonDialog = GetExpectedFileFullPath();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var result = base.GetNewExportDirector();
			try
			{ File.Delete(GetExpectedFileFullPath()); }
			catch { }
			return result;
		}

		protected override void TearDown()
		{
			base.TearDown();
			try
			{ File.Delete(GetExpectedFileFullPath()); }
			catch { }
		}

		#endregion

		#region Implementation

		protected virtual ZString GetExpectedFileFullPath() => Path.Combine(Env.TempPath, "TEST_20061005120000.xml");

		WhsXmlExportToFileDirector<TDocket, TValueObject> exportToFileDirector => (WhsXmlExportToFileDirector<TDocket, TValueObject>)ExportDirector;

		#endregion
	}
}
