using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public abstract class ExcelProcessor<T, TResult>
{
	public virtual void ProcessExcel(XlsFile xls)
	{
		Console.WriteLine("Processing {0}...", ProcessName);
		var data = ExcelHelper.GetDataFromExcel(xls, Converter);

		if (data.Count == 0)
		{
			Console.Error.WriteLine("No {0} found in the excel file.", ProcessName);
			return;
		}

		var refModels = Converter.ConvertToRefModels(data);
		ExportToXMLFile(refModels);
		Console.WriteLine("{0} processed.", ProcessName);
	}

	public string OutputPath { get; }

	public string FileName { get; }

	protected ExcelProcessor(string outputPath, string fileName)
	{
		OutputPath = outputPath;
		FileName = fileName;
	}

	protected virtual void ExportToXMLFile(List<TResult> refModels)
	{
		var xmlWriterConfig = GetXMLWriterConfiguration();
		XmlWriterHelper.ExportToXMLFile(DataSource, Path.Combine(OutputPath, FileName), xmlWriterConfig, DateTime.Now, refModels);
	}

	protected abstract XmlWriterConfiguration GetXMLWriterConfiguration();

	protected abstract IDataConverter<T, TResult> Converter { get; }

	protected abstract string DataSource { get; }

	protected abstract string ProcessName { get; }
}
