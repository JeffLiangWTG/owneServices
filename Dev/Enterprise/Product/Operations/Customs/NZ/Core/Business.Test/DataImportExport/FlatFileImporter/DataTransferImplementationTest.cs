using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.Data.FlatFileImporter.Testing
{
	public class DataTransferImplementationTest : DataTransfer.Testing.DataTransferImplTest
	{
		public void TestGetFlatFileDataImporter()
		{
			DataTransferTestClass transfer = new DataTransferTestClass();
			AssertEquals(typeof(FlatFileInvoiceDataImporter), transfer.GetTypeOfFlatFileDataImporter());
		}

		#region Setup

		protected class DataTransferTestClass : DataTransferImplementation
		{
			public Type GetTypeOfFlatFileDataImporter()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				JobDeclaration declaration = JobDeclaration.New(factory);
				return GetFlatFileImporter("", declaration).GetType();
			}
		}

		#endregion
	}
}
