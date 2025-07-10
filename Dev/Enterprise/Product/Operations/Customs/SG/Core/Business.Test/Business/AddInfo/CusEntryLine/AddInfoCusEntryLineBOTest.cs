using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoCusEntryLine))]
	public class AddInfoCusEntryLineBOTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoCusEntryLineLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoCusEntryLineValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			return new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
		}
	}
}
