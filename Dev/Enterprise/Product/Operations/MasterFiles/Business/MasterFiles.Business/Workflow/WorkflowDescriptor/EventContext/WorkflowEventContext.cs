using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowEventContext : NonPersistentBusinessObject
	{
		public WorkflowEventContext() { }

		public WorkflowEventContext(WorkflowEventContextPair contextPair)
		{
			AvailableContextStepPairs = new[] { contextPair };
			MasterClassifier = contextPair.MasterClassifier.Code;
			MasterType = contextPair.MasterType.Code;
		}

		public WorkflowEventContextPair[] AvailableContextStepPairs
		{
			get
			{
				return availableContextStepPairs ?? (emptyAvailableContextStepPairs ?? (emptyAvailableContextStepPairs = System.Array.Empty<WorkflowEventContextPair>()));
			}
			set
			{
				availableContextStepPairs = value;

				MasterClassifierInfo.RefreshBinding();
				MasterTypeInfo.RefreshBinding();
			}
		}
		WorkflowEventContextPair[] availableContextStepPairs;
		WorkflowEventContextPair[] emptyAvailableContextStepPairs;

		#region MasterClassifier

		[List("MasterClassifiers")]
		[MaxLength(3)]
		public ZString MasterClassifier
		{
			get { return masterClassifier; }
			set
			{
				SetNonPersistentPropertyValue(MasterClassifierInfo, ref masterClassifier, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMasterClassifier();
					Validation.ValidateMasterType();
				}
				MasterTypeInfo.RefreshBinding();
				MasterClassifierDescriptionInfo.RefreshBinding();
				MasterTypeDescriptionInfo.RefreshBinding();
			}
		}
		ZString masterClassifier;

		public ZPropertyInfo MasterClassifierInfo
		{
			get { return GetZPropertyInfo(nameof(MasterClassifier)); }
		}

		public CodeDescriptionPairList MasterClassifiers
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (AvailableContextStepPairs != null)
				{
					foreach (var classifier in AvailableContextStepPairs.Where(context => context.MasterType.Code == MasterType || MasterType.IsEmpty || MasterTypeInfo.HasErrors()).Select(context => context.MasterClassifier))
					{
						if (!result.ContainsCode(classifier))
						{
							result.Add(classifier);
						}
					}
					result.Sort();
				}
				return result;
			}
		}

		public ZString MasterClassifierDescription
		{
			get
			{
				var eventContextPair = AvailableContextStepPairs != null ? AvailableContextStepPairs.FirstOrDefault(pair => pair.MasterClassifier.Code == MasterClassifier) : null;
				return eventContextPair != null ? (ZString)eventContextPair.MasterClassifier.Description : ZString.Empty;
			}
		}

		public ZPropertyInfo MasterClassifierDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MasterClassifierDescription)); }
		}

		#endregion

		#region MasterType

		[List("MasterTypes")]
		[MaxLength(3)]
		public ZString MasterType
		{
			get { return masterType; }
			set
			{
				SetNonPersistentPropertyValue(MasterTypeInfo, ref masterType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMasterType();
					Validation.ValidateMasterClassifier();
				}
				MasterClassifierInfo.RefreshBinding();
				MasterTypeDescriptionInfo.RefreshBinding();
				MasterClassifierDescriptionInfo.RefreshBinding();
			}
		}
		ZString masterType;

		public ZPropertyInfo MasterTypeInfo
		{
			get { return GetZPropertyInfo(nameof(MasterType)); }
		}

		public CodeDescriptionPairList MasterTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (AvailableContextStepPairs != null)
				{
					foreach (var type in AvailableContextStepPairs.Where(context => context.MasterClassifier.Code == MasterClassifier || MasterClassifier.IsEmpty || MasterClassifierInfo.HasErrors()).Select(context => context.MasterType))
					{
						if (!result.ContainsCode(type.Code))
						{
							result.Add(type);
						}
					}
					result.Sort();
				}
				return result;
			}
		}

		public ZString MasterTypeDescription
		{
			get
			{
				var eventContextPair = AvailableContextStepPairs != null ? AvailableContextStepPairs.FirstOrDefault(pair => pair.MasterType.Code == MasterType) : null;
				return eventContextPair != null ? (ZString)eventContextPair.MasterType.Description : ZString.Empty;
			}
		}

		public ZPropertyInfo MasterTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MasterTypeDescription)); }
		}

		#endregion

		#region Validation

		public WorkflowEventContextValidation Validation
		{
			get { return new WorkflowEventContextValidation(this); }
		}

		#endregion

		internal WorkflowEventContextPair ToWorkflowEventContextPair()
		{
			return
				(AvailableContextStepPairs != null
					? AvailableContextStepPairs.FirstOrDefault(pair => pair.MasterClassifier.Code == MasterClassifier && pair.MasterType.Code == MasterType)
					: null)
				?? new WorkflowEventContextPair(new CodeDescriptionPair(MasterClassifier.ToString(), (NoResString)MasterClassifier), new CodeDescriptionPair(MasterType.ToString(), (NoResString)MasterType));
		}
	}
}
