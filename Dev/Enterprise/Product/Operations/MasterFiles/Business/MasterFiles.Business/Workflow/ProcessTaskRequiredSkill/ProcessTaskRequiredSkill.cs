using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using WTG.DevTools.ServiceClient.Assess;
using WTG.WiseTechAcademy;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskRequiredSkill : AutoProcessTaskRequiredSkill
	{
		public ProcessTaskRequiredSkill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("HRJobSkill")]
		[List("HRJobSkill")]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject HRJobSkill (IHRJobSkill) is an interface")]
		public override ZGuid P9S_HS
		{
			get => base.P9S_HS;
			set => base.P9S_HS = value;
		}

		protected bool P9S_HS_ReadOnly => true;
		protected bool P9S_WiseTechAcademySubjectCode_ReadOnly => true;

		[ResourceStringData("ProcessTaskRequiredSkill.LearningUnitName", Caption = "Learning Unit Name", FullDescription = "The name of the learning unit required for this task", ShortCaption = "Unit Name")]
		public ZString LearningUnitName
		{
			get
			{
				if (P9S_Aspect is { IsValid: true } aspect)
				{
					var key = FormattableString.Invariant($"LearningUnitName_{aspect}");
					return Factory.GetCachedValue(key, () =>
					{
						var serviceResponse = AssessServiceClient.GetLearningUnitNameAsync(aspect.ToGuid()).GetAwaiter().GetResult();
						return serviceResponse.IsSuccess ? serviceResponse.Content : null;
					});
				}

				if (P9S_WiseTechAcademySubjectCode is { IsEmpty: false } unitId)
				{
					return WiseTechAcademyApiClient.GetLearningUnitNameOrPlaceholder(unitId);
				}
				return ZString.Empty;
			}
		}

		[ResourceStringData("ProcessTaskRequiredSkill.HasCompletedLearningUnit", Caption = "Has Completed Learning Unit", FullDescription = "Has the staff member assigned to this task completed the learning unit", ShortCaption = "Complete")]
		public ZBool HasCompletedLearningUnit
		{
			get
			{
				var assignedStaff = ProcessTask?.AssignedStaffMember;
				if (assignedStaff == null)
				{
					return ZBool.False;
				}

				var personPK = assignedStaff.GS_PER;

				if (personPK.IsValid && P9S_Aspect is { IsValid: true } aspectPK)
				{
					var serviceResponse = AssessServiceClient.HasPassedLearningUnitAsync(aspectPK.ToGuid(), personPK.ToGuid()).GetAwaiter().GetResult();
					return serviceResponse.IsSuccess && serviceResponse.Content;
				}

				if (P9S_WiseTechAcademySubjectCode is { IsEmpty: false } unitId && personPK.IsValid)
				{
					return CourseCompletionHelper.HasUserCompletedLearningUnit(personPK.ToGuid(), unitId, WiseTechAcademyApiClient);
				}
				return ZBool.False;
			}
		}

		IWiseTechAcademyApiClient WiseTechAcademyApiClient => wiseTechAcademyApiClient ??= ObjectFactory.Get<IWiseTechAcademyApiClient>();
		IWiseTechAcademyApiClient wiseTechAcademyApiClient;

		IAssessServiceClient AssessServiceClient => assessServiceClient ??= ObjectFactory.Get<IAssessServiceClient>();
		IAssessServiceClient assessServiceClient;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			P9S_WiseTechAcademySubjectCode = ZString.Empty;
			P9S_Aspect = ZGuid.NewZGuid();
		}
#endif
	}
}
