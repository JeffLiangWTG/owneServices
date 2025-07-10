using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ReportingBookAccountingJournalPrintOptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ReportingBookAccountingJournalPrintOptionCollection()
		{
		}

		public new ReportingBookAccountingJournalPrintOption this[int i]
		{
			get { return SetParent((ReportingBookAccountingJournalPrintOption)Elements[i]); }
		}

		public new ReportingBookAccountingJournalPrintOption AddNew()
		{
			return SetParent((ReportingBookAccountingJournalPrintOption)base.AddNew());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new ReportingBookAccountingJournalPrintOptionCollection();
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetParent(new ReportingBookAccountingJournalPrintOption());
		}

		ReportingBookAccountingJournalPrintOption SetParent(ReportingBookAccountingJournalPrintOption method)
		{
			method.CurrentFallbackLevel = CurrentFallbackLevel;
			return method;
		}
	}
}
