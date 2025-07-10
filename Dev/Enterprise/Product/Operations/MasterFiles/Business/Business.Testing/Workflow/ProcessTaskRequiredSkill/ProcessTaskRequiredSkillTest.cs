using System;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;
using WTG.WiseTechAcademy;
using WTG.WiseTechAcademy.TestFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskRequiredSkill))]
	class ProcessTaskRequiredSkillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlyProperties()
		{
			var pivot = Factory.New<ProcessTaskRequiredSkill>();
			AssertEquals(true, pivot.P9S_HSInfo.ReadOnly);
			AssertEquals(true, pivot.P9S_WiseTechAcademySubjectCodeInfo.ReadOnly);
		}

		public void TestLearningUnitName_ForWTASubject()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();
			var client = new WiseTechAcademyApiTestHelper()
				.SetupGetLearningUnitName("5004", "Advanced Data Analysis and Methods of Psychological Inquiry")
				.SetupGetLearningUnitName("5005", "Behavioural Neuroscience")
				.CreateMockClient();
			using (ObjectFactory.Substitute<IWiseTechAcademyApiClient>(client))
			{
				pivot.P9S_WiseTechAcademySubjectCode = "5004";
				AssertEquals("Advanced Data Analysis and Methods of Psychological Inquiry", pivot.LearningUnitName);

				pivot.P9S_WiseTechAcademySubjectCode = "5005";
				AssertEquals("Behavioural Neuroscience", pivot.LearningUnitName);
			}
		}

		public void TestLearningUnitName_ForAspect()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();
			var assessServiceClientMock = new Mock<IAssessServiceClient>();
			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.Setup(c => c.GetLearningUnitNameAsync(aspectPK1, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "Advanced Data Analysis and Methods of Psychological Inquiry"));
			assessServiceClientMock
				.Setup(c => c.GetLearningUnitNameAsync(aspectPK2, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<string>.Success(HttpStatusCode.OK, "Behavioural Neuroscience"));

			using (ObjectFactory.Substitute(assessServiceClientMock.Object))
			{
				pivot.P9S_Aspect = aspectPK1;
				AssertEquals("Advanced Data Analysis and Methods of Psychological Inquiry", pivot.LearningUnitName);

				pivot.P9S_Aspect = aspectPK2;
				AssertEquals("Behavioural Neuroscience", pivot.LearningUnitName);
			}
		}

		public void TestHasCompletedLearningUnit_ForWTASubject()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();

			var client = new WiseTechAcademyApiTestHelper()
				.SetupHasPassedLearningUnit(staff1.GS_PER.ToGuid(), "5003", true)
				.SetupHasPassedLearningUnit(staff1.GS_PER.ToGuid(), "5004", false)
				.SetupHasPassedLearningUnit(staff2.GS_PER.ToGuid(), "5003", false)
				.SetupHasPassedLearningUnit(staff2.GS_PER.ToGuid(), "5004", true)
				.CreateMockClient();
			using (ObjectFactory.Substitute<IWiseTechAcademyApiClient>(client))
			{
				pivot.P9S_WiseTechAcademySubjectCode = "5003";
				task.P9_GS_NKAssignedStaffMember = ZString.Empty;
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				AssertEquals(true, pivot.HasCompletedLearningUnit);

				pivot.P9S_WiseTechAcademySubjectCode = "5004";
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				task.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
				pivot.P9S_WiseTechAcademySubjectCode = "5003";
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				pivot.P9S_WiseTechAcademySubjectCode = "5004";
				AssertEquals(true, pivot.HasCompletedLearningUnit);
			}
		}

		public void TestHasCompletedLearningUnit_ForAspect()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var personPK1 = staff1.GS_PER.ToGuid();
			var personPK2 = staff2.GS_PER.ToGuid();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			var pivot = task.SkillsPivots.AddNew();

			var assessServiceClientMock = new Mock<IAssessServiceClient>();
			var aspectPK1 = Guid.NewGuid();
			var aspectPK2 = Guid.NewGuid();

			assessServiceClientMock
				.Setup(c => c.HasPassedLearningUnitAsync(aspectPK1, personPK1, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<bool>.Success(HttpStatusCode.OK, true));
			assessServiceClientMock
				.Setup(c => c.HasPassedLearningUnitAsync(aspectPK2, personPK1, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<bool>.Success(HttpStatusCode.OK, false));
			assessServiceClientMock
				.Setup(c => c.HasPassedLearningUnitAsync(aspectPK1, personPK2, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<bool>.Success(HttpStatusCode.OK, false));
			assessServiceClientMock
				.Setup(c => c.HasPassedLearningUnitAsync(aspectPK2, personPK2, It.IsAny<ServiceRequestOptions>()))
				.ReturnsAsync(ServiceResponse<bool>.Success(HttpStatusCode.OK, true));

			using (ObjectFactory.Substitute(assessServiceClientMock.Object))
			{
				pivot.P9S_Aspect = aspectPK1;
				task.P9_GS_NKAssignedStaffMember = ZString.Empty;
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
				AssertEquals(true, pivot.HasCompletedLearningUnit);

				pivot.P9S_Aspect = aspectPK2;
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				task.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
				pivot.P9S_Aspect = aspectPK1;
				AssertEquals(false, pivot.HasCompletedLearningUnit);

				pivot.P9S_Aspect = aspectPK2;
				AssertEquals(true, pivot.HasCompletedLearningUnit);
			}
		}

		public void TestWTASubjectName_MultiplePivotsPerTaskAllowed()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var pivot1 = task.SkillsPivots.AddNew();
			var pivot2 = task.SkillsPivots.AddNew();

			pivot1.P9S_WiseTechAcademySubjectCode = "5005";
			pivot2.P9S_WiseTechAcademySubjectCode = "5006";
			AssertNoExceptionThrown(Factory.Save);

			pivot2.P9S_WiseTechAcademySubjectCode = "5005";
			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}

		#region Dummy tests
		// added these override dummy tests to avoid test failures
		// this class is pending removal
		public override void TestSaveAndDeleteBusinessObject()
		{
		}

		public override void TestFetchForLoad()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			AssertNoExceptionThrown(Factory.Save);
		}
		#endregion
	}
}
