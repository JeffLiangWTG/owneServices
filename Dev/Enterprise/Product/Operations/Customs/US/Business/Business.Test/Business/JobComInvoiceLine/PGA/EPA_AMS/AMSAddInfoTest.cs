using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSAddInfo))]
	class AMSAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AMSAddInfo(AMSLine.B7_AddInfoDataInfo);
		}

		AMS AMSLine
		{
			get { return amsLine ?? (amsLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().AMSLines.AddNew()); }
		}
		AMS amsLine;
	}
}
