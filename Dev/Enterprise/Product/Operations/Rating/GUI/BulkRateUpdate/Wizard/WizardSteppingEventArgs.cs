using System.ComponentModel;

namespace Enterprise.Rating.GUI
{
	public class WizardSteppingEventArgs : CancelEventArgs
	{
		#region Nested Types

		public enum Direction
		{
			Unknown,
			Back,
			Forward
		}

		#endregion

		#region Members

		readonly Direction direction = Direction.Unknown;

		public Direction MovementDirection
		{
			get { return direction; }
		}

		#endregion

		#region Ctor

		public WizardSteppingEventArgs(Direction direction)
		{
			this.direction = direction;
		}

		#endregion
	}
}

