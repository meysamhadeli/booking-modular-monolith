using System.Diagnostics;
using System.Diagnostics.Metrics;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.OpenTelemetryCollector.DiagnosticsProvider;

namespace BuildingBlocks.OpenTelemetryCollector.CoreDiagnostics.Query;

public class QueryHandlerMetrics
{
    private readonly UpDownCounter<long> _activeQueriesCounter;
    private readonly Counter<long> _totalQueriesNumber;
    private readonly Counter<long> _successQueriesNumber;
    private readonly Counter<long> _failedQueriesNumber;
    private readonly Histogram<double> _handlerDuration;

    private Stopwatch _timer;

    public QueryHandlerMetrics(IDiagnosticsProvider diagnosticsProvider)
    {
        _activeQueriesCounter = diagnosticsProvider.Meter.CreateUpDownCounter<long>(
            TelemetryTags.Metrics.Application.Queries.ActiveCount,
            unit: "{active_queries}",
            description: "Number of queries currently being handled"
        );

        _totalQueriesNumber = diagnosticsProvider.Meter.CreateCounter<long>(
            TelemetryTags.Metrics.Application.Queries.TotalExecutedCount,
            unit: "{total_queries}",
            description: "Total number of executed query that sent to query handlers"
        );

        _successQueriesNumber = diagnosticsProvider.Meter.CreateCounter<long>(
            TelemetryTags.Metrics.Application.Queries.SuccessCount,
            unit: "{success_queries}",
            description: "Number queries that handled successfully"
        );

        _failedQueriesNumber = diagnosticsProvider.Meter.CreateCounter<long>(
            TelemetryTags.Metrics.Application.Queries.FailedCount,
            unit: "{failed_queries}",
            description: "Number queries that handled with errors"
        );

        _handlerDuration = diagnosticsProvider.Meter.CreateHistogram<double>(
            TelemetryTags.Metrics.Application.Queries.HandlerDuration,
            unit: "s",
            description: "Measures the duration of query handler"
        );
    }

    public void StartExecuting<TQuery>()
    {
        var queryName = typeof(TQuery).Name;
        var handlerType = typeof(TQuery)
            .Assembly.GetTypes()
            .FirstOrDefault(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                        && i.GetGenericArguments()[0] == typeof(TQuery)
                    )
            );
        var queryHandlerName = handlerType?.Name;

        var tags = new TagList
        {
            { TelemetryTags.Tracing.Application.Queries.Query, queryName },
            { TelemetryTags.Tracing.Application.Queries.QueryType, typeof(TQuery).FullName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandler, queryHandlerName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandlerType, handlerType?.FullName },
        };

        if (_activeQueriesCounter.Enabled)
        {
            _activeQueriesCounter.Add(1, tags);
        }

        if (_totalQueriesNumber.Enabled)
        {
            _totalQueriesNumber.Add(1, tags);
        }

        _timer = Stopwatch.StartNew();
    }

    public void FinishExecuting<TQuery>()
    {
        var queryName = typeof(TQuery).Name;
        var handlerType = typeof(TQuery)
            .Assembly.GetTypes()
            .FirstOrDefault(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                        && i.GetGenericArguments()[0] == typeof(TQuery)
                    )
            );
        var queryHandlerName = handlerType?.Name;

        var tags = new TagList
        {
            { TelemetryTags.Tracing.Application.Queries.Query, queryName },
            { TelemetryTags.Tracing.Application.Queries.QueryType, typeof(TQuery).FullName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandler, queryHandlerName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandlerType, handlerType?.FullName },
        };

        if (_activeQueriesCounter.Enabled)
        {
            _activeQueriesCounter.Add(-1, tags);
        }

        if (_handlerDuration.Enabled)
        {
            _handlerDuration.Record(_timer.Elapsed.TotalSeconds, tags);
        }

        if (_successQueriesNumber.Enabled)
        {
            _successQueriesNumber.Add(1, tags);
        }
    }

    public void FailedQuery<TQuery>()
    {
        var queryName = typeof(TQuery).Name;
        var handlerType = typeof(TQuery)
            .Assembly.GetTypes()
            .FirstOrDefault(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                        && i.GetGenericArguments()[0] == typeof(TQuery)
                    )
            );
        var queryHandlerName = handlerType?.Name;

        var tags = new TagList
        {
            { TelemetryTags.Tracing.Application.Queries.Query, queryName },
            { TelemetryTags.Tracing.Application.Queries.QueryType, typeof(TQuery).FullName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandler, queryHandlerName },
            { TelemetryTags.Tracing.Application.Queries.QueryHandlerType, handlerType?.FullName },
        };

        if (_activeQueriesCounter.Enabled)
        {
            _activeQueriesCounter.Add(-1, tags);
        }

        if (_handlerDuration.Enabled)
        {
            _handlerDuration.Record(_timer.Elapsed.TotalSeconds, tags);
        }

        if (_failedQueriesNumber.Enabled)
        {
            _failedQueriesNumber.Add(1, tags);
        }
    }
}