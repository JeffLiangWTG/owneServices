namespace Enterprise.Customs.NZ.Business.TariffValidation.Testing
{
	using System;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;

	public class NZTariffFormatterTest : TransactionedTestCase
	{
		public void TestDisplayFormat()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1234.56.78.90A", tariffFormatter.DisplayFormat("1234567890A"));
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("1234.56.78.90A", tariffFormatter.DisplayFormat("1234567890A"));
			}
		}

		public void TestFormatCompleteTariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1234.56.78.90A", tariffFormatter.Format("1234567890A"));
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("1234567890A", tariffFormatter.Format("1234567890A"));
			}
		}

		public void TestFormatIncompleteTariffWithTrailingCharacter()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("8502.20.20.0", tariffFormatter.Format("8502.20200A"));
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("850220200", tariffFormatter.Format("8502.20200A"));
			}
		}

		public void TestFormatInCompleteTariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1234.56", tariffFormatter.Format("123456"));
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("123456", tariffFormatter.Format("123456"));
			}
		}

		public void TestExciseTariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("99.20.20", tariffFormatter.Format("992020"));
				AssertEquals("99.20.20L", tariffFormatter.Format("992020L"));
			}
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("992020", tariffFormatter.Format("992020"));
				AssertEquals("992020L", tariffFormatter.Format("992020L"));
			}
		}

		#region Implementation

		protected NZTariffFormatter tariffFormatter;
		protected override void SetUp()
		{
			base.SetUp();
			tariffFormatter = new NZTariffFormatter();
		}

		#endregion
	}
}
