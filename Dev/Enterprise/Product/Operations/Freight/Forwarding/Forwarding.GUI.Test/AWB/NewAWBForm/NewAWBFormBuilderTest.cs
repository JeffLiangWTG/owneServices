using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	public class NewAWBFormBuilderTest : BaseFreightTest
	{
		#region TestSelectCarrierMAWB

		public void TestSelectCarrierMAWB()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			NewAWBFormBuilderForReflection builder = new NewAWBFormBuilderForReflection();

			ZFormModaliser.ShowDialogsInTest = true;
			try
			{
				builder.ShowDialog(consol);
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = false;
			}

			AssertEquals(false, consol.JK_IsNeutralMaster);
		}

		public class NewAWBFormBuilderForReflection : NewAWBFormBuilder
		{
			protected override NewAWBForm CreateAWBForm(JobMawb mawb)
			{
				var awbForm = base.CreateAWBForm(mawb);
				awbForm.Shown += new EventHandler(awbForm_Shown);
				return awbForm;
			}

			void awbForm_Shown(object sender, EventArgs e)
			{
				FieldInfo jM_IsPaperBoundRadioButtonInfo = typeof(NewAWBForm).GetField("JM_IsPaperBoundRadioButton", BindingFlags.NonPublic | BindingFlags.Instance);
				ZRadioButton button = (ZRadioButton)jM_IsPaperBoundRadioButtonInfo.GetValue(sender);
				button.Checked = true;
			}
		}

		#endregion

		#region MasterBillMAWB_CreatedFromNewAWBFormBuilder

		[RequiresSTA]
		public void TestShowDialog_ConsolIsSavedAfterMAWBFormIsSaved()
		{
			var consol = GetConsol();

			using (var mawbForm = new NewAWBFormBuilderForTest())
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					mawbForm.MawbFormForTesting.Shown += (s, e) => mawbForm.MawbFormForTesting.OkButton.PerformClick();
				});

				mawbForm.ShowDialog(consol);
			}

			Assert("Consol should be IsNeutralMaster", consol.JK_IsNeutralMaster);
			AssertEquals("Consol master bill number should still be 081", "081", consol.MasterBillAirlinePrefix);
			AssertEquals("Consol master bill number should default from the MAWB form", "00000000", consol.MasterBillMAWB);
			AssertEquals("Consol master bill number should default from the MAWB form", "08100000000", consol.JK_MasterBillNum);

			AssertNotNull("Expected Allocated MAWB on the consol", consol.MAWBAllocation.AllocatedMawb);
			AssertNotNull("Expected Allocated MAWB is saved", consol.MAWBAllocation.AllocatedMawb.IsInDatabase);
			AssertEquals("Should not be pending allocation", false, consol.MAWBAllocation.GetShouldAllocate());
			AssertEquals("Should not be pending deallocation", false, consol.MAWBAllocation.GetShouldDeallocate());

			Factory.Save();

			var mawb = Factory.LoadTop1<JobMawb>(new ZQuery());
			mawb.Reload();
			AssertEquals("Expected mawb to be linked to the consol", consol.PK, mawb.JM_ParentID);
			AssertEquals("Expected mawb to be linked to the consol", consol.MAWBAllocation.AllocatedMawb, mawb);
			AssertEquals("Expected mawb to be linked to the consol", consol.JK_MasterBillNum, "08100000000");
		}

		public void TestShowDialog_ConsolNotSavedAfterMAWBFormIsSaved()
		{
			var consol = GetConsol();
			Factory.Save();

			var existingMawb = Factory.LoadTop1<JobMawb>(new ZQuery());
			AssertNull("Pre-condition: Expected no mawbs to already exist", existingMawb);
			AssertEquals("Pre-condition: consol MAWB should just be the airline prefix entered", "081", consol.JK_MasterBillNum);
			AssertEquals("Pre-condition: consol should not be neutral master by default", false, consol.JK_IsNeutralMaster);

			using (var mawbForm = new NewAWBFormBuilderForTest())
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					mawbForm.MawbFormForTesting.Shown += (s, e) => mawbForm.MawbFormForTesting.OkButton.PerformClick();
				});

				mawbForm.ShowDialog(consol);
			}

			var mawb = Factory.LoadTop1<JobMawb>(new ZQuery());
			AssertNotNull("Expected to find mawb in factory", mawb);
			AssertEquals("Expected mawb to be linked to the consol.", mawb, consol.MAWBAllocation.AllocatedMawb);

			AssertEquals("Consol master bill number should default from the MAWB form", "08100000000", consol.JK_MasterBillNum);
			AssertEquals("Consol should be is neutral master", true, consol.JK_IsNeutralMaster);

			var newFactory = new BusinessObjectFactory();
			var reloadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			var reloadedMawb = newFactory.Load<JobMawb>(mawb.PK);

			AssertEquals("Expected consol fields to be reset", "081", reloadedConsol.JK_MasterBillNum);
			AssertEquals("Expected consol fields to be reset", false, reloadedConsol.JK_IsNeutralMaster);
			AssertNotNull("Expected mawb to have found", reloadedMawb);
			AssertNull("Expected mawb not to have a parent", reloadedMawb.Parent);

			Factory.Save();
		}

		[RequiresSTA]
		public void TestShowDialog_AllocateMawb_AllocatesCreatedMawb()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "00000000";
			mawb.JM_ServiceLevel = "STD";
			Factory.Save();

			var consol = GetConsol();
			JobMawb createdMawb = null;

			using (var mawbForm = new NewAWBFormBuilderForTest("00000081"))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					mawbForm.MawbFormForTesting.Shown += (s, e) =>
					{
						createdMawb = (JobMawb)mawbForm.MawbFormForTesting.BusinessEntity;
						mawbForm.MawbFormForTesting.OkButton.PerformClick();
					};
				});

				mawbForm.ShowDialog(consol);
			}

			AssertNotNull("mawb has been created", createdMawb);
			AssertEquals("created mawb has no errors", false, createdMawb.HasErrors);
			AssertEquals("created mawb has been saved", true, createdMawb.IsInDatabase);
			AssertEquals("created mawb allocation is not persisted", ZGuid.Empty, createdMawb.JM_ParentID);
			AssertEquals("created mawb allocation is not persisted", ZString.Empty, createdMawb.JM_ParentTableCode);

			Assert("Consol should be IsNeutralMaster", consol.JK_IsNeutralMaster);
			AssertEquals("Consol master bill number should default from the MAWB form", "00000081", consol.MasterBillMAWB);
			AssertEquals("Consol master bill number should default from the MAWB form", "08100000081", consol.JK_MasterBillNum);

			AssertNotNull("Allocated MAWB", consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("Allocated MAWB number", "00000081", consol.MAWBAllocation.AllocatedMawb.JM_MAWB);
			AssertEquals("Allocated MAWB PK", createdMawb.PK, consol.MAWBAllocation.AllocatedMawb.PK);

			Factory.Save();
		}

		[RequiresSTA]
		public void TestShowDialog_FormCancelled()
		{
			var consol = GetConsol();

			using (var mawbForm = new NewAWBFormBuilderForTest())
			{
				mawbForm.ShowDialog(consol);
			}

			AssertEquals("Consol Master Bill number should remain blank", ZString.Empty, consol.MasterBillMAWB);
			AssertEquals("Consol IsNeutralMaster should remain false", false, consol.JK_IsNeutralMaster);
		}

		ForwardingConsol GetConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = OrgCarrierServiceLevel.StandardCode;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_RL_NKLoadPort = "AUBNE";

			return consol;
		}

		class NewAWBFormBuilderForTest : NewAWBFormBuilder, IDisposable
		{
			public NewAWBFormBuilderForTest(string mawbNumber = "00000000")
			{
				this.mawbNumber = mawbNumber;
			}

			readonly ZString mawbNumber;

			protected override NewAWBForm CreateAWBForm(JobMawb mawb)
			{
				mawb.JM_MAWB = mawbNumber;

				return MawbFormForTesting ?? (MawbFormForTesting = base.CreateAWBForm(mawb));
			}

			public NewAWBForm MawbFormForTesting;

			#region IDisposable Members

			public void Dispose()
			{
				if (MawbFormForTesting != null)
				{
					MawbFormForTesting.Dispose();
				}
			}

			#endregion
		}

		#endregion

		#region TestNewEventHandler

		[ExpectNoExceptions]
		public void TestNewEventHandler()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			EventHandler eventHandler = NewAWBFormBuilder.NewEventHandler(parentMock.Object);

			parentMock.Setup(m => m.IsAir).Returns(false);
			eventHandler(null, null);
			parentMock.VerifyAll();
		}

		#endregion

		#region Mock Testing ShowDialog

		#region TestShowDialog_PermitionDenied

		public void TestShowDialog_PermitionDenied()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			string error = @"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Reference Files -> MAWB Stock -> Edit";

			Env.Security.JobMAWBModify.IsAllowed = false;
			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertMultilineASCIIEquals("Expecting correct dialog message.", error, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
		}

		#endregion

		#region TestShowDialog_NonAir

		public void TestShowDialog_NonAir()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.IsAir).Returns(false);

			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("Question This option only applies to direct, agent, coload AWB or gateway agent air consols with local Load Port.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_NotDirectOrAgent

		public void TestShowDialog_NotDirectOrAgent()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(false);
			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("Question This option only applies to direct, agent, coload AWB or gateway agent air consols with local Load Port.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_InvalidPrefix

		public void TestShowDialog_InvalidPrefix()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.Factory).Returns(Factory);

			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parentMock.Setup(m => m.MasterBillAirlinePrefix).Returns("XXX");

			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("Question You must first enter a valid airline prefix.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
			parentMock.Verify(m => m.MasterBillAirlinePrefix, Times.Exactly(2));
			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_MissingPrefix

		public void TestShowDialog_MissingPrefix()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parentMock.Setup(m => m.MasterBillAirlinePrefix).Returns("");

			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("Question You must first enter a valid airline prefix.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);

			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_ExistingMawb

		public void TestShowDialog_ExistingMawb()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.Factory).Returns(Factory);

			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081"));
			ZGuid guid = ZGuid.NewZGuid();

			JobMawb existingMawb = Factory.New<JobMawb>();
			existingMawb.JM_ParentID = guid;
			existingMawb.JM_ParentTableCode = "Z0";

			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parentMock.Setup(m => m.MasterBillAirlinePrefix).Returns(airline.RM_EagleAddedAirlinePrefixOrAccountingCode);
			parentMock.Setup(m => m.PK).Returns(guid);
			parentMock.Setup(m => m.Prefix).Returns("Z0");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("Question This consol already has a MAWB allocated.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull("Should not have shown the dialog", ZFormModaliser.LastFormShownDialogForTest);
			parentMock.Verify(m => m.MasterBillAirlinePrefix, Times.Exactly(2));
			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_Canceled

		[RequiresSTA]
		public void TestShowDialog_Canceled()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.Factory).Returns(Factory);

			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081"));

			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parentMock.Setup(m => m.MasterBillAirlinePrefix).Returns(airline.RM_EagleAddedAirlinePrefixOrAccountingCode);
			parentMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			parentMock.Setup(m => m.Prefix).Returns("Z0");
			parentMock.Setup(m => m.AWBServiceLevel).Returns("ALL");
			parentMock.Setup(m => m.MasterBillMAWB).Returns("00000000");

			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNotNull("Should have shown a dialog", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("should have shown the correct dialog", typeof(NewAWBForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			JobMawb mawb = (JobMawb)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;

			AssertNotNull(mawb);
			AssertNotEquals("Should be using a new factory for the dialog", Factory, mawb.Factory);
			AssertEquals("JM_ParentID", ZGuid.Empty, mawb.JM_ParentID);
			AssertEquals("JM_ParentTableCode", "", mawb.JM_ParentTableCode);
			AssertEquals("JM_ServiceLevel", "ALL", mawb.JM_ServiceLevel);
			AssertEquals("JM_Airline3DigitPrefix", airline.RM_EagleAddedAirlinePrefixOrAccountingCode, mawb.JM_Airline3DigitPrefix);
			AssertEquals("JM_MAWB", "00000000", mawb.JM_MAWB);
			AssertEquals("JM_Airline3DigitPrefixInfo.ReadOnly", true, mawb.JM_Airline3DigitPrefixInfo.ReadOnly);

			parentMock.Verify(m => m.MasterBillAirlinePrefix, Times.Exactly(3));
			parentMock.Verify(m => m.AWBServiceLevel, Times.Exactly(6));
			parentMock.VerifyAll();
		}

		#endregion

		#region TestShowDialog_Success

		[RequiresSTA]
		public void TestShowDialog_Success()
		{
			var parentMock = new Mock<IMAWBAllocationParent>();
			parentMock.Setup(m => m.Factory).Returns(Factory);

			RefAirline airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081"));

			parentMock.Setup(m => m.IsAir).Returns(true);
			parentMock.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parentMock.Setup(m => m.MasterBillAirlinePrefix).Returns(airline.RM_EagleAddedAirlinePrefixOrAccountingCode);
			parentMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			parentMock.Setup(m => m.Prefix).Returns("Z0");
			parentMock.Setup(m => m.AWBServiceLevel).Returns(OrgCarrierServiceLevel.AllCode);
			parentMock.Setup(m => m.MasterBillMAWB).Returns("00000000");
			parentMock.Setup(m => m.IsNeutralMaster).Returns(ZBool.True);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			NewAWBFormBuilder.ShowDialog(parentMock.Object);

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNotNull("Should have shown a dialog", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("should have shown the correct dialog", typeof(NewAWBForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			JobMawb mawb = (JobMawb)((ZForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest;

			AssertNotNull(mawb);
			AssertNotEquals("Should be using a new factory for the dialog", Factory, mawb.Factory);
			AssertEquals("JM_ParentID", ZGuid.Empty, mawb.JM_ParentID);
			AssertEquals("JM_ParentTableCode", "", mawb.JM_ParentTableCode);
			AssertEquals("JM_ServiceLevel", OrgCarrierServiceLevel.AllCode, mawb.JM_ServiceLevel);
			AssertEquals("JM_Airline3DigitPrefix", airline.RM_EagleAddedAirlinePrefixOrAccountingCode, mawb.JM_Airline3DigitPrefix);
			AssertEquals("JM_MAWB", "00000000", mawb.JM_MAWB);
			AssertEquals("JM_Airline3DigitPrefixInfo.ReadOnly", true, mawb.JM_Airline3DigitPrefixInfo.ReadOnly);

			parentMock.Verify(m => m.MasterBillAirlinePrefix, Times.Exactly(3));
			parentMock.Verify(m => m.AWBServiceLevel, Times.Exactly(6));
			parentMock.Verify(m => m.IsNeutralMaster, Times.Exactly(1));
			parentMock.VerifyAll();
		}

		#endregion

		#region Implementation

		#region NewAWBFormBuilder

		NewAWBFormBuilder NewAWBFormBuilder
		{
			get
			{
				if (newAWBFormBuilder == null)
				{
					newAWBFormBuilder = new NewAWBFormBuilder();
				}
				return newAWBFormBuilder;
			}
		}

		NewAWBFormBuilder newAWBFormBuilder;

		#endregion

		#endregion

		#endregion
	}
}
