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
	[TestedType(typeof(ARTermsCycle))]
	sealed class ARTermsCycleTest : RegistryBusinessObjectTemplateTestCase
	{
		ZXmlSerializer TermsCycleSerialiser
		{
			get
			{
				return termsCycleSerialiser ?? (termsCycleSerialiser = ZXmlSerializer.New(typeof(ARTermsCycle)));
			}
		}
		ZXmlSerializer termsCycleSerialiser;

		public void TestXMLSerialization()
		{
			Cycle1.ToDay = 3;
			Cycle1.PaymentDay = 6;
			string result;
			using (StringWriter stringWriter = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
			{
				TermsCycleSerialiser.Serialize(writer, Cycle1);
				result = stringWriter.ToString();
			}
			AssertContains("<ARTermsCycle><ToDay>3</ToDay><PaymentDay>6</PaymentDay></ARTermsCycle>", result);

			AssertEquals((ZByte)31, Cycle2.ToDay);
			AssertEquals((ZByte)1, Cycle2.PaymentDay);

			using (StringReader stringReader = new StringReader(result))
			using (XmlTextReader reader = new XmlTextReader(stringReader))
			{
				Cycle2 = (ARTermsCycle)TermsCycleSerialiser.Deserialize(reader);
			}

			AssertEquals((ZByte)3, Cycle2.ToDay);
			AssertEquals((ZByte)6, Cycle2.PaymentDay);
		}

		public void TestToDayValidation()
		{
			Cycle1.ToDay = 0;
			AssertHasError(Cycle1.ToDayInfo, "The value must be between 1 and 31.");
			Cycle1.ToDay = 32;
			AssertHasError(Cycle1.ToDayInfo, "The value must be between 1 and 31.");
			Cycle1.ToDay = 1;
			AssertNoErrors(Cycle1.ToDayInfo);
			Cycle2.ToDay = 1;
			AssertHasError(Cycle1.ToDayInfo, "Terms cycle with the same 'To Day' value already exists.");
			AssertHasError(Cycle2.ToDayInfo, "Terms cycle with the same 'To Day' value already exists.");
			Cycle2.ToDay = 2;
			AssertNoErrors(Cycle1.ToDayInfo);
			AssertNoErrors(Cycle2.ToDayInfo);
		}

		public void TestPaymentDayValidation()
		{
			Cycle1.PaymentDay = 0;
			AssertHasError(Cycle1.PaymentDayInfo, "The value must be between 1 and 31.");
			Cycle1.PaymentDay = 32;
			AssertHasError(Cycle1.PaymentDayInfo, "The value must be between 1 and 31.");
			Cycle1.PaymentDay = 1;
			AssertNoErrors(Cycle1.PaymentDayInfo);
			Cycle2.PaymentDay = 1;
			AssertNoErrors(Cycle1.PaymentDayInfo);
			AssertNoErrors(Cycle2.PaymentDayInfo);
			Cycle2.PaymentDay = 2;
			AssertNoErrors(Cycle1.PaymentDayInfo);
			AssertNoErrors(Cycle2.PaymentDayInfo);
		}

		public void TestFromDay()
		{
			Cycle1.ToDay = 3;
			Cycle2.ToDay = 6;
			Cycle3.ToDay = 9;
			AssertEquals((ZByte)10, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)4, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)7, Cycle3.FromDayCalculated);
			Cycle1.ToDay = 2;
			AssertEquals((ZByte)10, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)3, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)7, Cycle3.FromDayCalculated);
			Cycle1.ToDay = 1;
			AssertEquals((ZByte)10, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)2, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)7, Cycle3.FromDayCalculated);
			Cycle2.ToDay = 8;
			AssertEquals((ZByte)10, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)2, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)9, Cycle3.FromDayCalculated);
			Cycle3.ToDay = 16;
			AssertEquals((ZByte)17, Cycle1.FromDayCalculated);
			AssertEquals((ZByte)2, Cycle2.FromDayCalculated);
			AssertEquals((ZByte)9, Cycle3.FromDayCalculated);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ARTermsCycle result = new ARTermsCycle(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

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

		ARTermsCycle Cycle1;
		ARTermsCycle Cycle2;
		ARTermsCycle Cycle3;

		protected override void SetUp()
		{
			base.SetUp();
			ARTermsCycleCollection collection = new ARTermsCycleCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			Cycle1 = collection.AddNew();
			Cycle2 = collection.AddNew();
			Cycle3 = collection.AddNew();
		}

		#endregion
	}
}
