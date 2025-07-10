using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.Business
{
	public class TraceMonitor : NonPersistentBusinessObject
	{
		#region Constructor

		public TraceMonitor()
			: base()
		{
			Tracer = ObjectFactory.Get<ITracer>();
			TracerConfiguration = (ITracerConfiguration)Tracer;
		}

		#endregion

		#region LogCallStack

		public ZBool LogCallStack
		{
			get { return logCallStack; }
			set { SetNonPersistentPropertyValue(LogCallStackInfo, ref logCallStack, value); }
		}

		public ZPropertyInfo LogCallStackInfo
		{
			get { return GetZPropertyInfo(nameof(LogCallStack)); }
		}

		ZBool logCallStack;

		#endregion

		#region LogDateTime

		public ZBool LogDateTime
		{
			get { return logDateTime; }
			set { SetNonPersistentPropertyValue(LogDateTimeInfo, ref logDateTime, value); }
		}

		public ZPropertyInfo LogDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(LogDateTime)); }
		}

		ZBool logDateTime;

		#endregion

		#region LogThreadId

		public ZBool LogThreadId
		{
			get { return logThreadId; }
			set { SetNonPersistentPropertyValue(LogThreadIdInfo, ref logThreadId, value); }
		}

		public ZPropertyInfo LogThreadIdInfo
		{
			get { return GetZPropertyInfo(nameof(LogThreadId)); }
		}

		ZBool logThreadId;

		#endregion

		#region LogProcessId

		public ZBool LogProcessId
		{
			get { return logProcessId; }
			set { SetNonPersistentPropertyValue(LogProcessIdInfo, ref logProcessId, value); }
		}

		public ZPropertyInfo LogProcessIdInfo
		{
			get { return GetZPropertyInfo(nameof(LogProcessId)); }
		}

		ZBool logProcessId;

		#endregion

		#region  MessageWriter

		public IMessageWriter MessageWriter { get; set; }

		#endregion

		#region TraceSourceSettingsList

		public TraceSourceSettingsCollection TraceSourceSettingsList
		{
			get
			{
				if (traceSourceSettingsList == null)
				{
					traceSourceSettingsList = GetTraceSourceSettingsCollectionCore();
					RegisterEditableChildObject(traceSourceSettingsList);
				}
				return traceSourceSettingsList;
			}
		}
		TraceSourceSettingsCollection traceSourceSettingsList;

		protected virtual TraceSourceSettingsCollection GetTraceSourceSettingsCollectionCore() => TraceSourceSettingsCollection.GetTraceSources();

		#endregion

		#region IsTracing
		public bool IsTracing => TracerConfiguration.CheckWhetherTraceSourceExists(TraceSourceSettingsList.GetAllCodes());

		#endregion

		#region Functions

		public void IntitializeTraceSources()
		{
			TraceSourceConfiguration traceSourceConfiguration = new TraceSourceConfiguration()
			{
				MessageWriter = MessageWriter,
				GetTracePrefix = GetTracePrefix,
				LogCallStack = LogCallStack,
				LogDateTime = LogDateTime,
				LogProcessId = LogProcessId,
				LogThreadId = LogThreadId
			};

			var traceSourceSettings = new List<TraceSourceSettingsConfiguration>();
			foreach (TraceSourceSettings settings in TraceSourceSettingsList)
			{
				traceSourceSettings.Add(settings.ToTraceSourceSettingsConfiguration());
			}
			traceSourceConfiguration.TraceSourceSettings = traceSourceSettings;

			TracerConfiguration.InitializeTraceSources(traceSourceConfiguration);
		}

		public void ClearTraceSources() => TracerConfiguration.RemoveTraceSource(TraceSourceSettingsList.GetAllCodes());

		string GetTracePrefix() => FormattableString.Invariant($"[GC:{Env.CurrentCompany.Code}][GB:{Env.CurrentBranch.Code}][GE:{Env.CurrentDepartment.Code}][GS:{Env.CurrentUser.Initials}]");

		readonly ITracer Tracer;
		readonly ITracerConfiguration TracerConfiguration;

		#endregion
	}
}
