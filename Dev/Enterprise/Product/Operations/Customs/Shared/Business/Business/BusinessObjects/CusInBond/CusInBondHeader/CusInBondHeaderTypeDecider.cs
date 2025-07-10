using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusInBondHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string applicationCode = (row != null) ? row[CusInBondHeader.Schema.BH_ApplicationCode].ToString() : "";

			var bizOType = GetType(applicationCode, row, factory);

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CusInBondHeader for Application Code '" + applicationCode + "' is unknown", "Cannot determine the CusInBondHeader object for Application Code '" + applicationCode + "'");
			}

			return bizOType;
		}

		Type GetType(string applicationCode, DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (!string.IsNullOrEmpty(applicationCode))
			{
				var types = (Hashtable)ObjectFactory.Get("CusInBondHeaderApplicationCodeTypes");
				var objectHandle = (ObjectHandle)types[applicationCode];
				if (objectHandle != null)
				{
					result = objectHandle.GetObjectType();
					if (typeof(TypeDecider).IsAssignableFrom(result))
					{
						var decider = (TypeDecider)Activator.CreateInstance(result);
						result = decider.GetTypeForLoad(row, factory);
					}
				}
			}
			return result;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
