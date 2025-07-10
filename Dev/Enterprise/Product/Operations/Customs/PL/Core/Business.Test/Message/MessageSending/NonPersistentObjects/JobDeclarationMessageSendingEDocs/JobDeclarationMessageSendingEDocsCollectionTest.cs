using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingEDocsCollection))]
sealed class JobDeclarationMessageSendingEDocsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMessageSendingEDocsCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationMessageSendingEDocsCollection(Factory, null, null, null));
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationMessageSendingEDocsCollection(Factory, () => new CodeDescriptionPairList(), null, null));
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationMessageSendingEDocsCollection(Factory,
			() => new CodeDescriptionPairList(), new CustomsDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>()), null));
	}

	protected override JobDeclarationMessageSendingEDocsCollection GetCollectionToTest() => collection;

	protected override BusinessObject GetNewElementToAddToTheCollection() => new JobDeclarationMessageSendingEDocs(Factory, collection);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
		collection = new JobDeclarationMessageSendingEDocsCollection(Factory, () => new CodeDescriptionPairList(), sendingObjectParent, () => new Dictionary<ZGuid, ZString>());
	}

	JobDeclarationMessageSendingEDocsCollection collection;
}
