using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}

		#endregion
	}
}
