using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationEventLockInfo))]
	sealed class DeclarationEventLockInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestEntryType()
		{
			var eventLockInfo = new DeclarationEventLockInfo();

			eventLockInfo.EntryType = "AAA";
			eventLockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;

			Assert(!eventLockInfo.EntryTypeInfo.ReadOnly);
			AssertEquals("AAA", eventLockInfo.EntryType);

			eventLockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			Assert(eventLockInfo.EntryTypeInfo.ReadOnly);
			AssertEquals(Core.Constants.Customs.EntryHeaderTypes.Codes.All, eventLockInfo.EntryType);

			eventLockInfo.EntryType = "AAA";
			eventLockInfo.EventSource = string.Empty;

			Assert(eventLockInfo.EntryTypeInfo.ReadOnly);
			AssertEquals("AAA", eventLockInfo.EntryType);
		}

		#region Validation

		public void TestValidateEventType()
		{
			var messageOfNotFound = "Please enter a value.";
			var messageOfInvalidSelection = "Enter a valid selection.";

			var eventLockInfo = new DeclarationEventLockInfo();

			eventLockInfo.EventType = "@#&";
			AssertHasError(eventLockInfo.EventTypeInfo, messageOfInvalidSelection);

			eventLockInfo.EventType = AutoEvents.ArrivalCode;
			AssertNoError(eventLockInfo.EventTypeInfo, messageOfInvalidSelection);

			eventLockInfo.EventType = ZString.Empty;
			AssertHasError(eventLockInfo.EventTypeInfo, messageOfNotFound);
		}

		public void TestValidateEventType_UniqueCheck()
		{
			var message = "There is an existing item with same data.";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var config = new DeclarationLockConfig(fallbackLevel, Factory);

			var event1 = config.EventInfos.AddNew();
			event1.EventType = "ARV";
			event1.EventReference = "DEP=AAA";
			event1.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			event1.RunPreSaveValidation();
			AssertNoError(event1.EventTypeInfo, message);

			var event2 = config.EventInfos.AddNew();
			event2.EventType = "ARV";
			event2.EventReference = "DEP=AAA";
			event2.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			event1.RunPreSaveValidation();
			event2.RunPreSaveValidation();

			AssertHasError(event1.EventTypeInfo, message);
			AssertHasError(event2.EventTypeInfo, message);

			event2.EventType = "DOT";

			event1.RunPreSaveValidation();
			event2.RunPreSaveValidation();

			AssertNoError(event1.EventTypeInfo, message);
			AssertNoError(event2.EventTypeInfo, message);
		}

		public void TestValidateEventSource()
		{
			var message = "Enter a valid selection.";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var eventLockInfo = new DeclarationEventLockInfo(fallbackLevel, Factory);

			eventLockInfo.EventSource = "XXX";
			AssertHasError(eventLockInfo.EventSourceInfo, message);

			eventLockInfo.EventSource = string.Empty;
			AssertHasError(eventLockInfo.EventSourceInfo, "Please enter a value.");

			eventLockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;
			AssertNoError(eventLockInfo.EventSourceInfo, message);
		}

		public void TestValidateEntryType()
		{
			var messageOfNotFound = "Please enter a value.";
			var messageOfInvalidSelection = "Enter a valid selection.";

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var eventLockInfo = new DeclarationEventLockInfo(fallbackLevel, Factory);

			eventLockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;

			eventLockInfo.EntryType = "@#&";
			AssertHasError(eventLockInfo.EntryTypeInfo, messageOfInvalidSelection);

			eventLockInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
			AssertNoError(eventLockInfo.EntryTypeInfo, messageOfInvalidSelection);

			eventLockInfo.EntryType = ZString.Empty;
			AssertHasError(eventLockInfo.EntryTypeInfo, messageOfNotFound);

			eventLockInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration;
			AssertNoError(eventLockInfo.EntryTypeInfo, messageOfNotFound);

			eventLockInfo.EntryType = "@#&";
			AssertNoError(eventLockInfo.EntryTypeInfo, messageOfInvalidSelection);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var lockInfo = new DeclarationEventLockInfo
			{
				EventType = "ARV",
				EventReference = "DEP=AAA",
				EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.Declaration,
				EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All
			};

			return lockInfo;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
