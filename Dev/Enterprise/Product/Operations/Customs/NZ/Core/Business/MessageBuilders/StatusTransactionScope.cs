using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business
{
	public class StatusTransactionScope
	{
		public StatusTransactionScope()
		{
			propertyInfoValues = new Dictionary<ZPropertyInfo, IZType>();
			logEntries = new List<StmALog>();
		}

		readonly Dictionary<ZPropertyInfo, IZType> propertyInfoValues;
		readonly List<StmALog> logEntries;

		public void Add(params ZPropertyInfo[] infos)
		{
			if (infos != null)
			{
				foreach (var info in infos)
				{
					if (info != null)
					{
						propertyInfoValues[info] = info.OriginalValue;
					}
				}
			}
		}

		public void AddLog(StmALog log)
		{
			logEntries.Add(log);
		}

		public void Rollback()
		{
			foreach (var infoValue in propertyInfoValues)
			{
				var propertyInfo = infoValue.Key;

				if (!(propertyInfo.BizObj?.IsDeleted ?? true))
				{
					var originalValue = infoValue.Value;

					if (!(propertyInfo.Value?.Equals(originalValue) ?? false))
					{
						propertyInfo.Value = originalValue;
					}
				}
			}

			foreach (var log in logEntries)
			{
				log?.Delete();
			}
		}
	}
}
