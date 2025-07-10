using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationJobSkillGroup))]
	sealed class GlbAccreditationJobSkillGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			var skillPivot = group.SkillPivots.AddNew();

			AssertNoExceptionThrown(delegate
			{ group.Delete(); });
			Assert(group.IsDeleted);
			Assert(skillPivot.IsDeleted);
		}

		class GlbPersonForTest : IGlbPerson
		{
			readonly Dictionary<ZGuid, ZDateTime> completionDates = new Dictionary<ZGuid, ZDateTime>();
			public void SetCompletedDate(ZGuid skillPk, ZDateTime completion)
			{
				completionDates.Add(skillPk, completion);
			}

			readonly Dictionary<ZGuid, ZDecimal> weightedScores = new Dictionary<ZGuid, ZDecimal>();
			public void SetWeightedScore(ZGuid skillPk, ZDecimal weightedScore)
			{
				weightedScores.Add(skillPk, weightedScore);
			}

			public void IncrementReadOnlyIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public void DecrementReadOnlyIncludingChildren(bool decrementToZero = false)
			{
				throw new NotImplementedException();
			}

			public void RefreshBindingIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public void ClearHasChangesIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public bool HasChanges { get; set; }
			public bool HasChangesNotIncludingChildren { get; }
			public uint LastChangeNumber { get; }
			public bool IsInDatabase { get; }
			public bool IsInDatabaseIncludingChildren { get; }
			public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged = delegate { };
			public event EventHandler UpdatedByDataRefreshIncludingChildren = delegate { };
			public event EventHandler<NotificationsChangedEventArgs> NotificationsChanged = delegate { };
			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count { get; }
			public object SyncRoot { get; }
			public bool IsSynchronized { get; }
			public int Add(object value)
			{
				throw new NotImplementedException();
			}

			public bool Contains(object value)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public int IndexOf(object value)
			{
				throw new NotImplementedException();
			}

			public void Insert(int index, object value)
			{
				throw new NotImplementedException();
			}

			public void Remove(object value)
			{
				throw new NotImplementedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotImplementedException();
			}

			public object this[int index]
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			public bool IsReadOnly { get; }
			public bool IsFixedSize { get; }
			public object AddNew()
			{
				throw new NotImplementedException();
			}

			public void AddIndex(PropertyDescriptor property)
			{
				throw new NotImplementedException();
			}

			public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
			{
				throw new NotImplementedException();
			}

			public int Find(PropertyDescriptor property, object key)
			{
				throw new NotImplementedException();
			}

			public void RemoveIndex(PropertyDescriptor property)
			{
				throw new NotImplementedException();
			}

			public void RemoveSort()
			{
				throw new NotImplementedException();
			}

			public bool AllowNew { get; }
			public bool AllowEdit { get; }
			public bool AllowRemove { get; }
			public bool SupportsChangeNotification { get; }
			public bool SupportsSearching { get; }
			public bool SupportsSorting { get; }
			public bool IsSorted { get; }
			public PropertyDescriptor SortProperty { get; }
			public ListSortDirection SortDirection { get; }
			public event ListChangedEventHandler ListChanged = delegate { };
			public bool HasNotifications()
			{
				throw new NotImplementedException();
			}

			public bool HasNotifications(INotificationType type)
			{
				throw new NotImplementedException();
			}

			public INotificationType GetHighestSeverityNotificationType()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<INotification> Notifications { get; }
			public ZGuid Identifier { get; }
			public void SuspendValidation()
			{
				throw new NotImplementedException();
			}

			public void ResumeValidation()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			public void RunPreSaveValidation()
			{
				throw new NotImplementedException();
			}

			public void RunPreSaveValidationFetch(bool executeHints)
			{
				throw new NotImplementedException();
			}

			public void MarkAsNeedingValidationIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public void ValidateIfQuickAndImprovesPreSaveValidationPerformance()
			{
				throw new NotImplementedException();
			}

			public void NotifyRegisteredChildEditable()
			{
				throw new NotImplementedException();
			}

			public void DeleteForDataRefresh()
			{
				throw new NotImplementedException();
			}

			public BusinessObjectFactory Factory { get; }
			public bool IgnoreValidationSuspended { get; set; }
			public bool IsValidationSuspended { get; }
			public string TableName { get; }
			public ZString HumanReadableName { get; }
			public bool CanContinueWithSave { get; }
			public IBusiness[] Children { get; }
			public bool CanDeleteForDataRefresh { get; }

			public ZGuid PK { get; }
		}
	}
}
