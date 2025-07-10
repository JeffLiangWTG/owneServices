using System;
using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.HtmlBlockWriter;
using Attribute = Enterprise.Customs.PL.Business.HtmlBlockWriter.Attribute;

namespace Enterprise.Customs.PL.Business;

public interface IHtmlBlockWriter
{
	void WriteStyle(string css);
	void WriteHeader(ZString caption, string headerTag = HtmlTextWriterTag.H2, ZString @class = default, StyleProperty[] styles = null);
	void WriteThematicBreak();
	void WriteLineBreak();
	void WriteParamValueTable(IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default);
	void WriteParamValueTable(ZString caption, IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default);
	void WriteParamValuesTableTranspose(ZString[] titleColumns, IEnumerable<ParamValues> paramValuesList, ZString @class = default, ZBool skipEmptyValues = default, ZBool rowIndex = default);
	void WriteParamValuesTableTranspose(ZString caption, ZString[] titleColumns, IEnumerable<ParamValues> paramValuesList, ZString @class = default, ZBool skipEmptyValues = default, ZBool rowIndex = default);
	void WriteParamValueSequence(IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default);
	void WriteParamValueSequence(ZString caption, IEnumerable<IParamValue> paramValues, ZString @class = default, ZBool skipEmptyValues = default);
	void WriteParagraph(ZString text, Attribute[] attributes = null, StyleProperty[] styles = null);
	void WriteInParagraph(Action<HtmlBlockWriter> writeAction, Attribute[] attributes = null, StyleProperty[] styles = null);
	void WriteMultipleTextWithCaption(IEnumerable<IParamValue> paramValueEnumeration);
	void WriteText(string str);
	string ToString();
}
