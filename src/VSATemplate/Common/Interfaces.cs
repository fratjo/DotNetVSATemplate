namespace VSATemplate.Common;

// Interface for commands (write operations)
public interface ICommand { }

// Interface for queries (read operations)
public interface IQuery<TResponse> { }

// Interface for command handlers
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

// Interface for query handlers
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
