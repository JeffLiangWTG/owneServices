using System;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public abstract class TestCase
{
	protected abstract string TestClassName { get; }

	protected Assembly ExecutingAssembly => executingAssembly ??= Assembly.GetExecutingAssembly();
	Assembly executingAssembly;

	[OneTimeSetUp]
	public virtual void OneTimeSetUp()
	{
		FileHelper = new TestFileHelper(TestClassName);
		_ = FileHelper.CreateOutputFolder(ExecutingAssembly);
	}

	protected Mock<IDateTimeProvider> DateProvider => dateProvider ??= CreateDateProvider();
	Mock<IDateTimeProvider> dateProvider;

	Mock<IDateTimeProvider> CreateDateProvider()
	{
		var dateProvider = new Mock<IDateTimeProvider>();
		dateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2021, 3, 25));
		return dateProvider;
	}

	protected TestFileHelper FileHelper { get; private set; }

	[OneTimeTearDown]
	public virtual void OneTimeTearDown()
	{
		FileHelper.DeleteOutputFolder();
	}
}
