using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs._CustomsTemplate_.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappersTesting
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes._TemplateCountryName_; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}

		#endregion
	}
}
