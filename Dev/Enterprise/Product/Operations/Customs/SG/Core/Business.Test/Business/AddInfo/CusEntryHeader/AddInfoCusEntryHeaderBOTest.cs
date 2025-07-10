using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryHeader))]
	public class AddInfoCusEntryHeaderBOTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoCusEntryHeaderLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoCusEntryHeaderValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			return new AddInfoCusEntryHeader(entryHeader.CH_AddInfoInfo);
		}
	}
}
