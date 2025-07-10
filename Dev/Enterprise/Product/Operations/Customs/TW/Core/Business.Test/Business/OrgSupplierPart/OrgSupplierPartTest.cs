using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Taiwan, pivot.CI_RN_NKCountry);
			}
		}

		public void TestPivotsType()
		{
			var part = (OrgSupplierPart)GetNewBusinessObject();
			AssertType<CusClassPartPivotCollection<CusClassPartPivot>>(part.PivotsForBinding);
		}

		#region ExpectedClassificationCollectionType
		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

		#endregion

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgSupplierPart.New(Factory);
		}
		#endregion
	}
}
