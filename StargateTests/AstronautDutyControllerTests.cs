using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests
{
    public class AstronautDutyControllerTests
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
        public async Task GetAstronautDutiesByNameAsync_FoundMatchingName()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName
            {
                Name = "John Doe"
            };

            //Act
            var person = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(person.Person);
            Assert.Equal(request.Name, person.Person.Name);
            Assert.Equal(1, person.Person.PersonId);
            Assert.Equal("1LT", person.Person.CurrentRank);
            Assert.Equal("Commander", person.Person.CurrentDutyTitle);
            Assert.NotNull(person.AstronautDuties);
            Assert.Equal(person.Person.PersonId, person.AstronautDuties.First().PersonId); //Could fail if more than one duty?
            Assert.Equal(person.Person.CurrentRank, person.AstronautDuties.First().Rank);
            Assert.Equal(person.Person.CurrentDutyTitle, person.AstronautDuties.First().DutyTitle);
        }

        [Fact]
        public async Task GetAstronautDutiesByNameAsync_NoMatchingName()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName
            {
                Name = "Test Test"
            };

            //Act
            var person = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Null(person.Person);
            Assert.Empty(person.AstronautDuties);
        }

        [Fact]
        public async Task CreateAstronautDutyAsync_CreateNewDutyFoundPerson()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreateAstronautDutyPreProcessor(context);
            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "MSTR",
                DutyTitle = "Master Chief",
                DutyStartDate = DateTime.UtcNow
            };

            //Act
            await preProcessor.Process(request, CancellationToken.None);
            var value = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(value.Id);
        }

        [Fact]
        public async Task CreateAstronautDutyAsync_CreateNewDutyNoMatchingPerson()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreateAstronautDutyPreProcessor(context);
            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "Test Test",
                Rank = "MSTR",
                DutyTitle = "Master Chief",
                DutyStartDate = DateTime.UtcNow
            };

            //Act Assert
                var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
                    preProcessor.Process(request, CancellationToken.None)
                );
            Assert.Equal(400, exception.StatusCode);
        }

        [Fact]
        public async Task CreateAstronautDutyAsync_CreateNewDutyMatchingPreviousDuty()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreateAstronautDutyPreProcessor(context);
            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "MSTR",
                DutyTitle = "Commander",
                DutyStartDate = DateTime.Now
            };

            //Act
            await preProcessor.Process(request, CancellationToken.None);
            var value = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(value.Id);
        }

        [Fact]
        public async Task CreateAstronautDutyAsync_CreateNewDutyRetiredDuty()
        {
            //Arrange
            var context = CreateInMemoryContext();
            var preProcessor = new CreateAstronautDutyPreProcessor(context);
            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "RTR",
                DutyTitle = "RETIRED",
                DutyStartDate = DateTime.Now
            };

            //Act
            await preProcessor.Process(request, CancellationToken.None);
            var value = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(value.Id);
        }

        //TODO FIX TEST LOGIC
        //[Fact]
        //public async Task CreateAstronautDutyAsync_CreateNewDutyMatchingPreviousDuty()
        //{
        //    //Arrange
        //    var context = CreateInMemoryContext();
        //    var preProcessor = new CreateAstronautDutyPreProcessor(context);
        //    var handler = new CreateAstronautDutyHandler(context);
        //    var request = new CreateAstronautDuty
        //    {
        //        Name = "John Doe",
        //        Rank = "1LT",
        //        DutyTitle = "Commander",
        //        DutyStartDate = DateTime.Now
        //    };

        //    //Act Assert
        //    var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
        //        preProcessor.Process(request, CancellationToken.None)
        //    );
        //    Assert.Equal(400, exception.StatusCode);
        //}
    }
}
