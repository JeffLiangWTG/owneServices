using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Aga.Business.Tree;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	class DeduplicationMonitoringModel : ITreeModel
	{
		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;

		internal Dictionary<string, MonitoringObjectValue> monitoringObject;

		public DeduplicationMonitoringModel(ConcurrentDictionary<string, MonitoringObjectValue> monitoringObject)
		{
			this.monitoringObject = new Dictionary<string, MonitoringObjectValue>();
			SetContext(monitoringObject);
		}

		public void SetContext(ConcurrentDictionary<string, MonitoringObjectValue> mObject)
		{
			if (mObject != null)
			{
				monitoringObject.Clear();

				foreach (var data in mObject)
				{
					monitoringObject.Add(data.Key, data.Value);
				}
			}
		}

		public IEnumerable GetChildren(TreePath treePath)
		{
			if (treePath.IsEmpty())
			{
				var dictionary = new SortedDictionary<string, MonitoringObjectValue>(new ExecutionPathComparer());

				foreach (var dict in monitoringObject)
				{
					dictionary.Add(dict.Key, dict.Value);
				}

				foreach (var dict in dictionary)
				{
					var list = dict.Value.Value as IEnumerable;
					int recordsCount;
					string debugLog = null;
					if (dict.Value.Value is string debugLogStr)
					{
						recordsCount = 0;
						list = null;
						debugLog = debugLogStr;
					}
					else
					{
						recordsCount = list?.Cast<object>().Count() ?? 0;
					}

					var executionInfo = string.Format((NoResString)"{1} Records in ({0:n0}ms)", dict.Value.ExecutionTimeInMilliseconds, recordsCount);
					DeduplicationMonitoringMethodNameItem item = new DeduplicationMonitoringMethodNameItem(dict.Key, executionInfo);
					item.Icon = GetIconFromKey(dict.Key);
					item.DebugLog = debugLog;
					GenerateQuery(list, item);
					yield return item;
				}
			}
			else
			{
				var items = new List<DeduplicationMonitoringBaseItem>();

				if (treePath.LastNode is DeduplicationMonitoringBaseItem parent)
				{
					if (parent is DeduplicationMonitoringMethodNameItem)
					{
						MonitoringObjectValue value;
						if (monitoringObject.TryGetValue(parent.ItemPath, out value))
						{
							var list = value.Value as IEnumerable;
							var subType = ValueType(value.Value);
							var counter = 0;

							if (subType != null)
							{
								foreach (var listItem in list)
								{
									counter++;

									var totalMilliseconds = (long)GetExecutionTime(listItem).TotalMilliseconds;
									var executionInfo = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}: [{1:n0}ms]", counter, totalMilliseconds);

									items.Add(new DeduplicationMonitoringTypeItem(subType.Name, executionInfo, counter, parent));
								}
							}
						}
					}
					else if (parent is DeduplicationMonitoringTypeItem)
					{
						var factory = new BusinessObjectFactory();
						MonitoringObjectValue value;

						if (monitoringObject.TryGetValue(parent.Parent.ItemPath, out value))
						{
							var list = value.Value as IEnumerable;
							var subType = ValueType(value.Value);
							var counter = 0;

							if (subType != null)
							{
								foreach (var listItem in list)
								{
									counter++;

									if (parent.Tag.ToString() == counter.ToString())
									{
										foreach (var property in subType.GetProperties())
										{
											var result = string.Empty;
											var propertyValue = property.GetValue(listItem, null);
											var val = string.Empty;
											var propertyName = property.Name;

											if ((propertyValue as ICollection) != null)
											{
												result = ((ICollection)propertyValue).Count.ToString();
											}
											else if (!string.IsNullOrEmpty((val = ReadableValue(listItem, parent, property, subType, factory))))
											{
												result = val;
												propertyName = propertyName.Replace("PK", "");
											}
											else if (propertyValue != null)
											{
												result = propertyValue.ToString();
											}

											if (propertyValue != null)
											{
												items.Add(new DeduplicationMonitoringValueItem(propertyName, result, parent));
											}
										}
									}
									else
									{
										continue;
									}
								}
							}
						}
					}

					foreach (DeduplicationMonitoringBaseItem item in items)
					{
						yield return item;
					}
				}
			}
		}

		Image GetIconFromKey(string key)
		{
			Image image = null;

			if (key.StartsWith(DeduplicationDebuggerParticipant.OrganizationPrefix, StringComparison.Ordinal))
			{
				image = Properties.Resources.Organization;
			}
			else if (key.StartsWith(DeduplicationDebuggerParticipant.PersonPrefix, StringComparison.Ordinal))
			{
				image = Properties.Resources.Person;
			}

			return image;
		}

		void GenerateQuery(IEnumerable list, DeduplicationMonitoringMethodNameItem item)
		{
			if (list is IEnumerable<DeduplicationDebuggerMaster> patternMatchingResultList)
			{
				if (patternMatchingResultList.Any())
				{
					var queries = new StringBuilder();
					var hashesByTable = new Dictionary<string, DataTable>();
					var reporter = patternMatchingResultList.First().Reporter as IPatternTableDecider;

					item.QueryTitle = ResString.GetMultilingualString("4efbafee-f3a4-482c-b928-dfa5e1d1261a", "Click here to show the queries used to find pattern matches");

					if (reporter != null)
					{
						foreach (var fieldsToSearch in reporter.FieldsToSearch.GroupBy(x => new { x.Item1, x.Item2 }).Select(x => x.First()))
						{
							var table = fieldsToSearch.Item2;
							DataTable hashes;

							if (!hashesByTable.TryGetValue(table, out hashes))
							{
								hashes = new DataTable();
								hashes.Locale = CultureInfo.InvariantCulture;
								hashes.Columns.Add((NoResString)"Value", typeof(int));
								hashesByTable.Add(table, hashes);
							}

							hashes.Rows.Add(fieldsToSearch.Item1);
						}
					}

					var execResultInserts = new List<string>();

					foreach (var tableCode in hashesByTable.Keys)
					{
						var query = string.Format(CultureInfo.InvariantCulture, "exec SP_GetPatternMatchingData @hashedValues=@hashedValueTable{0}, @tableToSearch=N'{0}'", tableCode);

						execResultInserts.Add("INSERT INTO @TempGetPatternMatchingData " + query);
					}

					queries.AppendLine();
					queries.AppendLine("DECLARE @TempGetPatternMatchingData TABLE (HashedValue int, ParentID uniqueidentifier, Organisation uniqueidentifier, CountryCode char(3), Person uniqueidentifier, TablePrefix char(3))");
					queries.AppendLine("");

					foreach (var insert in execResultInserts)
					{
						queries.AppendLine(insert);
					}
					queries.AppendLine(@"
SELECT 
	* 
FROM 
	@TempGetPatternMatchingData 
LEFT JOIN 
	dbo.OrgHeader ON OH_PK = Organisation");

					var excludeMasterPK = @"WHERE
	OH_PK <> '{0}'
";
					queries.AppendLine(string.Format(CultureInfo.InvariantCulture, excludeMasterPK, patternMatchingResultList.First().PK));
					queries.AppendLine(@"ORDER BY
	OH_PK
");

					item.Tables = hashesByTable;
					item.DBCommandText = queries.ToString();
				}
			}
		}

		string OrgHeaderReadableValue(BusinessObjectFactory factory, object curVal)
		{
			var header = factory.Load<OrgHeader>((Guid)curVal);

			return header != null && header.IsInDatabase ? header.OH_FullName + " (" + curVal.ToString() + ")" : curVal.ToString();
		}

		string OrgContactReadableValue(BusinessObjectFactory factory, object curVal)
		{
			var contact = factory.Load<OrgContact>((Guid)curVal);

			return contact != null && contact.IsInDatabase ? contact.OC_ContactName + " (" + curVal.ToString() + ")" : curVal.ToString();
		}

		string ReadableValue(object item, DeduplicationMonitoringBaseItem parent, PropertyInfo currentProperty, Type type, BusinessObjectFactory factory)
		{
			var result = string.Empty;

			if (parent.ItemPath == nameof(ScoringResult))
			{
				if (currentProperty.Name == nameof(ScoringResult.MasterPK) || currentProperty.Name == nameof(ScoringResult.TargetPK))
				{
					var property = currentProperty.Name == nameof(ScoringResult.MasterPK) ? type.GetProperty(nameof(ScoringResult.MasterType)) : type.GetProperty(nameof(ScoringResult.TargetType));
					var typeValue = (Type)property.GetValue(item, null);

					var curVal = currentProperty.GetValue(item, null);

					if (typeValue == typeof(CargoWise.Glow.Model.Interfaces.IOrgHeader))
					{
						result = OrgHeaderReadableValue(factory, curVal);
					}
					else if (typeValue == typeof(CargoWise.Glow.Model.Interfaces.IOrgContact))
					{
						result = OrgContactReadableValue(factory, curVal);
					}
				}
			}
			else
			{
				if (currentProperty.Name == nameof(PatternMatchingResultModel.OrgPK))
				{
					var currentValue = Guid.Empty;

					if ((currentValue = (Guid)currentProperty.GetValue(item, null)) != Guid.Empty)
					{
						return OrgHeaderReadableValue(factory, currentValue);
					}
				}
			}

			return result;
		}

		Type ValueType(object value)
		{
			Type valueType = null;
			if (value != null && !(value is string))
			{
				valueType = value.GetType().GetGenericArguments()[0];
			}
			return valueType;
		}

		TimeSpan GetExecutionTime(object obj)
		{
			if (obj.GetType() == typeof(ScoringResult))
			{
				return ((ScoringResult)obj).ExecutionTime;
			}
			else
			{
				return TimeSpan.Zero;
			}
		}

		public bool IsLeaf(TreePath treePath)
		{
			return treePath.LastNode is DeduplicationMonitoringValueItem;
		}

		TreePath GetPath(DeduplicationMonitoringBaseItem item)
		{
			if (item == null)
			{
				return TreePath.Empty;
			}
			else
			{
				Stack<object> stack = new Stack<object>();
				while (item != null)
				{
					stack.Push(item);
					item = item.Parent;
				}
				return new TreePath(stack.ToArray());
			}
		}

		internal void OnNodesChanged(DeduplicationMonitoringBaseItem item)
		{
			if (NodesChanged != null)
			{
				TreePath path = GetPath(item.Parent);
				NodesChanged(this, new TreeModelEventArgs(path, new object[] { item }));
			}
		}

		void OnNodesInserted(DeduplicationMonitoringBaseItem item)
		{
			if (NodesInserted != null)
			{
				TreePath path = GetPath(item.Parent);
				NodesInserted(this, new TreeModelEventArgs(path, new object[] { item }));
			}
		}

		void OnNodesRemoved(DeduplicationMonitoringBaseItem item)
		{
			if (NodesRemoved != null)
			{
				TreePath path = GetPath(item.Parent);
				NodesRemoved(this, new TreeModelEventArgs(path, new object[] { item }));
			}
		}

		public void OnStructureChanged()
		{
			StructureChanged?.Invoke(this, new TreePathEventArgs());
		}
	}
}
