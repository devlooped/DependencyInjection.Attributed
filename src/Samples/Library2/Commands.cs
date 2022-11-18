using Merq;

namespace Library;

public record NoOp() : ICommand;

public record Echo(string Message) : ICommand<string>;

// Simulates missing ctor arg in older library (2)
public record Echo2(string Message) : ICommand<string>;

public record NoOpAsync() : IAsyncCommand;

public record EchoAsync(string Message) : IAsyncCommand<string>;
