extern alias Library1;
extern alias Library2;

using Merq;
using Microsoft.Extensions.DependencyInjection;

// Initialize services
var collection = new ServiceCollection();

// Library1 contains [Service]-annotated classes, which will be automatically registered here.
collection.AddServices();

var services = collection.BuildServiceProvider();
var handler = services.GetRequiredService<ICommandHandler<Library1::Library.Echo, string>>();

var message = handler.Execute(new Library1::Library.Echo("Hello"));

Console.WriteLine(message);
