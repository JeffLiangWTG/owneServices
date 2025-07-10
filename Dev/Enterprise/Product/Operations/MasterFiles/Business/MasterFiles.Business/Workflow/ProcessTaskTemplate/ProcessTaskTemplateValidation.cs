using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskTemplateValidation : AutoProcessTaskTemplateValidation
	{
		public ProcessTaskTemplateValidation(AutoProcessTaskTemplate parent)
			: base(parent)
		{
		}

		#region Parent

		protected new ProcessTaskTemplate Parent
		{
			get { return (ProcessTaskTemplate)base.Parent; }
		}

		#endregion

		#region Workflow Type

		protected override void CheckP0_ProcessType()
		{
			base.CheckP0_ProcessType();

			MandatoryValidation.CheckEntered(Parent.P0_ProcessTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_ProcessTypeInfo);

			if (!Parent.P0_IsPartialTemplate)
			{
				CheckTemplateCriteriaUnique(Parent.P0_ProcessTypeInfo);
			}
		}

		void CheckTemplateCriteriaUnique(ZPropertyInfo property)
		{
			if (Parent.P0_IsActive)
			{
				if (WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value)
				{
					ValidateP0_EffectiveStartDateUtc();
					ValidateP0_EffectiveEndDateUtc();
				}
				else if (Parent.Factory.LoadTop1<ProcessTaskTemplate>(IdenticalTemplateQuery()) != null)
				{
					property.AddError(Res.GetString("4c5c111b-c51b-4289-915a-567924d64867", "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria."));
				}
			}
		}

		ZQuery IdenticalTemplateQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(ProcessTaskTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_IsPartialTemplate, SQLComparisonOperator.Equal, ZBool.False);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_IsUniversal, SQLComparisonOperator.Equal, Parent.P0_IsUniversal);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_IsSystem, SQLComparisonOperator.Equal, ZBool.False);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, SQLComparisonOperator.Equal, Parent.P0_ProcessType);

			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType1, Parent.P0_SubType1);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType2, Parent.P0_SubType2);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType3, Parent.P0_SubType3);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType4, Parent.P0_SubType4);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_SubType5, Parent.P0_SubType5);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_OH_Client, Parent.P0_OH_Client.IsEmpty ? null : Parent.P0_OH_Client);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_WW, Parent.P0_WW.IsEmpty ? null : Parent.P0_WW);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_LoadPortCountry, Parent.P0_LoadPortCountry);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_DischargePortCountry, Parent.P0_DischargePortCountry);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_GB, Parent.P0_GB.IsEmpty ? null : Parent.P0_GB);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_GE, Parent.P0_GE.IsEmpty ? null : Parent.P0_GE);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_GC, Parent.P0_GC.IsEmpty ? null : Parent.P0_GC);

			return query;
		}

		#endregion

		#region Name

		protected override void CheckP0_Name()
		{
			base.CheckP0_Name();

			MandatoryValidation.CheckEntered(Parent.P0_NameInfo);

			if (!Parent.P0_Name.IsEmpty)
			{
				var query = new ZQuery(ProcessTaskTemplateSchema.P0_Name, Parent.P0_Name)
					.AddToFilter(ProcessTaskTemplateSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Exists(typeof(ProcessTaskTemplate), query))
				{
					Parent.P0_NameInfo.AddError(Res.GetString("4eb212db-e96b-4c9a-8160-b28ca12ed7c5", "The Name has been duplicated and must be unique."));
				}
			}
		}

		#endregion

		#region Criteria Field 1/2/3/4/5

		protected override void CheckP0_SubType1()
		{
			base.CheckP0_SubType1();

			if (Parent.SubType1IsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.P0_SubType1Info);
			}

			ListValidation.ErrorIfInvalidCode(Parent.P0_SubType1Info);

			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_SubType1Info);
		}

		protected override void CheckP0_SubType2()
		{
			base.CheckP0_SubType2();

			if (Parent.SubType2IsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.P0_SubType2Info);
			}

			ListValidation.ErrorIfInvalidCode(Parent.P0_SubType2Info);

			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_SubType2Info);
		}

		protected override void CheckP0_SubType3()
		{
			base.CheckP0_SubType3();

			if (Parent.SubType3IsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.P0_SubType3Info);
			}

			ListValidation.ErrorIfInvalidCode(Parent.P0_SubType3Info);

			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_SubType3Info);
		}

		protected override void CheckP0_SubType4()
		{
			base.CheckP0_SubType4();

			if (Parent.SubType4IsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.P0_SubType4Info);
			}

			ListValidation.ErrorIfInvalidCode(Parent.P0_SubType4Info);

			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_SubType4Info);
		}

		protected override void CheckP0_SubType5()
		{
			base.CheckP0_SubType5();

			if (Parent.SubType5IsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.P0_SubType5Info);
			}

			ListValidation.ErrorIfInvalidCode(Parent.P0_SubType5Info);

			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_SubType5Info);
		}

		#endregion

		#region Load/Discharge Country

		protected override void CheckP0_LoadPortCountry()
		{
			base.CheckP0_LoadPortCountry();
			ListValidation.ErrorIfInvalidCode(Parent.P0_LoadPortCountryInfo);
			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_LoadPortCountryInfo);
		}

		protected override void CheckP0_DischargePortCountry()
		{
			base.CheckP0_DischargePortCountry();
			ListValidation.ErrorIfInvalidCode(Parent.P0_DischargePortCountryInfo);
			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_DischargePortCountryInfo);
		}

		#endregion

		#region Client / Branch / Department

		protected override void CheckP0_OH_Client()
		{
			base.CheckP0_OH_Client();
			ListValidation.ErrorIfInvalidPK(Parent.P0_OH_ClientInfo, Parent.ClientList);
			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_OH_ClientInfo);
		}

		protected override void CheckP0_GB()
		{
			base.CheckP0_GB();
			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_GBInfo);
		}

		protected override void CheckP0_GE()
		{
			base.CheckP0_GE();
			ValidateP0_ProcessType(); //Checks for collisions
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_GEInfo);
		}

		protected override void CheckP0_GC()
		{
			base.CheckP0_GC();
			ValidateP0_ProcessType(); //Checks for collisions 
		}

		protected override void CheckP0_WW()
		{
			base.CheckP0_WW();
			ValidateP0_ProcessType(); //Checks for collisions
			CheckWarehouseType(Parent.P0_WWInfo);
			CheckCriteriaEnteredForDisallowedTemplateTypes(Parent.P0_WWInfo);
		}

		void CheckWarehouseType(ZPropertyInfo info)
		{
			ListValidation.ErrorIfInvalidPK(info, Parent.Lookups.Warehouses);
		}

		#endregion

		#region Fallback Method

		protected override void CheckP0_CustomFieldFallback()
		{
			base.CheckP0_CustomFieldFallback();
			MandatoryValidation.CheckEntered(Parent.P0_CustomFieldFallbackInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_CustomFieldFallbackInfo);
		}

		protected override void CheckP0_TaskFallbackMethod()
		{
			base.CheckP0_TaskFallbackMethod();
			MandatoryValidation.CheckEntered(Parent.P0_TaskFallbackMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_TaskFallbackMethodInfo);
		}

		protected override void CheckP0_MilestoneFallbackMethod()
		{
			base.CheckP0_MilestoneFallbackMethod();
			MandatoryValidation.CheckEntered(Parent.P0_MilestoneFallbackMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_MilestoneFallbackMethodInfo);
		}

		protected override void CheckP0_TriggerFallbackMethod()
		{
			base.CheckP0_TriggerFallbackMethod();
			MandatoryValidation.CheckEntered(Parent.P0_TriggerFallbackMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_TriggerFallbackMethodInfo);

			if (Parent.P0_IsUniversal && Parent.P0_TriggerFallbackMethod == FallbackTypeList.Codes.EmptyFallback)
			{
				Parent.P0_TriggerFallbackMethodInfo.AddError(Res.GetString("4d78c326-88c8-4368-8a7c-76b17ea7d68b", "Universal Templates cannot use {0} Trigger Fallback Method.", FallbackTypeList.Codes.EmptyFallback));
			}
		}

		protected override void CheckP0_ValidationFallbackMethod()
		{
			base.CheckP0_ValidationFallbackMethod();
			MandatoryValidation.CheckEntered(Parent.P0_ValidationFallbackMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P0_ValidationFallbackMethodInfo);
		}

		#endregion

		#region IsPartial

		void CheckCriteriaEnteredForDisallowedTemplateTypes(ZPropertyInfo info)
		{
			if (Parent.P0_IsPartialTemplate)
			{
				if (CriteriaPropertyInfos.Any(i => i.Name == info.Name) && !info.Value.IsEmpty)
				{
					info.AddError(PartialTemplateSelectionCriteriaError);
				}
			}
		}

		internal static string PartialTemplateSelectionCriteriaError
		{
			get { return Res.GetString("5bd3ad69-3fdc-49d3-a3b4-358f5b3de4a5", "Criteria properties must be empty when using partial templates as they are only applied by TMP Completion Trigger Actions."); }
		}

		IEnumerable<ZPropertyInfo> CriteriaPropertyInfos
		{
			get
			{
				yield return Parent.P0_SubType1Info;
				yield return Parent.P0_SubType2Info;
				yield return Parent.P0_SubType3Info;
				yield return Parent.P0_SubType4Info;
				yield return Parent.P0_SubType5Info;
				yield return Parent.P0_LoadPortCountryInfo;
				yield return Parent.P0_DischargePortCountryInfo;
				yield return Parent.P0_GEInfo;
				yield return Parent.P0_GBInfo;
				yield return Parent.P0_WWInfo;
				yield return Parent.P0_OH_ClientInfo;
			}
		}

		#endregion

		#region IsUniversal

		protected override void CheckP0_IsUniversal()
		{
			base.CheckP0_IsUniversal();

			var descriptor = Parent.WorkflowDescriptor;

			if (Parent.P0_IsUniversal)
			{
				if (!WorkflowDataRegistry.Instance.EnableUniversalTemplates.Value)
				{
					Parent.P0_IsUniversalInfo.AddWarning(Res.GetString("6f5899c6-d595-477b-8edf-79e8f4d5402a", "Universal Templates are disabled in the registry. Triggers defined on this template will neither appear on jobs nor be fired by matching events. See registry item [{0} -> {1}].", WorkflowDataRegistry.Instance.EnableUniversalTemplates.Category, WorkflowDataRegistry.Instance.EnableUniversalTemplates.Caption));
				}

				if (descriptor != null && !descriptor.SupportsUniversalTemplates)
				{
					Parent.P0_IsUniversalInfo.AddError(Res.GetString("656328bc-42fa-4ece-8f64-32b9d06efb17", "This Process Type does not support Universal templates."));
				}

				if (Parent.P0_IsPartialTemplate)
				{
					Parent.P0_IsUniversalInfo.AddError(Res.GetString("ef3b04fd-e897-4b86-abce-36796056d205", "Universal Templates must not be also marked as Partial."));
				}
			}
			else if (descriptor != null && !descriptor.SupportsWorkflowTemplates && descriptor.SupportsUniversalTemplates)
			{
				Parent.P0_IsUniversalInfo.AddError(Res.GetString("4e07597c-61a9-4f0a-a6c5-845654814407", "This Process Type only supports Universal templates."));
			}
		}

		#endregion

		#region Effective Start/End Date

		protected override void CheckP0_EffectiveEndDateUtc()
		{
			if (!Parent.P0_IsPartialTemplate)
			{
				CheckEffectiveDateRangeSelf(Parent.P0_EffectiveEndDateUtcInfo);
				CheckEffectiveDateRangeOther(Parent.P0_EffectiveEndDateUtcInfo);
			}
		}

		protected override void CheckP0_EffectiveStartDateUtc()
		{
			if (!Parent.P0_IsPartialTemplate)
			{
				CheckEffectiveDateRangeSelf(Parent.P0_EffectiveStartDateUtcInfo);
				CheckEffectiveDateRangeOther(Parent.P0_EffectiveStartDateUtcInfo);
			}
		}

		void CheckEffectiveDateRangeSelf(ZPropertyInfo info)
		{
			var startDate = Parent.P0_EffectiveStartDateUtc;
			var endDate = Parent.P0_EffectiveEndDateUtc;
			if (!startDate.IsEmpty && !endDate.IsEmpty && startDate > endDate)
			{
				info.AddError(Res.GetString("3d3f99b5-59bc-42bf-b4fb-934233c464d1", "Cannot have Effective End Date before Effective Start Date."));
			}
		}

		void CheckEffectiveDateRangeOther(ZPropertyInfo info)
		{
			if (Parent.P0_IsActive && WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value)
			{
				var identicalTemplates = Parent.Factory.Load<ProcessTaskTemplate>(IdenticalTemplateQuery());

				if (identicalTemplates.Any())
				{
					var startDate = Parent.P0_EffectiveStartDateUtc;
					var endDate = Parent.P0_EffectiveEndDateUtc;
					if (startDate.IsEmpty && endDate.IsEmpty)
					{
						AddCollisionError(info, identicalTemplates[0]);
					}
					else
					{
						foreach (var otherTemplate in identicalTemplates)
						{
							if (HasDatePairCollision(startDate, endDate, otherTemplate.P0_EffectiveStartDateUtc, otherTemplate.P0_EffectiveEndDateUtc))
							{
								AddCollisionError(info, otherTemplate);
								break; // Just return one error. Don't want to be too greedy.
							}
						}
					}
				}
			}
		}

		static void AddCollisionError(ZPropertyInfo info, ProcessTaskTemplate otherTemplate)
		{
			info.AddError(Res.GetString("cda707cc-b03f-48ce-8096-236659b65c92",
				"Effective Date Range collides with another template of the same criteria. (Start Time [{0}], End Time [{1}]).",
				ToDateString(otherTemplate.P0_EffectiveStartDateUtc),
				ToDateString(otherTemplate.P0_EffectiveEndDateUtc)));
		}

		static ZString ToDateString(ZDateTime dateTime)
		{
			return dateTime.IsEmpty ? Res.GetString("WordRepresentingTimeFieldIsEmpty", "Empty") : dateTime.ToLongTimeString();
		}

		bool HasDatePairCollision(ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			ZDateTime SetMax(ZDateTime date) => date.IsEmpty ? new ZDateTime(DateTime.MaxValue.AddTicks(-1)) : date;
			ZDateTime SetMin(ZDateTime date) => date.IsEmpty ? new ZDateTime(DateTime.MinValue.AddTicks(1)) : date; // Adding a tick, since DateTime.MinValue == ZDateTime.Invalid
			start1 = SetMin(start1);
			start2 = SetMin(start2);
			end1 = SetMax(end1);
			end2 = SetMax(end2);

			if (start1 == start2 || end1 == end2)
			{
				return true; // This will pick up empty date collisions.
			}
			else
			{
				return start1 < end2 && end1 > start2;
			}
		}

		#endregion

		#region IsActive
		protected override void CheckP0_IsActive()
		{
			base.CheckP0_IsActive();

			if (Parent.P0_IsActive && Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed && !Env.Security.WorkflowTaskTemplatesEdit.IsAllowed)
			{
				Parent.P0_IsActiveInfo.AddError(Res.GetString("dbb23936-823f-44fd-9ad3-03720e16c901", "Templates cannot be activated by users without template editing permissions"));
			}
		}
		#endregion
	}
}
