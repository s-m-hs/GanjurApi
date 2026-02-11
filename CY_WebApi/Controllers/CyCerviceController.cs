using CY_DM;
using CY_BM;
using CY_WebApi.DataAccess;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CY_WebApi.Services;
using System.Security.Claims;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyCerviceController : ControllerBase
    {
        private readonly CyContext _db;
        private readonly IMapper _mapper;

        public CyCerviceController(CyContext db, IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        [HttpPost("addService")]
        async public Task<ActionResult> addService([FromBody] ServiceDTO dto) { 
        
            var newService=_mapper.Map<CyService>(dto);

            _db.CyService.Add(newService);
            await _db.SaveChangesAsync();

            return Ok(newService.ID);

        }

        [HttpGet("getService")]
        async public Task<ActionResult> getService(int id) { 
        var currentService=await _db.CyService.Where(x=>x.IsVisible && x.ID == id).FirstOrDefaultAsync();
            if (currentService == null) return BadRequest(new { msg = "رسید پیدا نشد" });

           var dto=_mapper.Map<ServiceDTO>(currentService);


            return Ok(dto);
        }


        [HttpGet("getAllServicesByType")]
        async public Task<ActionResult> getServices(GuaranteeType type)
        {
            var servicesList =await _db.CyService.Where(x => x.IsVisible && x.Type == type).ToListAsync();

            var dto = _mapper.Map<List<ServiceDTO>>(servicesList);

            dto.Reverse();

            return Ok(dto);
        }


    }
}