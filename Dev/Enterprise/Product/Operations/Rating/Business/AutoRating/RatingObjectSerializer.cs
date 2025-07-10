using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class SafeXmlTextWriter : XmlTextWriter
	{
		const char BackTick = '`';

		public SafeXmlTextWriter(TextWriter wr) : base(wr) { }

		// For generic Types, Type.GetName() will return strings containing backtick characters (`) that are illegal in XML element names
		// Refer Type.GetType() method in .Net doco
		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			if (localName.Contains(BackTick))
			{
				base.WriteStartElement(prefix, localName.Split(new char[] { BackTick }, 2)[0], ns);
			}
			else
			{
				base.WriteStartElement(prefix, localName, ns);
			}
		}
	}

	public class RatingObjectSerializer
	{
		public RatingObjectSerializer()
		{
			InitializeSupportedInterfaces();
		}

		public string GetXML(object businessEntity)
		{
			using (var writer = new StringWriter())
			using (var xr = new SafeXmlTextWriter(writer))
			{
				GenerateXml(xr, businessEntity);
				return writer.GetStringBuilder().ToString();
			}
		}

		public string GetJSON(object businessEntity)
		{
			var xml = GetXML(businessEntity);

			var doc = new XmlDocument();
			doc.LoadXml(xml);

			var json = doc.DocumentElement?.FirstChild?.ToJSON();
			return json;
		}

		void InitializeSupportedInterfaces()
		{
			foreach (var interfaceType in typeof(AutoRatingProxyBase).GetInterfaces())
			{
				supportedRatingInterfaces.Add(interfaceType, interfaceType.Name);
			}
		}

		void GenerateXml(XmlTextWriter xr, object businessEntity)
		{
			xr.WriteStartDocument();
			xr.WriteStartElement("root");

			if (businessEntity != null)
			{
				xr.WriteStartElement(businessEntity.GetType().Name);
				WriteTag(xr, businessEntity.GetType().FullName);

				var supporter = businessEntity as IRatingSupporter;

				if (supporter != null)
				{
					AddAdaptersInfo(xr, supporter, CostSell.Cost);
					AddAdaptersInfo(xr, supporter, CostSell.Revenue);
					AddAdditionalJobsInfo(xr, supporter);
				}
				else
				{
					foreach (var interfaceType in businessEntity.GetType().GetInterfaces().Where(x => supportedRatingInterfaces.Keys.Contains(x)))
					{
						Add(xr, businessEntity, interfaceType);
					}
				}

				xr.WriteEndElement();
			}

			xr.WriteEndElement();
			xr.Close();
		}

		void AddAdditionalJobsInfo(XmlTextWriter xr, IRatingSupporter supporter)
		{
			var invoicingPlugIns = supporter.AdaptersProvider.GetAdditionalJobs();

			if (invoicingPlugIns.Count > 0)
			{
				const string additionalJobs = "AdditionalJobs"; // Developer information form

				xr.WriteStartElement(additionalJobs);
				WriteTag(xr, additionalJobs);

				foreach (var additionalJob in invoicingPlugIns)
				{
					var additionalJobSupporter = additionalJob as IRatingSupporter;

					if (additionalJobSupporter != null)
					{
						xr.WriteStartElement(additionalJobSupporter.GetType().Name);
						WriteTag(xr, additionalJobSupporter.GetType().FullName);

						AddAdaptersInfo(xr, additionalJobSupporter, CostSell.Cost);
						AddAdaptersInfo(xr, additionalJobSupporter, CostSell.Revenue);

						xr.WriteEndElement();
					}
				}

				xr.WriteEndElement();
			}
		}

		void AddAdaptersInfo(XmlTextWriter xr, IRatingSupporter supporter, CostSell costOrSell)
		{
			xr.WriteStartElement(costOrSell.ToString());
			var interactor = new ExplorerLogger();
			var options = costOrSell == CostSell.Cost
				? AutoRateOptions.AutorateCosts
				: AutoRateOptions.AutorateRevenue;

			foreach (var adapter in supporter.AdaptersProvider.GetAdapters(interactor, options))
			{
				xr.WriteStartElement(adapter.GetType().Name);
				WriteTag(xr, adapter.GetType().FullName);

				foreach (var adapterInterface in adapter.GetType().GetInterfaces().Where(x => supportedRatingInterfaces.Keys.Contains(x)))
				{
					Add(xr, adapter, adapterInterface);
				}

				xr.WriteEndElement();
			}

			if (interactor.Logs.Any())
			{
				WriteTag(xr, interactor.Logs.ToStringWithNewLineBetweenStrings());
			}

			xr.WriteEndElement();
		}

		void Add(XmlTextWriter xr, object value, Type interfaceType)
		{
			if (!interfaceType.IsInstanceOfType(value) || !supportedRatingInterfaces.ContainsKey(interfaceType))
			{
				return;
			}

			xr.WriteStartElement(supportedRatingInterfaces[interfaceType]);
			WriteTag(xr, value.GetType().FullName);

			foreach (var property in interfaceType.GetProperties())
			{
				if (property.Name == nameof(IAutoRatingFreightInfo.RateableMeasures))
				{
					var measures = (RateableMeasureSet)property.GetValue(value, null);
					AddMeasures(xr, measures);
				}
				else if (property.CanRead)
				{
					Add(xr, property.GetValue(value, null), property.Name);
				}
			}

			xr.WriteEndElement();
		}

		void Add(XmlTextWriter xr, object value, string name)
		{
			if (!objectsBeingProcessed.Contains(value))
			{
				try
				{
					objectsBeingProcessed.Add(value);

					if (CanSetNode(value))
					{
						xr.WriteStartElement(name);
						SetNode(xr, value, name);
						xr.WriteEndElement();
					}
				}
				finally
				{
					objectsBeingProcessed.Remove(value);
				}
			}
		}

		readonly List<object> objectsBeingProcessed = new List<object>();

		bool CanSetNode(object value)
		{
			var valueAsJobServiceInfo = value as JobServiceInfo;

			if (valueAsJobServiceInfo != null)
			{
				return valueAsJobServiceInfo.IsEnabled;
			}

			return true;
		}

		#region SuppressResourceStringsCheckRegion

		void AddMeasures(XmlTextWriter xr, RateableMeasureSet measures)
		{
			const string LegacyMeasuresName = "Measures";
			xr.WriteStartElement(LegacyMeasuresName);

			bool hasMeasures = false;

			foreach (var measureType in measures.GetMeasureTypes())
			{
				xr.WriteStartElement(measureType.ToString());

				WriteTag(xr, measures.ValueAndUnitString(measureType));

				xr.WriteEndElement();
				hasMeasures = true;
			}

			AddMeasureErrors(xr, measures);

			if (!hasMeasures)
			{
				WriteEmptyCollection(xr);
			}

			xr.WriteEndElement();
		}

		void AddMeasureErrors(XmlTextWriter xr, RateableMeasureSet measures)
		{
			xr.WriteStartElement("MeasureErrors");
			var errors = measures.InvalidMeasuresWithErrors;
			foreach (var item in errors)
			{
				Add(xr, string.Join("\r\n", errors[item.Key].ToArray()), item.Key.ToString() + "Errors");
			}
			xr.WriteEndElement();
		}

		string GetJobServiceInfoDescription(JobServiceInfo jobServiceInfo, string elementText)
		{
			var properties = jobServiceInfo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var stringBuilder = new ZStringBuilder();

			foreach (var property in properties.Where(property => property.CanRead))
			{
				var value = property.GetValue(jobServiceInfo);
				var valueAsString = ConvertSimpleValueToString(value, elementText) ?? "(Unexpected value)";
				stringBuilder.AppendFormat("{0}: {1}\r\n", property.Name, valueAsString);
			}
			return stringBuilder.ToString();
		}

		string ConvertSimpleValueToString(object value, string elementText)
		{
			var valueType = value?.GetType();

			if (value == null)
			{
				return "(null)";
			}
			else if (value is ZGuid valueAsZGuid)
			{
				SchemaColumn column = null;
				Type type = null;

				if (elementText == nameof(MeasureDimension.ContainerType))
				{
					column = RefContainerSchema.RC_Code;
					type = typeof(RefContainer);
				}
				else if (elementText == nameof(MeasureDimension.Product))
				{
					column = OrgSupplierPartSchema.OP_PartNum;
					type = typeof(OrgSupplierPart);
				}
				else if (elementText == nameof(MeasureDimension.Warehouse))
				{
					column = WhsWarehouseSchema.WW_WarehouseCode;
					type = ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouse>();
				}

				if (column != null)
				{
					var bo = new BusinessObjectFactory().Load(type, valueAsZGuid);
					return ZString.Format("{0} {{{1}}}", bo != null ? bo[column] : "", valueAsZGuid);
				}
				else
				{
					return value.ToString();
				}
			}
			else if (simpleProperties.Contains(valueType) || valueType.IsEnum)
			{
				var valueAsString = value.ToString();

				return string.IsNullOrEmpty(valueAsString) ? "(empty)" : valueAsString;
			}
			else if (value is TimeSpan valueAsTimeSpan)
			{
				return valueAsTimeSpan.ToHoursAndMinutesString();
			}
			else if (value is OrgHeader valueAsOrgHeader)
			{
				return ZString.Format("{0}\r\n{1}", valueAsOrgHeader.OH_Code, valueAsOrgHeader.OH_FullNameTruncated);
			}
			else if (value is CodeDescriptionPair valueAsCodeDescriptionPair)
			{
				return ZString.Format("{0} {1}", valueAsCodeDescriptionPair.Code, valueAsCodeDescriptionPair.Description);
			}
			else if (value is OrgAddress valueAsOrgAddress)
			{
				return valueAsOrgAddress.OA_Code;
			}
			else if (value is JobDocAddress valueAsJobDocAddress)
			{
				return valueAsJobDocAddress.AddressAsASingleLine;
			}
			else if (value is ILocation valueAsILocation)
			{
				return valueAsILocation.Code.ToString();
			}
			else
			{
				return null;
			}
		}

		void SetNode(XmlTextWriter xr, object value, string elementText)
		{
			var valueAsString = ConvertSimpleValueToString(value, elementText);
			// actually check for null. Not empty string or otherwise.
			if (valueAsString != null)
			{
				xr.WriteValue(valueAsString);
				return;
			}
			else if (value is JobServiceInfo valueAsJobServiceInfo)
			{
				xr.WriteValue(GetJobServiceInfoDescription(valueAsJobServiceInfo, elementText));
			}
			else if (value is IEnumerable && !(value is NonPersistentBusinessObject))
			{
				var collection = (IEnumerable)value;
				var i = 0;

				foreach (var elem in collection)
				{
					Add(xr, elem, GetCollectionElementName(collection, elem, i++));
				}

				if (i == 0)
				{
					WriteEmptyCollection(xr);
				}
			}
			else
			{
				var serialized = RatingTypeSerializer.Serialize(value);
				xr.WriteValue(serialized);
			}
		}

		static void WriteEmptyCollection(XmlTextWriter xr)
		{
			WriteTag(xr, "(empty collection)");
		}

		#endregion

		string GetCollectionElementName(IEnumerable collection, object elem, int i)
		{
			var typeOfCollection = collection.GetType();

			if (typeOfCollection == typeof(Creditors))
			{
				var chargeCodeGroup = ((Creditors)collection).FindChargeCodeGroup(elem);

				if (!string.IsNullOrEmpty(chargeCodeGroup))
				{
					return chargeCodeGroup;
				}
			}

			return Regex.Escape(ZString.Format("element{0}", i));
		}

		readonly HashSet<Type> simpleProperties = new HashSet<Type>
		{
			typeof(ZString),
			typeof(ZInt),
			typeof(ZDecimal),
			typeof(ZDateTime),
			typeof(ZBool),
			typeof(Money),
			typeof(AutoRatingStatusInfo),
			typeof(string),
			typeof(bool),
			typeof(int),
		};

		static void WriteTag(XmlTextWriter xr, string value)
		{
			xr.WriteElementString(tagElement, value);
		}

		public const string tagElement = "tagEl";

		readonly IDictionary<Type, string> supportedRatingInterfaces = new Dictionary<Type, string>();

		class ExplorerLogger : IAutoRatingInteractor
		{
			public List<string> Logs = new List<string>();

			public void Log(LogType type, string message)
			{
				if (type == LogType.Error)
				{
					Logs.Add((NoResString)"Error: " + message); // development tool only
				}
			}

			public void Log(LogType type, string message, Exception ex)
			{
				if (type == LogType.Error)
				{
					Logs.Add((NoResString)"Error: " + message + (NoResString)" :-Exception[" + ex.GetType().FullName + (NoResString)"] - " + ex.Message); // development tool only
				}
			}

			public bool YesNoWarning(string message)
			{
				return true;
			}
		}
	}
}

