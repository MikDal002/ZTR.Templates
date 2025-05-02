using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System;
using System.IO;
using ZtrTemplates.Configuration.Shared;

namespace ConsoleTemplate.DependencyInjection;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar()
    {
        _services = new ServiceCollection();

        // --- Configuration Setup ---
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            // IF file doesn't exists run _build project first.
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // --- Dependency Injection Setup ---
        _services.AddSingleton<IConfiguration>(configuration);
        _services.Configure<ZtrTemplates.Configuration.Shared.UpdateOptions>(configuration.GetSection(nameof(UpdateOptions)));

        // Register application services
        _services.AddSingleton<IUpdateService, UpdateService>();
        // Add other services here if needed
    }

    public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => _services.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => _services.AddSingleton(service, _ => factory());
}
