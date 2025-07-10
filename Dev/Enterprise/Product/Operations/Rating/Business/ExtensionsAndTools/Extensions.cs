using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Enterprise.Rating.Business
{
	public static class Extensions
	{
		#region ZQuery

		public static ZQuery And(this ZQuery query, params ZQuery[] queries)
		{
			return queries.Aggregate(query, (q1, q2) => new ZQuery(q1, JoinCondition.And, q2));
		}

		public static ZQuery Or(this ZQuery query, params ZQuery[] queries)
		{
			return queries.Aggregate(query, (q1, q2) => new ZQuery(q1, JoinCondition.Or, q2));
		}

		#endregion

		#region OrgHeader

		public static IEnumerable<OrgHeader> RelatedManagementParents(this OrgHeader org)
		{
			return GetRelatedOrganisations(org, x => x.RelatedManagementParentRelations.Organisations, new List<ZGuid>());
		}

		public static IEnumerable<OrgHeader> RelatedManagementParentsAndSelf(this OrgHeader org)
		{
			yield return org;
			foreach (var relatedManagementParent in org.RelatedManagementParents())
			{
				yield return relatedManagementParent;
			}
		}

		public static IEnumerable<OrgHeader> RelatedManagementSubsidiaries(this OrgHeader org)
		{
			return GetRelatedOrganisations(org, x => x.RelatedManagementSubsidiaryRelations.Organisations, new List<ZGuid>()).ToArray();
		}

		static IEnumerable<OrgHeader> GetRelatedOrganisations(this OrgHeader org, Func<OrgHeader, IEnumerable<OrgHeader>> getRelations, List<ZGuid> incursionList)
		{
			if (org == null || incursionList.Contains(org.PK))
			{
				yield break;
			}

			incursionList.Add(org.PK);
			foreach (var parent in getRelations(org))
			{
				yield return parent;
				foreach (var relatedManagementParent in parent.GetRelatedOrganisations(getRelations, incursionList))
				{
					yield return relatedManagementParent;
				}
			}
		}

		#endregion

		#region BusinessObject

		public static void CopyToDataRow(this BusinessObject sourceBusinessObject, RowFactory rowFactory, DataRow clonedRow, IEnumerable<string> excludedColumns)
		{
			var table = rowFactory.GetTable(sourceBusinessObject.TableName);
			var sourceRow = rowFactory.LoadFromPK(sourceBusinessObject.TableName, sourceBusinessObject.PK);
			var columns = sourceRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
			var includedColumns = columns.Except(excludedColumns);

			foreach (var column in includedColumns)
			{
				clonedRow[column] = sourceRow[column];
			}

			clonedRow[ZDataUtils.GetPKNameFromTable(table)] = Guid.NewGuid();
		}

		#endregion

		public static IZType Getter(this IIndexer indexer, string propertyName)
		{
			return indexer.GetType().GetProperty(propertyName).GetValue(indexer) as IZType;
		}

		public static ZString ToTransportMode(this FreightMode freightMode)
		{
			if ((freightMode & FreightMode.AIR) == FreightMode.AIR)
			{
				return Core.Constants.TransportModes.Air;
			}

			if ((freightMode & FreightMode.SEA) == FreightMode.SEA)
			{
				return Core.Constants.TransportModes.Sea;
			}

			if ((freightMode & FreightMode.ROA) == FreightMode.ROA)
			{
				return Core.Constants.TransportModes.Road;
			}

			if ((freightMode & FreightMode.RAI) == FreightMode.RAI)
			{
				return Core.Constants.TransportModes.Rail;
			}

			return ZString.Empty;
		}

		public static string HumanReadableName(this IAutoRating ratingAdapter)
		{
			return new ZStringBuilder(HumanReadableNames(ratingAdapter)).ToStringWithDelimiterBetweenAppends(", ");
		}

		public static ZString[] HumanReadableNames(this IAutoRating ratingAdapter)
		{
			if (ratingAdapter == null)
			{
				return Array.Empty<ZString>();
			}

			var stringList = ratingAdapter.AutoRatedFor != null
				? ratingAdapter.AutoRatedFor.Where(x => x != null).Select(x => x.HumanReadableName).ToArray()
				: new ZString[] { ratingAdapter.ToString() };

			return stringList;
		}

		public static ConcurrentDictionary<TKey, TValue> ToConcurrent<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			return new ConcurrentDictionary<TKey, TValue>(dictionary);
		}

		public static string ToStringWithDelimiterBetweenStrings(this IEnumerable<string> strings, string delimiter)
		{
			return new ZStringBuilder(strings).ToStringWithDelimiterBetweenAppends(delimiter);
		}

		public static string ToStringWithNewLineBetweenStrings(this IEnumerable<string> strings)
		{
			return new ZStringBuilder(strings).ToStringWithNewLineBetweenAppends();
		}

		public static string ToStringWithNewLineBetweenStrings(this IEnumerable<ZString> strings)
		{
			return new ZStringBuilder(strings).ToStringWithNewLineBetweenAppends();
		}

		public static string ToJSON(this object obj) => JsonConvert.SerializeObject(obj, new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		});

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static string ToJsonSafe(this object obj, Formatting formatting = Formatting.None)
		{
			try
			{
				return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
				{
					Formatting = formatting,
					NullValueHandling = NullValueHandling.Ignore
				});
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string ToYAML(this object obj) => new Serializer().Serialize(obj);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static string ToYAMLSafe(this object obj)
		{
			try
			{
				//TODO: See if we can update YamlDotNet and use its new feature to ignore errors while deserializing
				return new Serializer().Serialize(obj);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static Exception Report(this Exception exception, RatingCriteria criteria)
		{
			if (!exception.Data.Contains("AutoRatingInfo"))
			{
				exception.Data.Add("AutoRatingInfo", criteria.ToXML());
			}

			return exception;
		}

		// Written as an extension since the 'criteria' can be null when called
		// by the `Report` method above. Catching the null in this method makes
		// the code neater than having to handle the null in the `Report` method
		// and in this method also.
		static internal ZString ToXML(this RatingCriteria criteria)
		{
			try
			{
				var serializer = new RatingObjectSerializer();
				return serializer.GetXML(criteria.GetRatingAdapter());
			}
			catch (Exception ex)
			{
				try
				{
					var writeString = new StringWriter();
					using (var writeXml = System.Xml.XmlWriter.Create(writeString))
					{
						WriteException(writeXml, ex);
					}
					return writeString.ToString();
				}
				catch (Exception ex2)
				{
					return $"ToXML failed and the exception could not be serialised with: {ex2.Message}";
				}
			}
		}

		static void WriteException(System.Xml.XmlWriter writer, Exception exception, string name = "Exception")
		{
			writer.WriteStartElement(name);
			writer.WriteElementString("Type", exception.GetType().Name);
			writer.WriteElementString("Message", exception.Message);
			writer.WriteElementString("Source", exception.Source);
			writer.WriteElementString("Stack", exception.StackTrace);

			if (exception.InnerException != null)
			{
				WriteException(writer, exception.InnerException, "InnerException");
			}

			writer.WriteEndElement();
		}
	}
}
