using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
#if DEBUG
	public
#endif
 abstract class DeletingProcessor<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where T : BusinessObject
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected DeletingProcessor()
		{
			processedObjects = new Dictionary<T, bool>();
		}

		protected T GetOrCreate(ZQuery query)
		{
			T result = Factory.LoadTop1<T>(query) ?? Factory.New<T>();
			AddUsed(result);
			return result;
		}

		public void RemoveUnused()
		{
			foreach (T code in Factory.Load<T>(Filter))
			{
				if (!processedObjects.ContainsKey(code))
				{
					code.Delete();
				}
			}
		}

		protected virtual ZQuery Filter
		{
			get { return new ZQuery(); }
		}

		protected void AddUsed(T bizO)
		{
			processedObjects[bizO] = true;
		}
		readonly Dictionary<T, bool> processedObjects;

		/// <summary>
		/// This loads all records to save round-trip times to hit the db for each record.
		/// However this should be used if one message contains all the records.
		/// Otherwise use FetchHint like in CarrierProcessor
		/// </summary>
		protected void PreLoadIntoFactory()
		{
			Factory.Load<T>(Filter);
		}

		public void ResetUnused()
		{
			foreach (T code in Factory.Load<T>(Filter))
			{
				if (!processedObjects.ContainsKey(code))
				{
					Reset(code);
				}
			}
		}

		protected virtual void Reset(T code)
		{
		}
	}
}
