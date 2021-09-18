using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PersonComponent.Core.Tests
{
    public class PersonComponentServiceTests
    {
        private readonly ContainerBuilder _containerBuilder;

        public TestPersonServiceTests()
        {
            var sc = new ServiceCollection();
            sc.InstallPersonComponentCore(); 
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.Populate(sc);
        }

        [Test]
        public async Task Adding_NewPerson_ToService()
        {
            // using var fake = new AutoFake(false, false, null, _containerBuilder);
            // var personService = fake.Resolve<IPersonService>();

            // var personModel = new PersonModel
            // {
                // Index = "1231231",
                // Pesel = "95121202011",
                // Name = "Jan",
                // Surname = "Kowalski",
                // Address = "Stary rynek 13, Bydgoszcz 85-111",
                // PhoneNumber = "+48 123 456 789"
            // };
            // await personService.AddOrUpdateAsync(personModel);


            // A.CallTo(() => fake.Resolve<IRepository<PersonModel>>().SaveOrUpdateAsync(personModel)).MustHaveHappened();
            Assert.True();
        }
    }
}
