using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	public abstract class XmlSerialisableCollection<T, P> : NonPersistentBusinessObjectCollection<T>, IXmlSerializable
		where T : NonPersistentBusinessObject, IXmlSerializable
		where P : BusinessObject, IStmNoteParent
	{
		public XmlSerialisableCollection(P parentBizO)
			: base(parentBizO.Factory)
		{
			this.parentBizO = parentBizO;
		}

		public override void Load()
		{
			if (!IsLoaded)
			{
				ReadFromNote();
				IsLoaded = true;
			}
		}

		public void Save()
		{
			WriteToNote();
		}

		#region Hidden Note

		void ReadFromNote()
		{
			XmlSerialisableCollectionHiddenNote hiddenNote = new XmlSerialisableCollectionHiddenNote(this);
			ZBlob data = hiddenNote.GetData();
			if (!data.IsEmpty)
			{
				using (MemoryStream memoryStream = new MemoryStream(data))
				using (XmlTextReader reader = new XmlTextReader(memoryStream))
				{
					reader.ReadToFollowing(XmlCollectionName);
					((IXmlSerializable)this).ReadXml(reader);
				}
			}
		}

		void WriteToNote()
		{
			using (MemoryStream memoryStream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(XmlCollectionName);
				((IXmlSerializable)this).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();

				writer.Flush();

				XmlSerialisableCollectionHiddenNote hiddenNote = new XmlSerialisableCollectionHiddenNote(this);
				hiddenNote.SetData(memoryStream.ToArray());
			}
		}

		class XmlSerialisableCollectionHiddenNote : HiddenNote
		{
			public XmlSerialisableCollectionHiddenNote(XmlSerialisableCollection<T, P> collection)
				: base(collection.parentBizO, false)
			{
				this.collection = collection;
			}

			public ZBlob GetData()
			{
				return (!IsEmpty) ? Note.ST_NoteData : ZBlob.Empty;
			}

			public void SetData(byte[] data)
			{
				Note.ST_NoteData = new ZBlob(data);
			}

			protected override ZString Description
			{
				get { return collection.XmlCollectionName; }
			}

			protected override bool IsEmpty
			{
				get { return !HasNote || Note.ST_NoteData.IsEmpty; }
			}

			readonly XmlSerialisableCollection<T, P> collection;
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (!reader.IsEmptyElement)
			{
				reader.ReadStartElement();
				while (reader.IsStartElement(XmlElementName))
				{
					BusinessObject newBizO = AddNew();
					using (newBizO.SuspendSettingHasChanges())
					{
						((IXmlSerializable)newBizO).ReadXml(reader);
					}
				}
				reader.ReadEndElement();
			}
			else
			{
				reader.Skip();
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (IXmlSerializable bizO in this)
			{
				writer.WriteStartElement(XmlElementName);
				bizO.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		protected virtual string XmlCollectionName { get { return this.GetType().Name; } }
		protected virtual string XmlElementName { get { return typeof(T).Name; } }

		public readonly P parentBizO;
	}
}

// Tested in concrete classes
