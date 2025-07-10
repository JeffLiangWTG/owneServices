using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReportingBookAccountingJournalPrintOption))]
	class ReportingBookAccountingJournalPrintOptionTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new ReportingBookAccountingJournalPrintOption();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportingBookAccountingJournalPrintOption();
		}

		protected new ReportingBookAccountingJournalPrintOption BizObj
		{
			get { return (ReportingBookAccountingJournalPrintOption)base.BizObj; }
		}

		#endregion
	}
}
