using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Milestone : DocDataObject, IMilestone
	{
		#region Sequence

		public ZInt Sequence
		{
			get => sequence;
			set
			{
				if (SetNonPersistentPropertyValue(SequenceInfo, ref sequence, value))
				{
					Validate(SequenceInfo);
				}
			}
		}

		ZInt sequence;

		public ZPropertyInfo SequenceInfo => GetZPropertyInfo(nameof(Sequence));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region EventCode

		public ZString EventCode
		{
			get => eventCode;
			set
			{
				if (SetNonPersistentPropertyValue(EventCodeInfo, ref eventCode, value))
				{
					Validate(EventCodeInfo);
				}
			}
		}

		ZString eventCode;

		public ZPropertyInfo EventCodeInfo => GetZPropertyInfo(nameof(EventCode));

		#endregion

		#region EstimatedDate

		public ZDateTime EstimatedDate
		{
			get => estimatedDate;
			set
			{
				if (SetNonPersistentPropertyValue(EstimatedDateInfo, ref estimatedDate, value))
				{
					Validate(EstimatedDateInfo);
				}
			}
		}

		ZDateTime estimatedDate;

		public ZPropertyInfo EstimatedDateInfo => GetZPropertyInfo(nameof(EstimatedDate));

		#endregion

		#region ActualDate

		public ZDateTime ActualDate
		{
			get => actualDate;
			set
			{
				if (SetNonPersistentPropertyValue(ActualDateInfo, ref actualDate, value))
				{
					Validate(ActualDateInfo);
				}
			}
		}

		ZDateTime actualDate;

		public ZPropertyInfo ActualDateInfo => GetZPropertyInfo(nameof(ActualDate));

		#endregion

		#region ConditionType

		public ZString ConditionType
		{
			get => conditionType;
			set
			{
				if (SetNonPersistentPropertyValue(ConditionTypeInfo, ref conditionType, value))
				{
					Validate(ConditionTypeInfo);
				}
			}
		}

		ZString conditionType;

		public ZPropertyInfo ConditionTypeInfo => GetZPropertyInfo(nameof(ConditionType));

		#endregion

		#region ConditionReference

		public ZString ConditionReference
		{
			get => conditionReference;
			set
			{
				if (SetNonPersistentPropertyValue(ConditionReferenceInfo, ref conditionReference, value))
				{
					Validate(ConditionReferenceInfo);
				}
			}
		}

		ZString conditionReference;

		public ZPropertyInfo ConditionReferenceInfo => GetZPropertyInfo(nameof(ConditionReference));

		#endregion
	}
}
