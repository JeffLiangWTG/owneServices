using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;

namespace Enterprise.MasterData.Common
{
	public class PatternMatchingRecalculator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Regenerate Events

		public event EventHandler RecalculateStart;

		public event EventHandler<RecalculatingEventArgs> Recalculating;

		public event EventHandler<RecalculatedEventArgs> Recalculated;

		#endregion

		RecalculatingEventArgs recalculatingEventArgs;
		RecalculatedEventArgs recalculatedEventArgs;

		public RecalculatingEventArgs RecalculatingEventArgs
		{
			get
			{
				return recalculatingEventArgs ?? (recalculatingEventArgs = new RecalculatingEventArgs());
			}
		}

		public RecalculatedEventArgs RecalculatedEventArgs
		{
			get
			{
				return recalculatedEventArgs ?? (recalculatedEventArgs = new RecalculatedEventArgs());
			}
		}

		readonly TBizo targetObject;
		readonly SynchronizationContext syncContext = SynchronizationContext.Current;
		readonly SendOrPostCallback sendRecalculateStartDelegate;
		readonly SendOrPostCallback sendRecalculatingDelegate;
		readonly SendOrPostCallback sendRecalculatedDelegate;
		public List<DeduplicationDebuggerMaster> DebuggerMessages;
		readonly IDeduplicationDebuggerParticipant debuggerParticipant;

		void StartInvoke(object obj)
		{
			RecalculateStart?.Invoke(targetObject, EventArgs.Empty);
		}

		void RecalculatingInvoke(object obj)
		{
			Recalculating?.Invoke(targetObject, (RecalculatingEventArgs)obj);
		}

		void RecalculatedInvoke(object obj)
		{
			Recalculated?.Invoke(targetObject, (RecalculatedEventArgs)obj);
		}

		public void RecalculatingInvokeMethod(RecalculatingEventArgs args)
		{
			syncContext?.Send(sendRecalculatingDelegate, args);
		}

		public PatternMatchingRecalculator(TBizo targetObject)
		{
			this.targetObject = targetObject;
			sendRecalculateStartDelegate = new SendOrPostCallback(StartInvoke);
			sendRecalculatingDelegate = new SendOrPostCallback(RecalculatingInvoke);
			sendRecalculatedDelegate = new SendOrPostCallback(RecalculatedInvoke);

			debuggerParticipant = new DeduplicationDebuggerParticipant("PatternRecalculator");
		}

		public PatternMatchingRecalculator(TBizo targetObject, IDeduplicationDebuggerParticipant debuggerParticipant)
			: this(targetObject)
		{
			this.debuggerParticipant = debuggerParticipant;
		}

		public void Regenerate(object glowBizO)
		{
			targetGlowBizO = glowBizO;
			RegenerateCore();
		}

		public async Task RegenerateAsync(object glowBizO)
		{
			targetGlowBizO = glowBizO;

			Task task = Task.Run(() =>
			{
				RegenerateCore();
			});

			await task;
		}

		protected void RegenerateCore()
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					syncContext?.Send(sendRecalculateStartDelegate, null);
					RecalculatingEventArgs.Current = 0;
					RecalculatingEventArgs.ProgressText = Res.GetString("1BB98C3E-CD1C-4DF8-9D4B-878B11E26A08", "Recalculating data...");
					syncContext?.Send(sendRecalculatingDelegate, RecalculatingEventArgs);
					int count = Recalcaulate();
					RecalculatingEventArgs.ProgressText = Res.GetString("DA480A10-4BC8-43D9-A574-8F7080125A3F", "Operation completed successfully.");
					syncContext?.Send(sendRecalculatingDelegate, RecalculatingEventArgs);
					RecalculatedEventArgs.EffectiveCount = count;
				}
				catch (Exception err) when (!err.IsCriticalException())
				{
					RecalculatedEventArgs.ErrorMessage = err.Message;
					RecalculatedEventArgs.EffectiveCount = 0;
				}
				finally
				{
					syncContext?.Send(sendRecalculatedDelegate, RecalculatedEventArgs);
				}
			}
		}

		int Recalcaulate()
		{
			var factory = new BusinessObjectFactory();
			var targetObjectInNewDb = factory.Load<TBizo>(targetObject.PK);
			var generators = targetObjectInNewDb.RegenerationEntities(this);
			var total = 0;
			var count = 0;

			using (var timer = new DeduplicationPerformanceMonitor())
			{
				foreach (var generator in generators)
				{
					total += generator.InitializeDataCounter(targetObjectInNewDb, factory);
				}

				RecalculatingEventArgs.Total = total;
				DeduplicationUtils.DebuggerHubInstance.Register(debuggerParticipant);

				foreach (var generator in generators)
				{
					count += generator.RegeneratePatterns(targetObjectInNewDb, factory);
				}

				debuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, DebuggerMessages, nameof(Recalcaulate), timer.ElapsedDuration, targetGlowBizO);

				return count;
			}
		}

		protected object targetGlowBizO;
	}
}
