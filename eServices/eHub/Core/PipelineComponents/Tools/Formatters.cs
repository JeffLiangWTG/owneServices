using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public abstract class BaseFormatter : IFormatter
	{
		public virtual SerializationBinder Binder
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public virtual StreamingContext Context
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public virtual ISurrogateSelector SurrogateSelector
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public abstract void Serialize(Stream stm, object obj);
		public abstract object Deserialize(Stream stm);
	}

	public class RawStringFormatter : BaseFormatter
	{
		public override void Serialize(Stream stream, object obj)
		{
			var rawString = obj as RawString;
			byte[] byteArray = rawString.ToByteArray();
			stream.Write(byteArray, 0, byteArray.Length);
		}

		public override object Deserialize(Stream stream)
		{
			var streamReader = new StreamReader(stream, true);
			string result = streamReader.ReadToEnd();
			return new RawString(result);
		}
	}

	[CustomFormatter(typeof(RawStringFormatter))]
	[Serializable]
	public class RawString
	{
		[XmlIgnore]
		string rawString;

		public RawString(string str)
		{
			if (str == null) throw new ArgumentNullException();

			rawString = str;
		}

		public RawString()
		{
		}

		public byte[] ToByteArray()
		{
			return Encoding.UTF8.GetBytes(rawString);
		}

		public override string ToString()
		{
			return rawString;
		}
	}

}
