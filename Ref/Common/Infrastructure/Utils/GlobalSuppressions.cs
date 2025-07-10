// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Suppress it in the project only do not suppress it globally because this method is used in test only.", Scope = "member", Target = "~M:CargoWise.RefDbRepo.Common.Utils.OdbcDriverHelper.GetMicrosoftAccessDriverCore(System.String)~System.String")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "The new API does not support netstandard2.0 and only suppresses .net8.0 warnings", Scope = "member", Target = "~M:CargoWise.RefDbRepo.Common.Utils.LogWrapper.GetSerilogSettings(System.String)~System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}}")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "The new API does not support netstandard2.0 and only suppresses .net8.0 warnings", Scope = "member", Target = "~M:CargoWise.RefDbRepo.Common.Utils.DataSetStructureProvider.GetDataSetsStructureFromTTFile~System.Collections.Generic.IEnumerable{System.String[]}")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Do not dispose, suppress this warning because it's part of external api we need to follow, see https://github.com/wespday/serilog-sinks-kafka/blob/master/src/Serilog.Sinks.Kafka/LoggerConfigurationKafkaExtensions.cs  for example", Scope = "member", Target = "~M:CargoWise.RefDbRepo.Common.Utils.KafkaSinkLoggerConfigurationExtensions.Kafka(Serilog.Configuration.LoggerSinkConfiguration,System.Int32,System.Int32,System.String,System.String,Confluent.Kafka.SecurityProtocol,System.Nullable{System.Int32})~Serilog.LoggerConfiguration")]
