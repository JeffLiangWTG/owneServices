using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISBondData))]
	sealed class DISBondDataTest : XmlSerializableNonPersistentBusinessObjectTest<DISBondData>
	{
		public void TestReadOnly()
		{
			var businessObject = (DISBondData)GetNewBusinessObject();
			foreach (ZPropertyInfo info in businessObject.ZPropertyInfoHash)
			{
				Assert("no property should be readonly", !info.ReadOnly);
			}

			businessObject.DefaultBondCode = "1";
			foreach (ZPropertyInfo info in businessObject.ZPropertyInfoHash)
			{
				if (info.Name == DISBondData.Schema.DefaultBondCode)
				{
					Assert("All fields other than DefaultBondCode should be readonly", !info.ReadOnly);
				}
				else
				{
					Assert("All fields other than DefaultBondCode should be readonly", info.ReadOnly);
				}
			}
		}

		public void TestDefaultBondDetails()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var bondDataListMock = new Mock<IDISBondDataDefault>();
			bondDataListMock.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondDataListMock.Setup(m => m.BondNumber).Returns(new ZString("423987432"));
			bondDataListMock.Setup(m => m.SuretyCode).Returns(new ZString("345"));
			bondDataListMock.Setup(m => m.BondAmount).Returns(new ZDecimal(250.20m));
			bondDataListMock.Setup(m => m.Code).Returns(new ZString("1"));
			bondDataListMock.Setup(m => m.Description).Returns(new ZString("blah"));
			defaultValues.Setup(m => m.DefaultBondData).Returns(new IDISBondDataDefault[] { bondDataListMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var bondData = disDocument.BondData;
			bondData.DefaultBondCode = "1";
			AssertEquals("SGT", bondData.BondName);
			AssertEquals("423987432", bondData.BondNumber);
			AssertEquals("345", bondData.SuretyCode);
			AssertEquals(250.20m, bondData.BondAmount);
			var list = bondData.DefaultBondDataList;
			AssertEquals(1, list.Count);
			AssertEquals("1", list[0].Code);
			AssertEquals("blah", list[0].Description);

			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			bondDataListMock.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			return new DISBondData(disDocument);
		}
	}
}
