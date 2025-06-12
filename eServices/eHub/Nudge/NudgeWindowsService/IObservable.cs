using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Nudge
{
	public interface IObservable
	{
		void RegisterSubscriber(IObserver subscriber);
	}
}
