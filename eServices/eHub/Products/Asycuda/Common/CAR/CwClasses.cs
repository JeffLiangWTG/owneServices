using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

// Copied from C:\dev\Enterprise\Product\Operations\Customs\ASYCUDA\ASYCUDA.Business\CAR\XmlToCar
// I cannot branch from $/Dev to $/eServcies, so copying instead.
// Please view original file and this file in KDiff to see changes. 
namespace CargoWise.eHub.Products.AsycudaCustoms.Common
{	
	#region XML

	class Builder
	{
		internal Document build(string paramString)
		{
			var doc = new Document();
			doc.LoadXml(paramString);
			return doc;
		}
	}
	
	public static class XmlNodeExtender
	{
		public static string getValue(this XmlNode input)
		{
			return input.InnerXml;
		}

		public static XmlNodeList query(this XmlNode input, string xPath)
		{
			return input.SelectNodes(xPath);
		}

		internal static string getQualifiedName(this XmlNode input)
		{
			return input.Name;  
		}
		 
		internal static XmlNode getFirstChildElement(this XmlNode input, string p)
		{
			return input.SelectSingleNode(p);
		}

		internal static List<XmlNode> getChildElements(this XmlNode input)
		{
			var list = new List<XmlNode>();
			foreach (XmlNode node in input.ChildNodes)
			{
				list.Add(node);
			}
			return list;
		}
	}

	class Document : XmlDocument
	{
		internal XmlNodeList query(string paramString)
		{
			return SelectNodes(paramString);
		}

		internal XmlNode getRootElement()
		{
			return this.DocumentElement;
		}
	}
	#endregion

	static class StringExtender
	{
		public static String valueOf(int p)
		{
			return p.ToString();
		}
	}

	public static class StringExtensions
	{
		/// <summary>
		/// DJC. Java's string.substring(int, int) asks for the start index and the END INDEX.  c.f. C# which asks for the start index and the LENGTH
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="endIndex">END INDEX, not length!</param>
		/// <returns></returns>
		public static string substringJavaStyle(this string s, int startIndex, int endIndex)
		{
			return s.Substring(startIndex, endIndex - startIndex);
		}
	}

	public class Vector : ArrayList
	{
		internal void add(int index, string value)
		{
			Insert(index, value);
		}

		internal void add(int index, object value)
		{
			Insert(index, value);
		}

		internal void add(object value)
		{
			Add(value);
		}

		internal void addElement(string value)
		{
			Add(value);
		}

		internal void addElement(object value)
		{
			Add(value);
		}

		internal object elementAt(int index)
		{
			return this[index];
		}

		internal object get(int p)
		{
			return this[p];
		}

		internal int size()
		{
			return Count;
		}

		internal Vector clone()
		{
			var result = new Vector();
			for (int i = 0; i < Count; i++)
			{
				result.Insert(i, this[i]);
			}
			return result;
		}

		internal void clear()
		{
			Clear();
		}
	}

	public class Integer
	{
		private int value;

		public Integer(int value)
		{
			this.value = value;
		}

		public static Integer valueOf(int init)
		{
			return new Integer(init);
		}

		public static Integer valueOf(object init)
		{
			int value = int.Parse(init.ToString());
			return new Integer(value);
		}

		internal byte byteValue()
		{
			return Convert.ToByte(value);
		}

		internal static int parseInt(string p)
		{
			return int.Parse(p);
		}

		public override string ToString()
		{
			return value.ToString();
		}
	}
	
	class DataOutputStreamWrapper
	{
		public DataOutputStreamWrapper(Stream stream)
		{
			this.stream = stream;
		}

		internal void writeBytes(string p)
		{
			Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(p);
			stream.Write(bytes, 0, bytes.Length);
		}

		internal void writeByte(int i)
		{
			// NB, here we will be writing int{0} as byte{0} and NOT converting it to the byte value that represents the ascii "0" (i.e. we are not writing byte{48})
			var character = Convert.ToChar(i);
			byte[] bytes = new byte[1];
			System.Buffer.BlockCopy(new char[]{character}, 0, bytes, 0, bytes.Length);
			stream.Write(bytes, 0, bytes.Length);
		}

		internal void write(int i)
		{
			writeByte(i);
		}

		readonly Stream stream;
	}
}