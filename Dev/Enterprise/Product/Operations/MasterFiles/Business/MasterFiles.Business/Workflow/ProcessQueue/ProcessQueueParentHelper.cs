using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessQueueParentHelper
	{
		public ProcessQueueParentHelper(IProcessQueueParent parent) : this(parent, typeof(ProcessQueue))
		{
		}

		public ProcessQueueParentHelper(IProcessQueueParent parent, Type typeOfProcessQueue)
		{
			Type baseType = typeof(ProcessQueue);
			if (!baseType.IsAssignableFrom(typeOfProcessQueue))
			{
				throw new NotSupportedException("Type has to be (or a subclass of) " + baseType.FullName);
			}
			this.TypeOfProcessQueue = typeOfProcessQueue;
			this.Parent = parent;
		}

		public ActiveProcessQueueCollection ActiveProcessQueueForBinding
		{
			get
			{
				if (fActiveProcessQueueForBinding == null)
				{
					fActiveProcessQueueForBinding = new ActiveProcessQueueCollection(Factory);
					fActiveProcessQueueForBinding.Add(ActiveProcessQueue);
				}
				return fActiveProcessQueueForBinding;
			}
		}

		public ProcessQueue CurrentQueue
		{
			get
			{
				if (fCurrentQueue == null)
				{
					fCurrentQueue = LoadOrAddNewQueue();
					RegisterCurrentQueueAsEditableChildObject();
				}
				return fCurrentQueue;
			}
		}

		public void DeleteProcessQueue()
		{
			ProcessQueue queueToBeDeleted = GetQueueToBeDeleted();
			if (queueToBeDeleted != null)
			{
				queueToBeDeleted.Delete();
				ResetActiveProcessQueueForBinding();
			}
		}

		#region Implementation

		ActiveProcessQueue ActiveProcessQueue
		{
			get
			{
				if (fActiveProcessQueue == null)
				{
					fActiveProcessQueue = ActiveProcessQueue.New(CurrentQueue);
				}
				return fActiveProcessQueue;
			}
		}

		BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		void RegisterCurrentQueueAsEditableChildObject()
		{
			BusinessObject bizO = Parent as BusinessObject;
			if (bizO != null && !bizO.IsRegisteredEditableChildObject(CurrentQueue))
			{
				bizO.RegisterEditableChildObject(CurrentQueue);
			}
		}

		ProcessQueue LoadOrAddNewQueue()
		{
			ProcessQueue result = LoadQueue()
				?? (ProcessQueue)Factory.New(TypeOfProcessQueue);
			result.Parent = Parent;
			return result;
		}

		ProcessQueue LoadQueue()
		{
			ZQuery filter = new ZQuery(ProcessQueueSchema.P4_ParentTableCode, Parent.TablePrefix);
			filter.FetchOnlyFromLocalCache = !Parent.IsInDatabase;
			filter.AddToFilter(ProcessQueueSchema.P4_ParentID, Parent.PK);
			return (ProcessQueue)Factory.LoadTop1(TypeOfProcessQueue, filter);
		}

		ProcessQueue GetQueueToBeDeleted()
			=> fCurrentQueue ?? LoadQueue();

		void ResetActiveProcessQueueForBinding()
		{
			fActiveProcessQueueForBinding = new ActiveProcessQueueCollection(Factory);
		}

		readonly IProcessQueueParent Parent;
		readonly Type TypeOfProcessQueue;

		ProcessQueue fCurrentQueue;
		ActiveProcessQueueCollection fActiveProcessQueueForBinding;
		ActiveProcessQueue fActiveProcessQueue;

		#region For Test
#if DEBUG

		internal void ResetCurrentQueue()
		{
			if (fCurrentQueue != null)
			{
				fCurrentQueue.Delete();
				fCurrentQueue = null;
			}
		}

#endif
		#endregion

		#endregion
	}
}
