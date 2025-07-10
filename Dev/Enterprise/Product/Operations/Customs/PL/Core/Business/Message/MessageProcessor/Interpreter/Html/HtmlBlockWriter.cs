using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

public sealed class HtmlBlockWriter : IHtmlBlockWriter, IDisposable
{
	public record Attribute(string Name, string Value)
	{
		public static implicit operator Attribute((string Name, string Value) tuple) => new(tuple.Name, tuple.Value);
	}

	public record StyleProperty(string Name, string Value)
	{
		public static implicit operator StyleProperty((string Name, string Value) tuple) => new(tuple.Name, tuple.Value);
	}

	public interface IWriteCallback
	{
		void Write(HtmlBlockWriter writer);
	}

	readonly StringWriter stringWriter = new();

	public void Dispose()
	{
		stringWriter.Dispose();
	}

	public void WriteStyle(string css)
	{
		WriteTag(HtmlTextWriterTag.Style);
		stringWriter.Write(css);
		EndWriteTag(HtmlTextWriterTag.Style);
	}

	public void WriteHeader(ZString caption, string level = HtmlTextWriterTag.H2, ZString @class = default, StyleProperty[] styles = null)
	{
		if (caption.IsEmpty)
		{
			return;
		}

		using (Tag(level, attributes: [(HtmlTextWriterAttribute.Class, @class)], styles))
		{
			WriteText(caption);
		}
	}

	public void WriteThematicBreak() => WriteVoidTag(HtmlTextWriterTag.Hr);

	public void WriteLineBreak() => WriteVoidTag(HtmlTextWriterTag.Br);

	public void WriteParamValueTable(IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default)
		=> WriteParamValueTable(caption: ZString.Empty, paramValues, @class, skipEmptyValues);

	public void WriteParamValueTable(ZString caption, IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default)
	{
		using (Table(attributes: [(HtmlTextWriterAttribute.Class, @class)]))
		{
			if (!caption.IsEmpty)
			{
				WriteTableCaption(caption);
			}

			using (TableBody())
			{
				foreach (var paramValue in paramValues)
				{
					if (paramValue.IsEmpty
						&& (skipEmptyValues && !paramValue.ShouldSkipEmptyValue.HasValue
							|| paramValue.ShouldSkipEmptyValue.GetValueOrDefault()))
					{
						continue;
					}

					using (TableRow())
					{
						WriteTableHeaderCell(paramValue.Name);
						WriteTableCell(paramValue);
					}
				}
			}
		}
	}

	public void WriteParamValuesTableTranspose(ZString[] titleColumns, IEnumerable<ParamValues> paramValuesList, ZString @class = default, ZBool skipEmptyValues = default, ZBool rowIndex = default)
		=> WriteParamValuesTableTranspose(caption: ZString.Empty, titleColumns, paramValuesList, @class, skipEmptyValues, rowIndex);

	public void WriteParamValuesTableTranspose(ZString caption, ZString[] titleColumns, IEnumerable<ParamValues> paramValuesList, ZString @class = default, ZBool skipEmptyValues = default, ZBool rowIndex = default)
	{
		using (Table(attributes: [(HtmlTextWriterAttribute.Class, @class)]))
		{
			if (!caption.IsEmpty)
			{
				WriteTableCaption(caption);
			}

			using (TableBody())
			{
				if (titleColumns is not null)
				{
					using (TableRow())
					{
						if (rowIndex)
						{
							WriteTableHeaderCell(ZString.Empty);
						}
						WriteTableHeaderRow(titleColumns);
					}
				}

				foreach (var paramValue in paramValuesList)
				{
					if (paramValue.IsEmpty
						&& (skipEmptyValues && !paramValue.ShouldSkipEmptyValue.HasValue
							|| paramValue.ShouldSkipEmptyValue.GetValueOrDefault()))
					{
						continue;
					}

					using (TableRow())
					{
						if (rowIndex)
						{
							WriteTableCell(paramValue.Name);
						}
						WriteTableRow(paramValue.StringValues);
					}
				}
			}
		}
	}

	public void WriteParamValueSequence(IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default)
		=> WriteParamValueSequence(caption: ZString.Empty, paramValues, @class, skipEmptyValues);

	public void WriteParamValueSequence(ZString caption, IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default)
	{
		var first = true;

		using (Paragraph(attributes: [(HtmlTextWriterAttribute.Class, @class)]))
		{
			foreach (var paramValue in paramValues)
			{
				if (first && !caption.IsEmpty)
				{
					using (Tag(HtmlTextWriterTag.H3))
					{
						WriteText(caption);
					}
				}

				if (paramValue.IsEmpty
					&& (skipEmptyValues && !paramValue.ShouldSkipEmptyValue.HasValue
						|| paramValue.ShouldSkipEmptyValue.GetValueOrDefault()))
				{
					continue;
				}

				if (!first)
				{
					WriteLineBreak();
				}
				WriteText(paramValue.Name + ": ");
				WriteText(paramValue);

				first = false;
			}
		}
	}

