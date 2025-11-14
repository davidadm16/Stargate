using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

            //var query = $"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE \'{request.Name}\' = a.Name";

            //var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query);

            //result.Person = person;

            //Better to use this LINQ method instead of an explicitly typed query string
            result.Person = await (from p in _context.People
                                   join ad in _context.AstronautDetails
                                       on p.Id equals ad.PersonId into people
                                   from ad in people.DefaultIfEmpty() //Left join
                                   where p.Name == request.Name
                                   select new PersonAstronaut
                                   {
                                       PersonId = p.Id,
                                       Name = p.Name,
                                       CurrentRank = ad.CurrentRank,
                                       CurrentDutyTitle = ad.CurrentDutyTitle,
                                       CareerStartDate = ad.CareerStartDate,
                                       CareerEndDate = ad.CareerEndDate
                                   }).FirstOrDefaultAsync();

            //query = $"SELECT * FROM [AstronautDuty] WHERE {person.PersonId} = PersonId Order By DutyStartDate Desc";

            //var duties = await _context.Connection.QueryAsync<AstronautDuty>(query);

            //result.AstronautDuties = duties.ToList();

            if(result.Person != null)
            {
                result.AstronautDuties = await _context.AstronautDuties.Where(a => a.PersonId == result.Person.PersonId).OrderByDescending(o => o.DutyStartDate).ToListAsync();

            }

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
