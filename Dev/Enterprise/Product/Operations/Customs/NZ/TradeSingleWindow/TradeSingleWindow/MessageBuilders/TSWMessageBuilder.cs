using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public enum TSWTransactionTypes { None = 0, Cancel = 1, Change = 4, Replace = 5, Original = 9, Completion = 22 }

	public abstract class TSWMessageBuilderVersion1<T> : TSWMessageBuilderBase<T>
	{
		protected TSWMessageBuilderVersion1()
			: base(Version, CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1.DocumentMetadata.NzCountryCode.Nz.GetXmlEnumAttributeFromValue())
		{ }

		const string Version = "V1.0";
		protected abstract CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1.DocumentMetadata.WcoDocumentNameCode WCODocumentName { get; }
		protected abstract CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1.DocumentMetadata.NzDocumentNameCode NZDocumentName { get; }

		protected sealed override string GetNZDocumentName()
		{
			return NZDocumentName.GetXmlEnumAttributeFromValue();
		}

		protected override string GetWCODocumentName()
		{
			return WCODocumentName.GetXmlEnumAttributeFromValue();
		}
	}

	public abstract class TSWMessageBuilder<T> : TSWMessageBuilderBase<T>
	{
		protected TSWMessageBuilder()
			: base(Version, CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata.NzCountryCode.Nz.GetXmlEnumAttributeFromValue())
		{ }

		const string Version = "V1.1";
		protected abstract CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata.WcoDocumentNameCode WCODocumentName { get; }
		protected abstract CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata.NzDocumentNameCode NZDocumentName { get; }

		protected sealed override string GetNZDocumentName()
		{
			return NZDocumentName.GetXmlEnumAttributeFromValue();
		}

		protected override string GetWCODocumentName()
		{
			return WCODocumentName.GetXmlEnumAttributeFromValue();
		}
	}

	public abstract class TSWMessageBuilderBase<T>
	{
		protected TSWMessageBuilderBase(string version, string nzCountryCode)
		{
			this.version = Argument.NotNull(version, nameof(version));
			this.nZCountryCode = Argument.NotNull(nzCountryCode, nameof(nZCountryCode));
			generated = false;
		}

		public abstract ZString MessageType { get; }
		protected abstract T DeclarationMessage();
		public abstract ZBool DeclarantPinRequired { get; }
		public abstract ZString DeclarantPinEncrypted { get; }
		protected abstract string GetWCODocumentName();
		protected abstract string GetNZDocumentName();
		readonly string nZCountryCode;
		readonly string version;

		public string GetXMLMessage()
		{
			string xmlMessage = Regex.Replace(GetMessageText(), "[~#£≠β≥]", "");
			var reg = new Regex(@"(<((?!<|>|/).)*>){1}((?!<|>).)*(</((?!<|>).)*>){1}", RegexOptions.Singleline);
			var matches = reg.Matches(xmlMessage);
			foreach (var match in matches)
			{
				var matchStr = match.ToString();
				if (matchStr.Contains("\r") || matchStr.Contains("\n") || matchStr.Contains("\t"))
				{
					var replaceStr = matchStr.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
					xmlMessage = xmlMessage.Replace(matchStr, replaceStr);
				}
			}
			return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(xmlMessage));
		}

		public string GetMessageText()
		{
			GenerateIfNotAlreadyGenerated();
			return SerializeToXML(DeclarationMessage(), GetWCODocumentName(), GetNZDocumentName(), nZCountryCode, version);
		}

		bool generated;
		protected void GenerateIfNotAlreadyGenerated()
		{
			if (!generated)
			{
				generated = true;
			}
		}

		protected ZString GetTransactionTypeCode(TSWTransactionTypes transactionType)
		{
			var result = ZString.Empty;

			switch (transactionType)
			{
				case TSWTransactionTypes.Original:
					result = TransactionTypeList.Codes.Original;
					break;
				case TSWTransactionTypes.Cancel:
					result = TransactionTypeList.Codes.Cancel;
					break;
				case TSWTransactionTypes.Change:
					result = TransactionTypeList.Codes.Change;
					break;
				case TSWTransactionTypes.Replace:
					result = TransactionTypeList.Codes.Replace;
					break;
				case TSWTransactionTypes.Completion:
					result = TransactionTypeList.Codes.Completion;
					break;
			}

			return result;
		}

		protected byte[] GetATTACHEDAsBytes()
		{
			const string str = "ATTACHED";
			return System.Convert.FromBase64String(str);
		}

		protected X CurrencyID<X>(ZString currencyCode) where X : struct, IConvertible
		{
			return Extensions.GetXmlEnumValueFromAttribute<X>(currencyCode);
		}

		protected X MeasurementType<X>(ZString measurementCode) where X : struct, IConvertible
		{
			return Extensions.GetXmlEnumValueFromAttribute<X>(measurementCode);
		}

		protected X PopulateMimeCode<X>(ZString fileName, X defaultValue) where X : struct, IConvertible
		{
			var fileExtension = Path.GetExtension(fileName).ToUpper();
			if (!fileExtensionToMimeTypeDictionary.TryGetValue(fileExtension, out var mimeCodeDesc))
			{
				mimeCodeDesc = defaultValue.ToString();
			}

			return Enum.TryParse<X>(mimeCodeDesc, out var result) && Enum.IsDefined(typeof(X), result) ? result : defaultValue;
		}

		readonly ImmutableDictionary<string, string> fileExtensionToMimeTypeDictionary = new Dictionary<string, string>
		{
			{ ".PDF", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ApplicationPdf) },
			{ ".DOCX", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ApplicationVndOpenxmlformatsOfficedocumentWordprocessingmlDocument) },
			{ ".CSV", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.TextCsv) },
			{ ".XLSX", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet) },
			{ ".TIF", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ImageTiff) },
			{ ".GIF", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ImageGif) },
			{ ".PNG", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ImagePng) },
			{ ".JPEG",nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ImageJpeg) },
			{ ".JPG", nameof(CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing.MimeMediaTypeContentType.ImageJpeg) }
		}.ToImmutableDictionary();

		#region XML Serialization

		static string SerializeToXML(T message, string wCODocumentName, string nZDocumentName, string nZCountryCode, string version)
		{
			string result = "";

			using (var declarationWriter = new StringWriterWithEncoding())
			{
				var xmlSerialiser = ZXmlSerializer.New(typeof(T));
				using (var fragmentWriter = new XmlFragmentWriter(declarationWriter))
				{
					fragmentWriter.Formatting = Formatting.Indented;
					xmlSerialiser.Serialize(fragmentWriter, message);

					using (var documentMetadataWriter = new StringWriterWithEncoding())
					{
						using (var xmlWriter = new XmlTextWriter(documentMetadataWriter))
						{
							xmlWriter.WriteStartDocument();
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteStartElement("DocumentMetadata", "urn:wco:datamodel:WCO:DM:1");
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteElementString("WCODataModelVersion", "3.2");
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteElementString("WCODocumentName", wCODocumentName);
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteElementString("CountryCode", nZCountryCode);
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteElementString("AgencyAssignedCustomizedDocumentName", nZDocumentName);
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteElementString("AgencyAssignedCustomizedDocumentVersion", version);
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteRaw(declarationWriter.ToString());
							xmlWriter.WriteWhitespace("\n");
							xmlWriter.WriteEndDocument();
							xmlWriter.Close();      // CodeAnalysis is complaining about object being disposed multiple times.... CA2202
						}

						result = documentMetadataWriter.ToString();
						documentMetadataWriter.Close(); // CodeAnalysis is complaining about object being disposed multiple times.... CA2202
					}

					declarationWriter.Close();  // CodeAnalysis is complaining about object being disposed multiple times.... CA2202
				}
			}

			return result;
		}

		class StringWriterWithEncoding : StringWriter
		{
			readonly Encoding encoding;

			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Irrelevant for NZ xml message generation")]
			public StringWriterWithEncoding()
				: base(new StringBuilder())
			{
				encoding = Encoding.UTF8;
			}

			public override Encoding Encoding
			{
				get { return encoding; }
			}
		}

		class XmlFragmentWriter : XmlTextWriter
		{
			public XmlFragmentWriter(TextWriter w) : base(w) { }

			bool _skip;

			public override void WriteStartElement(string prefix, string localName, string ns)
			{
				if (string.IsNullOrEmpty(ns))
				{
					ns = null;
				}

				base.WriteStartElement(prefix, localName, ns);
			}

			public override void WriteStartAttribute(string prefix, string localName, string ns)
			{
				if (prefix == "xmlns" && (localName == "xsd" || localName == "xsi"))
				{
					_skip = true;
					return;
				}

				base.WriteStartAttribute(prefix, localName, ns);
			}

			public override void WriteString(string text)
			{
				if (_skip)
				{
					return;
				}

				base.WriteString(text);
			}

			public override void WriteEndAttribute()
			{
				if (_skip)
				{
					_skip = false;
					return;
				}

				base.WriteEndAttribute();
			}

			public override void WriteStartDocument()
			{
			}
		}

		#endregion

		#region Pointer Elements

		#region Bills

		internal void StoreBillsPointedTo(IDeclaration declaration)
		{
			int count = declaration.MasterBills.Count();
			foreach (IAssociatedTransportDocument bill in declaration.AllBills)
			{
				count++;
				bill.MessageSequence = count;
				BillsPointedTo.Add(bill.PK, bill);
			}
		}

		internal Dictionary<ZGuid, IAssociatedTransportDocument> BillsPointedTo
		{
			get
			{
				return billsPointedTo ?? (billsPointedTo = new Dictionary<ZGuid, IAssociatedTransportDocument>());
			}
		}
		Dictionary<ZGuid, IAssociatedTransportDocument> billsPointedTo;

		internal int GetBillElementSequence(ZGuid billPK)
		{
			int result = 0;
			IAssociatedTransportDocument pointedToBillWrapper = null;
			if (BillsPointedTo.TryGetValue(billPK, out pointedToBillWrapper))
			{
				result = pointedToBillWrapper.MessageSequence;
			}

			return result;
		}

		internal IAssociatedTransportDocument GetBillElementDetails(ZGuid billPK)
		{
			IAssociatedTransportDocument pointedToBillWrapper = null;
			if (BillsPointedTo.TryGetValue(billPK, out pointedToBillWrapper))
			{
			}

			return pointedToBillWrapper;
		}

		#endregion

		#region Equipment

		internal void StoreEquipmentPointedTo(IEnumerable<ITransportEquipment> containers)
		{
			int count = 1;
			containersPointedTo = null;
			foreach (ITransportEquipment container in containers.OrderBy(x => x.ContainerNumber).ThenBy(x => x.PK).Where(x => !ContainersPointedTo.ContainsKey(x.PK)))
			{
				container.MessageSequence = count++;
				ContainersPointedTo.Add(container.PK, container);
			}
		}

		internal Dictionary<ZGuid, ITransportEquipment> ContainersPointedTo
		{
			get
			{
				return containersPointedTo ?? (containersPointedTo = new Dictionary<ZGuid, ITransportEquipment>());
			}
		}
		Dictionary<ZGuid, ITransportEquipment> containersPointedTo;

		internal int GetContainerElementSequence(ZGuid packGroupPK)
		{
			int result = 0;
			ITransportEquipment pointedToEquipmentWrapper = null;
			if (ContainersPointedTo.TryGetValue(packGroupPK, out pointedToEquipmentWrapper))
			{
				result = pointedToEquipmentWrapper.MessageSequence;
			}

			return result;
		}

		#endregion

		#region Stuffing Locations

		internal void StoreStuffingLocationsPointedTo(IICRConsignment consignment)
		{
			int count = 1;
			foreach (ITransportEquipment container in consignment.Containers)
			{
				ZInt pointedToStuffingLocation = 0;
				if (container.StuffingLocation.IsValid)
				{
					if (!StuffingLocationPointedTo.TryGetValue(container.PK, out pointedToStuffingLocation))
					{
						StuffingLocationPointedTo.Add(container.PK, count++);
					}
				}
			}
		}

		internal Dictionary<ZGuid, ZInt> StuffingLocationPointedTo
		{
			get
			{
				return stuffingLocationPointedTo ?? (stuffingLocationPointedTo = new Dictionary<ZGuid, ZInt>());
			}
		}
		Dictionary<ZGuid, ZInt> stuffingLocationPointedTo;

		internal int GetStuffingLocationSequence(ZGuid containerPK)
		{
			ZInt stuffingSequence = 0;
			StuffingLocationPointedTo.TryGetValue(containerPK, out stuffingSequence);
			return stuffingSequence;
		}

		#endregion

		#region Packaging

		internal void StorePackagingPointedTo(IDeclaration declaration)
		{
			int count = 1;
			foreach (IPackaging packaging in declaration.Packaging)
			{
				if (packaging.NumberOfPackages > 0)
				{
					packaging.MessageSequence = count;
					count++;
					PackagingPointedTo.Add(packaging.PK, packaging);
				}
			}
		}

		internal Dictionary<ZGuid, IPackaging> PackagingPointedTo
		{
			get
			{
				return packagingPointedTo ?? (packagingPointedTo = new Dictionary<ZGuid, IPackaging>());
			}
		}
		Dictionary<ZGuid, IPackaging> packagingPointedTo;

		internal int GetPackagingElementSequence(ZGuid packagePK)
		{
			int result = 0;
			IPackaging pointedToPackagingWrapper = null;
			if (PackagingPointedTo.TryGetValue(packagePK, out pointedToPackagingWrapper))
			{
				result = pointedToPackagingWrapper.MessageSequence;
			}

			return result;
		}

		#endregion

		#endregion
	}

	public static class TSWConstants
	{
		public const string SendersReferencePlaceHolder = "||SNDREFPHLDR||";
	}
}
