using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketToPrintEventArgsTest : TestCaseWithFactory
	{
		#region Test Cases

		#region Constructors

		public void TestConstructor()
		{
			WhsDocket docket = GetNewDocket();
			WhsDocketLabelControl docketDocument = new WhsDocketLabelControl(docket, 1);
			WhsDocketToPrintEventArgs e = GetNewDocketToPrintEventArgs(docketDocument, GetDataContextToTest());
			AssertSame("DocketDocument must be the same as EventArgs DocketDocument", docketDocument, e.DocketDocument);
			AssertEquals("DataContext", GetDataContextToTest(), e.DataContext);
			AssertEquals("Continue to print is true", true, e.ContinueToPrint);
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract WhsDocket GetNewDocket();

		protected abstract WhsDocketToPrintEventArgs GetNewDocketToPrintEventArgs(WhsDocketLabelControl docketDocument, Core.Constants.DataContext dataContext);

		protected abstract Core.Constants.DataContext GetDataContextToTest();

		protected virtual WhsDocketLabelControl GetNewDocketDocument(WhsDocket docket)
		{
			return new WhsDocketLabelControl(docket, 1);
		}

		#endregion
	}
}
