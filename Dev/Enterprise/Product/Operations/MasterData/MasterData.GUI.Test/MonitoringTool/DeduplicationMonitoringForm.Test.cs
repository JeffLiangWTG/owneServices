using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationMonitoringFormTest : TestCase
	{
		public void TestShowImportButtonCorrectly()
		{
			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var form = new DeduplicationMonitoringForm())
			{
				form.Show();
				Assert(form.ImportButton.Visible);
			}

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.DHL))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var form = new DeduplicationMonitoringForm())
			{
				form.Show();
				Assert(!form.ImportButton.Visible);
			}
		}

		public void TestRegisterAndRemoveParticipant()
		{
			AssertNull("Precondition", DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));
			using (var form = new DeduplicationMonitoringForm())
			{
				form.Show();
				AssertEquals(form, DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));

				form.Close();
				AssertNull(DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));
			}
		}

		string OrgPrefix => DeduplicationDebuggerParticipant.OrganizationPrefix;
		string PerPrefix => DeduplicationDebuggerParticipant.PersonPrefix;

		public void TestFilterCheckedChange()
		{
			try
			{
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();

					DeduplicationMonitoringForm.MonitoringObjects.TryAdd("Org_1", new MonitoringObjectValue());
					DeduplicationMonitoringForm.MonitoringObjects.TryAdd("Per_2", new MonitoringObjectValue());
					form.AddNodes();
					AssertFilteredResult(form, "Org_1", "Per_2");

					var filterOrg = form.FilterDropdownList.Items.Cast<ZString>().IndexOf((item) =>
					{
						form.DeduplicationTypeDict.TryGetValue(item, out var tag);
						return tag == OrgPrefix;
					});
					var filterPer = form.FilterDropdownList.Items.Cast<ZString>().IndexOf((item) =>
					{
						form.DeduplicationTypeDict.TryGetValue(item, out var tag);
						return tag == PerPrefix;
					});

					form.FilterDropdownList.SetItemChecked(filterOrg, false);
					AssertFilteredResult(form, "Per_2");

					form.FilterDropdownList.SetItemChecked(filterPer, false);
					AssertFilteredResult(form);

					form.FilterDropdownList.SetItemChecked(filterOrg, true);
					AssertFilteredResult(form, "Org_1");
				}
			}
			finally
			{
				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		public void TestFilterWhenReceive()
		{
			try
			{
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();
					form.ToggleButton.PerformClick();

					var filterOrg = form.FilterDropdownList.Items.Cast<ZString>().IndexOf((item) =>
					{
						form.DeduplicationTypeDict.TryGetValue(item, out var tag);
						return tag == OrgPrefix;
					});
					var filterPer = form.FilterDropdownList.Items.Cast<ZString>().IndexOf((item) =>
					{
						form.DeduplicationTypeDict.TryGetValue(item, out var tag);
						return tag == PerPrefix;
					});
					form.FilterDropdownList.SetItemChecked(filterOrg, false);
					form.Receive(new object(), "Org", new TimeSpan(0));
					form.Receive(new object(), "Per", new TimeSpan(0));
					AssertFilteredResult(form, "Per_2");

					form.FilterDropdownList.SetItemChecked(filterPer, false);
					form.Receive(new object(), "Org", new TimeSpan(0));
					form.Receive(new object(), "Per", new TimeSpan(0));
					AssertFilteredResult(form);

					form.FilterDropdownList.SetItemChecked(filterOrg, true);
					form.Receive(new object(), "Org", new TimeSpan(0));
					form.Receive(new object(), "Per", new TimeSpan(0));
					AssertFilteredResult(form, "Org_1", "Org_3", "Org_5");

					form.FilterDropdownList.SetItemChecked(filterPer, true);
					AssertFilteredResult(form, "Org_1", "Org_3", "Org_5", "Per_2", "Per_4", "Per_6");
				}
			}
			finally
			{
				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		public void TestShowHideDropdownList()
		{
			using (var form = new DeduplicationMonitoringForm())
			{
				form.Show();
				AssertEquals(false, form.FilterDropdownList.Visible);

				form.FilterButton.PerformClick();
				AssertEquals(true, form.FilterDropdownList.Visible);

				form.FilterButton.PerformClick();
				AssertEquals(false, form.FilterDropdownList.Visible);
			}
		}

		void AssertFilteredResult(DeduplicationMonitoringForm form, params string[] keys)
		{
			var model = form.DeduplicationMonitoringUserControl.model;
			AssertNotNull(model);

			if (keys != null && keys.Any())
			{
				AssertContainsExactElementsInAnyOrder(keys, model.monitoringObject.Select(u => u.Key));
			}
			else
			{
				AssertEquals(0, model.monitoringObject.Count);
			}
		}
	}

	public class DeduplicationMonitoringFormTestWithFacotry : TestCaseWithFactory
	{
		public void TestExportResultCorrectly()
		{
			try
			{
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();
					form.ToggleButton.PerformClick();

					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
					form.Receive(new List<IOrgHeader>() { new DeduplicationOrgHeader(orgHeader) }, "PopulateTargetGlows_TargetGlows", new TimeSpan());
					form.Receive(new List<IGlbPerson>() { glbPerson.CreateIGlbPerson() }, "PopulateTargetGlows_TargetGlows", new TimeSpan());

					AssertNoExceptionThrown(() => SaveMonitoringObjectToFileUtils.Serializer(DeduplicationMonitoringForm.MonitoringObjects));
				}
			}
			finally
			{
				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		public void TestExportDiagnosticButton()
		{
			try
			{
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();
					Assert(!form.ExportDiagnosticsButton.Enabled);

					form.ToggleButton.PerformClick();
					Assert(!form.ExportDiagnosticsButton.Enabled);

					var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
					form.Receive(new List<IGlbPerson>() { glbPerson.CreateIGlbPerson() }, "Per_PopulateTargetGlows_TargetGlows", new TimeSpan());
					Assert(form.ExportDiagnosticsButton.Enabled);

					form.ExportDiagnosticsButton.PerformClick();
					AssertEquals("This function is only available for organization duplicate detection.", UnitTestUserNotification.Instance.LastMessage.Text);

					form.Receive(new List<IOrgHeader>(), "Org_PopulateTargetGlows_TargetGlows", new TimeSpan());
					Assert(form.ExportDiagnosticsButton.Enabled);

					form.ExportDiagnosticsButton.PerformClick();
					AssertEquals("There is no available data to export.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		public void TestExportDiagnosticToNativeXML()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var fileLocator = mocks.Create<ISaveFileLocator>();
			var serializer = mocks.Create<IBusinessSerializer>();
			var validator = mocks.Create<IExportValidator>();
			var logger = mocks.Create<IUserNotification>();
			var businessObject = mocks.Create<IBusiness>();
			var fileName = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString());
			var exporter = new NativeXmlExportService
			{
				Serializer = serializer.Object,
				Validator = validator.Object,
				FileLocator = fileLocator.Object,
				Logger = logger.Object
			};

			try
			{
				UnitTestUserNotification.Instance.ClearMessages();
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();
					form.exportService = exporter;
					form.ToggleButton.PerformClick();

					using (var stream = File.OpenWrite(fileName))
					{
						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						Factory.Save();
						form.Receive(new List<IOrgHeader>() { new DeduplicationOrgHeader(orgHeader) }, "Org_PopulateTargetGlows_TargetGlows", new TimeSpan());
						Assert(form.ExportDiagnosticsButton.Enabled);

						fileLocator.Setup(m => m.GetFileStream(new[] { businessObject.Object }, out fileName)).Returns(stream);
						serializer.Setup(m => m.Export(new[] { businessObject.Object }, null, null)).Returns(stream);

						form.ExportDiagnosticsButton.PerformClick();
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(true, File.Exists(fileName));
						stream.Close();
					}
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
				{
					File.Delete(fileName);
				}

				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		public void TestImportMonitoringObjectsFromJsonFile()
		{
			try
			{
				using (var form = new DeduplicationMonitoringForm())
				{
					form.Show();
					form.ToggleButton.PerformClick();

					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.MainAddress.OA_Address1 = "TEST ADDRESS 1";
					orgHeader.MainAddress.AddressCapability.AddNew();
					orgHeader.BrandsOrRelatedNames.AddNew();
					orgHeader.OrgWebURLs.AddNew();
					orgHeader.CustomsCodes.AddNew();
					var contact = orgHeader.Contacts.AddNew();
					contact.ContactItems.AddNew();

					var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
					glbPerson.Certificates.AddNew();
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					var branch = Factory.NewWithValidTestData<GlbBranch>();
					branch.GB_OH_OrgProxy = orgHeader.PK;
					staff.GS_PER = glbPerson.PK;
					staff.GS_GB_HomeBranch = branch.PK;
					var applicant = Factory.New<Integration.Recruiter.IHRJobApplicant>();
					applicant.HA_PER = glbPerson.PK;

					var findPotentialTargetsResult = new HashSet<PatternMatchingResultModel>()
					{
						new PatternMatchingResultModel() { CountryCode = "AU" }
					}.AsEnumerable();

					var findPotentialTargetsResultFromGP1 = new HashSet<PatternMatchingResultModel>()
					{
						new PatternMatchingResultModel() { CountryCode = "AU" }
					}.AsEnumerable().Where(u => u.CountryCode != null); // Mock the file from very old GP1 version

					form.Receive(new List<IOrgHeader>() { new DeduplicationOrgHeader(orgHeader) }, "PopulateTargetGlows_TargetGlows", new TimeSpan());
					form.Receive(new List<IGlbPerson>() { glbPerson.CreateIGlbPerson() }, "PopulateTargetGlows_TargetGlows", new TimeSpan());
					form.Receive(findPotentialTargetsResult, "FindPotentialTargets", new TimeSpan());
					form.Receive(findPotentialTargetsResultFromGP1, "FindPotentialTargetsFromGP1", new TimeSpan());

					AssertImportResult(DeduplicationMonitoringForm.MonitoringObjects, form, out var result1);
					AssertImportResult(result1, form, out _);
				}
			}
			finally
			{
				DeduplicationMonitoringForm.MonitoringObjects = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Testing")]
		void AssertImportResult(ConcurrentDictionary<string, MonitoringObjectValue> keyValuePairs, DeduplicationMonitoringForm form, out ConcurrentDictionary<string, MonitoringObjectValue> result)
		{
			MemoryStream memoryStream = null;
			AssertNoExceptionThrown(() => memoryStream = SaveMonitoringObjectToFileUtils.Serializer(keyValuePairs));

			using (memoryStream)
			{
				memoryStream.Position = 0;
				using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
				{
					result = form.GetMonitoringObjectsFromJSONString(reader.ReadToEnd());
					AssertEquals(4, result.Count);
					AssertNotNull(result["PopulateTargetGlows_TargetGlows_1"]);
					AssertNotNull(result["PopulateTargetGlows_TargetGlows_2"]);
					AssertNotNull(result["FindPotentialTargets_3"]);
					AssertNotNull(result["FindPotentialTargetsFromGP1_4"]);
				}
			}
		}

		public void TestFilterDropdownListTabIndex()
		{
			using (var form = new DeduplicationMonitoringForm())
			{
				form.Show();
				Assert("FilterDropdownList requires a larger TabIndex than DeduplicationMonitoringUserControl to assure its correct rendering order in Winzor", form.FilterDropdownList.TabIndex > form.DeduplicationMonitoringUserControl.TabIndex);
			}
		}
	}
}