	public void WriteParagraph(ZString text, Attribute[] attributes = null, StyleProperty[] styles = null)
	{
		if (text.IsEmpty)
		{
			return;
		}

		using (Paragraph(attributes, styles))
		{
			WriteText(text);
		}
	}

	public void WriteInParagraph(Action<HtmlBlockWriter> writeAction, Attribute[] attributes = null, StyleProperty[] styles = null)
	{
		using (Paragraph(attributes, styles))
		{
			writeAction(this);
		}
	}

	public void WriteMultipleTextWithCaption(IEnumerable<IParamValue> paramValueEnumeration)
	{
		using var enumerator = paramValueEnumeration.Where(x => !x.IsEmpty).GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		var first = enumerator.Current;
		var next = enumerator.MoveNext() ? enumerator.Current : null;
		WriteTextWithCaption(first, writeCaption: next != null);
		while (next != null)
		{
			WriteLineBreak();
			WriteTextWithCaption(next, writeCaption: true);
			next = enumerator.MoveNext() ? enumerator.Current : null;
		}
		return;

		void WriteTextWithCaption(IParamValue paramValue, bool writeCaption)
		{
			if (writeCaption && !paramValue.Name.IsEmpty)
			{
				using (Tag(HtmlTextWriterTag.H5, styles: [
					(HtmlTextWriterStyle.MarginTop, (NoResString)"5px"),
					(HtmlTextWriterStyle.MarginBottom, (NoResString)"2px")]))
				{
					WriteText(paramValue.Name);
				}
			}

			WriteText(paramValue);
		}
	}

	public void WriteText(string str)
	{
		var index = 0;
		foreach (var line in str.SplitByLine())
		{
			if (index++ > 0)
			{
				WriteLineBreak();
			}

			stringWriter.WriteEncodedText(line);
		}
	}

	public override string ToString() => stringWriter.ToString();

	IDisposable Tag(string tag, Attribute[] attributes = null, StyleProperty[] styles = null)
		=> new DisposableAction(() => BeginWriteTag(tag, attributes, styles), () => EndWriteTag(tag));

	void BeginWriteTag(string tag, Attribute[] attributes = null, StyleProperty[] styles = null)
	{
		stringWriter.WriteBeginTag(tag);

		attributes?.Where(x => !string.IsNullOrEmpty(x.Value)).ForEach(attribute =>
			stringWriter.WriteAttribute(attribute.Name, attribute.Value));

		if (styles?.Length > 0)
		{
			var style = string.Concat(styles.Select(s => FormattableString.Invariant($"{s.Name}:{s.Value};")));
			stringWriter.WriteAttribute(HtmlTextWriterAttribute.Style, style);
		}

		stringWriter.Write(StringWriterExtensions.TagRightChar);
	}

	void WriteTag(string tag)
	{
		stringWriter.WriteBeginTag(tag);
		stringWriter.Write(StringWriterExtensions.TagRightChar);
	}

	void EndWriteTag(string endTag) => stringWriter.WriteEndTag(endTag);

	void WriteText(object value)
	{
		switch (value)
		{
			case string str:
				WriteText(str);
				break;

			case IParamStringValue paramStringValue:
				WriteText(paramStringValue.StringValue);
				break;

			case IWriteCallback writeCallback:
				writeCallback.Write(this);
				break;

			default:
				WriteText(value?.ToString());
				break;
		}
	}

	IDisposable Paragraph(Attribute[] attributes = null, StyleProperty[] styles = null) => Tag(HtmlTextWriterTag.P, attributes, styles);

	IDisposable Table(Attribute[] attributes = null, StyleProperty[] styles = null) => Tag(HtmlTextWriterTag.Table, attributes, styles);

	void WriteTableCaption(ZString caption)
	{
		using (Tag(HtmlTextWriterTag.Caption))
		using (Tag(HtmlTextWriterTag.H3))
		{
			WriteText(caption);
		}
	}
	IDisposable TableBody() => Tag(HtmlTextWriterTag.Tbody);
	IDisposable TableRow() => Tag(HtmlTextWriterTag.Tr);

	void WriteTableHeaderCell(string value)
	{
		using (Tag(HtmlTextWriterTag.Th))
		{
			WriteText(value);
		}
	}

	void WriteTableHeaderRow(ZString[] values)
	{
		foreach (var value in values)
		{
			using (Tag(HtmlTextWriterTag.Th))
			{
				WriteText(value);
			}
		}
	}

	void WriteTableCell(object value)
	{
		using (Tag(HtmlTextWriterTag.Td))
		{
			WriteText(value);
		}
	}

	void WriteTableRow(ZString[] values)
	{
		foreach (var value in values)
		{
			using (Tag(HtmlTextWriterTag.Td))
			{
				WriteText(value);
			}
		}
	}

	void WriteVoidTag(string tag)
	{
		stringWriter.WriteBeginTag(tag);
		stringWriter.Write(StringWriterExtensions.SpaceChar);
		stringWriter.Write(StringWriterExtensions.SlashChar);
		stringWriter.Write(StringWriterExtensions.TagRightChar);
	}
}
