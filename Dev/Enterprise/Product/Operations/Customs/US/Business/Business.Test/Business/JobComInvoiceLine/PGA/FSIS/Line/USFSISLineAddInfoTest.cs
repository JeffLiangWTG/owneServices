using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFSISLineAddInfo))]
	class USFSISLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new USFSISLineAddInfo(FSISLine.B7_AddInfoDataInfo);
		}

		USFSISLine FSISLine
		{
			get { return fFSISLine ?? (fFSISLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew()); }
		}
		USFSISLine fFSISLine;
	}
}
