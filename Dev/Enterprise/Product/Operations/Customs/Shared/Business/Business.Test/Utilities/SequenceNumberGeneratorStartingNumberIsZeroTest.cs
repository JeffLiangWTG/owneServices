using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SequenceNumberGeneratorStartingNumberIsZeroTest : TestCaseWithFactory
	{
		public void TestSequenceStartingNumber()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			AssertEquals(0, entryInstructionForTest.SequenceGenerator.SequenceStartingNumber);
		}

		public void TestSequenceMaxNumber()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			AssertEquals(byte.MaxValue, entryInstructionForTest.SequenceGenerator.SequenceMaxNumber);
		}

		public void TestRecalculateWhenAdded()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			var jobDocAddressForTest1 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest2 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest3 = entryInstructionForTest.JobDocAddressForTests.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("jobDocAddressForTest1", (ZByte)0, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest2", (ZByte)1, jobDocAddressForTest2.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest3", (ZByte)2, jobDocAddressForTest3.E2_AddressSequence);
			});
		}

		public void TestRecalculateWhenAboutToBeDetachedOrDeleted()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			var jobDocAddressForTest1 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest2 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest3 = entryInstructionForTest.JobDocAddressForTests.AddNew();

			entryInstructionForTest.JobDocAddressForTests.RemoveAndDelete(jobDocAddressForTest2);
			CombineAssertions(() =>
			{
				AssertEquals("jobDocAddressForTest1", (ZByte)0, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest3", (ZByte)1, jobDocAddressForTest3.E2_AddressSequence);

				var jobDocAddressForTest4 = entryInstructionForTest.JobDocAddressForTests.AddNew();
				AssertEquals("jobDocAddressForTest4", (ZByte)2, jobDocAddressForTest4.E2_AddressSequence);
			});
		}

		public void TestRecalculateAll()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			var jobDocAddressForTest1 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest2 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest3 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			using (entryInstructionForTest.SequenceGenerator.GetLineNumberSuspender())
			{
				jobDocAddressForTest2.E2_AddressSequence = 2;
			}
			CombineAssertions(() =>
			{
				AssertEquals("jobDocAddressForTest1", (ZByte)0, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest2", (ZByte)2, jobDocAddressForTest2.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest3", (ZByte)2, jobDocAddressForTest3.E2_AddressSequence);

				entryInstructionForTest.SequenceGenerator.ReCalculateAll();
				AssertEquals("new jobDocAddressForTest1", (ZByte)0, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("new jobDocAddressForTest2", (ZByte)1, jobDocAddressForTest2.E2_AddressSequence);
				AssertEquals("new jobDocAddressForTest3", (ZByte)2, jobDocAddressForTest3.E2_AddressSequence);
			});
		}

		public void TestRecalculateAll_Excessive()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			for (int i = 0; i <= byte.MaxValue; i++)
			{
				entryInstructionForTest.JobDocAddressForTests.AddNew();
			}
			entryInstructionForTest.JobDocAddressForTests.AddNew();
			entryInstructionForTest.JobDocAddressForTests.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Count = 255 + 1 + 2", 258, entryInstructionForTest.JobDocAddressForTests.Count);
				entryInstructionForTest.SequenceGenerator.ReCalculateAll();
				AssertEquals("E2_AddressSequence = 0 Count", 3, entryInstructionForTest.JobDocAddressForTests.Cast<JobDocAddressForTest>().Count(x => x.E2_AddressSequence == 0));
			});
		}

		public void TestRecalculateWhenRenumbered()
		{
			var entryInstructionForTest = Factory.New<CusEntryInstructionForTest>();
			var jobDocAddressForTest1 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest2 = entryInstructionForTest.JobDocAddressForTests.AddNew();
			var jobDocAddressForTest3 = entryInstructionForTest.JobDocAddressForTests.AddNew();

			CombineAssertions(() =>
			{
				jobDocAddressForTest2.E2_AddressSequence = 2;
				AssertEquals("jobDocAddressForTest1", (ZByte)0, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest2", (ZByte)2, jobDocAddressForTest2.E2_AddressSequence);
				AssertEquals("jobDocAddressForTest3", (ZByte)1, jobDocAddressForTest3.E2_AddressSequence);

				jobDocAddressForTest1.E2_AddressSequence = 4;
				AssertEquals("new jobDocAddressForTest1", (ZByte)2, jobDocAddressForTest1.E2_AddressSequence);
				AssertEquals("new jobDocAddressForTest2", (ZByte)1, jobDocAddressForTest2.E2_AddressSequence);
				AssertEquals("new jobDocAddressForTest3", (ZByte)0, jobDocAddressForTest3.E2_AddressSequence);
			});
		}

		class CusEntryInstructionForTest : CusEntryInstruction, ISequenceNumberHeader
		{
			public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ChildEditable]
			public JobDocAddressForTestCollection JobDocAddressForTests
			{
				get
				{
					if (jobDocAddressForTests == null)
					{
						jobDocAddressForTests = new JobDocAddressForTestCollection(this);
						jobDocAddressForTests.Load();
						RegisterEditableChildObject(jobDocAddressForTests);
					}
					return jobDocAddressForTests;
				}
			}
			JobDocAddressForTestCollection jobDocAddressForTests;

			IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(JobDocAddressForTests);

			internal ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this, () => 0, () => byte.MaxValue));
			ShortSequenceNumberGenerator sequenceGenerator;
		}

		class JobDocAddressForTestCollection : DependentBusinessObjectCollection<JobDocAddressForTest, CusEntryInstructionForTest>
		{
			public JobDocAddressForTestCollection(CusEntryInstructionForTest master) : base(master)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(JobDocAddressForTest);

			protected override SchemaGuidColumn FKSchemaColumnInDependent => JobDocAddressSchema.E2_ParentID;

			protected override void SetDefaultsForNewChild(BusinessObject child)
			{
				base.SetDefaultsForNewChild(child);
				var jobDocAddressForTest = (JobDocAddressForTest)child;
				jobDocAddressForTest.E2_ParentTableCode = Master.TablePrefix;
				jobDocAddressForTest.E2_AddressType = MasterFiles.Integration.AutoDocAddressTypes.Codes.Acquirer;
			}

			protected override void OnRemoving(BusinessObject bizO)
			{
				base.OnRemoving(bizO);
				var jobDocAddressForTest = bizO as JobDocAddressForTest;
				jobDocAddressForTest?.Instruction?.SequenceGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(jobDocAddressForTest);
			}
		}

		class JobDocAddressForTest : JobDocAddress, IShortSequenceNumberLine
		{
			public JobDocAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public CusEntryInstructionForTest Instruction => Factory.Load<CusEntryInstructionForTest>(E2_ParentID);

			public override ZByte E2_AddressSequence
			{
				get => base.E2_AddressSequence;
				set
				{
					var oldValue = E2_AddressSequence;
					base.E2_AddressSequence = value;
					if (!IsCopying && oldValue != E2_AddressSequence)
					{
						Instruction?.SequenceGenerator.RecalculateWhenRenumbered(this, new ZShort(oldValue));
					}
				}
			}

			protected new bool E2_AddressSequence_ReadOnly
			{
				get { return false; }
			}

			public override ZGuid E2_ParentID
			{
				get => base.E2_ParentID;
				set
				{
					var oldValue = E2_ParentID;
					base.E2_ParentID = value;
					if (!IsCopying && oldValue != E2_ParentID)
					{
						Instruction?.SequenceGenerator.RecalculateWhenAdded(this);
					}
				}
			}

			ZShort ISequenceNumberLine<ZShort>.SequenceNumber
			{
				get => new ZShort(E2_AddressSequence);
				set => E2_AddressSequence = ZByte.ParseSafe(value.ToString(), ZByte.Zero);
			}

			ZGuid ISequenceNumberLine.FKToHeader => E2_ParentID;
		}
	}
}
