using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(DocumentProvider))]
abstract class DocumentProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : DocumentProvider
{
	public void TestSequenceNumeric()
	{
		info.CSI_LineNo = 1;
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public virtual void TestType()
	{
		AssertEquals("TYP", Provider.Type);
	}

	public virtual void TestReferenceNumber()
	{
		AssertEquals("RefNum", Provider.ReferenceNumber);
	}

	protected abstract string DocType { get; }

	protected override T GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		info = Factory.CreateCusSupportingInfo(DocType, string.Empty, "RefNum", null, "TYP", header);

		provider = (T)Activator.CreateInstance(typeof(T), info);
	}

	T provider;
	protected CusSupportingInfo info;
}
