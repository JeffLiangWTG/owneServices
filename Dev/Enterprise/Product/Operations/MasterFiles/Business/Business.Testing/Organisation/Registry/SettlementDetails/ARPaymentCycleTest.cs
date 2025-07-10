using System;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARPaymentCycle))]
	sealed class ARPaymentCycleTest : RegistryBusinessObjectTemplateTestCase
	{
		ZXmlSerializer PaymentCycleSerialiser
		{
			get
			{
				return paymentCycleSerialiser ?? (paymentCycleSerialiser = ZXmlSerializer.New(typeof(ARPaymentCycle)));
			}
		}
		ZXmlSerializer paymentCycleSerialiser;

		public void TestXMLSerialization()
		{
			var dummyCycle1 = new ARPaymentCycle();
			var dummyCycle2 = new ARPaymentCycle();

			dummyCycle1.ToDay = 3;
			dummyCycle1.PaymentDay = 6;
			string result;
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				PaymentCycleSerialiser.Serialize(writer, dummyCycle1);
				result = stringWriter.ToString();
			}
			AssertContains("<ARPaymentCycle><ToDay>3</ToDay><PaymentDay>6</PaymentDay></ARPaymentCycle>", result);

			AssertEquals((ZByte)1, dummyCycle2.ToDay);
			AssertEquals((ZByte)1, dummyCycle2.PaymentDay);

			using (StringReader stringReader = new StringReader(result))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				dummyCycle2 = (ARPaymentCycle)paymentCycleSerialiser.Deserialize(reader);
			}

			AssertEquals((ZByte)3, dummyCycle2.ToDay);
			AssertEquals((ZByte)6, dummyCycle2.PaymentDay);
		}

		public void TestToDayValidation()
		{
			Cycle1.ToDay = 0;
			AssertHasError(Cycle1.ToDayInfo, "The value must be 1 or greater.");
			Cycle1.ToDay = 1;
			Cycle2.ToDay = 1;
			Cycle3.ToDay = 1;
			AssertHasError(Cycle1.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			AssertHasError(Cycle2.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			AssertHasError(Cycle3.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			Cycle1.ToDay = 2;
			AssertNoErrors(Cycle1.ToDayInfo);
			AssertHasError(Cycle2.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			AssertHasError(Cycle3.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			Cycle2.ToDay = 2;
			AssertHasError(Cycle1.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			AssertHasError(Cycle2.ToDayInfo, "Payment cycle with the same 'Cycle #' already exists.");
			AssertNoErrors(Cycle3.ToDayInfo);
			Cycle1.ToDay = 1;
			Cycle3.ToDay = 3;
			AssertNoErrors(Cycle1.ToDayInfo);
			AssertNoErrors(Cycle2.ToDayInfo);
			AssertNoErrors(Cycle3.ToDayInfo);
		}

		public void TestPaymentDayValidation()
		{
			Cycle1.PaymentDay = 3;
			Cycle2.PaymentDay = 6;
			Cycle3.PaymentDay = 9;
			AssertEquals((ZByte)1, Cycle1.ToDay);
			AssertEquals((ZByte)2, Cycle2.ToDay);
			AssertEquals((ZByte)3, Cycle3.ToDay);

			Cycle1.PaymentDay = 10;

			AssertEquals((ZByte)3, Cycle1.ToDay);
			AssertEquals((ZByte)1, Cycle2.ToDay);
			AssertEquals((ZByte)2, Cycle3.ToDay);

			Cycle3.PaymentDay = 12;

			AssertEquals((ZByte)2, Cycle1.ToDay);
			AssertEquals((ZByte)1, Cycle2.ToDay);
			AssertEquals((ZByte)3, Cycle3.ToDay);

			Cycle1.PaymentDay = 2;
			Cycle2.PaymentDay = 2;
			Cycle3.PaymentDay = 2;

			AssertHasError(Cycle1.PaymentDayInfo, "Payment cycle with the same 'Payment Day' value already exists.");
			AssertHasError(Cycle2.PaymentDayInfo, "Payment cycle with the same 'Payment Day' value already exists.");
			AssertHasError(Cycle3.PaymentDayInfo, "Payment cycle with the same 'Payment Day' value already exists.");
		}

		public void TestFromDay()
		{
			AssertEquals((ZByte)0, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)0, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)0, Cycle3.FromDayCalculated);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ARPaymentCycle result = new ARPaymentCycle(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		ARPaymentCycle Cycle1;
		ARPaymentCycle Cycle2;
		ARPaymentCycle Cycle3;

		protected override void SetUp()
		{
			base.SetUp();
			ARPaymentCycleCollection collection = new ARPaymentCycleCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			Cycle1 = collection.AddNew();
			Cycle2 = collection.AddNew();
			Cycle3 = collection.AddNew();
		}

		#endregion
	}
}
