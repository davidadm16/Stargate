using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            //var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id";

            //var people = await _context.Connection.QueryAsync<PersonAstronaut>(query);

            //result.People = people.ToList();

            LogEvent logEvent = new LogEvent()
            {
                Name = "GetPeople",
                Description = "Method for Getting all the People",
                StartTime = DateTime.UtcNow
            };

            _context.LogEvents.Add(logEvent);

            //Better to use this LINQ method instead of an explicitly typed query string
            result.People = await (from p in _context.People
                                   join ad in _context.AstronautDetails
                                       on p.Id equals ad.PersonId into people
                                   from ad in people.DefaultIfEmpty() //Left join
                                   select new PersonAstronaut
                                   {
                                       PersonId = p.Id,
                                       Name = p.Name,
                                       CurrentRank = ad.CurrentRank,
                                       CurrentDutyTitle = ad.CurrentDutyTitle,
                                       CareerStartDate = ad.CareerStartDate,
                                       CareerEndDate = ad.CareerEndDate
                                   }).ToListAsync();

            logEvent.IsException = false;
            logEvent.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
