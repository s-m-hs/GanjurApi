using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CY_DM;
using CY_BM;
using CY_WebApi.Models;
using System.Runtime.InteropServices;
using AutoMapper;
using CY_WebApi.DataAccess;
using CY_WebApi.Services;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyKeyDatasController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly Repository<CyKeyData> _repo;
        public CyKeyDatasController(CyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _repo = new Repository<CyKeyData>(context);
        }

        // GET: api/CyKeyDatas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyDataDTO>>> GetCyKeyData()
        {
            var resu = await _repo.TableNoTracking.ToListAsync();// ; _db.CyKeyData.ToListAsync();
            return Ok(resu.Select(c => _mapper.Map<KeyDataDTO>(c)).ToList());
        }

        // GET: api/CyKeyDatas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KeyDataDTO>> GetCyKeyData(int id)
        {
            var CyKeyData = await _repo.Find(id);
            if (CyKeyData == null)
            {
                return NotFound();
            }

            return _mapper.Map<KeyDataDTO>(CyKeyData);
        }

        // PUT: api/CyKeyDatas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCyKeyData(int id, KeyDataDTO KeyDataDTO)
        {
            var CyKeyData = _mapper.Map<CyKeyData>(KeyDataDTO);
            if (id != CyKeyData.ID)
            {
                return BadRequest("Not Valid ID");
            }
            try
            {
                await _repo.Update(CyKeyData);//  _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // POST: api/CyKeyDatas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<KeyDataDTO>> PostCyKeyData(KeyDataDTO KeyDataDTO)
        {
            var CyKeyData = _mapper.Map<CyKeyData>(KeyDataDTO);
            CyKeyData = await _repo.Insert(CyKeyData);
            return CreatedAtAction("GetCyKeyData", new { id = CyKeyData.ID }, _mapper.Map<KeyDataDTO>(CyKeyData));
        }

        // DELETE: api/CyKeyDatas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCyKeyData(int id)
        {
            await _repo.Delete(id);
            return NoContent();
        }

        //[TypeAuthorize([UserType.SysAdmin])]
        [HttpGet("GetOrderStatus")]
        public async Task<IActionResult> GetOrderStatus()
        {
            var OrderStatusList = _repo.TableNoTracking.Where(o => o.Tag == "orderStatus").ToList();
            return Ok(_mapper.Map<ICollection<KeyDataDTO>>(OrderStatusList));
        }

        //[TypeAuthorize([UserType.SysAdmin])]
        [HttpGet("GetOrderItemStatus")]
        public async Task<IActionResult> GetOrderItemStatus()
        {
            var OrderStatusList = _repo.TableNoTracking.Where(o => o.Tag == "orderItemStatus").ToList();
            return Ok(_mapper.Map<ICollection<KeyDataDTO>>(OrderStatusList));
        }
    }
}
