using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeader))]
	sealed class DocJobComInvoiceGroupHeaderTest : DocBaseJobComInvoiceGroupHeaderAbstractTest<JobComInvoiceGroupHeader, DocJobComInvoiceGroupHeader>
	{
		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override DocJobComInvoiceGroupHeader CreateGroupHeaderWrapper(JobComInvoiceGroupHeader groupHeaderInternal)
		{
			return DocJobComInvoiceGroupHeader.New(groupHeaderInternal, Factory);
		}

		#endregion
	}
}
