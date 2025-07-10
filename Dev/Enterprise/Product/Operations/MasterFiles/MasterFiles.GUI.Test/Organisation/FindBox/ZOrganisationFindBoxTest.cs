using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ZOrganisationFindBoxTest : TestCaseWithDummy
	{
		public class ZOrganisationFindBoxForTest : ZOrganisationFindBox
		{
			public ZOrganisationFindBoxForTest() : base()
			{
			}

			public new EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
			{
				return base.CreateEmbeddedPopup(module);
			}

			public new bool AutoCompleteText(bool explicitAutoComplete)
			{
				return base.AutoCompleteText(explicitAutoComplete);
			}
		}

		[RequiresSTA]
		public void TestCreateEmbeddedPopup()
		{
			using (ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest())
			{
				TestCollection bOCollection = new TestCollection(Factory);
				testFindBox.List = bOCollection;

				DummyFilterGridModule testModule = new DummyFilterGridModule();

				bOCollection.AllowTemp = false;
				using (EmbeddedModulePopup testPopup1 = testFindBox.CreateEmbeddedPopup(testModule))
				{
					AssertEquals("Findbox should use regular popup", typeof(EmbeddedModulePopup), testPopup1.GetType());
				}

				bOCollection.AllowTemp = true;
				using (EmbeddedModulePopup testPopup2 = testFindBox.CreateEmbeddedPopup(testModule))
				{
					AssertEquals("Findbox should use temporary org popup", typeof(OrganisationEmdeddedModulePopup), testPopup2.GetType());
				}
			}
		}

		[ExpectException(typeof(ZOrganisationFindBoxException))]
		public void TestInvalidModuleID()
		{
			using (ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest())
			{
				testFindBox.ModuleID = ModuleIDs.JobShipment;
			}
		}

		[ExpectException(typeof(ZOrganisationFindBoxException))]
		public void TestInvalidBindToList()
		{
			using (ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest())
			{
				DummyBusinessObjectCollection dummyBOCollection = new DummyBusinessObjectCollection(Factory);
				testFindBox.List = dummyBOCollection;

				using (DummyFilterGridModule testModule = new DummyFilterGridModule())
				{
					testFindBox.CreateEmbeddedPopup(testModule);
				}
			}
		}

		public void TestShowTemporaryOrgPopup()
		{
			using (ZChildForm testForm = new ZChildForm())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				TestCollection collection = new TestCollection(factory);
				ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest();
				testFindBox.List = collection;

				testForm.Controls.Add(testFindBox);

				((ITemporaryOrganisationFindBox)testFindBox).ShowTemporaryOrgPopup(null);
				AssertNull("AllowNewTemporaryOrganisations is false, so popup should not get shown", ZFormModaliser.ActiveForm);

				collection.AllowTemp = true;
				((ITemporaryOrganisationFindBox)testFindBox).ShowTemporaryOrgPopup(null);
				TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
				try
				{
					AssertNotNull("AllowNewTemporaryOrganisations is false, so popup should get shown", popup);
				}
				finally
				{
					DisposeActiveForm();
				}
			}
		}

		public void TestNewTempOrgFactoryIsNotFormFactory()
		{
			using (ZChildForm testForm = new ZChildForm())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				TestCollection collection = new TestCollection(factory);
				collection.AllowTemp = true;

				ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest();
				testFindBox.List = collection;

				testForm.Controls.Add(testFindBox);

				((ITemporaryOrganisationFindBox)testFindBox).ShowTemporaryOrgPopup(null);
				TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
				try
				{
					AssertNotNull(popup);
					Assert("Popup should use a different factory to the collection", popup.BusinessEntity.Factory != factory);
				}
				finally
				{
					DisposeActiveForm();
				}
			}
		}

		[ExpectException(typeof(ZOrganisationFindBoxException))]
		public void TestListDoesNotImplementIOrgHeaderCollection()
		{
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);
			using (ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest())
			{
				testFindBox.List = dummyCollection;
			}
		}

		[ExpectNoExceptions()]
		public void TestNullList()
		{
			using (ZOrganisationFindBoxForTest testFindBox = new ZOrganisationFindBoxForTest())
			{
				testFindBox.List = null;
			}
		}

		[ExpectNoExceptions]
		public void TestAutoCompleteText_WhenCollectionIsNull()
		{
			using (var findBox = new ZOrganisationFindBoxForTest())
			{
				findBox.List = null;
				findBox.AutoCompleteText(false);
			}
		}

		#region TestUnmatchedOrganisation

		public void TestOrgFindBoxControlPressF3KeyWithUnmatchedOrg()
		{
			var dummyBusinessObject2 = Factory.New<DummyBusinessObjectWithLookups>();
			var helper = new UnmatchOrgRecordTestHelper(dummyBusinessObject2, ConsigneeRec, ConsignorRec, PickupAgentRec, ClientRec);

			var dummyBusinessObject = Factory.New<DummyBusinessObjectWithLookups>();
			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.ST_NoteType = "INT";
			stmNote.ST_ParentID = dummyBusinessObject.PK;
			stmNote.ST_Table = "DummyBizo";
			stmNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			stmNote.ST_NoteContext = "AAA";
			stmNote.ST_NoteText = helper.UnmatchOrgRecords.AsXml();
			Factory.Save();

			var unmatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "UNMATCHED"));

			AssertNotNull(unmatchedOrg);

			dummyBusinessObject.OrgPK = unmatchedOrg.PK;
			dummyBusinessObject2.OrgPK = unmatchedOrg.PK;

			TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(dummyBusinessObject, "OrgPK", "DummyPickupAgentList");
			TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(dummyBusinessObject, "OrgPK", "Lookups.DummyConsignorList");
			TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(dummyBusinessObject, "OrgPK", "DummyClientList");
			TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(dummyBusinessObject2, "OrgPK", "Lookups.DummyConsignorList");
			TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(dummyBusinessObject2, "OrgPK", "DummyConsigneeList");

			dummyBusinessObject.OrgPK = ZGuid.Empty;
			dummyBusinessObject2.OrgPK = ZGuid.Empty;

			helper.AssertOrgFieldDefaults(dummyBusinessObject.DummyPickupAgentList.DefaultsForNewChild, PickupAgentRec);
			helper.AssertOrgFieldDefaults(dummyBusinessObject.Lookups.DummyConsignorList.DefaultsForNewChild, ConsignorRec);
			helper.AssertOrgFieldDefaults(dummyBusinessObject.DummyClientList.DefaultsForNewChild, ClientRec);
			helper.AssertOrgFieldDefaults(dummyBusinessObject2.Lookups.DummyConsignorList.DefaultsForNewChild, ConsignorRec);
			helper.AssertOrgFieldDefaults(dummyBusinessObject2.DummyConsigneeList.DefaultsForNewChild, ConsigneeRec);
		}

		void TestOrgFindBoxControlPressF3KeyWithUnmatchedOrgWithdDataSourceAndDataMember(object dataSource, string dataMember, string bindToList)
		{
			using (var form = new ZForm(dataSource))
			{
				var orgFindBox = new ZOrganisationFindBoxForTest();
				orgFindBox.SetDataBinding(dataSource, dataMember);
				if (!string.IsNullOrEmpty(bindToList))
				{
					orgFindBox.BindToList = bindToList;
				}
				form.Controls.Add(orgFindBox);
				form.Show();

				orgFindBox.CodeBox.Focus();

				AssertEquals("UNMATCHED", orgFindBox.CodeBox.Text);
				KeySender.SendKeyDownToProcessCmdKey(orgFindBox.CodeBox, (int)Keys.F3);
				AssertEquals("should ask whether create a new org when press F3", "Would you like to create a new organization with details defaulted from 'Unmatched Org Details' Note?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		class DummyBusinessObjectWithLookups : DummyBusinessObject, IStmNoteParent
		{
			public DummyBusinessObjectWithLookups(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public ZGuid OrgPK
			{
				get { return orgPK; }
				set { orgPK = value; }
			}
			ZGuid orgPK = ZGuid.Empty;

			public OrganisationsFindBoxCollection DummyClientList
			{
				get
				{
					if (dummyClientList == null)
					{
						dummyClientList = new OrganisationsFindBoxCollection(Factory);
						dummyClientList.OrganisationType = OrganisationTypes.Debtor;
					}
					return dummyClientList;
				}
			}
			OrganisationsFindBoxCollection dummyClientList;

			[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
			public OrganisationsFindBoxCollection DummyConsigneeList
			{
				get { return dummyConsineeList ?? (dummyConsineeList = new OrganisationsFindBoxCollection(Factory)); }
			}
			OrganisationsFindBoxCollection dummyConsineeList;

			[OrganisationDefaultProvider(DocAddressType = "PAG")]
			public OrganisationsFindBoxCollection DummyPickupAgentList
			{
				get { return dummyPickupAgentList ?? (dummyPickupAgentList = new OrganisationsFindBoxCollection(Factory)); }
			}
			OrganisationsFindBoxCollection dummyPickupAgentList;

			public DummyLookupsWithOrganisationFindBoxCollection Lookups
			{
				get { return lookups ?? (lookups = new DummyLookupsWithOrganisationFindBoxCollection(this)); }
			}
			DummyLookupsWithOrganisationFindBoxCollection lookups;

			#region IStmNoteParent Members

			bool IStmNoteParent.IsInDatabase
			{
				get
				{
					return false;
				}
			}

			bool IStmNoteParent.IsDeleted
			{
				get
				{
					return false;
				}
			}

			Notes IStmNoteParent.Notes
			{
				get
				{
					if (notes == null)
					{
						notes = new Notes(this);
					}
					return notes;
				}
			}
			Notes notes;

			ZGuid IStmNoteParent.NotesParentPK
			{
				get
				{
					return PK;
				}
			}

			string IStmNoteParent.NotesParentTableName
			{
				get
				{
					return this.TableName;
				}
			}

			BusinessObjectFactory IStmNoteParent.NotesFactory
			{
				get
				{
					return Factory;
				}
			}

			GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate
			{
				get
				{
					throw new NotImplementedException();
				}

				set
				{
					throw new NotImplementedException();
				}
			}

			bool IStmNoteParent.SupportsNotes
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
			{
				get
				{
					return Array.Empty<BusinessObject>();
				}
			}

			StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			NoteTypeCollection IStmNoteParent.NoteTypes
			{
				get
				{
					return new NoteTypeCollection();
				}
			}

			#endregion
		}

		class DummyLookupsWithOrganisationFindBoxCollection : ZLookups
		{
			public DummyLookupsWithOrganisationFindBoxCollection(BusinessObject parent)
			: base(parent)
			{
			}

			[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
			public OrganisationsFindBoxCollection DummyConsignorList
			{
				get { return dummyConsignorList ?? (dummyConsignorList = new OrganisationsFindBoxCollection(Factory)); }
			}
			OrganisationsFindBoxCollection dummyConsignorList;
		}

		#region UnmatchedRecords

		UnmatchOrgRecord ConsigneeRec
		{
			get
			{
				if (consigneeRec == null)
				{
					consigneeRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"consignee addr 1",
						"consignee addr 2",
						"consigneeName",
						"2222",
						"NSW",
						"sydney",
						"consignee",
						"cneOwnerCode",
						"");
				}
				return consigneeRec;
			}
		}
		UnmatchOrgRecord consigneeRec;

		UnmatchOrgRecord ConsignorRec
		{
			get
			{
				if (consignorRec == null)
				{
					consignorRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"consignor addr 1",
						"consignor addr 2",
						"consignorName",
						"1111",
						"NSW",
						"city",
						"consignor",
						"CnrownerCode",
						"");
				}
				return consignorRec;
			}
		}
		UnmatchOrgRecord consignorRec;

		UnmatchOrgRecord ClientRec
		{
			get
			{
				if (clientRec == null)
				{
					clientRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Debtor,
						nameof(OrganisationTypes.Debtor),
						"client addr 1",
						"client addr 2",
						"clientName",
						"2222",
						"NSW",
						"sydney",
						"client",
						"cltOwnerCode",
						"");
				}
				return clientRec;
			}
		}
		UnmatchOrgRecord clientRec;

		UnmatchOrgRecord PickupAgentRec
		{
			get
			{
				if (pickupAgentRec == null)
				{
					pickupAgentRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.None,
						"None",
						"pickup addr 1",
						"pickup addr 2",
						"pickup agent Name",
						"2222",
						"NSW",
						"sydney",
						"pickup",
						"pagOwnerCode",
						"PAG");
				}
				return pickupAgentRec;
			}
		}
		UnmatchOrgRecord pickupAgentRec;

		#endregion

		#endregion

		#region Implementation

		class TestCollection : OrgHeaderCollection
		{
			public bool AllowTemp;

			public TestCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override bool AllowNewTemporaryOrganisations
			{
				get { return AllowTemp; }
			}
		}

		void DisposeActiveForm()
		{
			IDisposable formToDispose = ZFormModaliser.ActiveForm;
			if (formToDispose != null)
			{
				formToDispose.Dispose();
			}
		}

		#endregion
	}
}
