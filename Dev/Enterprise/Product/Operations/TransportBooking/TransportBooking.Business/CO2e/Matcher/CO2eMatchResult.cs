using System.Collections.Generic;
using System.Linq;

namespace Enterprise.TransportBookings.Business
{
	public class CO2eMatchResult
	{
		public List<ICO2eMatchAction> Actions { get; private set; }

		public CO2eMatchResult()
		{
			Actions = new List<ICO2eMatchAction>();
		}

		public void Add(ICO2eMatchAction action)
		{
			Actions.Add(action);
		}

		public bool IsValid => Actions.Find(action => action is CO2eUnLoadAction) != null;

		public override string ToString()
		{
			return string.Join("\n", Actions.Select(action => action.ToString()));
		}
	}
}
