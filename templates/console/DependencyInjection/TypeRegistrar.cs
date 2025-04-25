using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System;

namespace ConsoleTemplate;

public class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;
    public TypeRegistrar()
    {
        _services = new ServiceCollection();
        _services.AddSingleton<IUpdateService>(new UpdateService("https://frog02-20366.wykr.es/bee/downloads"));
    }
    public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());
    public void Register(Type service, Type implementation) => _services.AddSingleton(service, implementation);
    public void RegisterInstance(Type service, object implementation) => _services.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => _services.AddSingleton(service, _ => factory());
}
