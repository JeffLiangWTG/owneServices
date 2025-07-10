using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	public class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
	{
		protected override Type GetExpectedCollectionType() => typeof(AdditionalInfoCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => additionalInfos;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			additionalInfos = declaration.AdditionalInfos;
		}
		AdditionalInfoCollection additionalInfos;
	}
}
