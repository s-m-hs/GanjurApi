using CY_DM;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysPcController : ControllerBase
    {
        private readonly CyContext _db;

        public SysPcController(CyContext db)
        {
            _db = db;

        }


        [HttpPost]
        async public Task<ActionResult> newFactor([FromBody] SysPC pc)
        {
            if (pc == null) return BadRequest();
            pc.CreateDate = DateTime.UtcNow;
            pc.IsVisible = true;

            await _db.SysPC.AddAsync(pc);

            await _db.SaveChangesAsync();

            return Ok(pc);
        }

        [HttpGet("getFactor")]
        async public Task<ActionResult> getFactor(int? id)
        {
            var query = _db.SysPC.Where(x => x.IsVisible).Include(s => s.HardWare).ToList();

            if (id != null)
            {
                var factor = query.Where(x => x.ID == id).FirstOrDefault();
                return Ok(factor);

            }
            query.Reverse();
            return Ok(query);
        }

        [HttpDelete("delFactor")]
        async public Task<ActionResult> delFactor(int? id)
        {
            var factor = await _db.SysPC.Where(x => x.IsVisible && x.ID == id).FirstOrDefaultAsync();
            if (factor == null) return BadRequest();
            factor.IsVisible = false;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("editFactor")]
        public async Task<ActionResult> editFactor([FromBody] SysPC pc)
        {
            var factor = await _db.SysPC
                .AsNoTracking()
                .Include(s => s.HardWare)
                .FirstOrDefaultAsync(x => x.IsVisible && x.ID == pc.ID);

            if (factor == null)
                return NotFound();

            // مقادیر جدید
            factor.LastModified = pc.LastModified;
            factor.ShopSale = pc.ShopSale;
            factor.CustmerPhone = pc.CustmerPhone;
            factor.CustmerName = pc.CustmerName;
            factor.Description = pc.Description;
            factor.IsFactor = pc.IsFactor;
            factor.HardWare = pc.HardWare;

            // EF باید بدونه این entity قراره آپدیت بشه:
            _db.SysPC.Update(factor);

            await _db.SaveChangesAsync();

            return Ok();
        }




    }
}
