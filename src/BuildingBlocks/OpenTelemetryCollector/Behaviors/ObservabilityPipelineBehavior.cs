using BuildingBlocks.Core.CQRS;
using BuildingBlocks.OpenTelemetryCollector.CoreDiagnostics.Commands;
using BuildingBlocks.OpenTelemetryCollector.CoreDiagnostics.Query;
using MediatR;

namespace BuildingBlocks.OpenTelemetryCollector.Behaviors;

public class ObservabilityPipelineBehavior<TRequest, TResponse>(
    CommandHandlerActivity commandActivity,
    CommandHandlerMetrics commandMetrics,
    QueryHandlerActivity queryActivity,
    QueryHandlerMetrics queryMetrics
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest message, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var isCommand = message is ICommand<TResponse>;
        var isQuery = message is IQuery<TResponse>;

        if (isCommand)
        {
            commandMetrics.StartExecuting<TRequest>();
        }

        if (isQuery)
        {
            queryMetrics.StartExecuting<TRequest>();
        }

        try
        {
            if (isCommand)
            {
                var commandResult = await commandActivity.Execute<TRequest, TResponse>(
                                        async (activity, ct) => await next(),
                                        cancellationToken
                                    );

                commandMetrics.FinishExecuting<TRequest>();

                return commandResult;
            }

            if (isQuery)
            {
                var queryResult = await queryActivity.Execute<TRequest, TResponse>(
                                      async (activity, ct) => await next(),
                                      cancellationToken
                                  );

                queryMetrics.FinishExecuting<TRequest>();

                return queryResult;
            }
        }
        catch (System.Exception)
        {
            if (isCommand)
            {
                commandMetrics.FailedCommand<TRequest>();
            }

            if (isQuery)
            {
                queryMetrics.FailedQuery<TRequest>();
            }

            throw;
        }

        return await next();
    }
}