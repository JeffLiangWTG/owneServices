using System;

namespace Enterprise.Freight.Business
{
	public class ValueNotSetEventArgs : EventArgs
	{
		#region Ctor

		public ValueNotSetEventArgs(string propertyName, string reason)
		{
			PropertyName = propertyName;
			Reason = reason;
		}

		#endregion

		#region Properties

		public string PropertyName
		{
			get;
			private set;
		}

		public string Reason
		{
			get;
			private set;
		}

		#endregion
	}
}
