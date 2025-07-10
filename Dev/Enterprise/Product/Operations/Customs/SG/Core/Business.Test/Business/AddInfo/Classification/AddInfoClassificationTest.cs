using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoClassification))]
	public class AddInfoClassificationTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoClassificationLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoClassificationValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddInfoClassification(Factory.New<Classification>().CC_AddInfoInfo);
		}
	}
}
