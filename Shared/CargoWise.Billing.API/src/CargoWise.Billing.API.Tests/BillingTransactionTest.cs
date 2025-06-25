using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.Billing.API.Tests
{
	[TestFixture]
	public class BillingTransactionTest
	{
		[Test]
		public void TestRegularExpressionForTextFields()
		{
			var re = new Regex(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$");

			Assert.That(re.IsMatch("aA}^"));
			Assert.That(re.IsMatch("测试"));
			Assert.That(re.IsMatch("\r\n\t"));
			Assert.That(re.IsMatch("\u0020"));

			Assert.That(!re.IsMatch("\u0008"));
			Assert.That(!re.IsMatch("\u000B"));
			Assert.That(!re.IsMatch("\u000C"));
			Assert.That(!re.IsMatch("\u000E"));
			Assert.That(!re.IsMatch("\u001F"));
		}

		[Test]
		public void TestToString()
		{
			var transaction = ObjectGenerator.CreateWithAllPropertiesSet<BillingTransaction>();
			var transactionString = transaction.ToString();
			var transactionType = typeof(BillingTransaction);
			var properties = transactionType.GetProperties();
			foreach (var property in properties)
			{
				string propertyString;
				if (property.Name == "ServiceOccuredUTC")
				{
					propertyString = Convert.ToDateTime(property.GetValue(transaction)).ToString("s");
				}
				else
				{
					propertyString = property.GetValue(transaction).ToString();
				}
				Assert.That(transactionString, Contains.Substring(property.Name + ": " + propertyString));
			}
		}

		[Test]
		public void TestToStringShowsAllProperties()
		{
			var transaction = new BillingTransaction();
			var str = transaction.ToString();
			foreach (var propertyInfo in transaction.GetType().GetProperties())
			{
				Assert.That(str, Contains.Substring(propertyInfo.Name));
			}
		}
	}
}
