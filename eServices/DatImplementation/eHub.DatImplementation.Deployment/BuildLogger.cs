using Dat.Integration;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;

namespace eHub.DatImplementation.Deployment
{
    public class BuildLogger : ILogger
    {
        #region Member variables

        readonly ITaskLogger taskLogger;
        readonly Dictionary<string, IDisposable> buildTasks = new Dictionary<string, IDisposable>();

        #endregion

        #region Properties

        public LoggerVerbosity Verbosity { get; set; }
        public string Parameters { get; set; }

        #endregion

        #region Constructor

        public BuildLogger(ITaskLogger logger)
        {
            taskLogger = logger;
        }

        #endregion

        #region Methods

        public void Initialize(IEventSource eventSource)
        {
            eventSource.ProjectStarted += (sender, e) => StartRecordTask(string.Format("Project: {0}", e.ProjectFile));
            eventSource.ProjectFinished += (sender, e) => EndRecordTask(string.Format("Project: {0}", e.ProjectFile));

            eventSource.BuildStarted += (sender, e) => StartRecordTask(string.Format("Build: {0}", e.SenderName));
            eventSource.BuildFinished += (sender, e) => EndRecordTask(string.Format("Build: {0}", e.SenderName));

            eventSource.TargetStarted += (sender, e) => StartRecordTask(string.Format("Target: {0}.{1}", e.ProjectFile, e.TargetName));
            eventSource.TargetFinished += (sender, e) => EndRecordTask(string.Format("Target: {0}.{1}", e.ProjectFile, e.TargetName));

            eventSource.TaskStarted += (sender, e) => StartRecordTask(string.Format("Task: {0}.{1}", e.ProjectFile, e.SenderName));
            eventSource.TaskFinished += (sender, e) => EndRecordTask(string.Format("Task: {0}.{1}", e.ProjectFile, e.SenderName));

            eventSource.MessageRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Info: {0}, {1}", e.SenderName, e.Message));
            eventSource.StatusEventRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Status: {0}, {1}", e.SenderName, e.Message));

            eventSource.WarningRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Warn: {0}, {1}", e.SenderName, e.Message));
            eventSource.ErrorRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Error: {0}, {1}", e.SenderName, e.Message));
            eventSource.CustomEventRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Event: {0}, {1}", e.SenderName, e.Message));
            eventSource.AnyEventRaised += (sender, e) => taskLogger.RecordInfo(string.Format("Event: {0}, {1}", e.SenderName, e.Message));
        }

        public void Shutdown()
        {
            foreach (var project in buildTasks.Keys)
            {
                EndRecordTask(project);
            }
        }

        #endregion

        #region Helpers
        void StartRecordTask(string project)
        {
            if (!buildTasks.ContainsKey(project))
            {
                buildTasks.Add(project, taskLogger.RecordTask(project));
            }
        }

        void EndRecordTask(string project)
        {
            if (buildTasks.ContainsKey(project))
            {
                var task = buildTasks[project];
                buildTasks.Remove(project);

                if (task != null)
                {
                    task.Dispose();
                    task = null;
                }
            }
        }

        #endregion
    }
}
