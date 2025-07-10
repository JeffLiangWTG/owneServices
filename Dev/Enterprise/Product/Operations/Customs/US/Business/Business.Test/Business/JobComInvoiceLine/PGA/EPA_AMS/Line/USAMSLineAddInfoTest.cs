using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USAMSLineAddInfo))]
	class USAMSLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAMSLine()
		{
			var addInfo = GetNewBusinessObject() as USAMSLineAddInfo;
			AssertEquals(AMSLine.PK, addInfo.AMSLine.PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USAMSLineAddInfo(AMSLine.B7_AddInfoDataInfo);
		}

		AMSLine AMSLine
		{
			get { return amsDetails ?? (amsDetails = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().AMSLines.AddNew().AMSLines.AddNew()); }
		}
		AMSLine amsDetails;
	}
}
