using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public delegate void WriteValueDelegate<in TDataSource>(HtmlBlockWriter writer, TDataSource dataSource);
public delegate bool CheckIsEmptyDelegate<in TDataSource>(TDataSource dataSource);

public sealed record HtmlBlockWriterParam<TDataSource>(
	ZString Name,
	TDataSource DataSource,
	WriteValueDelegate<TDataSource> WriteValueCallback,
	CheckIsEmptyDelegate<TDataSource> CheckIsEmptyCallback,
	bool? ShouldSkipEmptyValue = null)
	: IParamValue, HtmlBlockWriter.IWriteCallback
{
	public HtmlBlockWriterParam(
		ZString name,
		TDataSource dataSource,
		WriteValueDelegate<TDataSource> writeValueCallback,
		bool? shouldSkipEmptyValue = null)
		: this(name, dataSource, writeValueCallback, _ => false, shouldSkipEmptyValue)
	{
	}

	public bool IsEmpty => CheckIsEmptyCallback(DataSource);

	public void Write(HtmlBlockWriter writer)
	{
		if (ShouldSkipEmptyValue != false || !IsEmpty)
		{
			WriteValueCallback(writer, DataSource);
		}
	}
}
