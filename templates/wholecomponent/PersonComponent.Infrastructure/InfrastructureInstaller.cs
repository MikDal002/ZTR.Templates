using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PersonComponent.Core
{
    public static class InfrastructureInstaller
    {
        public static void InstallPersonComponentInfrastructure(this IServiceCollection services)
        {
            // Here add your services from core
            // services.AddTransient<IPersonService, PersonService>();
            // services.AddTransient<IValidator<PersonModel>, PersonValidator>();
        }
    }
}
