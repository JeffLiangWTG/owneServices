using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CommonCusPermitHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;
			string applicationCode = (row != null) ? row[CusPermitHeaderSchema.Constants.CPH_ApplicationCode].ToString() : "";

			bizOType = GetType(applicationCode, row, factory);

			if (bizOType == null)
			{
				ErrorReporter.ReportOnce("CommonCusPermitHeader for Application Code '" + applicationCode + "' is unknown", "Cannot determine the CommonCusPermitHeader object for Application Code '" + applicationCode + "'");
			}

			return bizOType;
		}

		Type GetType(string applicationCode, DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (!string.IsNullOrEmpty(applicationCode))
			{
				var types = (Hashtable)ObjectFactory.Get("CommonCusPermitHeaderApplicationCodeTypes");
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
