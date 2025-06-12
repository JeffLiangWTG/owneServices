using Microsoft.Build.Framework;

namespace eHub.DatImplementation.TestObject.Deployment
{
    public class TestEventSource : IEventSource
    {
        public event BuildMessageEventHandler MessageRaised;
        public event BuildErrorEventHandler ErrorRaised;
        public event BuildWarningEventHandler WarningRaised;
        public event BuildStartedEventHandler BuildStarted;
        public event BuildFinishedEventHandler BuildFinished;
        public event ProjectStartedEventHandler ProjectStarted;
        public event ProjectFinishedEventHandler ProjectFinished;
        public event TargetStartedEventHandler TargetStarted;
        public event TargetFinishedEventHandler TargetFinished;
        public event TaskStartedEventHandler TaskStarted;
        public event TaskFinishedEventHandler TaskFinished;
        public event CustomBuildEventHandler CustomEventRaised;
        public event BuildStatusEventHandler StatusEventRaised;
        public event AnyEventHandler AnyEventRaised;

        public void RaiseEventAnyEventRaised(object sender, BuildEventArgs e)
        {
            var handler = AnyEventRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventStatusEventRaised(object sender, BuildStatusEventArgs e)
        {
            var handler = StatusEventRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventCustomEventRaised(object sender, CustomBuildEventArgs e)
        {
            var handler = CustomEventRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventTaskFinished(object sender, TaskFinishedEventArgs e)
        {
            var handler = TaskFinished;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventTaskStarted(object sender, TaskStartedEventArgs e)
        {
            var handler = TaskStarted;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventTargetFinished(object sender, TargetFinishedEventArgs e)
        {
            var handler = TargetFinished;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventTargetStarted(object sender, TargetStartedEventArgs e)
        {
            var handler = TargetStarted;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            var handler = ProjectFinished;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            var handler = ProjectStarted;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventBuildFinished(object sender, BuildFinishedEventArgs e)
        {
            var handler = BuildFinished;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventBuildStarted(object sender, BuildStartedEventArgs e)
        {
            var handler = BuildStarted;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventWarningRaised(object sender, BuildWarningEventArgs e)
        {
            var handler = WarningRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventErrorRaised(object sender, BuildErrorEventArgs e)
        {
            var handler = ErrorRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public void RaiseEventMessageRaised(object sender, BuildMessageEventArgs e)
        {
            var handler = MessageRaised;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }
    }
}