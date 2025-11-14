using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests
{
    public class PersonControllerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var value = new StargateContext(options);
            value.Database.EnsureCreated();//Ensures seeded data is added to DB
            return value;
        }

        [Fact]
        public async Task GetPeopleAsync_GetAllPeople()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();
            
            //Act
            var people = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(people);
        }

        [Fact]
        public async Task GetPersonByNameAsync_ReturnPersonWhenFound()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName
            {
                Name = "John Doe"
            };

            //Act
            var person = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(request.Name, person.Person.Name);
            Assert.Equal(1, person.Person.PersonId);
            Assert.Equal("1LT", person.Person.CurrentRank);
            Assert.Equal("Commander", person.Person.CurrentDutyTitle);
        }

        [Fact]
        public async Task GetPersonByNameAsync_ReturnNullWhenNotFound()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName
            {
                Name = "Test Test"
            };

            //Act
            var person = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Null(person.Person);
        }

        [Fact]
        public async Task CreatePersonAsync_AddPersonWithUniqueName()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreatePersonPreProcessor(context);
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson
            {
                Name = "Test Test"
            };

            //Act
            await preProcessor.Process(request, CancellationToken.None);
            var personId = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(personId);
        }

        [Fact]
        public async Task CreatePersonAsync_AddPersonWithDuplicateName()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreatePersonPreProcessor(context);
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson
            {
                Name = "John Doe"
            };

            //Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                preProcessor.Process(request, CancellationToken.None)
            );
            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task CreatePersonAsync_AddUniquePersonTwice()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreatePersonPreProcessor(context);
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson
            {
                Name = "Test Test"
            };

            //Act
            //Adds Unique Name First Time
            await preProcessor.Process(request, CancellationToken.None);
            var value = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(value.Id);

            //Tries to readd the Unique Name a second time
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                preProcessor.Process(request, CancellationToken.None)
            );
            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task UpdatePersonAsync_UpdateFoundPerson()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new UpdatePersonPreProcessor(context);
            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "David Diaz"
            };

            //Act
            await preProcessor.Process(request, CancellationToken.None);
            var value = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(value.Id);
            Assert.Equal(request.NewName, value.Name);
        }

        [Fact]
        public async Task UpdatePersonAsync_UpdateNotFoundPerson()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new UpdatePersonPreProcessor(context);
            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "David Diaz",
                NewName = "Emily Diaz"
            };

            //Act
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                preProcessor.Process(request, CancellationToken.None)
            );
            Assert.Equal(400, exception.StatusCode);
        }
    }
}
