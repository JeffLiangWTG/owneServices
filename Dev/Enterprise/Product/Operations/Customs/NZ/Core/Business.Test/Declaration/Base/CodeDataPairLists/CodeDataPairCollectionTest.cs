using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.ComponentModel;
	using Enterprise.Customs.NZ.Business.MasterFiles;

	public abstract class CodeDataPairCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : CodeDataPairCollection
	{
		public void TestCopyContentsOverwriting()
		{
			CodeDataPairCollection source = GetCollectionToTest();
			CodeDataPairCollection destination = GetCollectionToTest();
			AssertNotEquals("Precondition: Source and Destination collections are not the same collection", source, destination);

			CodeDataPair pair1 = source.AddNew("ONE", "TWO");
			source.CopyContentsOverwriting(destination);
			AssertEquals("destination.Count", 1, destination.Count);
			AssertEquals("ONE", destination[0].ZO_Code);
			AssertEquals("TWO", destination[0].ZO_Data);

			CodeDataPair pair2 = source.AddNew("555", "LASTLY");
			source.CopyContentsOverwriting(destination);
			AssertEquals("destination.Count", 2, destination.Count);
			AssertEquals("ONE", destination[0].ZO_Code);
			AssertEquals("TWO", destination[0].ZO_Data);
			AssertEquals("555", destination[1].ZO_Code);
			AssertEquals("LASTLY", destination[1].ZO_Data);

			source.RemoveAndDelete(pair1);
			source.CopyContentsOverwriting(destination);
			AssertEquals("destination.Count", 1, destination.Count);
			AssertEquals("555", destination[0].ZO_Code);
			AssertEquals("LASTLY", destination[0].ZO_Data);

			source.RemoveAndDelete(pair2);
			source.CopyContentsOverwriting(destination);
			AssertEquals("destination.Count", 0, destination.Count);
		}

		public void TestCodesInListHaveChanged()
		{
			CodeDataPairCollection collection = CollectionAgainstInvoiceLine;
			collection ??= CollectionAgainstDeclaration;
			codesInListHaveChangedEventFiredCount = 0;
			collection.CodesInListHaveChanged += new CodeDataPairCollection.CodesInListHaveChangedEventHandler(CollectionAgainstInvoiceLine_CodesInListHaveChanged);
			AssertEquals("CodesInListHaveChangedEventFiredCount", 0, codesInListHaveChangedEventFiredCount);
			CodeDataPair data1 = collection.AddNew();
			AssertEquals("CodesInListHaveChangedEventFiredCount", 0, codesInListHaveChangedEventFiredCount);
			data1.ZO_Code = "AAA";
			AssertEquals("CodesInListHaveChangedEventFiredCount", 1, codesInListHaveChangedEventFiredCount);
			data1.ZO_Code = "AAA";
			AssertEquals("CodesInListHaveChangedEventFiredCount", 1, codesInListHaveChangedEventFiredCount);
			data1.ZO_Code = "BBB";
			AssertEquals("CodesInListHaveChangedEventFiredCount", 2, codesInListHaveChangedEventFiredCount);
			data1.ZO_Code = "BBB";
			AssertEquals("CodesInListHaveChangedEventFiredCount", 2, codesInListHaveChangedEventFiredCount);
			CodeDataPair data2 = collection.AddNew("", "");
			AssertEquals("CodesInListHaveChangedEventFiredCount", 2, codesInListHaveChangedEventFiredCount);
			CodeDataPair data3 = collection.AddNew("AAA", "");
			AssertEquals("CodesInListHaveChangedEventFiredCount", 3, codesInListHaveChangedEventFiredCount);
			collection.RemoveAndDelete(data3);
			AssertEquals("CodesInListHaveChangedEventFiredCount", 4, codesInListHaveChangedEventFiredCount);
			collection.RemoveAndDeleteAll();
			AssertEquals("CodesInListHaveChangedEventFiredCount", 5, codesInListHaveChangedEventFiredCount);
		}
		int codesInListHaveChangedEventFiredCount;
		void CollectionAgainstInvoiceLine_CodesInListHaveChanged()
		{
			codesInListHaveChangedEventFiredCount += 1;
		}

		public void TestAllCodeDataPairCollectionsHaveMaximumRowsForValidationSet()
		{
			CodeDataPairCollection collectionAgainstDeclaration = CollectionAgainstDeclaration;
			if (collectionAgainstDeclaration != null)
			{
				Assert("CollectionAgainstDeclaration.MaximumRowsForValidation > 0", collectionAgainstDeclaration.MaxCount > 0);
			}

			CodeDataPairCollection collectionAgainstInvoiceLine = CollectionAgainstInvoiceLine;
			if (collectionAgainstInvoiceLine != null)
			{
				Assert("CollectionAgainstInvoiceLine.MaximumRowsForValidation > 0", collectionAgainstInvoiceLine.MaxCount > 0);
			}
		}

		public void TestLoadFromStringDoesntRemoveDuplicateCodes()
		{
			CodeInfos.LoadFromString("WIN=000000001A^WIN=000000002B");
			AssertEquals("CodeInfos.Count", 2, CodeInfos.Count);
			Assert("Not changed", !CodeInfos.HasChanges);
		}

		public void TestLoadFromStringWithNoDuplicatesRemovesDuplicateCodes()
		{
			CodeInfos.LoadFromStringWithNoDuplicates("DEF^WIN=000000001A^ABC^WIN=000000002B^WIN=000000001A^DEF^ABC");
			AssertEquals("CodeInfos.Count", 4, CodeInfos.Count);
		}

		public void TestParent()
		{
			AssertEquals("CodeInfos.Parent", Classification, CodeInfos.Parent);
		}

		public void TestAddOrUpdateExisting()
		{
			AssertEquals("", CodeInfos.AggregatedCodes);
			AssertEquals("", CodeInfos.AggregatedDatas);

			CodeInfos.AddOrUpdateExisting("ABC", "123");
			AssertEquals("ABC", CodeInfos.AggregatedCodes);
			AssertEquals("123", CodeInfos.AggregatedDatas);

			CodeInfos.AddOrUpdateExisting("ABC", "456");
			AssertEquals("ABC", CodeInfos.AggregatedCodes);
			AssertEquals("456", CodeInfos.AggregatedDatas);

			CodeInfos.AddOrUpdateExisting("CBA", "456");
			AssertEquals("ABC/CBA", CodeInfos.AggregatedCodes);
			AssertEquals("456/456", CodeInfos.AggregatedDatas);

			CodeInfos.AddOrUpdateExisting("", "789");
			AssertEquals("ABC/CBA", CodeInfos.AggregatedCodes);
			AssertEquals("456/456", CodeInfos.AggregatedDatas);
		}

		public void TestDoNotReSerialiseWhenHasChangesChangesToFalse()
		{
			var collection = CollectionAgainstDeclaration;

			if (collection == null)
			{
				Assert(true);
			}
			else
			{
				var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
				Declaration.JE_JS = shipment.PK;
				shipment.RegisterEditableChildObject(Declaration);

				var codeInfo = collection.AddNew();
				codeInfo.ZO_Code = "AA";
				using (codeInfo.SuspendSettingHasChanges())
				{
					codeInfo.ZO_Data = "BB";
				}

				AssertNoExceptionThrown(delegate
				{ Factory.Save(); });
			}
		}

		public void TestAggregatedCodes()
		{
			Assert("PreCondition:CodeInfos is empty", CodeInfos.Count == 0);
			CodeDataPair info = CodeInfos.AddNew();
			info.ZO_Code = "ZZZ";
			info.ZO_Data = "111";

			CodeDataPair info2 = CodeInfos.AddNew();
			info2.ZO_Code = "XXX";
			info2.ZO_Data = "222";

			AssertEquals("Aggregated codes", "ZZZ/XXX", CodeInfos.AggregatedCodes);
		}

		public void TestAggregatedDatas()
		{
			Assert("PreCondition:CodeInfos is empty", CodeInfos.Count == 0);
			CodeDataPair info = CodeInfos.AddNew();
			info.ZO_Code = "ZZZ";
			info.ZO_Data = "111";

			CodeDataPair info2 = CodeInfos.AddNew();
			info2.ZO_Code = "XXX";
			info2.ZO_Data = "222";

			AssertEquals("Aggregated Datas", "111/222", CodeInfos.AggregatedDatas);
		}

		public void TestDuplicateCodeIsError()
		{
			Classification.CC_ClassificationType = "SG4";
			CodeDataPair info = CodeInfos.AddNew();
			info.ZO_Code = info.ZO_CodeList[0].Code;
			AssertEquals("PreCondition:No error in code", false, info.ZO_CodeInfo.HasErrors());

			CodeDataPair info2 = CodeInfos.AddNew();
			AssertEquals("PreCondition:No error in Info2 code", false, info2.ZO_CodeInfo.HasErrors());

			info2.ZO_Code = info.ZO_Code;
			AssertEquals("Error if Duplicates not allowed", info.ShouldValidateForDuplicateCodes, info2.ZO_CodeInfo.HasError(info2.ZO_Code + " already exists."));

			info2.ZO_Code = "";
			AssertEquals("Back to no duplicate, hence no error", false, info2.ZO_CodeInfo.HasError(info2.ZO_Code + " already exists."));
		}

		public void TestLoadFromStringDoesntChangeHasChanges()
		{
			Assert("Precondition:Not changed yet", !CodeInfos.HasChanges);
			ZString addInfoString = "APE^APD=SOMEDATA";
			CodeInfos.LoadFromString(addInfoString);
			Assert("Not changed", !CodeInfos.HasChanges);
		}

		public void TestLoadFromString()
		{
			ZString addInfoString = "APE^APD=SOMEDATA";
			CodeInfos.LoadFromString(addInfoString);
			AssertEquals("Count", 2, CodeInfos.Count);

			AssertEquals("First instance Code", "APE", CodeInfos[0].ZO_Code);
			AssertEquals("First instance Data", ZString.Empty, CodeInfos[0].ZO_Data);
			AssertEquals("Second instance Code", "APD", CodeInfos[1].ZO_Code);
			AssertEquals("Second instance Data", "SOMEDATA", CodeInfos[1].ZO_Data);
		}

		public void TestToString()
		{
			CodeDataPair info1 = CodeInfos.AddNew();
			info1.ZO_Code = ZString.Empty;

			CodeDataPair info2 = CodeInfos.AddNew();
			info2.ZO_Code = "APE";

			CodeDataPair info3 = CodeInfos.AddNew();
			info3.ZO_Code = "APD";
			info3.ZO_Data = "SOMEDATA";

			AssertEquals("ToString()", "APE^APD=SOMEDATA", CodeInfos.ToString());
		}

		public void TestUpdateRelatedAddInfoString()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "123-456-789";
			CodeDataPair code = classification.PermitCodes.AddNew();
			code.ZO_Code = "XXX";
			Factory.Save();
			AssertEquals("AddInfo string", "PermitCodes=XXX", classification.CC_AddInfo);

			code.ZO_Data = "123";
			Factory.Save();
			AssertEquals("AddInfo string", "PermitCodes=XXX=123", classification.CC_AddInfo);

			classification.PermitCodes.RemoveAndDelete(code);
			Factory.Save();
			AssertEquals("AddInfo string", ZString.Empty, classification.CC_AddInfo);
		}

		public void TestDeclarationWhenAgainstDeclaration()
		{
			CodeDataPairCollection collection = CollectionAgainstDeclaration;
			if (collection != null)
			{
				AssertEquals("Collection.Declaration", Declaration, collection.Declaration);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		public void TestDeclarationWhenAgainstInvoiceLine()
		{
			CodeDataPairCollection collection = CollectionAgainstInvoiceLine;
			if (collection != null)
			{
				AssertEquals("Collection.Declaration", Declaration, collection.Declaration);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		public void TestParentDeclarationWhenAgainstDeclaration()
		{
			CodeDataPairCollection collection = CollectionAgainstDeclaration;
			if (collection != null)
			{
				AssertEquals("Collection.ParentDeclaration", Declaration, collection.ParentDeclaration);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		public void TestParentDeclarationWhenAgainstInvoiceLine()
		{
			CodeDataPairCollection collection = CollectionAgainstInvoiceLine;
			if (collection != null)
			{
				AssertEquals("Collection.ParentDeclaration", null, collection.ParentDeclaration);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		public void TestParentInvoiceLineWhenAgainstDeclaration()
		{
			CodeDataPairCollection collection = CollectionAgainstDeclaration;
			if (collection != null)
			{
				AssertEquals("Collection.ParentInvoiceLine", null, collection.ParentInvoiceLine);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		public void TestParentInvoiceLineWhenAgainstInvoiceLine()
		{
			CodeDataPairCollection collection = CollectionAgainstInvoiceLine;
			if (collection != null)
			{
				AssertEquals("Collection.ParentInvoiceLine", InvoiceLine, collection.ParentInvoiceLine);
			}
			else
			{
				Assert("Not Valid In This Context", condition: true);
			}
		}

		#region Implementation
		protected CodeDataPairCollection CodeInfos
		{
			get
			{
				if (fCodeInfos == null)
				{
					fCodeInfos = GetCollectionToTest();
					AssertEquals("Concrete GetCollectionToTest() needs to pass in Classification as second parameter.", Classification, fCodeInfos.Parent);
				}
				return fCodeInfos;
			}
		}
		CodeDataPairCollection fCodeInfos;

		protected CusClassification Classification
		{
			get
			{
				if (fClassification == null)
				{
					fClassification = Factory.New<CusClassification>();
				}
				return fClassification;
			}
		}
		CusClassification fClassification;

		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
					fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					JobComInvoiceHeader invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
					fInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		protected abstract CodeDataPairCollection CollectionAgainstDeclaration { get; }
		protected abstract CodeDataPairCollection CollectionAgainstInvoiceLine { get; }
		#endregion
	}
}
