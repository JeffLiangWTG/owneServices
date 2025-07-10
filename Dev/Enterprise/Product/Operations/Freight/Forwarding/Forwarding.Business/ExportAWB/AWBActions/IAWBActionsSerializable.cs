using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	internal interface IAWBActionsSerializable : IXmlSerializable
	{
		string SettingsCacheName { get; }
		void LoadSettings();
		void SaveSettings();
		List<ZPropertyInfo> GetPropertiesToSerialize();
		IDisposable GetValidationSuspender();
		IDisposable SuspendSettingHasChanges();
	}
}
