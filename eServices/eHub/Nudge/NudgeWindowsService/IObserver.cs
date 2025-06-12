using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Nudge
{
	public interface IObserver
	{
		void SubscribeTo(IObservable topic);
		void OnNotify(IObservable topic);
	}
}
