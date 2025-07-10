using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public interface IDialog
	{
		IBusiness DataSource { get; }
		Type TypeOfForm { get; }
	}
}
