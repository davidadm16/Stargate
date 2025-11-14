using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string currentName { get; set; } = string.Empty;
        public string newName { get; set; } = string.Empty;

        //public string CurrentRank { get; set; } = string.Empty;

        //public string CurrentDutyTitle { get; set; } = string.Empty;

        //public DateTime? CareerStartDate { get; set; }

        //public DateTime? CareerEndDate { get; set; }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;
        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {

            var existingPerson = await _context.People.FirstOrDefaultAsync(p => p.Name == request.currentName);

            if (existingPerson != null)
            {
                try
                {
                    existingPerson.Name = request.newName;

                    //TODO Also Implement Update or Adding of AstronautDetail/Duty if possible
                    //if (existingPerson.AstronautDuties != null) 
                    //{
                    //    existingPerson
                    //}



                    await _context.SaveChangesAsync();
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Error in saving data");
                }
            }
            else //There is no user in the database, a new user should be created
            {
                //TODO Implement route to already existing method
            }

            return new UpdatePersonResult()
            {
                Id = existingPerson.Id,
                Name = existingPerson.Name
            };

        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; } = string.Empty;
    }
}
