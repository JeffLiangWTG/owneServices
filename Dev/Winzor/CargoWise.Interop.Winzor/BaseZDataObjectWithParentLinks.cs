using System;
using System.ComponentModel;

namespace CargoWise.Interop.DataObjects;

public abstract class BaseZDataObjectWithParentLinks : Component
{
	#region ParentTableLink 

	public string ParentTableLink
	{
		get { return parentTableLink; }
		set { parentTableLink = value; }
	}

	string parentTableLink = string.Empty;

	#endregion

	#region ParentIDLink

	public Guid ParentIDLink
	{
		get { return parentIDLink; }
		set { parentIDLink = value; }
	}

	Guid parentIDLink = Guid.Empty;

	#endregion
}
