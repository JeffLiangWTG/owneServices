using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationReservedField))]
	sealed class JobDeclarationReservedFieldTest : ReservedFieldTest<JobDeclarationReservedField>
	{
		[ExpectNoExceptions]
		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();
			var reservedField = (ReservedField)GetNewBusinessObject();
			NUnit.Framework.Assert.That(reservedField.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobDeclarationSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(reservedField.Parent, NUnit.Framework.Is.EqualTo(jobDeclaration).Using(CustomComparers.TypeComparison));
		}

		#region Implementation
		protected override IEnumerable<JobDeclarationReservedField> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().ReservedFields.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return jobDeclaration.ReservedFields.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			reservedField = jobDeclaration.ReservedFields.AddNew();
		}

		JobDeclaration jobDeclaration;
		JobDeclarationReservedField reservedField;
		#endregion
	}
}
