using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxy.Interface.Converters;
using CargoWise.Data.SqlProxy.Interface.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NLog;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.SqlProxyServer.Server;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer")]
public static class SqlProxyApi
{
	[ThreadSafe]
	static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages")]
	static void BuildApp(WebApplication app, ISqlProxy glowLoader)
	{
		app.MapPost(SqlProxyUrls.Scalar, async (SqlProxyRequest request, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.ExecuteScalarAsync(request, ct), request));

		app.MapPost(SqlProxyUrls.NonQuery, async (SqlProxyRequest request, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.ExecuteNonQueryAsync(request, ct), request));

		_ = app.MapPost(SqlProxyUrls.Reader,
			async (SqlProxyRequest request, HttpContext httpContext, CancellationToken ct) =>
			{
				try
				{
					// Set the response headers
					httpContext.Response.ContentType = "application/json";
					httpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

					// Get the response stream
					await using var responseStream = httpContext.Response.BodyWriter.AsStream();
					await using var streamWriter = new StreamWriter(responseStream);
					streamWriter.AutoFlush = true;

					// Assume that the reader in this case was DisposableSqlDataReaderWrapper
					// DisposableSqlDataReaderWrapper required due to Microsoft.Data.SqlClient.SqlDataReader being
					// impossible to inherit from, while also exposing specific functions that we require for extra semantics.
					// Those functions aren't available in the DbDataReader base class, so we have to wrap it manually ourselves.
					await using var dataReader = await glowLoader.ExecuteReaderAsync(request, ct);
					var serialized =
						DataReaderSerializer.SerializeDataReader((DisposableSqlDataReaderWrapper)dataReader);

					try
					{
						await foreach (var item in serialized.WithCancellation(httpContext.RequestAborted))
						{
							if (ct.IsCancellationRequested)
							{
								break;
							}

							var wrapped = SqlProxyResponse<SqlProxyReaderResponseItem>.Success(item);
							var json = JsonConvert.SerializeObject(wrapped);
							await streamWriter.WriteLineAsync(json);
						}
					}
					catch (Exception ex)
					{
						try
						{
							var wrapped = SqlProxyResponse<SqlProxyReaderResponseItem>.Failure(ex);
							var json = JsonConvert.SerializeObject(wrapped);
							await streamWriter.WriteLineAsync(json);
						}
						catch (Exception ex2)
						{
							var wrapped = SqlProxyResponse<SqlProxyReaderResponseItem>.Failure(
								new Exception(
									$"An exception occurred, but couldn't be serialized.\nOriginal exception:\n{ex2}"));
							var json = JsonConvert.SerializeObject(wrapped);
							await streamWriter.WriteLineAsync(json);
						}
					}

					await streamWriter.FlushAsync(ct);
				}
				catch (Exception ex)
				{
					var executeException = new Exception($"SqlProxyRequest: {request}", ex);
					Logger.Error(executeException);
					throw executeException;
				}
			});

		app.MapPost(SqlProxyUrls.BulkCopy, async (SqlProxyBulkCopyRequest request, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.BulkCopyAsync(request, ct), request));

		app.MapPost(SqlProxyUrls.BeginTransaction, async (SqlProxyRequest request, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.BeginTransactionAsync(request, ct), request));

		app.MapPost(SqlProxyUrls.RollbackTransaction, async (Guid transactionId, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.RollbackTransactionAsync(transactionId, ct), null));

		app.MapPost(SqlProxyUrls.CommitTransaction, async (Guid transactionId, CancellationToken ct) =>
			await ResponseFromTask(glowLoader.CommitTransactionAsync(transactionId, ct), null));

		return;

		async Task<SqlProxyResponse<T>> ResponseFromTask<T>(Task<T> task, SqlProxyRequest? request)
		{
			try
			{
				return SqlProxyResponse<T>.Success(await task);
			}
			catch (Exception ex)
			{
				var executeException = new Exception($"SqlProxyRequest: {request}", ex);
				Logger.Error(executeException);

				return SqlProxyResponse<T>.Failure(ex);
			}
		}
	}

	public static async Task<IDisposable> StartServerOverHttpAsync(ISqlProxy sqlLink, int port)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.ConfigureKestrel(opts =>
		{
			opts.Listen(IPAddress.IPv6Any, port, listenOptions => listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
		});

		var app = builder.Build();
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		BuildApp(app, sqlLink);

		var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
		lifetime.ApplicationStopping.Register(() =>
		{
			app.StopAsync().GetAwaiter().GetResult();
		});

		await app.StartAsync();

		return app;
	}
}
